using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

using log4net;

using UTIL = GTG.Utilities;

namespace VPMobileObjects
{
    public class ConfigHandler
    {
        private const string _MOD = Constants.NAMESPACE + "." + "VPMobileObjects" + ".";
        #region public

        #region pubilc properties
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
        #endregion

        #region public static methods
  
        //public static VPMobileSettings DeserializeConfig(ILog logFile, string path = null)
        public static VPMobileSettings DeserializeConfig(string path = null)
        {
            const string LOCAL = _MOD + "DeserializeConfig(ILog, string)";
            string nonNullPath = path ?? Constants.NULL;
            try
            {
                //UTIL.Utilities.LogMessage(logFile, UTIL.Utilities.LogType.Info,
                //                          string.Format(Constants.FMT_ENTERING, LOCAL) +
                //                          string.Format("path: {0}", nonNullPath.Trim()));

                path = (File.Exists(path) ? path : Path.Combine(AssemblyDirectory, "VPMobileSettings"));

                if (!File.Exists(path))
                {
                    //UTIL.Utilities.LogMessage(logFile, UTIL.Utilities.LogType.Warn,
                    //                         string.Format("Path: {0} does not currently exist", path));
                    return null;
                }
                else
                {
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        var ser = new XmlSerializer(typeof(VPMobileSettings));
                        return (VPMobileSettings)ser.Deserialize(fs);
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = string.Format(Constants.FMT_ERROR_IN, LOCAL);

                //UTIL.Utilities.LogMessage(logFile, UTIL.Utilities.LogType.Error, msg, ex);
                throw new ApplicationException(msg, ex);
            }
        }
        #endregion
        #endregion
    }
}
