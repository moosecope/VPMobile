using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Xml.Serialization;

using log4net;

using UTIL = GTG.Utilities;
using VPMobileObjects;

namespace VPMobileService
{
    [ServiceContract]
    public interface IVPMobileService
    {
        [OperationContract]
        VPMobileSettings GetConfig(String session, string configName);//, ILog logFile);

        [OperationContract]
        Tuple<string, string>[] GetConfigs(String session);// ILog logFile);

        [OperationContract]
        bool UpdateConfig(String session, VPMobileSettings config);//, ILog logFile);

        [OperationContract]
        bool AddConfig(String session, VPMobileSettings config);//, ILog logFile);
            
        [OperationContract]
        IEnumerable<RoutingFileInfo> GetAllRoutingFileInfo(String session);// ILog logFile);;

        [OperationContract]
        bool DeleteConfig(String session, string configName);//, ILog logFile);

        [OperationContract]
        IEnumerable<UpdateFileInfo> GetMainUpdateFileList(String session);// ILog logFile);;

        [OperationContract]
        IEnumerable<UpdateFileInfo> GetSplashUpdateFileList(String session);// ILog logFile);;

        [OperationContract]
        byte[] GetMainUpdateFile(String session, string relativePath);//, ILog logFile);

        [OperationContract]
        byte[] GetSplashUpdateFile(String session, string relativePath);//, ILog logFile);

    }
}
