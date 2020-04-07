using GTG.Utilities;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.ServiceModel;
using System.Threading;
using System.Windows;
using VPMobileSplashScreen.VPMobileService;

namespace VPMobileSplashScreen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private static String MOBILE_DIRECTORY = "VPMobile";
        private static String MOBILE_EXE_NAME = "VP Mobile.exe";
        private static String MOBILE_PROCESS_NAME = "VP Mobile";
        private bool _disconnected;
        private bool _closing;
        private UpdateFileInfo[] _serverFiles;
        private int _currentServerFile;

        public MainWindow()
        {
            InitializeComponent();
            CurrentStep = "Vantage Points Mobile is checking for updates";
            CurrentFile = ". . .";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckForUpdates();
            }
            catch (Exception ex)
            {
                var message = "Error checking for updates";
                Logging.LogMessage(Logging.LogType.Error, message, ex);
                MessageBox.Show(ex.ToString());
                Dispatcher.Invoke(() => Close());
            }
        }

        private VPMobileServiceClient _mobileService;
        public VPMobileServiceClient MobileService
        {
            get
            {
                if (_mobileService != null)
                    return _mobileService;
                _mobileService = new VPMobileServiceClient();
                _mobileService.ClientCredentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;
                _mobileService.ChannelFactory.Credentials.Windows.ClientCredential = CredentialCache.DefaultNetworkCredentials;
                _mobileService.GetMainUpdateFileListCompleted += GetUpdateFileList_Completed;
                _mobileService.GetMainUpdateFileCompleted += GetUpdateFile_Completed;
                return _mobileService;
            }
        }

        private String _currentFile;
        public String CurrentFile
        {
            get { return _currentFile; }
            set
            {
                _currentFile = value;
                NotifyPropertyChanged();
            }
        }

        private String _currentStep;
        public String CurrentStep
        {
            get { return _currentStep; }
            set
            {
                _currentStep = value;
                NotifyPropertyChanged();
            }
        }

        public String CurrentMobileDirectory
        {
            get
            {
                var path = Path.Combine(VPMobileObjects.ConfigHandler.AssemblyDirectory, MOBILE_DIRECTORY + Path.DirectorySeparatorChar);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
        }

        private bool CheckIfVPMobileIsRunning()
        {
            bool ret = false;
            try
            {
                Process[] pl = Process.GetProcessesByName(MOBILE_PROCESS_NAME);
                ret = pl != null && pl.Length > 0;
            }
            catch (Exception ex)
            {
                var message = "Error checking if VP Mobile is running";
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }

            return ret;
        }

        public void CheckForUpdates()
        {
            try
            {
                if (CheckIfVPMobileIsRunning())
                {
                    MessageBox.Show("Vantage Points Mobile is already running.  Please use that instance.");
                    Close();
                }

                CurrentFile = "Pulling file information from server";
                MobileService.GetMainUpdateFileListAsync();
            }
            catch (EndpointNotFoundException ex)
            {
                OnNetworkDisconnect(ex);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                var message = "Error checking file";
                Logging.LogMessage(Logging.LogType.Error, message, ex);
                CurrentFile = "";
                Dispatcher.Invoke(() => Close());
            }
        }

        private void GetUpdateFileList_Completed(object sender, GetMainUpdateFileListCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    if (e.Error is EndpointNotFoundException)
                    {
                        OnNetworkDisconnect(e.Error);
                    }
                    else if (e.Error is CommunicationException && e.Error.Message.Equals("The underlying connection was closed: A connection that was expected to be kept alive was closed by the server."))
                    {
                        OnNetworkDisconnect(e.Error);
                    }
                    else
                    {
                        var message = "Error checking file";
                        Logging.LogMessage(Logging.LogType.Error, message, e.Error);
                        if (!_closing)
                        {
                            MessageBox.Show(e.Error.ToString());
                            Dispatcher.Invoke(() => Close());
                        }
                    }
                }
                else
                {
                    _serverFiles = e.Result;
                    _currentServerFile = -1;
                    CheckNextFile();
                }
            }
            catch (EndpointNotFoundException ex)
            {
                OnNetworkDisconnect(ex);
            }
            catch (Exception ex)
            {
                var message = "Error getting update file list";
                Logging.LogMessage(Logging.LogType.Error, message, ex);
                if (!_closing)
                {
                    MessageBox.Show(ex.ToString());
                    Dispatcher.Invoke(() => Close());
                }
            }
        }

        private void CheckNextFile()
        {

            try
            {
                _currentServerFile++;
                if (_currentServerFile == _serverFiles.Count())
                {
                    OpenVPMobile();
                    return;
                }
                if (_serverFiles != null || _serverFiles.Count() > 0)
                {
                    UpdateFileInfo f = _serverFiles[_currentServerFile];
                    CurrentFile = "Checking file:  " + Path.GetFileName(f.FileName);
                    string filePath = Path.Combine(CurrentMobileDirectory, f.RelativeFilePath);
                    if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    }
                    if (!File.Exists(filePath) || File.GetLastWriteTimeUtc(filePath).CompareTo(f.FileDate) < 0)
                    {
                        Logging.LogMessage(Logging.LogType.Info, f.FileName + " - Local: " + File.GetLastWriteTimeUtc(filePath).ToString("MM/dd hh:mm:ss.ffff") + " || Server: " + f.FileDate.ToString("MM/dd hh:mm:ss.ffff"));
                        CurrentFile = "Updating file:  " + Path.GetFileName(f.FileName);
                        MobileService.GetMainUpdateFileAsync(f.RelativeFilePath, filePath);
                    }
                    else
                    {
                        CheckNextFile();
                    }
                }
            }
            catch (Exception ex)
            {
                var message = "Error checking file";
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        private void GetUpdateFile_Completed(object sender, GetMainUpdateFileCompletedEventArgs e)
        {
            try
            {
                if (e.Error != null)
                {
                    if (e.Error.InnerException is WebException && ((WebException)e.Error.InnerException).Status == WebExceptionStatus.RequestCanceled)
                    {
                        Thread sep = new Thread((ThreadStart)(() =>
                        {
                            try
                            {
                                Uri folder = new Uri(CurrentMobileDirectory);
                                string s = Uri.UnescapeDataString(folder.MakeRelativeUri(new Uri(e.UserState.ToString())).ToString().Replace('/', Path.DirectorySeparatorChar));
                                MobileService.GetMainUpdateFileAsync(s, e.UserState);
                            }
                            catch (Exception ex2)
                            {
                                var message = "Error pulling file from server";
                                Logging.LogMessage(Logging.LogType.Error, message, ex2);
                            }
                        }));
                        sep.Start();
                    }
                    else if (e.Error is CommunicationException && e.Error.Message.Equals("The underlying connection was closed: A connection that was expected to be kept alive was closed by the server."))
                    {
                        OnNetworkDisconnect(e.Error);
                    }
                    else if (e.Error is EndpointNotFoundException)
                    {
                        OnNetworkDisconnect(e.Error);
                    }
                    else
                    {
                        if (!_closing)
                        {
                            MessageBox.Show("Error updating file - " + e.UserState.ToString() + ": " + e.Error.ToString());
                        }
                    }
                }
                else
                {
                    if (e.Result != null)
                    {
                        File.WriteAllBytes(e.UserState.ToString(), e.Result);
                    }

                    CurrentFile = "Updated file:  " + Path.GetFileName(e.UserState.ToString());
                }
                CheckNextFile();
            }
            catch (EndpointNotFoundException ex)
            {
                OnNetworkDisconnect(ex);
            }
            catch (Exception ex)
            {
                var message = "Error updating file";
                Logging.LogMessage(Logging.LogType.Error, message, ex);
                if (!_closing)
                {
                    MessageBox.Show(ex.ToString());
                    OpenVPMobile();
                }
            }
        }

        private void OnNetworkDisconnect(Exception ex)
        {
            if (_disconnected)
                return;
            _disconnected = true;
            OpenVPMobile();
        }

        private void OpenVPMobile()
        {
            CurrentFile = "";
            CurrentStep = "Loading Vantage Points Mobile";
            string exePath = Path.Combine(VPMobileObjects.ConfigHandler.AssemblyDirectory, MOBILE_DIRECTORY, MOBILE_EXE_NAME );
            if (File.Exists(exePath))
            {
                Directory.SetCurrentDirectory(Path.Combine(VPMobileObjects.ConfigHandler.AssemblyDirectory, MOBILE_DIRECTORY));
                Process.Start(exePath, "/UpdateSplashScreen");
                Dispatcher.Invoke(() => Close());
            }
            else
            {
                if (!_closing)
                {
                    MessageBox.Show("Could not find main executable at " + exePath);
                    Dispatcher.Invoke(() => Close());
                }
            }
        }

        private void MainWindowClosing(object sender, CancelEventArgs e)
        {
            _closing = true;
        }
    }
}