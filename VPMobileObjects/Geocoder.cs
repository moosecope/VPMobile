using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using log4net;

using UTIL = GTG.Utilities;

namespace VPMobileObjects
{
	[ Serializable ]
	public class Geocoder
	{
        #region private module level variables
        private const string _MOD = Constants.NAMESPACE + "." + "Geocoder" + ".";

        private readonly Dictionary<string, string> m_Abbreviation = new Dictionary<string, string>();
        private readonly Dictionary<string, string> m_Abbreviation_Multi_Word = new Dictionary<string, string>();
        private readonly Dictionary<string, string> m_Cardinal_Number = new Dictionary<string, string>();
        private readonly Dictionary<string, string> m_Direction = new Dictionary<string, string>();
        private readonly Dictionary<string, string> m_Multi_Unit = new Dictionary<string, string>();
        private readonly Dictionary<string, string> m_Number_Suffix = new Dictionary<string, string>();
        private readonly Dictionary<string, string> m_Ordinal_Number = new Dictionary<string, string>();
        private readonly Dictionary<string, string> m_Route_Modifer = new Dictionary<string, string>();
        private readonly Dictionary<string, string> m_Street_Type = new Dictionary<string, string>();
        #endregion

        #region public
        #region public construtors
        public Geocoder()
        {
            GeocoderDatasets = new List<gtgGeocodeDataset>();
        }
        #endregion

        #region public properties
        public List<gtgGeocodeDataset> GeocoderDatasets { get; set; }
        #endregion

        #region public methods

        #region public static methods
        public static int Compute( string s, string t)//, ILog logFile )
		{
            //UTIL.Utilities.LogMessage(logFile, UTIL.Utilities.LogType.Info,
            //                          String.Format("Entering Compute(string, string, ILog): s: {0}, t:{1}", s, t));
			int result;
			if ( string.IsNullOrEmpty( s ) )
				result = ( string.IsNullOrEmpty( t ) ? 0 : t.Length );
			else
			{
                if (string.IsNullOrEmpty(t))
                {
                    result = s.Length;
                }
                else
                {
                    int i = s.Length;
                    int j = t.Length;
                    var d = new int[i + 1, j + 1];
                    int k = 0;
                    while (k <= i)
                        d[k, 0] = k++;
                    int l = 1;
                    while (l <= j)
                        d[0, l] = l++;
                    for (k = 1; k <= i; k++)
                    {
                        for (l = 1; l <= j; l++)
                        {
                            int cost = (t[l - 1] == s[k - 1]) ? 0 : 1;
                            int min = d[k - 1, l] + 1;
                            int min2 = d[k, l - 1] + 1;
                            int min3 = d[k - 1, l - 1] + cost;
                            d[k, l] = Math.Min(Math.Min(min, min2), min3);
                        }
                    }
                    result = d[i, j];
                }
			}
			return result;
		}
        #endregion

