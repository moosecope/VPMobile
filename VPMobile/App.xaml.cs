using Esri.ArcGISRuntime;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using VP_Mobile.ViewModels;

namespace VP_Mobile
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                // Deployed applications must be licensed at the Lite level or greater. 
                // See https://developers.arcgis.com/licensing for further details.

                for (int i = 0; i != e.Args.Length; ++i)
                {
                    if (e.Args[i] == "/UpdateSplashScreen")
                    {
                        MainViewModel.UpdateSplashScreen = true;
                    }
                }

                // Initialize the ArcGIS Runtime before any components are created.
                ArcGISRuntimeEnvironment.Initialize();
                Cache.License();

                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

                // Add the event handler for handling UI thread exceptions to the event.
                Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "ArcGIS Runtime initialization failed.");

                // Exit application
                this.Shutdown();
            }
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                MessageBox.Show(e.Exception.ToString(), "Unhandled Exception.");
                var appLog = new EventLog { Source = Process.GetCurrentProcess().ProcessName };
                appLog.WriteEntry(e.Exception.ToString(), EventLogEntryType.Error);
            }
            catch (Exception) { }
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            try
            {
                MessageBox.Show(e.Exception.ToString(), "Unhandled Exception.");
                var appLog = new EventLog { Source = Process.GetCurrentProcess().ProcessName };
                appLog.WriteEntry(e.Exception.ToString(), EventLogEntryType.Error);
            }
            catch (Exception) { }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                MessageBox.Show(e.ExceptionObject.ToString(), "Unhandled Exception.");
                var appLog = new EventLog { Source = Process.GetCurrentProcess().ProcessName };
                appLog.WriteEntry(e.ExceptionObject.ToString(), EventLogEntryType.Error);
            }
            catch (Exception) { }
        }
    }
}
