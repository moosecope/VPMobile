using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPMobileObjects
{
    public class UpdateFileInfo
    {
        #region public
        #region public properties
        public string RelativeFilePath { get; set; }
        public string FileName { get; set; }
        public DateTime FileDate { get; set; }
        public long FileSize { get; set; }
        #endregion
        #endregion

    }
}