		public void SetStandardizerInfo(gtgStandardizerInfo StandardizerInfo)//, ILog logFile)
		{
            const string LOCAL = _MOD + "SetStandardizerInfo(gtgStandardizerInfo, ILog)";
			try
			{
                //UTIL.Utilities.LogMessage(logFile, UTIL.Utilities.LogType.Info,
                //                          String.Format("Entering {0}", LOCAL));

                //  Note to self: Look into cleaning up this logic (MLR: 1/16/2018)
                m_Number_Suffix.Clear();
				foreach ( gtgStandardizerRule sr in StandardizerInfo.Number_Suffix )
				{
					if ( !m_Number_Suffix.ContainsKey( sr.SearchPattern ) )
						m_Number_Suffix.Add( sr.SearchPattern, sr.ReplaceWith );
				}

                m_Route_Modifer.Clear();
				foreach ( gtgStandardizerRule sr in StandardizerInfo.Route_Modifer )
				{
					if ( !m_Route_Modifer.ContainsKey( sr.SearchPattern ) )
						m_Route_Modifer.Add( sr.SearchPattern, sr.ReplaceWith );
				}

                m_Multi_Unit.Clear();
				foreach ( gtgStandardizerRule sr in StandardizerInfo.Multi_Unit )
				{
					if ( !m_Multi_Unit.ContainsKey( sr.SearchPattern ) )
						m_Multi_Unit.Add( sr.SearchPattern, sr.ReplaceWith );
				}

                m_Cardinal_Number.Clear();
				foreach ( gtgStandardizerRule sr in StandardizerInfo.Cardinal_Number )
				{
					if ( !m_Cardinal_Number.ContainsKey( sr.SearchPattern ) )
						m_Cardinal_Number.Add( sr.SearchPattern, sr.ReplaceWith );
				}

                m_Direction.Clear();
				foreach ( gtgStandardizerRule sr in StandardizerInfo.Direction )
				{
					if ( !m_Direction.ContainsKey( sr.SearchPattern ) )
						m_Direction.Add( sr.SearchPattern, sr.ReplaceWith );
				}

                m_Ordinal_Number.Clear();
				foreach ( gtgStandardizerRule sr in StandardizerInfo.Ordinal_Number )
				{
					if ( !m_Ordinal_Number.ContainsKey( sr.SearchPattern ) )
						m_Ordinal_Number.Add( sr.SearchPattern, sr.ReplaceWith );
				}

                m_Abbreviation.Clear();
				foreach ( gtgStandardizerRule sr in StandardizerInfo.Abbreviation )
				{
					if ( !m_Abbreviation.ContainsKey( sr.SearchPattern ) )
						m_Abbreviation.Add( sr.SearchPattern, sr.ReplaceWith );
				}

                m_Abbreviation_Multi_Word.Clear();
				foreach ( gtgStandardizerRule sr in StandardizerInfo.Abbreviation_Multi_Word )
				{
					if ( !m_Abbreviation_Multi_Word.ContainsKey( sr.SearchPattern ) )
						m_Abbreviation_Multi_Word.Add( sr.SearchPattern, sr.ReplaceWith );
				}

                m_Street_Type.Clear();
				foreach ( gtgStandardizerRule sr in StandardizerInfo.Street_Type )
				{
					if ( !m_Street_Type.ContainsKey( sr.SearchPattern ) )
						m_Street_Type.Add( sr.SearchPattern, sr.ReplaceWith );
				}
			}
			catch ( Exception ex )
			{
                //UTIL.Utilities.LogMessage(logFile, UTIL.Utilities.LogType.Error,
                //                          String.Format(Constants.FMT_ERROR_IN, LOCAL), ex);
            }
        }

