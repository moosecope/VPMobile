using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using GTG.Utilities;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml.Serialization;
using VP_Mobile.Models;
using VP_Mobile.StaticHelpers;
using VPMobileObjects;

namespace VP_Mobile.ViewModels
{
    [Serializable]
    public class StandardizeResult
    {
        public float HouseNumber;
        public string PreDir = string.Empty;
        public string StreetName = string.Empty;
        public string StreetType = string.Empty;
        public string SufDir = string.Empty;
        public string Unit = string.Empty;
        public string OriginalAddress = string.Empty;
        public string Zone = string.Empty;
        public string StreetNameSoundex = string.Empty;
    }

    [Serializable]
    public class GeocodeRecord
    {
        public StandardizeResult AddressStandardized;
        public object Location;
    }

    [Serializable]
    public class GeocodeDataset
    {
        public string Name = string.Empty;
        public Dictionary<string, List<GeocodeRecord>> RecordFamilies = new Dictionary<string, List<GeocodeRecord>>();
        public int MinMatchScore = 75;
    }

    [Serializable]
    public class GeocodeCandidate
    {
        public GeocodeRecord Candidate = null;
        public int Score = 100;

        public override string ToString()
        {
            if (Candidate == null || Candidate.AddressStandardized == null)
                return base.ToString();
            else
                return Score + " - " + Candidate.AddressStandardized.OriginalAddress;
        }
    }

    public class LocationToolViewModel : INotifyPropertyChanged
    {
        #region public
        #region public constructor
        public LocationToolViewModel()
        {
            try
            {
                Logging.LogMethodCall(ClassName);
                LoadPois();
                _latLonConverter = new LatLonDecimalDegreesConverter();
                Streets = new Dictionary<String, List<Geometry>>();
                Intersections = new ObservableCollection<Tuple<string, Geometry>>();
                Matches = new ObservableCollection<GeocodeCandidate>();
            }
            catch (Exception ex)
            {
                var message = "Error initializing Location Tools";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }
        #endregion

        #region public properties
        private MainViewModel _mainView;
        public MainViewModel MainView
        {
            get { return _mainView; }
            set
            {
                try
                {
                    Logging.LogMethodCall(ClassName);
                    if (_mainView != null)
                        _mainView.PropertyChanged -= MainView_PropertyChanged;
                    _mainView = value;
                    _mainView.PropertyChanged += MainView_PropertyChanged;
                    NotifyPropertyChanged();
                }
                catch (Exception ex)
                {
                    var message = "Error settign MainView";
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                    Logging.LogMessage(Logging.LogType.Error, message, ex);
                }
            }
        }


        #region Map Coordinates
        private LatLonDecimalDegreesConverter _latLonConverter;
        public double Lat
        {
            get { return _latLonConverter.DDLat; }
            set
            {
                _latLonConverter.DDLat = value;
                NotifyMapCoordinateChange();
            }
        }

        public double Long
        {
            get { return _latLonConverter.DDLong; }
            set
            {
                _latLonConverter.DDLong = value;
                NotifyMapCoordinateChange();
            }
        }

        public double LatDeg
        {
            get { return _latLonConverter.DMSLat_Deg; }
            set
            {
                _latLonConverter.DMSLat_Deg = value;
                NotifyMapCoordinateChange();
            }
        }

        public double LatMin
        {
            get { return _latLonConverter.DMSLat_Min; }
            set
            {
                _latLonConverter.DMSLat_Min = value;
                NotifyMapCoordinateChange();
            }
        }

        public double LatSec
        {
            get { return _latLonConverter.DMSLat_Sec; }
            set
            {
                _latLonConverter.DMSLat_Sec = value;
                NotifyMapCoordinateChange();
            }
        }

        public double LongDeg
        {
            get { return _latLonConverter.DMSLong_Deg; }
            set
            {
                _latLonConverter.DMSLong_Deg = value;
                NotifyMapCoordinateChange();
            }
        }

        public double LongMin
        {
            get { return _latLonConverter.DMSLong_Min; }
            set
            {
                _latLonConverter.DMSLong_Min = value;
                NotifyMapCoordinateChange();
            }
        }

        public double LongSec
        {
            get { return _latLonConverter.DMSLong_Sec; }
            set
            {
                _latLonConverter.DMSLong_Sec = value;
                NotifyMapCoordinateChange();
            }
        }
        #endregion

        #region Street Intersections

        private Dictionary<String, List<Geometry>> _streets;
        public Dictionary<String, List<Geometry>> Streets
        {
            get { return _streets; }
            set
            {
                try
                {
                    Logging.LogMethodCall(ClassName);
                    _streets = value;
                    _sortedStreets = CollectionViewSource.GetDefaultView(_streets);
                    _sortedStreets.SortDescriptions.Add(new SortDescription("Key", ListSortDirection.Ascending));
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(SortedStreets));
                }
                catch (Exception ex)
                {
                    var message = "Error setting Streets";
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                    Logging.LogMessage(Logging.LogType.Error, message, ex);
                }
            }
        }

