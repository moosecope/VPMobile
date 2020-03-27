using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using VPMobileObjects;
using GTG.Utilities;
using GTG.Utilities.Routing;

namespace VPMobileService
{
    public class VPMobileService : IVPMobileService
    {
        private const string _CONFIGS = "Configs";
        private const string _FMT_CONFIG = "{0}.config";
        private const string _FMT_NO_DIR = "Directory {0} does not currently exist!";
        private const string _MOD = Constants.NAMESPACE + "." + "VPMobileService" + ".";
        private const string _MCUC = "MainClientUpdateCache";
        private const string _ROUTING_DATA = "RoutingData";
        private const string _SCUC = "SplashClientUpdateCache";

        private readonly string _BASE_PATH = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase.Substring(8));

        #region public
        #region public IVPMobileService methods

        #region public IVPMobileService methods config related CRUD order
        public bool AddConfig(String session, VPMobileSettings config)//, ILog logFile)
        {
            const string LOCAL = _MOD + "AddConfig(VPMobileSettings, ILog)";

            bool retVal = false;

            try
            {
                Logging.LogMessage(Logging.LogType.Info,
                                          string.Format("Session: {0} ", session) + string.Format(Constants.FMT_ENTERING, LOCAL) +
                                          string.Format("config.Name: {0}", config.Name));
                
                string path = ReturnPath(_CONFIGS);

                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                path = Path.Combine(path, config.Name + ".config");
                using (FileStream fs = new FileStream(path, FileMode.CreateNew))
                {
                    XmlSerializer ser = new XmlSerializer(typeof(VPMobileSettings));
                    ser.Serialize(fs, config);
                }
                retVal = true;
            }
            catch (Exception ex)
            {
                Logging.LogMessage(Logging.LogType.Error,
                                          string.Format("Session: {0} ", session) + string.Format(Constants.FMT_ERROR_IN, LOCAL), ex);
                retVal = false;
            }
            return retVal;
        }

        public VPMobileSettings GetConfig(String session, string configName)//, ILog logFile)
        {
            const string LOCAL = _MOD + "AddConfig(string, ILog)";

            VPMobileSettings retVal = null;

            try
            {
                Logging.LogMessage(Logging.LogType.Info,
                                          string.Format("Session: {0} ", session) + string.Format(Constants.FMT_ENTERING, LOCAL) +
                                          string.Format("configName: {0}", configName));

                string path = ReturnPath(_CONFIGS, string.Format(_FMT_CONFIG, configName));

                //retVal = File.Exists(path) ? ConfigHandler.DeserializeConfig( logFile, path) : null;
                retVal = File.Exists(path) ? ConfigHandler.DeserializeConfig(path) : null;

                return retVal;
            }
            catch (Exception ex)
            {
                Logging.LogMessage(Logging.LogType.Error,
                                          string.Format("Session: {0} ", session) + string.Format(Constants.FMT_ERROR_IN, LOCAL), ex);
                retVal = null;
            }
            return retVal;
        }