		public gtgStandardizeResult StandardizeAddress(string strAddress)//, ILog logFile)
		{
            const string LOCAL = _MOD + "StandardizeAddress(string, ILog) ";

            var ret = new gtgStandardizeResult();
			string tmpString = null;
			try
			{
                //UTIL.Utilities.LogMessage(logFile, UTIL.Utilities.LogType.Info,
                //                          String.Format("Entering {0}: strAddress: {1}", LOCAL, strAddress));

                ret.OriginalAddress = strAddress;
				var regx = new Regex( "(?:[^ ]),|, +| +" );
				ret.OriginalAddress = regx.Replace(strAddress, delegate(Match m)
				{
					var val = m.Value;
					if (val.Equals("(?:[^ ]),)"))
						return " ,";

                    return val.Equals(", +") ? "," : " ";
				});

				string[ ] strSplit = ret.OriginalAddress.Trim().ToUpper().Split(
					new[ ]
						{
							' '
						} );
				int startIndex = 0;
				int endIndex = strSplit.Length - 1;
				bool bStreetTypeFound = false;
				float tmpFloat;

				if( float.TryParse( strSplit[0], out tmpFloat ) )
				{
					ret.HouseNumber = tmpFloat;
					tmpString = ( ( strSplit.Length > 1 ) ? strSplit[1] : string.Empty );
					switch( tmpString )
					{
						case "1/2":
							ret.HouseNumber += 0.5f;
							startIndex = 2;
							break;
						case "1/4":
							ret.HouseNumber += 0.25f;
							startIndex = 2;
							break;
						case "3/4":
							ret.HouseNumber += 0.75f;
							startIndex = 2;
							break;
						default:
							startIndex = 1;
							break;
					}
				}

				if ( m_Direction.TryGetValue(
					( strSplit.Length > startIndex ) ? strSplit[startIndex] : string.Empty, out tmpString ) )
				{
					ret.PreDir = tmpString;
					startIndex++;
				}

				for( int j = endIndex; j >= startIndex; j-- )
				{
					try
					{
						if (!m_Street_Type.TryGetValue(strSplit[j], out tmpString)) continue;
						ret.StreetType = tmpString;
						endIndex = j - 1;
						bStreetTypeFound = true;
						break;
					}
					catch( IndexOutOfRangeException ex)
                    {
                        //UTIL.Utilities.LogMessage(logFile, UTIL.Utilities.LogType.Error,
                        //                          String.Format(Constants.FMT_ERROR_IN, LOCAL), ex);
                    }
				}

				if ( !bStreetTypeFound )
				{
					for( int i = endIndex - 1; i >= startIndex; i-- )
					{
						try
						{
							if (!m_Multi_Unit.TryGetValue(strSplit[i], out tmpString)) continue;
							ret.Unit = tmpString;
							endIndex = i - 1;
							break;
						}
						catch ( IndexOutOfRangeException ) {}
					}

					for( int i = endIndex; i >= startIndex; i-- )
					{
						try
						{
							if (!m_Direction.TryGetValue(strSplit[i], out tmpString)) continue;
							ret.PreDir = tmpString;
							endIndex = i - 1;
							break;
						}
						catch ( IndexOutOfRangeException ex)
                        {
                            //UTIL.Utilities.LogMessage(logFile, UTIL.Utilities.LogType.Error,
                            //                          String.Format(Constants.FMT_ERROR_IN, LOCAL), ex);
                        }
					}
				}

                for ( int i = startIndex; i <= endIndex; i++ )
				{
					try
					{
						if ( m_Abbreviation.TryGetValue( strSplit[i], out tmpString ) )
							ret.StreetName = ret.StreetName + " " + tmpString;
						else
						{
							ret.StreetName = ret.StreetName + " "
							                 + ( m_Ordinal_Number.TryGetValue( strSplit[i], out tmpString ) ? tmpString : strSplit[i] );
						}
					}
					catch ( IndexOutOfRangeException ex)
					{
                        //UTIL.Utilities.LogMessage(logFile, UTIL.Utilities.LogType.Error,
                        //                          String.Format(Constants.FMT_ERROR_IN, LOCAL), ex);
                        break;
					}
				}

                ret.StreetName = ret.StreetName.Trim();
                foreach (var kp in m_Abbreviation_Multi_Word)
                {
                    ret.StreetName = ret.StreetName.Replace(kp.Key, kp.Value);
                }

                startIndex = endIndex + 1;
				endIndex = strSplit.Length - 1;

                for ( int i = startIndex; i <= endIndex; i++ )
				{
					try
					{
						if (!m_Direction.TryGetValue(strSplit[i], out tmpString)) continue;
						ret.SufDir = tmpString;
						startIndex = i + 1;
						break;
					}
					catch ( IndexOutOfRangeException ex)
					{
                        //UTIL.Utilities.LogMessage(logFile, UTIL.Utilities.LogType.Error,
                        //                          String.Format(Constants.FMT_ERROR_IN, LOCAL), ex);
                        break;
					}
				}

                for ( int i = startIndex; i <= endIndex; i++ )
				{
					try
					{
						if (!m_Multi_Unit.TryGetValue(strSplit[i], out tmpString)) continue;
						ret.Unit = strSplit[i];
						break;
					}
					catch ( IndexOutOfRangeException ex)
					{
                        //UTIL.Utilities.LogMessage(logFile, UTIL.Utilities.LogType.Error,
                        //                          String.Format(Constants.FMT_ERROR_IN, LOCAL), ex);
                        break;
					}
				}

                for ( int i = startIndex; i <= endIndex; i++ )
				{
					try
					{
						if (!strSplit[i].Contains(",")) continue;
						string[ ] strZoneSplit = strSplit[i].Split(
							new[ ]
							{
								','
							} );
						if ( strZoneSplit.Length > 1 )
							ret.Zone = strZoneSplit[1].Trim();
						break;
					}
					catch ( IndexOutOfRangeException )
					{
						break;
					}
				}
                ret.StreetNameSoundex = GenerateSoundexCode(ret.StreetName);//, logFile );
			}
			catch ( Exception ex )
			{
                //UTIL.Utilities.LogMessage(logFile, UTIL.Utilities.LogType.Error,
                //                          String.Format(Constants.FMT_ERROR_IN, LOCAL), ex);
            }
			return ret;
		}

