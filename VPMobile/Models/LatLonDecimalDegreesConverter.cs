using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VP_Mobile.Models
{
    public class LatLonDecimalDegreesConverter
    {
        private double _dmslat_deg;
        public double DMSLat_Deg
        {
            get { return _dmslat_deg; }
            set
            {
                _dmslat_deg = value;
                try
                {
                    ConvertSexagesimalToDecimal();
                }
                catch (NullReferenceException) { }
            }
        }

        private double _dmslat_min;
        public double DMSLat_Min
        {
            get { return _dmslat_min; }
            set
            {
                _dmslat_min = value;
                try
                {
                    ConvertSexagesimalToDecimal();
                }
                catch (NullReferenceException) { }
            }
        }

        private double _dmslat_sec;
        public double DMSLat_Sec
        {
            get { return _dmslat_sec; }
            set
            {
                _dmslat_sec = value;
                try
                {
                    ConvertSexagesimalToDecimal();
                }
                catch (NullReferenceException) { }
            }
        }

        private double _dmslong_deg;
        public double DMSLong_Deg
        {
            get { return _dmslong_deg; }
            set
            {
                _dmslong_deg = value;
                try
                {
                    ConvertSexagesimalToDecimal();
                }
                catch (NullReferenceException) { }
            }
        }

        private double _dmslong_min;
        public double DMSLong_Min
        {
            get { return _dmslong_min; }
            set
            {
                _dmslong_min = value;
                try
                {
                    ConvertSexagesimalToDecimal();
                }
                catch (NullReferenceException) { }
            }
        }

        private double _dmslong_sec;
        public double DMSLong_Sec
        {
            get { return _dmslong_sec; }
            set
            {
                _dmslong_sec = value;
                try
                {
                    ConvertSexagesimalToDecimal();
                }
                catch (NullReferenceException) { }
            }
        }

        private double _ddlat;
        public double DDLat
        {
            get { return _ddlat; }
            set
            {
                _ddlat = value;
                try
                {
                    ConvertDecimalToSexagesimal();
                }
                catch (NullReferenceException) { }
            }
        }

        private double _ddlong;
        public double DDLong
        {
            get { return _ddlong; }
            set
            {
                _ddlong = value;
                try
                {
                    ConvertDecimalToSexagesimal();
                }
                catch (NullReferenceException) { }
            }
        }

        private void ConvertDecimalToSexagesimal()
        {
            var val = Math.Abs(DDLat);

            _dmslat_deg = Math.Truncate(val);
            _dmslat_min = Math.Truncate((val - DMSLat_Deg) * 60);            
            _dmslat_sec = Math.Truncate((((val - DMSLat_Deg) * 60 - DMSLat_Min) * 60) * 1000) / 1000;

            val = Math.Abs(DDLong);

            _dmslong_deg = Math.Truncate(val);
            _dmslong_min = Math.Truncate((val - DMSLong_Deg) * 60);
            _dmslong_sec = Math.Truncate((((val - DMSLong_Deg) * 60 - DMSLong_Min) * 60) * 1000) / 1000;
        }

        private void ConvertSexagesimalToDecimal()
        {
            _ddlat = (double)(DMSLat_Deg + DMSLat_Min / 60.0 + DMSLat_Sec / 3600.0);
            _ddlong = (double)(DMSLong_Deg + DMSLong_Min / 60.0 + DMSLong_Sec / 3600.0);
        }
    }
}