        public Tuple<string, string>[] GetConfigs(String session)//, ILog logFile)
        {
            const string LOCAL = _MOD + "GetConfigs(ILog)";

            Tuple<string, string>[] retVal = null;

            try
            {
                Logging.LogMessage(Logging.LogType.Info,
                                          string.Format("Session: {0} ", session) + string.Format(Constants.FMT_ENTERING, LOCAL));

                string path = ReturnPath(_CONFIGS);

                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                string[] ret = Directory.GetFiles(path + @"\", "*.config");
                retVal = (from S in ret
                          //let config = ConfigHandler.DeserializeConfig(logFile, S)
                          let config = ConfigHandler.DeserializeConfig(S)
                          select new Tuple<string, string>(config.Name, config.Description)).ToArray<Tuple<string, string>>();
            }
            catch (Exception ex)
            {
                Logging.LogMessage(Logging.LogType.Error,
                                          string.Format("Session: {0} ", session) + string.Format(Constants.FMT_ERROR_IN, LOCAL), ex);
                retVal = null;
            }
            return retVal;
        }   
            
        public bool UpdateConfig(String session, VPMobileSettings config)//, ILog logFile)
        {
            const string LOCAL = _MOD + "UpdateConfig(VPMobileSettings, ILog)";

            bool retVal = false;

            try
            {
                Logging.LogMessage(Logging.LogType.Info,
                                          string.Format("Session: {0} ", session) + string.Format(Constants.FMT_ENTERING, LOCAL) +
                                          string.Format("config.Name: {0}", config.Name));

                string path = ReturnPath(_CONFIGS);

                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                path = Path.Combine(path, config.Name + ".config");

                using (FileStream fs = new FileStream(path, FileMode.Create))
                {
                    XmlSerializer ser = new XmlSerializer(typeof(VPMobileSettings));
                    ser.Serialize(fs, config);
                }
                retVal = true;
            }
            catch (Exception ex)
            {
                Logging.LogMessage(Logging.LogType.Error,
                                          string.Format("Session: {0} ", session) + string.Format(Constants.FMT_ERROR_IN, LOCAL), ex);
                retVal = false;
            }
            return retVal;
        }        
 
        public bool DeleteConfig(String session, string configName)//, ILog logFile)
        {
            const string LOCAL = _MOD + "DeleteConfig(string, ILog)";

            bool retVal = false;

            try
            {
                Logging.LogMessage(Logging.LogType.Info,
                                          string.Format("Session: {0} ", session) + string.Format(Constants.FMT_ENTERING, LOCAL) +
                                          string.Format("configName: {0}", configName));

                string path = ReturnPath(_CONFIGS, string.Format(_FMT_CONFIG, configName));

                if (File.Exists(path))
                {
                    File.Delete(path);
                    Logging.LogMessage(Logging.LogType.Warn,
                                             string.Format("session: {1} Directory {0} deleted", path, session));
                }
                else
                {
                    Logging.LogMessage(Logging.LogType.Warn,
                                              string.Format("session: {1} Could not delete path {0} because it does not exist", path, session));
                }

                retVal = true;
            }
            catch (Exception ex)
            {
                Logging.LogMessage(Logging.LogType.Error,
                                          string.Format("Session: {0} ", session) + string.Format(Constants.FMT_ERROR_IN, LOCAL), ex);
                retVal = false;
            }
            return retVal;
        }

        #endregion

        public IEnumerable<RoutingFileInfo> GetAllRoutingFileInfo(String session)//, ILog logFile)
        {
            const string LOCAL = _MOD + "GetAllRoutingFileInfo(ILog)";

            List<RoutingFileInfo> retVal = new List<RoutingFileInfo>();

            try
            {
                Logging.LogMessage(Logging.LogType.Info,
                                          string.Format("Session: {0} ", session) + string.Format(Constants.FMT_ENTERING, LOCAL));

                string path = ReturnPath(_MCUC, _ROUTING_DATA);

                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                IEnumerable<string> source = Directory.EnumerateFiles(path, "*.shp");
                var core = new Core();

                retVal = (from shapeFile in source
                          //let fields = (string[])core.GetFieldList(shapeFile, logFile)
                          let fields = (string[])core.GetFieldList(shapeFile)
                          select new RoutingFileInfo
                          {
                            Fields = fields == null ? new List<string>() : fields.ToList(),
                            RoutingFileName = Path.GetFileNameWithoutExtension(shapeFile)
                          }).ToList<RoutingFileInfo>();
            }
            catch (Exception ex)
            {
                Logging.LogMessage(Logging.LogType.Error,
                                          string.Format("Session: {0} ", session) + string.Format(Constants.FMT_ERROR_IN, LOCAL), ex);
            }
            return retVal;

        }

        #region public IVPMobileService methods Main Update
        public byte[] GetMainUpdateFile(String session, string relativePath)//, ILog logFile)
        {
            const string LOCAL = _MOD + "GetMainUpdateFile(string, ILog)";

            byte[] retVal = null;

            try
            {
                Logging.LogMessage(Logging.LogType.Info,
                                          string.Format("Session: {0} ", session) + string.Format(Constants.FMT_ENTERING, LOCAL) +
                                          string.Format("relativePath: {0}", relativePath));

                string path = ReturnPath(_MCUC, relativePath);

                retVal = File.ReadAllBytes(path);
            }
            catch (Exception ex)
            {
                Logging.LogMessage(Logging.LogType.Error,
                                          string.Format("Session: {0} ", session) + string.Format(Constants.FMT_ERROR_IN, LOCAL), ex);
                retVal = null;
            }
            return retVal;
        }

        public IEnumerable<UpdateFileInfo> GetMainUpdateFileList(String session)//, ILog logFile)
        {
            const string LOCAL = _MOD + "GetMainUpdateFileList(ILog)";

            IEnumerable<UpdateFileInfo> retVal = null; 

            try
            {
                Logging.LogMessage(Logging.LogType.Info,
                                          string.Format("Session: {0} ", session) + string.Format(Constants.FMT_ENTERING, LOCAL));

                List<UpdateFileInfo> ret = new List<UpdateFileInfo>();

                string path = ReturnPath(_MCUC);

                Uri folder = new Uri(path + "/");
                if (Directory.Exists(path))
                {
                    ret.AddRange(from fn in Directory.GetFiles(path, "*", SearchOption.AllDirectories)
                                 let fileInfo = new FileInfo(fn)
                                 select new UpdateFileInfo
                                 {
                                    FileName = Path.GetFileName(fn),
                                    FileSize = fileInfo.Length,
                                    FileDate = fileInfo.LastWriteTimeUtc,
                                    RelativeFilePath =
                                        Uri.UnescapeDataString(
                                            folder.MakeRelativeUri(new Uri(fn)).ToString().Replace('/', Path.DirectorySeparatorChar))
                                 });
                }
                else
                {
                    Logging.LogMessage(Logging.LogType.Warn,
                                              string.Format("Session: {0} ", session) + string.Format(_FMT_NO_DIR, path));
                }

                path = Path.Combine(path, _ROUTING_DATA);

                if (Directory.Exists(path))
                {
                    ret.AddRange(from fn in Directory.GetFiles(path)
                                 let fileInfo = new FileInfo(fn)
                                 select new UpdateFileInfo
                                 {
                                     FileName = Path.GetFileName(fn),
                                     FileSize = fileInfo.Length,
                                     FileDate = fileInfo.LastWriteTimeUtc,
                                     RelativeFilePath =
                                        Uri.UnescapeDataString(folder.MakeRelativeUri(new Uri(fn)).ToString().Replace('/', Path.DirectorySeparatorChar))
                                 });
                }
                else
                {
                    Logging.LogMessage(Logging.LogType.Warn,
                                              string.Format("Session: {0} ", session) + string.Format(_FMT_NO_DIR, path));
                }
                retVal = ret;
            }
            catch (Exception ex)
            {
                Logging.LogMessage(Logging.LogType.Error,
                                          string.Format("Session: {0} ", session) + string.Format(Constants.FMT_ERROR_IN, LOCAL), ex);
                retVal = new List<UpdateFileInfo>();
            }
            return retVal;
        }

        #endregion

        #region public IVPMobileService methods Splash Update
        public IEnumerable<UpdateFileInfo> GetSplashUpdateFileList(String session)// ILog logFile)
        {
            const string LOCAL = _MOD + "GetSplashUpdateFileList(ILog)";

            IEnumerable<UpdateFileInfo> retVal = null;

            try
            {
                Logging.LogMessage(Logging.LogType.Info,
                                          string.Format("Session: {0} ", session) + string.Format(Constants.FMT_ENTERING, LOCAL));

                List<UpdateFileInfo> ret = new List<UpdateFileInfo>();

                string path = ReturnPath(_SCUC);

                Uri folder = new Uri(path + "/");

                if (Directory.Exists(path))
                {
                    ret.AddRange(from fn in Directory.GetFiles(path, "*", SearchOption.AllDirectories)
                                 let fileInfo = new FileInfo(fn)
                                 select new UpdateFileInfo
                                 {
                                     FileName = Path.GetFileName(fn),
                                     FileSize = fileInfo.Length,
                                     FileDate = fileInfo.LastWriteTimeUtc,
                                     RelativeFilePath =
                                        Uri.UnescapeDataString(
                                            folder.MakeRelativeUri(new Uri(fn)).ToString().Replace('/', Path.DirectorySeparatorChar))
                                 });
                }
                else
                {
                    Logging.LogMessage(Logging.LogType.Warn,
                                              string.Format("Session: {0} ", session) + string.Format(_FMT_NO_DIR, path));
                }
                retVal = ret;
            }

            catch (Exception ex)
            {
                Logging.LogMessage(Logging.LogType.Error,
                                          string.Format("Session: {0} ", session) + string.Format(Constants.FMT_ERROR_IN, LOCAL), ex);
            }
            return retVal;
        }

        public byte[] GetSplashUpdateFile(String session, string relativePath)//, ILog logFile)
        {
            const string LOCAL = _MOD + "GetSplashUpdateFile(string, ILog)";

            byte[] retVal = null;

            try
            {
                Logging.LogMessage(Logging.LogType.Info,
                                          string.Format("Session: {0} ", session) + string.Format(Constants.FMT_ENTERING, LOCAL) +
                                          string.Format("relativePath: {0}", relativePath));

                string path = ReturnPath(_SCUC, relativePath);

                retVal = File.ReadAllBytes(path);
            }
            catch (Exception ex)
            {
                Logging.LogMessage(Logging.LogType.Error,
                                          string.Format("Session: {0} ", session) + string.Format(Constants.FMT_ERROR_IN, LOCAL), ex);
                retVal = null;
            }
            return retVal;
        }

        #endregion
        #endregion
        #endregion

        #region private
        #region private methods
        private string ReturnPath(params string[] directories)
        {
            const string DELIMITER = "\\";

            StringBuilder retVal = new StringBuilder(_BASE_PATH);

            foreach (string directory in directories)
            {
                if (!retVal.ToString().EndsWith(DELIMITER)) retVal.Append(DELIMITER);

                retVal.Append(directory);
            }

            return retVal.ToString().Trim();
        }
        #endregion
        #endregion
    }
}