		public string GenerateSoundexCode(string StreetNameToCode)//, ILog logFile)
		{
            const string LOCAL = _MOD + "StandardizeAddress(string, ILog) ";

            if (string.IsNullOrWhiteSpace(StreetNameToCode))
            {
                //UTIL.Utilities.LogMessage(logFile, UTIL.Utilities.LogType.Warn,
                //                         String.Format("Retruning from {0}: StreetNameToCode IsNullOrWhiteSpace", LOCAL));
                return string.Empty;
            }
			StreetNameToCode = StreetNameToCode.Trim();

            //UTIL.Utilities.LogMessage(logFile, UTIL.Utilities.LogType.Info,
            //                         String.Format("Entering {0}: StreetNameToCode: {1}", LOCAL, StreetNameToCode));


            char currLetter = 'p', lastLetter;
			var ret = new StringBuilder(StreetNameToCode.Substring(0,1).ToUpper(), StreetNameToCode.Length);
			for (int i = 1; i < StreetNameToCode.Length; i++)
			{
				lastLetter = currLetter;
				switch( char.ToUpperInvariant( StreetNameToCode[i] ) )
				{
					case 'B':
					case 'F':
					case 'P':
					case 'V':
						currLetter = '1';
						break;
					case 'C':
					case 'G':
					case 'J':
					case 'K':
					case 'Q':
					case 'S':
					case 'X':
					case 'Z':
						currLetter = '2';
						break;
					case 'D':
					case 'T':
						currLetter = '3';
						break;
					case 'H':
					case 'W':
						break;
					case 'L':
						currLetter = '4';
						break;
					case 'M':
					case 'N':
						currLetter = '5';
						break;
					case 'R':
						currLetter = '6';
						break;
					default:
						currLetter = '!';
						break;
				}
				if(lastLetter == '!' && currLetter == '!')
					continue;
				ret.Append(currLetter);
			}

			if( ret.Length < 4 )
				ret.Append(new string('0', 4 - ret.Length ));
			return ret.ToString();
		}

		public List<gtgGeocodeCandidate> GenerateCandidates(gtgStandardizeResult AddressIn)//, ILog logFile)
        {
            const string LOCAL = _MOD + "GenerateCandidates(gtgStandardizeResult, ILog) ";
            var ret = new List<gtgGeocodeCandidate>();
			try
			{
                //UTIL.Utilities.LogMessage(logFile, UTIL.Utilities.LogType.Info,
                //                          String.Format("Entering {0}: AddressIn.OriginalAddress: {1}", LOCAL, AddressIn.OriginalAddress));

                foreach ( gtgGeocodeDataset ds in GeocoderDatasets )
				{
					if ( ds.RecordFamilies.ContainsKey( AddressIn.StreetNameSoundex ) )
					{
						foreach ( gtgGeocodeRecord r in ds.RecordFamilies[AddressIn.StreetNameSoundex].Addresses )
						{
							var c = new gtgGeocodeCandidate();
							if ( r.AddressStandardized.HouseNumber != AddressIn.HouseNumber )
								c.Score -= 15;

							if ( r.AddressStandardized.PreDir != AddressIn.PreDir )
								c.Score -= 5;

                            if (r.AddressStandardized.StreetName != AddressIn.StreetName)
                                c.Score -= Compute(r.AddressStandardized.StreetName, AddressIn.StreetName);//, logFile );

                            if ( r.AddressStandardized.StreetType != AddressIn.StreetType )
								c.Score -= 5;

                            if ( r.AddressStandardized.SufDir != AddressIn.SufDir )
								c.Score -= 5;

                            if ( r.AddressStandardized.Unit != AddressIn.Unit )
								c.Score -= 5;

                            if ( r.AddressStandardized.Zone != AddressIn.Zone && !string.IsNullOrWhiteSpace( AddressIn.Zone ) )
								c.Score -= 10;

                            if ( c.Score >= ds.MinMatchScore )
							{
								c.Candidate = r;
								ret.Add( c );
							}
						}
					}
				}
			}
			catch ( Exception ex )
			{
                //UTIL.Utilities.LogMessage(logFile, UTIL.Utilities.LogType.Error,
                //                          String.Format(Constants.FMT_ERROR_IN, LOCAL), ex);
            }
			return ret;
		}

        #endregion

        #region public classes within Geocoder object

        [ Serializable ]
		public class gtgStandardizerRule
		{
            #region public

            #region public constructors
            public gtgStandardizerRule()
            {
                SearchPattern = string.Empty;
                ReplaceWith = string.Empty;
            }