        private ICollectionView _sortedStreets;
        public ICollectionView SortedStreets
        {
            get { return _sortedStreets; }
        }

        private KeyValuePair<String, List<Geometry>> _selectedStreet;
        public KeyValuePair<String, List<Geometry>> SelectedStreet
        {
            get { return _selectedStreet; }
            set
            {
                try
                {
                    Logging.LogMethodCall(ClassName);
                    _selectedStreet = value;
                    NotifyPropertyChanged();
                    UpdateIntersections();
                }
                catch (Exception ex)
                {
                    var message = "Error setting SelectedStreet";
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                    Logging.LogMessage(Logging.LogType.Error, message, ex);
                }
            }
        }

        private ObservableCollection<Tuple<String, Geometry>> _intersections;
        public ObservableCollection<Tuple<String, Geometry>> Intersections
        {
            get { return _intersections; }
            set
            {
                try
                {
                    Logging.LogMethodCall(ClassName);
                    _intersections = value;
                    _sortedIntersections = CollectionViewSource.GetDefaultView(_intersections);
                    _sortedIntersections.SortDescriptions.Add(new SortDescription("Item1", ListSortDirection.Ascending));
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(SortedIntersections));
                }
                catch (Exception ex)
                {
                    var message = "Error setting Intersections";
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                    Logging.LogMessage(Logging.LogType.Error, message, ex);
                }
            }
        }

        private ICollectionView _sortedIntersections;
        public ICollectionView SortedIntersections
        {
            get { return _sortedIntersections; }
        }