			public gtgStandardizerRule( string strSearchPattern, string strReplaceWith )
			{
				SearchPattern = strSearchPattern;
				ReplaceWith = strReplaceWith;
			}
            #endregion

            #region public properties
            public string SearchPattern { get; set; }
			public string ReplaceWith { get; set; }
            #endregion

            #endregion
        }

        [ Serializable ]
		public class gtgStandardizerInfo
		{
            #region public
            #region public constructor
            public gtgStandardizerInfo()
            {
                Abbreviation = new List<gtgStandardizerRule>();
                Abbreviation_Multi_Word = new List<gtgStandardizerRule>();
                Cardinal_Number = new List<gtgStandardizerRule>();
                Direction = new List<gtgStandardizerRule>();
                Multi_Unit = new List<gtgStandardizerRule>();
                Number_Suffix = new List<gtgStandardizerRule>();
                Ordinal_Number = new List<gtgStandardizerRule>();
                Route_Modifer = new List<gtgStandardizerRule>();
                Street_Type = new List<gtgStandardizerRule>();
            }
            #endregion

            #region public properties
            public List<gtgStandardizerRule> Abbreviation { get; set; }
            public List<gtgStandardizerRule> Abbreviation_Multi_Word { get; set; }
            public List<gtgStandardizerRule> Cardinal_Number { get; set; }
            public List<gtgStandardizerRule> Direction { get; set; }
            public List<gtgStandardizerRule> Multi_Unit { get; set; }
            public List<gtgStandardizerRule> Number_Suffix { get; set; }
            public List<gtgStandardizerRule> Ordinal_Number { get; set; }
            public List<gtgStandardizerRule> Route_Modifer { get; set; }
            public List<gtgStandardizerRule> Street_Type { get; set; }

            public string Name { get; set; }
            #endregion
            #endregion
        }

        [ Serializable ]
		public class gtgStandardizeResult
		{
            #region public
            #region public constructor
            public gtgStandardizeResult()
            {
                OriginalAddress = string.Empty;
                PreDir = string.Empty;
                StreetName = string.Empty;
                StreetNameSoundex = string.Empty;
                StreetType = string.Empty;
                SufDir = string.Empty;
                Unit = string.Empty;
                Zone = string.Empty;
            }
            #endregion

            #region public properties
            public float HouseNumber { get; set; }
            public string OriginalAddress { get; set; }
            public string PreDir { get; set; }
            public string StreetName { get; set; }
            public string StreetNameSoundex { get; set; }
            public string StreetType { get; set; }
            public string SufDir { get; set; }
            public string Unit { get; set; }
            public string Zone { get; set; }
            #endregion
            #endregion
        }

        [ Serializable ]
		public class gtgGeocodeRecord
		{
            #region public
            #region public properties
            public gtgStandardizeResult AddressStandardized { get; set; }
            public object Location { get; set; }
            #endregion
            #endregion
        }

        [ Serializable ]
		public class gtgGeocodeRecordFamily
		{
            #region public
            #region public constructor
            public gtgGeocodeRecordFamily()
            {
                Soundex = string.Empty;
                Addresses = new List<gtgGeocodeRecord>();
            }
            #endregion

            #region public properties
            public string Soundex { get; set; }
            public List<gtgGeocodeRecord> Addresses { get; set; }
            #endregion
            #endregion
        }

        [ Serializable ]
		public class gtgGeocodeDataset
		{
            #region public
            #region public constructor
            public gtgGeocodeDataset()
            {
                Name = string.Empty;
                RecordFamilies = new Dictionary<string, gtgGeocodeRecordFamily>();
                MinMatchScore = Constants.MIN_MATCH_SCORE;
            }
            #endregion

            #region public properties
			public Dictionary<string, gtgGeocodeRecordFamily> RecordFamilies { get; set; }

            public int MinMatchScore { get; set; }
            public string Name { get; set; }
            #endregion
            #endregion
        }

        [ Serializable ]
		public class gtgGeocodeCandidate
		{
            #region public
            #region public construtor
            public gtgGeocodeCandidate()
            {
                Candidate = null;
                Score = Constants.SCORE;
            }
            #endregion

            #region public properties
            public gtgGeocodeRecord Candidate { get; set; }
            public int Score { get; set; }
            #endregion
            #endregion
        }
        #endregion  
        #endregion
    }
}