        private Tuple<String, Geometry> _selectedIntersection;
        public Tuple<String, Geometry> SelectedIntersection
        {
            get { return _selectedIntersection; }
            set
            {
                _selectedIntersection = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Points Of Interests
        private ObservableCollection<PointOfInterestSettings> _pois;
        public ObservableCollection<PointOfInterestSettings> Pois
        {
            get { return _pois; }
            set
            {
                _pois = value;
                NotifyPropertyChanged();
            }
        }

        private PointOfInterestSettings _selectedPoi;
        public PointOfInterestSettings SelectedPoi
        {
            get { return _selectedPoi; }
            set
            {
                _selectedPoi = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        #region Addresses
        private List<GeocodeDataset> GeocoderDatasets = new List<GeocodeDataset>();

        private bool _geocoderAvailable;
        public bool GeocoderAvailable
        {
            get { return _geocoderAvailable; }
            set
            {
                _geocoderAvailable = value;
                NotifyPropertyChanged();
            }
        }
        
        private String _address;
        public String Address
        {
            get { return _address; }
            set
            {
                _address = value;
                NotifyPropertyChanged();
            }
        }

        private String _zone;
        public String Zone
        {
            get { return _zone; }
            set
            {
                _zone = value;
                NotifyPropertyChanged();
            }
        }

        private GeocodeCandidate _selectedAddress;
        public GeocodeCandidate SelectedAddress
        {
            get { return _selectedAddress; }
            set
            {
                _selectedAddress = value;
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<GeocodeCandidate> _matches;
        public ObservableCollection<GeocodeCandidate> Matches
        {
            get { return _matches; }
            set
            {
                _matches = value;
                NotifyPropertyChanged();
            }
        }
        
        public FeatureTable FeatureSource { get; internal set; }
        public string StreetNameField { get; internal set; }
        public bool GeocodersLoaded { get; internal set; }
        #endregion
        #endregion

        #region public events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
        #region public methods
        #region Map Coordinates
        public void FindCoordinateFromMap()
        {
            try
            {
                Logging.LogMethodCall(ClassName);
                MainView.FindVisible = false;
                MainView.RequestPointFromUser(delegate(MapPoint pnt)
                {
                    MainView.FindVisible = true;
                    Lat = pnt.Y;
                    Long = pnt.X;
                });
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error finding coordinates from map", ex);
            }
        }

        public void ShowCoordinateOnMap()
        {
            try
            {
                Logging.LogMethodCall(ClassName);
                MainView.Show(new MapPoint(Long, Lat, SpatialReferences.Wgs84), false);
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error showing coordinates on map", ex);
            }
        }

        public void RouteToCoordinate()
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                if ((Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) &&
                    (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) &&
                    (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)))
                {
                    MainView._lastGPSLocation = new MapPoint(Long, Lat, SpatialReferences.Wgs84);
                    return;
                }
                Logging.LogMethodCall(ClassName);
                MainView.RouteTo(new Models.Point
                {
                    Latitude = Lat,
                    Longitude = Long
                });
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error routing to coordinates", ex);
            }
        }
        #endregion

        #region Street Intersections

        public void ShowStreetOnMap()
        {
            try
            {
                Logging.LogMethodCall(ClassName);
                MainView.Show(GeometryEngine.Union(SelectedStreet.Value), false);
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error showing coordinates on map", ex);
            }
        }

        public void ShowIntersectionOnMap()
        {
            try
            {
                Logging.LogMethodCall(ClassName);
                MainView.Show(SelectedIntersection.Item2, false);
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error showing coordinates on map", ex);
            }
        }

        public void RouteToIntersection()
        {
            try
            {
                Logging.LogMethodCall(ClassName);
                var pnt = (MapPoint)GeometryEngine.Project(SelectedIntersection.Item2, SpatialReferences.Wgs84);
                MainView.RouteTo(new Models.Point
                {
                    Latitude = pnt.Y,
                    Longitude = pnt.X
                });
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error routing to coordinates", ex);
            }
        }
        #endregion

        #region Points Of Interests
        public void AddPoi()
        {
            try
            {
                Logging.LogMethodCall(ClassName);
                MessageBox.Show("Click on the map to specify the location for your new POI.");
                MainView.FindVisible = false;
                MainView.RequestPointFromUser(new Action<MapPoint>(pnt =>
                {
                    MainView.FindVisible = true;
                    string name = Interaction.InputBox("Enter a name for the new POI.", "Add POI", null);
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        while (!string.IsNullOrWhiteSpace(name) && Pois.Any(poi => poi.Name == name))
                        {
                            MessageBox.Show("The name entered is already being used by another POI.  Please specify another one.");
                            name = Interaction.InputBox("Enter a name for the new POI.", "Add POI", null);
                        }
                        if (!string.IsNullOrWhiteSpace(name))
                        {
                            Pois.Add(new PointOfInterestSettings
                            {
                                Name = name,
                                Lat = pnt.Y,
                                Long = pnt.X
                            });
                            UpdatePois();
                        }
                    }
                }));
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error adding points of interest", ex);
            }
        }

        public void RemovePoi()
        {
            try
            {
                Logging.LogMethodCall(ClassName);
                if (SelectedPoi != null)
                    Pois.Remove(SelectedPoi);
                UpdatePois();
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error removing points of interest", ex);
            }
        }

        public void ShowPoi()
        {
            try
            {
                Logging.LogMethodCall(ClassName);
                if (SelectedPoi != null)
                    MainView.Show(new MapPoint(SelectedPoi.Long, SelectedPoi.Lat, SpatialReferences.Wgs84), false);
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error removing points of interest", ex);
            }
        }

        public void RouteToPoi()
        {
            try
            {
                Logging.LogMethodCall(ClassName);
                MainView.RouteTo(new Models.Point
                {
                    Latitude = SelectedPoi.Lat,
                    Longitude = SelectedPoi.Long
                });
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error removing points of interest", ex);
            }
        }
        #endregion

        #region Addresses
        public void GenerateCandidates()
        {
            try
            {
                Logging.LogMethodCall(ClassName);
                Matches.Clear();
                if (string.IsNullOrWhiteSpace(Address))
                    MessageBox.Show("You must enter an address to search for.  Please try again.");
                else
                {
                    StandardizeResult sr = StandardizeAddress(Address);
                    if (!string.IsNullOrWhiteSpace(Zone))
                        sr.Zone = Zone.Trim().ToUpper();
                    Matches = new ObservableCollection<GeocodeCandidate>(GenerateCandidates(sr));
                    if (Matches.Count <= 0)
                        MessageBox.Show("No results found.  Please try again.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        public void ShowAddress()
        {
            try
            {
                Logging.LogMethodCall(ClassName);
                if (SelectedAddress == null)
                    return;
                MainView.Show((Geometry)SelectedAddress.Candidate.Location, false);
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error showing address", ex);
            }
        }

        public void RouteToAddress()
        {
            try
            {
                Logging.LogMethodCall(ClassName);
                if (SelectedAddress == null)
                    return;
                var pnt = (MapPoint)GeometryEngine.Project((Geometry)SelectedAddress.Candidate.Location, SpatialReferences.Wgs84);
                MainView.RouteTo(new Models.Point
                {
                    Latitude = pnt.Y,
                    Longitude = pnt.X
                });
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error routing to address", ex);
            }
        }

        public async void AddGeocoderDataSet(FeatureTable table, GeocoderSettings geocoder)
        {
            try
            {
                _cancel = false;
                Logging.LogMethodCall(ClassName, () => new Dictionary<String, object> { { nameof(table), table }, { nameof(geocoder), geocoder } });
                bool blHouseNumberField = false, blLeftFromField = false, blLeftToField = false, blRightFromField = false, blRightToField = false, blPreDirField = false, blStreetNameField = false, blStreetTypeField = false, blSufDirField = false, blAptNumberField = false, blZoneField = false;
                int flds = 0;
                if ((blHouseNumberField = !string.IsNullOrWhiteSpace(geocoder.HouseNumberField)) && table.Fields.Any(fld => fld.Name == geocoder.HouseNumberField)) flds++;
                if ((blLeftFromField = !string.IsNullOrWhiteSpace(geocoder.LeftFromField)) && table.Fields.Any(fld => fld.Name == geocoder.LeftFromField)) flds++;
                if ((blLeftToField = !string.IsNullOrWhiteSpace(geocoder.LeftToField)) && table.Fields.Any(fld => fld.Name == geocoder.LeftToField)) flds++;
                if ((blRightFromField = !string.IsNullOrWhiteSpace(geocoder.RightFromField)) && table.Fields.Any(fld => fld.Name == geocoder.RightFromField)) flds++;
                if ((blRightToField = !string.IsNullOrWhiteSpace(geocoder.RightToField)) && table.Fields.Any(fld => fld.Name == geocoder.RightToField)) flds++;
                if ((blPreDirField = !string.IsNullOrWhiteSpace(geocoder.PreDirField)) && table.Fields.Any(fld => fld.Name == geocoder.PreDirField)) flds++;
                if ((blStreetNameField = !string.IsNullOrWhiteSpace(geocoder.StreetNameField)) && table.Fields.Any(fld => fld.Name == geocoder.StreetNameField)) flds++;
                if ((blStreetTypeField = !string.IsNullOrWhiteSpace(geocoder.StreetTypeField)) && table.Fields.Any(fld => fld.Name == geocoder.StreetTypeField)) flds++;
                if ((blSufDirField = !string.IsNullOrWhiteSpace(geocoder.SufDirField)) && table.Fields.Any(fld => fld.Name == geocoder.SufDirField)) flds++;
                if ((blAptNumberField = !string.IsNullOrWhiteSpace(geocoder.AptNumberField)) && table.Fields.Any(fld => fld.Name == geocoder.AptNumberField)) flds++;
                if ((blZoneField = !string.IsNullOrWhiteSpace(geocoder.ZoneField)) && table.Fields.Any(fld => fld.Name == geocoder.ZoneField)) flds++;
                if (flds == 0)
                    ErrorHelper.OnMessage("No address records available for (" + geocoder.LayerName + ").");
                else
                {
                    var qf = new QueryParameters();
                    qf.WhereClause = "1=1";
                    var result = await table.QueryFeaturesAsync(qf);
                    var ds = new GeocodeDataset
                    {
                        Name = geocoder.LayerName,
                        MinMatchScore = geocoder.MinMatchScore
                    };
                    foreach (Feature row in result)
                    {
                        String address = String.Empty;

                        switch (geocoder.Type)
                        {
                            case GeocoderTypes.SingleField:
                                address = getDictionaryValue(row.Attributes, geocoder.StreetNameField, blStreetNameField);
                                break;
                            case GeocoderTypes.SingleHouse:
                                address = String.Format("{0} {1} {2} {3} {4} {5}",
                                    getDictionaryValue(row.Attributes, geocoder.HouseNumberField, blHouseNumberField),
                                    getDictionaryValue(row.Attributes, geocoder.AptNumberField, blAptNumberField),
                                    getDictionaryValue(row.Attributes, geocoder.PreDirField, blPreDirField),
                                    getDictionaryValue(row.Attributes, geocoder.StreetNameField, blStreetNameField),
                                    getDictionaryValue(row.Attributes, geocoder.StreetTypeField, blStreetTypeField),
                                    getDictionaryValue(row.Attributes, geocoder.SufDirField, blSufDirField));
                                    break;
                            //case GeocoderTypes.SingleRange:
                            //    continue;
                            //case GeocoderTypes.DualRange:
                            //    continue;
                        }
                        if (string.IsNullOrWhiteSpace(address))
                            continue;
                        GeocodeRecord gr = new GeocodeRecord
                        {
                            AddressStandardized = StandardizeAddress(address),
                            Location = row.Geometry
                        };
                        gr.AddressStandardized.Zone = getDictionaryValue(row.Attributes, geocoder.ZoneField, blZoneField);

                        string soundex = gr.AddressStandardized.StreetNameSoundex;
                        if (!ds.RecordFamilies.ContainsKey(soundex))
                        {
                            ds.RecordFamilies.Add(soundex, new List<GeocodeRecord>());
                        }
                        ds.RecordFamilies[soundex].Add(gr);
                        if (_cancel)
                            return;
                    }
                    if (ds.RecordFamilies.Count > 0)
                    {
                        GeocoderDatasets.Add(ds);
                        GeocoderAvailable = true;
                    }
                    else
                        ErrorHelper.OnMessage("No address fields found for  (" + geocoder.LayerName + ").");
                }
            }
            catch (Exception ex)
            {
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error setting up geocoder", ex);
            }
        }

        private String getDictionaryValue(IDictionary<String, object> dict, String field, bool pullField)
        {
            object tmpObject;
            if (!pullField || !dict.TryGetValue(field, out tmpObject) || Convert.IsDBNull(tmpObject) || tmpObject == null) return String.Empty;
            return tmpObject.ToString();

        }

        public void CancelLocationToolsBuild()
        {
            _cancel = true;
        }

        public void ClearGeocoderDataSets()
        {
            GeocoderDatasets.Clear();
            GeocoderAvailable = false;
        }
        #endregion

        public void ClearFindResults()
        {
            try
            {
                Logging.LogMethodCall(ClassName);
                MainView.ClearPointsFromMap(false);
            }
            catch (Exception ex)
            {
                var message = "Error clearing points from map";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }
        #endregion
        #endregion

        #region private
        private bool _cancel;
        private static String ClassName = "LocationToolViewModel";

        //  This method is called by the Set accessor of each property.
        //  The CallerMemberName attribute that is applied to the optional propertyName
        //  parameter causes the property name of the caller to be substituted as an argument.
        //  Note: Requires Framework 4.5 or higher
        private void NotifyPropertyChanged([CallerMemberName]  String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void NotifyMapCoordinateChange()
        {
            NotifyPropertyChanged("Lat");
            NotifyPropertyChanged("Long");
            NotifyPropertyChanged("LatDeg");
            NotifyPropertyChanged("LatMin");
            NotifyPropertyChanged("LatSec");
            NotifyPropertyChanged("LongDeg");
            NotifyPropertyChanged("LongMin");
            NotifyPropertyChanged("LongSec");
        }

        private void MainView_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyPropertyChanged("MainView");
        }

        private void UpdatePois()
        {
            try
            {
                Logging.LogMethodCall(ClassName);
                var path = Path.Combine(ConfigHandler.AssemblyDirectory, "POIS.config");
                var toSer = Pois.ToArray<PointOfInterestSettings>();
                
                using (var fs = new StreamWriter(path, false))
                {
                    var ser = new XmlSerializer(toSer.GetType());
                    ser.Serialize(fs, toSer);
                }
            }
            catch (Exception ex)
            {
                var message = "Error saving points of interests";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        private void LoadPois()
        {
            try
            {
                Logging.LogMethodCall(ClassName);
                var path = Path.Combine(ConfigHandler.AssemblyDirectory, "POIS.config");

                if (!File.Exists(path))
                {
                    Pois = new ObservableCollection<PointOfInterestSettings>();
                }
                else
                {
                    PointOfInterestSettings[] arrayOfPois;
                    using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        var ser = new XmlSerializer(typeof(PointOfInterestSettings[]));
                        arrayOfPois = (PointOfInterestSettings[])ser.Deserialize(fs);
                    }
                    Pois = new ObservableCollection<PointOfInterestSettings>(arrayOfPois);
                }
            }
            catch (Exception ex)
            {
                var message = "Error loading points of interests";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        private async void UpdateIntersections()
        {
            try
            {
                Logging.LogMethodCall(ClassName);
                var qf = new QueryParameters();
                var gemStreet = GeometryEngine.Union(_selectedStreet.Value);
                qf.Geometry = gemStreet;
                qf.SpatialRelationship = SpatialRelationship.Touches;
                var results = await FeatureSource.QueryFeaturesAsync(qf);
                Intersections.Clear();
                foreach (var result in results)
                {
                    var streetName = result.Attributes[StreetNameField].ToString();
                    if (SelectedStreet.Key == streetName)
                        continue;
                    var loc = GeometryEngine.Intersections(result.Geometry, gemStreet);
                    if (loc[0] is MapPoint)
                        Intersections.Add(new Tuple<string, Geometry>(streetName, loc[0]));
                    else if (loc[0] is Multipoint)
                        Intersections.Add(new Tuple<String, Geometry>(streetName, ((Multipoint)loc[0]).Points[0]));
                    else if (loc[0] is Multipart)
                    {
                        var geom = ((Multipart)loc[0]).Parts[0].Points[0];
                        Intersections.Add(new Tuple<string, Geometry>(streetName, geom));
                    }
                }
            }
            catch (Exception ex)
            {
                var message = "Error updating intersections";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        private StandardizeResult StandardizeAddress(string address)
        {
            Logging.LogMethodCall(ClassName, () => new Dictionary<String, Object> { { nameof(address), address } });
            var ret = new StandardizeResult();
            string tmpString = null;
            try
            {
                ret.OriginalAddress = address;
                var regx = new Regex(" +");
                ret.OriginalAddress = regx.Replace(address, " ").Trim();

                string[] strSplit = ret.OriginalAddress.ToUpper().Split(' ');
                int startIndex = 0;
                float tmpFloat;
                if (float.TryParse(strSplit[0], out tmpFloat))
                {
                    ret.HouseNumber = tmpFloat;
                    tmpString = ((strSplit.Length > 1) ? strSplit[1] : string.Empty);
                    switch (tmpString)
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
                for (int i = startIndex; i <= strSplit.Length - 1; i++)
                {
                    try
                    {
                        if (strSplit[i].Contains(','))
                        {
                            string[] strZoneSplit = strSplit[i].Split(',');
                            if (strZoneSplit.Length > 1)
                                ret.Zone = strZoneSplit[1].Trim();
                        }
                        else
                            ret.StreetName += " " + strSplit[i];
                    }
                    catch (IndexOutOfRangeException)
                    {
                        break;
                    }
                }
                ret.StreetName = ret.StreetName.Trim();
                ret.StreetNameSoundex = GenerateSoundexCode(ret.StreetName);
            }
            catch (Exception ex)
            {
                Logging.LogMessage(Logging.LogType.Error, "error on standardize address - " + address, ex);
            }
            return ret;
        }

        private string GenerateSoundexCode(string StreetNameToCode)
        {
            Logging.LogMethodCall(ClassName, () => new Dictionary<String, Object> { { nameof(StreetNameToCode), StreetNameToCode } });
            if (string.IsNullOrWhiteSpace(StreetNameToCode))
                return string.Empty;
            StreetNameToCode = StreetNameToCode.Trim();

            char currLetter = 'p', lastLetter;
            var ret = new StringBuilder(StreetNameToCode.Substring(0, 1).ToUpper(), StreetNameToCode.Length);
            for (int i = 1; i < StreetNameToCode.Length; i++)
            {
                lastLetter = currLetter;
                switch (char.ToUpperInvariant(StreetNameToCode[i]))
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
                if (lastLetter == '!' && currLetter == '!')
                    continue;
                ret.Append(currLetter);
            }

            if (ret.Length < 4)
                ret.Append(new string('0', 4 - ret.Length));
            return ret.ToString();
        }

        private List<GeocodeCandidate> GenerateCandidates(StandardizeResult AddressIn)
        {
            Logging.LogMethodCall(ClassName, () => new Dictionary<String, Object> { { nameof(AddressIn), AddressIn } });
            var ret = new List<GeocodeCandidate>();
            try
            {
                foreach (GeocodeDataset ds in GeocoderDatasets)
                {
                    if (ds.RecordFamilies.ContainsKey(AddressIn.StreetNameSoundex))
                    {
                        foreach (GeocodeRecord r in ds.RecordFamilies[AddressIn.StreetNameSoundex])
                        {
                            var c = new GeocodeCandidate();
                            if (r.AddressStandardized.HouseNumber != AddressIn.HouseNumber)
                                c.Score -= 15;
                            if (r.AddressStandardized.PreDir != AddressIn.PreDir)
                                c.Score -= 5;
                            if (r.AddressStandardized.StreetName != AddressIn.StreetName)
                                c.Score -= ComputeMinDifference(r.AddressStandardized.StreetName, AddressIn.StreetName);
                            if (r.AddressStandardized.StreetType != AddressIn.StreetType)
                                c.Score -= 5;
                            if (r.AddressStandardized.SufDir != AddressIn.SufDir)
                                c.Score -= 5;
                            if (r.AddressStandardized.Unit != AddressIn.Unit)
                                c.Score -= 5;
                            if (r.AddressStandardized.Zone != AddressIn.Zone && !string.IsNullOrWhiteSpace(AddressIn.Zone))
                                c.Score -= 10;
                            if (c.Score >= ds.MinMatchScore)
                            {
                                c.Candidate = r;
                                int index = 0;
                                for (index = 0; index < ret.Count - 1; index++)
                                {
                                    if (ret[index].Score < c.Score)
                                        break;
                                }
                                if (index >= ret.Count)
                                    ret.Add(c);
                                else
                                    ret.Insert(index, c);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.LogMessage(Logging.LogType.Error, "error generating candidates for - " + AddressIn.OriginalAddress, ex);
            }
            return ret;
        }

        private static int ComputeMinDifference(string itm1st, string itm2nd)
        {
            Logging.LogMethodCall(ClassName, () => new Dictionary<String, Object> { { nameof(itm1st), itm1st }, { nameof(itm2nd), itm2nd } });
            int result;
            if (string.IsNullOrEmpty(itm1st))
                result = (string.IsNullOrEmpty(itm2nd) ? 0 : itm2nd.Length);
            else
            {
                if (string.IsNullOrEmpty(itm2nd))
                    result = itm1st.Length;
                else
                {
                    int lngth1st = itm1st.Length;
                    int lngth2nd = itm2nd.Length;
                    var aryShortestRoute = new int[lngth1st + 1, lngth2nd + 1];
                    int idx1st = 0;
                    while (idx1st <= lngth1st)
                        aryShortestRoute[idx1st, 0] = idx1st++;
                    int idx2nd = 1;
                    while (idx2nd <= lngth2nd)
                        aryShortestRoute[0, idx2nd] = idx2nd++;
                    for (idx1st = 1; idx1st <= lngth1st; idx1st++)
                    {
                        for (idx2nd = 1; idx2nd <= lngth2nd; idx2nd++)
                        {
                            int prvMatch = (itm2nd[idx2nd - 1] == itm1st[idx1st - 1]) ? 0 : 1;
                            int prv1st = aryShortestRoute[idx1st - 1, idx2nd] + 1;
                            int prv2nd = aryShortestRoute[idx1st, idx2nd - 1] + 1;
                            int prvBoth = aryShortestRoute[idx1st - 1, idx2nd - 1] + prvMatch;
                            aryShortestRoute[idx1st, idx2nd] = Math.Min(Math.Min(prv1st, prv2nd), prvBoth);
                        }
                    }
                    result = aryShortestRoute[lngth1st, lngth2nd];
                }
            }
            return result;
        }
        #endregion
    }
}
