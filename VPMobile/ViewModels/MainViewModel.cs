using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using GTG.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Serialization;
using VP_Mobile.Models;
using VP_Mobile.StaticHelpers;
using VP_Mobile.VPMobileService;
using VP_Mobile.VPService;
using VPMobileObjects;
using VPMobileRuntime100_1_0.Model;
using GTG.Utilities.Routing;
using System.Net.Sockets;
using Esri.ArcGISRuntime;

namespace VP_Mobile.ViewModels
{
    /// <summary>
    /// Provides map data to an application
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        #region public
        #region public constructor
        public MainViewModel()
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                _map = new Map();
                _visibleBlades = new Queue<int>();
                _routingInstructions = new ObservableCollection<RoutingViewModel>();
                LoadedCaches = new Dictionary<string, Geodatabase>();
                CacheLayers = new Dictionary<string, List<FeatureLayer>>();
                Cache.CacheUpdating += CacheUpdating;
                Cache.GeodatabaseLoaded += GeodatabaseLoaded;
                Cache.TileCacheLoaded += TileCacheLoaded;
                _progressOpacity = 0.0;

                _progressTimer = new System.Timers.Timer(500);
                _progressTimer.Elapsed += ProgressTimer_Elapsed;
                _progressTimer.Start();

             
            }
            catch (Exception ex)
            {
                var message = "Error initializing MainViewModel";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }
        #endregion

        #region public properties
        private static bool? _updateSplashScreen;
        public static bool UpdateSplashScreen
        {
            get
            {
                if (_updateSplashScreen.HasValue)
                    return _updateSplashScreen.Value;
                return false;
            }
            set
            {
                _updateSplashScreen = value;
            }
        }

        private Map _map;
        public Map Map
        {
            get { return _map; }
            set
            {
                _map = value;
                NotifyPropertyChanged();
                Layers = _map.AllLayers;
            }
        }

        private MapView _mapView;
        public MapView MapView
        {
            get { return _mapView; }
            set
            {
                try
                {
                    Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                    if (_mapView != null)
                    {
                        _mapView.GeoViewTapped -= MapView_Tapped;
                        _mapView.ViewpointChanged -= MapView_ViewpointChanged;
                    }
                    _mapView = value;
                    if (_mapView != null)
                    {
                        _mapView.GeoViewTapped += MapView_Tapped;
                        _mapView.ViewpointChanged += MapView_ViewpointChanged;
                    }
                    NotifyPropertyChanged();
                }
                catch (Exception ex)
                {
                    var message = "Error setting MapView";
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                    Logging.LogMessage(Logging.LogType.Error, message, ex);
                }
            }
        }

        private IEnumerable<Layer> _layers;
        public IEnumerable<Layer> Layers
        {
            get { return _layers; }
            set
            {
                _layers = value;
                NotifyPropertyChanged();
                Legend.Layers = _layers;
            }
        }

        private double _progress;
        public double Progress
        {
            get { return _progress; }
            set
            {
                _progress = value;
                NotifyPropertyChanged();
            }
        }

        private double _progressTotal;
        public double ProgressTotal
        {
            get { return _progressTotal; }
            set
            {
                _progressTotal = value;
                NotifyPropertyChanged();
            }
        }

        private String _progressMessage;
        public String ProgressMessage
        {
            get { return _progressMessage; }
            set
            {
                _progressMessage = value;
                NotifyPropertyChanged();
            }
        }

        private double _progressOpacity;
        public double ProgressOpacity
        {
            get { return _progressOpacity; }
            set
            {
                _progressOpacity = value;
                NotifyPropertyChanged();
            }
        }

        private LegendViewModel _legend;
        public LegendViewModel Legend
        {
            get { return _legend; }
            set
            {
                _legend = value;
                NotifyPropertyChanged();
            }
        }

        private bool _findVisible;
        public bool FindVisible
        {
            get { return _findVisible; }
            set
            {
                _findVisible = value;
                if (_findVisible && SettingsVisible)
                    SettingsVisible = false;
                NotifyPropertyChanged();
                NotifyPropertyChanged("FindToolImage");
            }
        }

        private bool _identifying;
        public String FindToolImage
        {
            get
            {
                if (FindVisible)
                    return "/VP Mobile;component/Resources/VP Icon-06.png";
                else if (_identifying)
                    return "/VP Mobile;component/Resources/VP Icon-07.png";
                else
                    return "/VP Mobile;component/Resources/VP Icon-05.png";
            }
        }

        private bool _findButtonsVisible;
        public bool FindButtonsVisible
        {
            get { return _findButtonsVisible; }
            set
            {
                _findButtonsVisible = value;
                if(_findButtonsVisible)
                {
                    _identifying = false;
                    _mapClicked = null;
                    NotifyPropertyChanged("FindToolImage");
                }
                NotifyPropertyChanged();
            }
        }

        private bool _settingsVisible;
        public bool SettingsVisible
        {
            get { return _settingsVisible; }
            set
            {
                _settingsVisible = value;
                if (_settingsVisible && FindVisible)
                    FindVisible = false;
                NotifyPropertyChanged();
            }
        }

        private bool _settingsButtonsVisible;
        public bool SettingsButtonsVisible
        {
            get { return _settingsButtonsVisible; }
            set
            {
                _settingsButtonsVisible = value;
                NotifyPropertyChanged();
            }
        }

        public bool HasBaseMap
        {
            get
            {
                return Config != null && Config.MapServices.Any(srvc => srvc.IsBaseMap);
            }
        }

        private bool _networkDisconnect;
        public bool NetworkDisconnect
        {
            get { return _networkDisconnect; }
            set
            {
                _networkDisconnect = value;
                NotifyPropertyChanged();
            }
        }

        private Queue<int> _visibleBlades;

        private const int INCIDENT_BLADE = 0;
        public bool IncidentsVisible
        {
            get { return _visibleBlades.Contains(INCIDENT_BLADE); }
            set
            {
                try
                {
                    Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                    if (value)
                    {
                        _visibleBlades.Enqueue(INCIDENT_BLADE);
                        if (_visibleBlades.Count > 2)
                            _visibleBlades.Dequeue();
                        IdentifyVisible = false;
                    }
                    else
                    {
                        if (_visibleBlades.Contains(INCIDENT_BLADE))
                        {
                            var tmp = _visibleBlades.Dequeue();
                            if (tmp != INCIDENT_BLADE)
                            {
                                _visibleBlades.Dequeue();
                                _visibleBlades.Enqueue(tmp);
                            }
                        }
                    }
                    NotifyBladesChanged();
                }
                catch (Exception ex)
                {
                    var message = "Error setting IncidentsVisible";
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                    Logging.LogMessage(Logging.LogType.Error, message, ex);
                }
            }
        }

        private const int AVL_BLADE = 1;
        public bool AvlVisible
        {
            get { return _visibleBlades.Contains(AVL_BLADE); }
            set
            {
                try
                {
                    Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                    if (value)
                    {
                        _visibleBlades.Enqueue(AVL_BLADE);
                        if (_visibleBlades.Count > 2)
                            _visibleBlades.Dequeue();
                        IdentifyVisible = false;
                    }
                    else
                    {
                        if (_visibleBlades.Contains(AVL_BLADE))
                        {
                            var tmp = _visibleBlades.Dequeue();
                            if (tmp != AVL_BLADE)
                            {
                                _visibleBlades.Dequeue();
                                _visibleBlades.Enqueue(tmp);
                            }
                        }
                    }
                    NotifyBladesChanged();
                }
                catch (Exception ex)
                {
                    var message = "Error setting AvlVisible";
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                    Logging.LogMessage(Logging.LogType.Error, message, ex);
                }
            }
        }

        private const int LEGEND_BLADE = 2;
        public bool LegendVisible
        {
            get { return _visibleBlades.Contains(LEGEND_BLADE); }
            set
            {
                try
                {
                    Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                    if (value)
                    {
                        _visibleBlades.Enqueue(LEGEND_BLADE);
                        if (_visibleBlades.Count > 2)
                            _visibleBlades.Dequeue();
                        IdentifyVisible = false;
                    }
                    else
                    {
                        if (_visibleBlades.Contains(LEGEND_BLADE))
                        {
                            var tmp = _visibleBlades.Dequeue();
                            if (tmp != LEGEND_BLADE)
                            {
                                _visibleBlades.Dequeue();
                                _visibleBlades.Enqueue(tmp);
                            }
                        }
                    }
                    NotifyBladesChanged();
                }
                catch (Exception ex)
                {
                    var message = "Error setting LegendVisible";
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                    Logging.LogMessage(Logging.LogType.Error, message, ex);
                }
            }
        }

        private const int ROUTING_BLADE = 3;
        public bool RoutingVisible
        {
            get { return _visibleBlades.Contains(ROUTING_BLADE); }
            set
            {
                try
                {
                    Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                    if (value)
                    {
                        _visibleBlades.Enqueue(ROUTING_BLADE);
                        if (_visibleBlades.Count > 2)
                            _visibleBlades.Dequeue();
                        IdentifyVisible = false;
                    }
                    else
                    {
                        if (_visibleBlades.Contains(ROUTING_BLADE))
                        {
                            var tmp = _visibleBlades.Dequeue();
                            if (tmp != ROUTING_BLADE)
                            {
                                _visibleBlades.Dequeue();
                                _visibleBlades.Enqueue(tmp);
                            }
                        }
                    }
                    NotifyBladesChanged();
                }
                catch (Exception ex)
                {
                    var message = "Error setting RoutingVisible";
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                    Logging.LogMessage(Logging.LogType.Error, message, ex);
                }
            }
        }

        public bool RoutingEnabled
        {
            get
            {
                return RoutingInstructions != null && RoutingInstructions.Count > 0;
            }
        }

        private bool _identifyVisible;
        public bool IdentifyVisible
        {
            get { return _visibleBlades.Count <= 0 && _identifyVisible; }
            set
            {
                _identifyVisible = value;
                if (_identifyVisible)
                {
                    _visibleBlades.Clear();
                }
                NotifyBladesChanged();
            }
        }

        private ObservableCollection<RoutingViewModel> _routingInstructions;

        public ObservableCollection<RoutingViewModel> RoutingInstructions
        {
            get { return _routingInstructions; }
            set
            {
                _routingInstructions = value;
                NotifyPropertyChanged();
            }
        }

        private GpsSettings _gpsConfig;
        public GpsSettings GpsConfig
        {
            get
            {
                try
                {
                    Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                    if (_gpsConfig != null)
                        return _gpsConfig;

                    var path = Path.Combine(ConfigHandler.AssemblyDirectory, "GPS.config");
                    if (!File.Exists(path))
                    {
                        _gpsConfig = new GpsSettings();
                    }
                    else
                    {
                        using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            var ser = new XmlSerializer(typeof(GpsSettings));
                            _gpsConfig = (GpsSettings)ser.Deserialize(fs);
                        }
                    }
                    NotifyPropertyChanged("GpsEnabled");
                    return _gpsConfig;
                }
                catch (Exception ex)
                {
                    var message = "Error pulling GpsConfig";
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                    Logging.LogMessage(Logging.LogType.Error, message, ex);
                    return _gpsConfig;
                }
            }
        }

        public bool GpsEnabled
        {
            get
            {
                return _gpsConfig != null && _gpsConfig.Type != GpsType.None;
            }
        }

        private int _loading;
        private VPMobileSettings _config;
        public VPMobileSettings Config
        {
            get { return _config; }
            set
            {
                try
                {
                    Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                    _config = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(HasBaseMap));
                    if (_config == null)
                        return;

                    _loading = -1;
                    _initialLoad = true;
                    Application.Current.Dispatcher.Invoke(LoadNextMapService);
                }
                catch (Exception ex)
                {
                    var message = "Error setting Config";
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                    Logging.LogMessage(Logging.LogType.Error, message, ex);
                }
            }
        }

        private ObservableCollection<Feature> _identifiedFeatures;
        public ObservableCollection<Feature> IdentifiedFeatures
        {
            get { return _identifiedFeatures; }
            set
            {
                _identifiedFeatures = value;
                NotifyPropertyChanged();
            }
        }


        private UserSettingsViewModel _userSettings;
        public UserSettingsViewModel UserSettings
        {
            get { return _userSettings; }
            set
            {
                if(_userSettings != null)
                    _userSettings.PropertyChanged -= UserSettings_PropertyChanged;
                _userSettings = value;
                if (_userSettings != null)
                    _userSettings.PropertyChanged += UserSettings_PropertyChanged;
                NotifyPropertyChanged();
            }
        }

        private VPMServiceClient _vpService;
        public VPMServiceClient VPService
        {
            get
            {
                try
                {
                    Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                    if (_vpService != null)
                        return _vpService;
                    _vpService = new VPMServiceClient();
                    _vpService.ClientCredentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;
                    _vpService.ChannelFactory.Credentials.Windows.ClientCredential = CredentialCache.DefaultNetworkCredentials;
                    _vpService.GetAvlSettingsCompleted += GetAvlSettingsCompleted;
                    _vpService.GetAVLLastReportRecordDataCompleted += GetAVLLastReportRecordDataCompleted;
                    _vpService.GetDispatchSettingsCompleted += GetDispatchSettingsCompleted;
                    _vpService.GetDispatchRecordDataCompleted += GetDispatchRecordDataCompleted;
                    return _vpService;
                }
                catch (Exception ex)
                {
                    var message = "Error pulling VPService";
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                    Logging.LogMessage(Logging.LogType.Error, message, ex);
                    return _vpService;
                }
            }
        }

        private VPMobileServiceClient _mobileService;
        public VPMobileServiceClient MobileService
        {
            get
            {
                try
                {
                    Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                    if (_mobileService != null)
                        return _mobileService;
                    _mobileService = new VPMobileServiceClient();
                    _mobileService.ClientCredentials.Windows.AllowedImpersonationLevel = TokenImpersonationLevel.Impersonation;
                    _mobileService.ChannelFactory.Credentials.Windows.ClientCredential = CredentialCache.DefaultNetworkCredentials;
                    _mobileService.GetSplashUpdateFileListCompleted += GetSplashUpdateFileListCompleted;
                    _mobileService.GetSplashUpdateFileCompleted += GetSplashUpdateFileCompleted;
                    return _mobileService;
                }
                catch (Exception ex)
                {
                    var message = "Error pulling MobileService";
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                    Logging.LogMessage(Logging.LogType.Error, message, ex);
                    return _mobileService;
                }
            }
        }

        public Dictionary<String, Geodatabase> LoadedCaches { get; set; }
        public Dictionary<String, List<FeatureLayer>> CacheLayers { get; set; }
        public AvlListViewModel AvlList { get; set; }
        public IncidentsListViewModel IncidentsList { get; set; }
        public LocationToolViewModel LocationTools { get; internal set; }
        public GraphicsOverlay GpsGraphicsLayer { get; set; }
        public GraphicsOverlay AvlGraphicsLayer { get; set; }
        public GraphicsOverlay DispatchGraphicsLayer { get; set; }
        public GraphicsOverlay RoutingGraphicsLayer { get; set; }
        public GraphicsOverlay LocationGraphicsLayer { get; set; }
        public bool GpsKeepCentered { get; set; }
        #endregion

        #region public events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion
        #region public methods
        public void StartSplashScreenUpdate()
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                if (UpdateSplashScreen)
                    MobileService.GetSplashUpdateFileListAsync(Guid.NewGuid().ToString());
            }
            catch (Exception ex)
            {
                Logging.LogMessage(Logging.LogType.Error, "Error updating splash screen", ex);
            }
        }

        public void RouteTo(Models.Point to)
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name, () => new Dictionary<String, Object> {
                        { nameof(to.Latitude), to.Latitude },
                        { nameof(to.Longitude), to.Longitude }
                    });
                if (Config.Routing == null || _routingCore == null)
                {
                    ErrorHelper.OnMessage("Routing is not set up in this configuration.");
                    return;
                }
                if(_routeID == null)
                {
                    ErrorHelper.OnMessage("Routing is being built.");
                    return;
                }
                if(_lastGPSLocation == null)
                {
                    ErrorHelper.OnMessage("GPS is not currently available.");
                    return;
                }
                if (to == null)
                {
                    ErrorHelper.OnMessage("Unable to route to your destination.");
                    return;
                }

                var from = GeometryEngine.Project(_lastGPSLocation, _routingSpatialReference) as MapPoint;
                var projectedTo = GeometryEngine.Project(new MapPoint(to.Longitude, to.Latitude, SpatialReferences.Wgs84), _routingSpatialReference) as MapPoint;
                
                string strRoute = _routingCore.Point2Point(
                    _routeID, from.X + "|" + from.Y, projectedTo.X + "|" + projectedTo.Y, Config.Routing.StreetNameField);
                if (string.IsNullOrWhiteSpace(strRoute))
                    MessageBox.Show("Unable to route to your destination.");
                string[] strSplit = strRoute.Split('¿');
                var lines = new List<Segment>();
                foreach (var s2 in
                    from points in strSplit
                    select points.Split('¶'))
                {
                    MapPoint prev = null;
                    foreach (var strPnt2 in s2.Select(point => point.Split('|')).Where(strPnt => strPnt.Count() == 2))
                    {
                        double x;
                        double y;
                        if (double.TryParse(strPnt2[0], out x) && double.TryParse(strPnt2[1], out y))
                        {
                            var cur = new MapPoint(x, y);
                            if(prev != null)
                                lines.Add(new Esri.ArcGISRuntime.Geometry.LineSegment(prev, cur));
                            prev = cur;
                        }
                    }
                }
                if (lines.Count > 0)
                {
                    var line = new Polyline(lines, _routingSpatialReference);

                    RoutingGraphicsLayer.Graphics.Clear();

                    System.Windows.Media.Color colorRed = System.Windows.Media.Color.FromArgb(Color.Red.A, Color.Red.R, Color.Red.G, Color.Red.B);
                    var symbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, colorRed, 5);
                    RoutingGraphicsLayer.Graphics.Add(new Esri.ArcGISRuntime.UI.Graphic(line, new Dictionary<String, object> { { "IsRouting", true } }, symbol));
                    MapView.SetViewpointGeometryAsync(line.Extent, 50);

                    RoutingInstructions.Clear();
                    foreach (var instruction in strSplit[0].Split('|'))
                    {
                        if (!String.IsNullOrWhiteSpace(instruction))
                            RoutingInstructions.Add(new RoutingViewModel(instruction));
                    }
                    NotifyPropertyChanged("RoutingEnabled");
                }
                else
                {
                    MessageBox.Show("There was an error building the route to your destination.");
                    Logging.LogMessage(Logging.LogType.Error, String.Format("Error building route with '{0}'", strRoute));
                }
                if (to != null)
                    MapView.SetViewpointCenterAsync(new MapPoint(to.Longitude, to.Latitude, SpatialReferences.Wgs84), 10 * UserSettings.IncidentZoomWidth);
            }
            catch (Exception ex)
            {
                var message = "Error building route";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        public void Show(Esri.ArcGISRuntime.Geometry.Geometry geom, bool routing)
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                if (geom == null || geom.IsEmpty)
                    return;

                System.Windows.Media.Color colorRed = System.Windows.Media.Color.FromArgb(Color.Red.A, Color.Red.R, Color.Red.G, Color.Red.B);
                Symbol symbol = null;
                if (geom is MapPoint)
                symbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Diamond, colorRed, Config.MapIconSize / 2);
                else if (geom is Polygon)
                    symbol = new SimpleFillSymbol(SimpleFillSymbolStyle.ForwardDiagonal, colorRed, new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, colorRed, 2));
                else
                    symbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, colorRed, 2);

                if (routing)
                {
                    RoutingGraphicsLayer.Graphics.Clear();
                    RoutingGraphicsLayer.Graphics.Add(new Esri.ArcGISRuntime.UI.Graphic(geom, symbol));
                }
                else
                {
                    LocationGraphicsLayer.Graphics.Clear();
                    LocationGraphicsLayer.Graphics.Add(new Esri.ArcGISRuntime.UI.Graphic(geom, symbol));
                }

                if (geom is MapPoint)
                    MapView.SetViewpointCenterAsync((MapPoint)geom, 10 * UserSettings.IncidentZoomWidth);
                else
                    MapView.SetViewpointGeometryAsync(geom);
            }
            catch (Exception ex)
            {
                var message = "Error showing geometry on map";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        public void ZoomTo(Models.Point point)
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name, () => new Dictionary<String, Object> {
                        { nameof(point.Latitude), point.Latitude },
                        { nameof(point.Longitude), point.Longitude }
                    });
                if (point != null)
                    MapView.SetViewpointCenterAsync(new MapPoint(point.Longitude, point.Latitude, SpatialReferences.Wgs84), 10 * UserSettings.IncidentZoomWidth);
            }
            catch (Exception ex)
            {
                var message = "Error zooming to point";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        public void ZoomToGps()
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                _lastGPSTime = DateTime.Now;
                if (_lastGPSLocation != null && !_lastGPSLocation.IsEmpty)
                    MapView.SetViewpointCenterAsync(_lastGPSLocation, 10 * UserSettings.IncidentZoomWidth);

                if (UserSettings.GpsKeepNorth)
                    MapView.SetViewpointRotationAsync(_lastGpsAngle);
            }
            catch (Exception ex)
            {
                var message = "Error zooming to gps location";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        public void ZoomToFullExtent()
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                if ((Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) &&
                    (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) &&
                    (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)))
                {
                    var xtnt = MapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry.Extent;
                    var sptlRef = MapView.SpatialReference.Wkid;
                    MessageBox.Show("<FullExtent XMax=\"" + xtnt.XMax + "\" XMin=\"" + xtnt.XMin + "\" YMax=\"" + xtnt.YMax + "\" YMin=\"" + xtnt.YMin + "\" WKID=\"" + sptlRef + "\" MinScale=\"" + Map.MinScale + "\" MaxScale=\"" + Map.MaxScale + "\" /> ");
                }
                if(Config.FullExtent != null)
                {
                    var xtnt = new Esri.ArcGISRuntime.Geometry.Envelope(Config.FullExtent.XMin, Config.FullExtent.YMin, Config.FullExtent.XMax, Config.FullExtent.YMax, new SpatialReference(Config.FullExtent.WKID));

                    MapView.SetViewpointGeometryAsync(xtnt);
                    return;
                }
                Esri.ArcGISRuntime.Geometry.Geometry extent = null, extent2 = null;
                foreach (var lyr in Map.Basemap.BaseLayers)
                {
                    if (extent == null)
                        extent = lyr.FullExtent;
                    else if(lyr.FullExtent != null)
                        extent = GeometryEngine.Union(extent, lyr.FullExtent);
                }

                foreach (var lyr in Map.OperationalLayers)
                {
                    if (extent == null)
                        extent = lyr.FullExtent;
                    else
                    {
                        if (extent.SpatialReference != lyr.SpatialReference && lyr.FullExtent != null)
                            extent2 = GeometryEngine.Project(lyr.FullExtent, extent.SpatialReference);
                        else
                            extent2 = lyr.FullExtent;
                        if(extent != null && extent2 != null)
                            extent = GeometryEngine.Union(extent, extent2);
                    }
                }
                if (extent != null && !extent.IsEmpty)
                {
                    Logging.LogMessage(Logging.LogType.Debug, "Zooming to XMax=\"" + extent.Extent.XMax + "\" XMin=\"" + extent.Extent.XMin + "\" YMax=\"" + extent.Extent.YMax + "\" YMin=\"" + extent.Extent.YMin + "\" WKID=\"" + extent.SpatialReference);
                    MapView.SetViewpointGeometryAsync(extent);
                }
            }
            catch (Exception ex)
            {
                var message = "Error zooming to point";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        public void ZoomOut()
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                var scale = MapView.MapScale;
                MapView.SetViewpointScaleAsync(scale * 2);
            }
            catch (Exception ex)
            {
                var message = "Error zooming out";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        public void ZoomIn()
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                var scale = MapView.MapScale;
                MapView.SetViewpointScaleAsync(scale / 2);
            }
            catch (Exception ex)
            {
                var message = "Error zooming in";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        public void Identify()
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                _identifying = true;
                NotifyPropertyChanged("FindToolImage");
                _mapClicked = (async (point) =>
                {
                    _identifying = false;
                    NotifyPropertyChanged("FindToolImage");
                // identify all layers in the MapView, passing the tap point, tolerance, types to return, and max results
                IReadOnlyList<IdentifyLayerResult> idLayerResults = await MapView.IdentifyLayersAsync(MapView.LocationToScreen(point), 20, false, 5);
                    var ret = new ObservableCollection<Feature>();

                // iterate the results for each layer
                foreach (IdentifyLayerResult idResults in idLayerResults)
                    {
                    // get the layer identified and cast it to FeatureLayer
                    FeatureLayer idLayer = idResults.LayerContent as FeatureLayer;
                        if (!Config.MapServices.Any(srvc => srvc.IdentifyingLayers.Contains(((GeodatabaseFeatureTable)idLayer.FeatureTable).LayerInfo.ServiceLayerName)))
                            continue;

                    // iterate each identified GeoElement in the results for this layer
                    foreach (GeoElement idElement in idResults.GeoElements)
                        {
                        // cast the result GeoElement to Feature
                        Feature idFeature = idElement as Feature;

                            ret.Add(idFeature);
                        }
                    }

                    if (ret.Count <= 0)
                    {
                        ErrorHelper.OnMessage("No features were found in the area");
                        return;
                    }

                    IdentifiedFeatures = ret;
                    IdentifyVisible = true;
                });
            }
            catch (Exception ex)
            {
                var message = "Error on Identify";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        public void ClearRoutingInstructions()
        {
            try
            {
                RoutingInstructions.Clear();
                RoutingGraphicsLayer.Graphics.Clear();
                RoutingVisible = false;
                NotifyPropertyChanged("RoutingEnabled");
            }
            catch (Exception ex)
            {
                var message = "Error while clearing routing instructions";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        public void UpdateAvlUnits()
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    foreach (var group in AvlList.AvlGroups)
                    {
                        foreach (var unit in group.AvlUnits)
                        {
                            var index = AvlGraphicsLayer.Graphics.IndexOf(unit.Graphic);
                            if (index >= 0 && !group.Visible)
                                AvlGraphicsLayer.Graphics.RemoveAt(index);
                            else if (index < 0 && group.Visible)
                                AvlGraphicsLayer.Graphics.Add(unit.Graphic);
                        }
                    }
                }), DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                var message = "Error updating avl units on map";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        public void UpdateIncidents()
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                Application.Current.Dispatcher.BeginInvoke((Action)(() =>
                {
                    foreach (var incident in IncidentsList.Incidents)
                    {
                        if (incident.UnGeocoded)
                            continue;
                        var index = DispatchGraphicsLayer.Graphics.IndexOf(incident.Graphic);
                        var callType = IncidentsList.CallTypes.FirstOrDefault(cltp => cltp.CallTypeValue == incident.CallType);
                        if (index < 0 && (callType == null || callType.CallTypeActive))
                            DispatchGraphicsLayer.Graphics.Add(incident.Graphic);
                        else if (index >= 0 && callType != null && !callType.CallTypeActive)
                            DispatchGraphicsLayer.Graphics.RemoveAt(index);
                    }
                }), DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                var message = "Error updating incidents on map";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        public void RequestPointFromUser(Action<MapPoint> action, int wkid = 4326)
        {
            _mapClicked = action;
            _mapClickedSpatialReference = wkid;
        }

        public void ClearPointsFromMap(bool routing)
        {
            if (routing)
                RoutingGraphicsLayer.Graphics.Clear();
            else
                LocationGraphicsLayer.Graphics.Clear();
        }

        public void ConfigSelected(VPMobileSettings obj)
        {
            Config = obj;
        }

        public void ToggleBaseMap()
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                foreach (var lyr in Map.Basemap.BaseLayers)
                {
                    lyr.IsVisible = !lyr.IsVisible;
                }
            }
            catch (Exception ex)
            {
                var message = "Error toggling base maps";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        public void ToggleNightMode()
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                System.Windows.Media.Color colorClear = System.Windows.Media.Color.FromArgb(Color.Transparent.A, Color.Transparent.R, Color.Transparent.G, Color.Transparent.B);
                System.Windows.Media.Color colorBlack = System.Windows.Media.Color.FromArgb(Color.Black.A, Color.Black.R, Color.Black.G, Color.Black.B);
                if (MapView.BackgroundGrid.Color != colorBlack)
                {
                    MapView.BackgroundGrid.Color = colorBlack;
                    MapView.BackgroundGrid.GridLineColor = colorClear;
                }
                else
                {
                    System.Windows.Media.Color colorWhite = System.Windows.Media.Color.FromArgb(Color.White.A, Color.White.R, Color.White.G, Color.White.B);
                    MapView.BackgroundGrid.Color = colorWhite;
                    MapView.BackgroundGrid.GridLineColor = colorClear;
                }
            }
            catch (Exception ex)
            {
                var message = "Error toggling night mode";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        public void ViewHelpDocument()
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                if (File.Exists(ConfigHandler.AssemblyDirectory + "\\VPMobile_Help.pdf"))
                    Process.Start(ConfigHandler.AssemblyDirectory + "\\VPMobile_Help.pdf");
                else
                    MessageBox.Show("VP Mobile Help Document is not available.");
            }
            catch (Win32Exception ex)
            {
                if (ex.NativeErrorCode == -2147221003)
                    MessageBox.Show("You need Adobe Reader to view the help documentation.");
            }
            catch (Exception ex)
            {
                var message = "Error showing help document";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }
        #endregion
        #endregion

        #region private
        private System.Timers.Timer _incidentTimer;
        private System.Timers.Timer _avlTimer;
        private System.Timers.Timer _progressTimer;
        private DateTime _incidentLast, _avlLast;
        private Action<MapPoint> _mapClicked;
        private int _mapClickedSpatialReference;
        private Core _routingCore;
        private String _routeID;
        public MapPoint _lastGPSLocation;
        private DateTime _lastGPSTime;
        private GPSListener _gpsListener;
        private UdpClient _avlForward;
        private double _lastGpsAngle;
        private HashSet<Exception> _gpsExceptions;
        private ArcGISTiledLayer _initialTileLayer;
        private List<FeatureLayer> _initialFeatureLayers;
        private Esri.ArcGISRuntime.Geometry.Geometry _initialExtent;
        private SpatialReference _routingSpatialReference;
        private bool _initialLoad;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void NotifyBladesChanged()
        {
            NotifyPropertyChanged(nameof(IncidentsVisible));
            NotifyPropertyChanged(nameof(AvlVisible));
            NotifyPropertyChanged(nameof(LegendVisible));
            NotifyPropertyChanged(nameof(RoutingVisible));
            NotifyPropertyChanged(nameof(IdentifyVisible));
        }

        private void UserSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name, () => new Dictionary<String, Object> {
                        { nameof(e.PropertyName), e.PropertyName }
                    });
                switch (e.PropertyName)
                {
                    case nameof(UserSettings.AvlLabel):
                        if (AvlGraphicsLayer != null)
                        {
                            AvlGraphicsLayer.LabelsEnabled = UserSettings.AvlLabel;
                        }
                        break;
                    case nameof(UserSettings.GpsLocLabel):
                        if (GpsGraphicsLayer != null)
                        {
                            GpsGraphicsLayer.LabelsEnabled = UserSettings.GpsLocLabel;
                        }
                        break;
                    case nameof(UserSettings.GpsKeepNorth):
                        if (UserSettings.GpsKeepNorth)
                            ZoomToGps();
                        else
                            MapView.SetViewpointRotationAsync(0);
                        break;
                    case nameof(UserSettings.IncidentLabel):
                        if (DispatchGraphicsLayer != null)
                        {
                            DispatchGraphicsLayer.LabelsEnabled = UserSettings.IncidentLabel;
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                var message = "Error on user settings changed";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        private void SetupRouting()
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                string shapeFilePath = null;
                if (Config.Routing != null && File.Exists(shapeFilePath = ConfigHandler.AssemblyDirectory + @"\RoutingData\" + Config.Routing.ShapeFilePath + ".shp"))
                {
                    new Thread(
                        () =>
                        {
                            try
                            {
                                var SHPinfo = new FileInfo(shapeFilePath);
                                string directory = SHPinfo.DirectoryName;
                                string fileName = Path.GetFileNameWithoutExtension(Config.Routing.ShapeFilePath);
                                var RTGInfo = new FileInfo(Path.Combine(directory, fileName + ".rtg"));
                                var RTXInfo = new FileInfo(Path.Combine(directory, fileName + ".rtx"));
                                _routingCore = new Core();
                                string s;
                                if (RTGInfo.Exists && RTXInfo.Exists
                                     && (Math.Abs(RTGInfo.LastWriteTime.Ticks - RTXInfo.LastWriteTime.Ticks) > 36000000000L
                                          || SHPinfo.LastWriteTime.Ticks > RTGInfo.LastWriteTime.Ticks))
                                {
                                    s = _routingCore.RouteSettings(
                                        shapeFilePath, "feet", Config.Routing.OneWayField ?? string.Empty,
                                        Config.Routing.OneWayFieldIndicator ?? string.Empty,
                                        Config.Routing.SpeedLimitField ?? string.Empty, true);
                                }
                                else
                                {
                                    s = _routingCore.RouteSettings(
                                        shapeFilePath, "feet", Config.Routing.OneWayField ?? string.Empty,
                                        Config.Routing.OneWayFieldIndicator ?? string.Empty,
                                        Config.Routing.SpeedLimitField ?? string.Empty, false);
                                }
                                string normalized = Regex.Replace(s ?? string.Empty, "\\s", "");
                                if (string.IsNullOrWhiteSpace(s) || string.Equals(normalized, "0", StringComparison.OrdinalIgnoreCase))
                                    s = null;
                                _routeID = s;
                                if (_routeID == null)
                                    _routingCore = null;
                                else
                                    _routingSpatialReference = new SpatialReference(Config.Routing.WKID);
                            }
                            catch (Exception ex)
                            {
                                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error setting up routing", ex);
                            }
                        })
                    {
                        IsBackground = true
                    }.Start();
                }
                else
                    _routingCore = null;
            }
            catch (Exception ex)
            {
                var message = "Error starting background thread for routing setup";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        private async void TileCacheLoaded(object sender, TileCacheLoadedEventArgs e)
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                if (e == null || e.Error != null)
                {
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error syncing geodatabase", e.Error);
                    return;
                }

                // Create the basemap based on the tile cache
                _initialTileLayer = new ArcGISTiledLayer(e.Cache);
                await _initialTileLayer.LoadAsync();
                if(_initialLoad)
                {
                    if(e.Cache.FullExtent != null)
                    {
                        if (_initialExtent != null)
                        {
                            if(_initialExtent.SpatialReference == _initialTileLayer.SpatialReference)
                                _initialExtent = GeometryEngine.Union(_initialExtent, _initialTileLayer.FullExtent);
                            else
                            {
                                var projectedExtent = GeometryEngine.Project(_initialExtent, _initialTileLayer.SpatialReference);
                                _initialExtent = GeometryEngine.Union(_initialTileLayer.FullExtent, projectedExtent);
                            }
                        }
                        else
                            _initialExtent = e.Cache.FullExtent;
                    }
                }
                else
                {
                    var layers = new List<Layer>();
                    foreach (var lyr in Map.OperationalLayers)
                    {
                        layers.Add(lyr);
                    }
                    // Clear out the existing layers
                    Map.OperationalLayers.Clear();

                    Basemap basemap = new Basemap(_initialTileLayer);
                    Map.Basemap = basemap;

                    foreach (var lyr in layers)
                    {
                        Map.OperationalLayers.Add(lyr);
                    }
                }

                Application.Current.Dispatcher.Invoke(LoadNextMapService);
            }
            catch (Exception ex)
            {
                var message = "Error on tile cache loaded";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        private async void GeodatabaseLoaded(object sender, GeodatabaseLoadedEventArgs e)
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                if (e == null || e.Error != null)
                {
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, "Error syncing geodatabase", e.Error);
                    return;
                }

                List<FeatureLayer> oldLayers = null;
                Geodatabase oldDatabase = null;
                if (_initialLoad)
				{
					if (LoadedCaches.TryGetValue(e.CacheName, out oldDatabase) && CacheLayers.TryGetValue(e.CacheName, out oldLayers))
					{
						foreach (var lyr in oldLayers)
						{
							Map.OperationalLayers.Remove(lyr);
						}
						oldLayers.Clear();
						LoadedCaches.Remove(e.CacheName);
						if (Config.Geocoders.Any(gcdr => gcdr.CacheName == e.CacheName))
							LocationTools.CancelLocationToolsBuild();
						oldDatabase.Close();
					}
					else
						CacheLayers.Add(e.CacheName, new List<FeatureLayer>());

					LoadedCaches.Add(e.CacheName, e.Database);
					foreach (var tbl in e.Database.GeodatabaseFeatureTables.Reverse())
                    {
                        var layer = new FeatureLayer(tbl);
                        bool layerLoaded = false;
                        try
                        {
                            await layer.LoadAsync();
                            layerLoaded = true;
                        }
                        catch (ArcGISRuntimeException arcex)
                        {
                            var message = String.Format("Error loading geodatabase layer '{0}'", layer.Name);
                            Logging.LogMessage(Logging.LogType.Warn, message, arcex);
                        }
                        if (layerLoaded)
                        {
                            if (layer.FullExtent != null)
                            {
                                if (_initialExtent != null)
                                {
                                    if (_initialExtent.SpatialReference == null || _initialExtent.SpatialReference == layer.FullExtent.SpatialReference)
                                        _initialExtent = GeometryEngine.Union(_initialExtent, layer.FullExtent);
                                    else
                                    {
                                        var projectedExtent = GeometryEngine.Project(layer.FullExtent, _initialExtent.SpatialReference);
                                        _initialExtent = GeometryEngine.Union(_initialExtent, projectedExtent);
                                    }
                                }
                                else
                                    _initialExtent = layer.FullExtent;
                            }

                            if (tbl.LoadStatus == Esri.ArcGISRuntime.LoadStatus.Loaded)
                            {
                                layer.Name = tbl.LayerInfo.ServiceLayerName;
                                layer.IsVisible = tbl.LayerInfo.DefaultVisibility;
                            }
                            else
                            {
                                tbl.LoadStatusChanged += (sendr, ev) =>
                                {
                                    if (ev.Status == Esri.ArcGISRuntime.LoadStatus.Loaded)
                                    {
                                        layer.Name = tbl.LayerInfo.ServiceLayerName;
                                        layer.IsVisible = tbl.LayerInfo.DefaultVisibility;
                                    }
                                };
                            }

                            CacheLayers[e.CacheName].Add(layer);
                            if (_initialFeatureLayers == null)
                                _initialFeatureLayers = new List<FeatureLayer>();
                            _initialFeatureLayers.Add(layer);
                        }
                    }
                }
                else
                {
                    if (LoadedCaches.TryGetValue(e.CacheName, out oldDatabase) && CacheLayers.TryGetValue(e.CacheName, out oldLayers))
                    {
                        foreach (var lyr in oldLayers)
                        {
                            Map.OperationalLayers.Remove(lyr);
                        }
                        oldLayers.Clear();
                        LoadedCaches.Remove(e.CacheName);
                        if (Config.Geocoders.Any(gcdr => gcdr.CacheName == e.CacheName))
                            LocationTools.CancelLocationToolsBuild();
                        oldDatabase.Close();
                    }
                    else
                        CacheLayers.Add(e.CacheName, new List<FeatureLayer>());

                    LoadedCaches.Add(e.CacheName, e.Database);
                    // Loop through all feature tables in the geodatabase and add a new layer to the map
                    foreach (GeodatabaseFeatureTable table in e.Database.GeodatabaseFeatureTables.Reverse())
                    {
                        // Create a new feature layer for the table
                        FeatureLayer layer = new FeatureLayer(table);

                        //load the layer
                        bool layerLoaded = false;
                        try
                        {
                            await layer.LoadAsync();
                            layerLoaded = true;
                        }
                        catch (ArcGISRuntimeException arcex)
                        {
                            var message = String.Format("Error loading geodatabase layer '{0}'", layer.Name);
                            Logging.LogMessage(Logging.LogType.Warn, message, arcex);
                        }
                        if (layerLoaded)
                        {
                            if (table.LoadStatus == Esri.ArcGISRuntime.LoadStatus.Loaded)
                            {
                                layer.Name = table.LayerInfo.ServiceLayerName;
                                layer.IsVisible = table.LayerInfo.DefaultVisibility;
                            }
                            else
                            {
                                table.LoadStatusChanged += (sendr, ev) =>
                                {
                                    if (ev.Status == Esri.ArcGISRuntime.LoadStatus.Loaded)
                                    {
                                        layer.Name = table.LayerInfo.ServiceLayerName;
                                        layer.IsVisible = table.LayerInfo.DefaultVisibility;
                                    }
                                };
                            }

                            CacheLayers[e.CacheName].Add(layer);
                            // Add the new layer to the map
                            Map.OperationalLayers.Add(layer);
                        }
                    }
                    Legend.Layers = Map.AllLayers;
                }

                Application.Current.Dispatcher.Invoke(LoadNextMapService);
            }
            catch (Exception ex)
            {
                var message = "Error on geodatabase loaded";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        private void ProgressTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if ((Progress == ProgressTotal || Progress == 0.0) && ProgressOpacity > 0)
                    ProgressOpacity = ProgressOpacity - 0.1;
                else if (Progress < ProgressTotal && Progress > 0)
                    ProgressOpacity = 1.0;
            }
            catch (Exception ex)
            {
                Logging.LogMessage(Logging.LogType.Error, "Error on progress timer elapsed", ex);
            }
        }

        private void CacheUpdating(object sender, CacheUpdatingEventArgs e)
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                if (e == null)
                    return;
                Progress = e.Loaded;
                ProgressTotal = e.Total;
                ProgressMessage = e.Status;
            }
            catch (Exception ex)
            {
                var message = "Error on cache update";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        private async void LoadNextMapService()
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                _loading++;
                if (_loading == _config.MapServices.Count)
                {
                    if(_initialLoad)
                    {
                        if(_initialExtent.GeometryType == GeometryType.Polygon)
                        {
                            // the first layer that is added to the map defines the maximum extent that the map will move to
                            // so this layer has a single feature that is the full extent of all the layers that will be added to the map
                            var sr = _initialTileLayer != null ? _initialTileLayer.SpatialReference : _initialExtent.SpatialReference;
                            FeatureCollectionTable polysTable = new FeatureCollectionTable(new List<Field>(), GeometryType.Polygon, sr, _initialExtent.HasZ, _initialExtent.HasM);
                            Feature polyFeature = polysTable.CreateFeature();
                            polyFeature.Geometry = _initialExtent;
                            await polysTable.AddFeatureAsync(polyFeature);
                            FeatureCollection featuresCollection = new FeatureCollection();
                            featuresCollection.Tables.Add(polysTable);
                            var fLayer = new FeatureCollectionLayer(featuresCollection);
                            fLayer.IsVisible = false;
                            Map.OperationalLayers.Add(fLayer);
                            Map.OperationalLayers.Remove(fLayer);

                            if (_initialTileLayer != null)
                                Map.Basemap = new Basemap(_initialTileLayer);
                            foreach (FeatureLayer layer in _initialFeatureLayers)
                            {
                                Map.OperationalLayers.Add(layer);
                            }
                            Legend.Layers = Map.AllLayers;
                        }
                        _initialLoad = false;
                    }
                    ZoomToFullExtent();
                    IncidentViewModel.FirstDisplayItemKey = Config.FirstIncidentDisplayField;
                    IncidentViewModel.SecondDisplayItemKey = Config.SecondIncidentDisplayField;
                    Logging.LogMessage(Logging.LogType.Info, String.Format("AVL Config ID = {0}", Config.AvlID));
                    Logging.LogMessage(Logging.LogType.Info, String.Format("Dispatch Config ID = {0}", Config.DispatchID));
                    VPService.GetAvlSettingsAsync(Config.AvlID);
                    VPService.GetDispatchSettingsAsync(Config.DispatchID);
                    StartGPSListener();
                    SetupRouting();
                    Map.MaxScale = 0;
                    Map.MinScale = 0;
                }
                if(_loading > _config.MapServices.Count - 1)
                {
                    SetupStreetIntersectionFinder();
                    SetupAddressFinder();
                    return;
                }

                Cache.LoadCache(_config.MapServices[_loading]);
            }
            catch (Exception ex)
            {
                var message = "Error loading map service";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        private void StartGPSListener()
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                if (GpsConfig.Type == GpsType.None)
                    return;
                _gpsExceptions = new HashSet<Exception>();
                _gpsListener = new GPSListener(GpsConfig.Port, GpsConfig.ComBaudRate, GpsConfig.Type == GpsType.UDP);
                _gpsListener.GPSMessageRecieved += GPSMessageRecieved;
                _gpsListener.Start();
            }
            catch (Exception ex)
            {
                var message = "Error starting gps listener";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        private void ForwardToAVLServer(double lon, double lat, string unitID)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(Config.AvlServerAddress))
                    return;
                if(_avlForward == null)
                {
                    var uri = new Uri(Config.AvlServerAddress);
                    _avlForward = new UdpClient(uri.Host, uri.Port);
                }
                string message;
                if (Config.ReportUnitNumber)
                    message = $"gtgavldd|{unitID}|{lat}|{lon}";
                else
                    message = $"gtgavldd|Unknown|{lat}|{lon}";
                byte[] ba = Encoding.ASCII.GetBytes(message.ToCharArray());
                _avlForward.Send(ba, ba.Length);
            }
            catch (Exception ex)
            {
                var message = "Error sending AVL data to server \r\n" + ex.ToString();
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        private void GPSMessageRecieved(object sender, GPSMessageRecievedEventArgs e)
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                if (e.Error != null)
                {
                    if (!_gpsExceptions.Add(e.Error))
                        return;
                    var message = "Error recieving gps message -\r\n" + e.OriginalMessage;
                    if(e.ErrorInitializing)
                        ErrorHelper.OnMessage("Could not connect to GPS");
                    //ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, e.Error);
                    Logging.LogMessage(Logging.LogType.Error, message, e.Error);
                    return;
                }
                ForwardToAVLServer(e.Longitude, e.Latitude, UserSettings.UnitNumber);
                var newLocation = new MapPoint(e.Longitude, e.Latitude, SpatialReferences.Wgs84);
                if(_lastGPSLocation != null)
                {
                    double xDiff = newLocation.X - _lastGPSLocation.X;
                    double yDiff = newLocation.Y - _lastGPSLocation.Y;
                    var angle = Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI;
                    _lastGpsAngle = angle - 90;
                }
                _lastGPSLocation = newLocation;
                if (GpsGraphicsLayer == null)
                {
                    GpsGraphicsLayer = new GraphicsOverlay();
                    GpsGraphicsLayer.LabelDefinitions.Add(Labels.BuildLabelWithLocation(AvlViewModel.LATITUDE, AvlViewModel.LONGITUDE, Config.MapTextSize, Color.LightGoldenrodYellow, Color.DarkSlateGray));
                    GpsGraphicsLayer.LabelsEnabled = UserSettings.GpsLocLabel;

                    Symbol smbl = PictureMarkerSymbol.FromJson(AvlViewModel.GPS_SYMBOL_JSON);
                    var renderer = new SimpleRenderer(smbl);
                    GpsGraphicsLayer.Renderer = renderer;
                    Application.Current.Dispatcher.Invoke((Action)(() => MapView.GraphicsOverlays.Add(GpsGraphicsLayer)));
                }

                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    if (GpsGraphicsLayer.Graphics.Any())
                        GpsGraphicsLayer.Graphics.Clear();
                    GpsGraphicsLayer.Graphics.Add(new Esri.ArcGISRuntime.UI.Graphic(_lastGPSLocation, new Dictionary<String, object> { { AvlViewModel.LATITUDE, Math.Truncate(e.Latitude * 100000) / 100000 }, { AvlViewModel.LONGITUDE, Math.Truncate(e.Longitude * 100000) / 100000 } }));
                }));
                
                if(GpsKeepCentered)
                {
                    Application.Current.Dispatcher.Invoke(ZoomToGps);
                }
            }
            catch (Exception ex)
            {
                var message = "Error recieving gps message -\r\n" + e.OriginalMessage;
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        private void MapView_Tapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                if (_mapClicked != null)
                {
                    if (_mapClickedSpatialReference > 0)
                        _mapClicked.Invoke((MapPoint)GeometryEngine.Project(e.Location, new SpatialReference(_mapClickedSpatialReference)));
                    else
                        _mapClicked.Invoke(e.Location);
                    _mapClicked = null;
                    _mapClickedSpatialReference = -1;
                }
            }
            catch (Exception ex)
            {
                var message = "Error on map view clicked";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        private void MapView_ViewpointChanged(object sender, EventArgs e)
        {
            //need to figure out how to turn off GpsKeepCentered when map is moved by user
            //if (GpsKeepCentered && _lastGPSTime != null && (DateTime.Now - _lastGPSTime).Milliseconds > 100)
            //    GpsKeepCentered = false;
        }

        private void SetupStreetIntersectionFinder()
        {
            Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
            Application.Current.Dispatcher.Invoke((Action)(() =>
            {
                LocationTools.Streets.Clear();
                LocationTools.Intersections.Clear();
            }));
            Thread wait = new Thread((async() =>
            {
                try
                {
                    foreach (StreetFinderSettings finderSettings in Config.StreetList)
                    {
                        Geodatabase cache;
                        if (!LoadedCaches.TryGetValue(finderSettings.CacheName, out cache) || cache == null)
                        {
                            MessageBox.Show("Could not find " + finderSettings.CacheName + " Service in mobile cache", "Street Intersection Finder");
                            continue;
                        }
                        FeatureTable featureSource = cache.GeodatabaseFeatureTables.FirstOrDefault(tbl => tbl.ServiceLayerId.Equals(finderSettings.LayerID) && (tbl.LayerInfo == null || tbl.LayerInfo.ServiceLayerName == finderSettings.LayerName));
                        if (featureSource == null)
                        {
                            MessageBox.Show("Could not find " + finderSettings.LayerName + " Layer in " + finderSettings.CacheName + " Service", "Street Intersection Finder");
                            continue;
                        }
                        if (featureSource.LoadStatus == Esri.ArcGISRuntime.LoadStatus.Loaded)
                        {
                            if (!featureSource.Fields.Any(fld => fld.Name == finderSettings.StreetNameField))
                            {
                                MessageBox.Show("Could not find " + finderSettings.StreetNameField + " Field in " + finderSettings.LayerName +
                                                " Layer", "Street Intersection Finder");
                                continue;
                            }
                            var qf = new QueryParameters();
                            qf.WhereClause = "1=1";
                            var result = await featureSource.QueryFeaturesAsync(qf);
                            LocationTools.FeatureSource = featureSource;
                            LocationTools.StreetNameField = finderSettings.StreetNameField;
                            var join = new Dictionary<String, List<Esri.ArcGISRuntime.Geometry.Geometry>>();
                            foreach (var feature in result)
                            {
                                var streetName = feature.Attributes[finderSettings.StreetNameField]?.ToString();
                                if (String.IsNullOrWhiteSpace(streetName))
                                    continue;
                                List<Esri.ArcGISRuntime.Geometry.Geometry> geom;
                                if (!join.TryGetValue(streetName, out geom))
                                {
                                    var lst = new List<Esri.ArcGISRuntime.Geometry.Geometry>();
                                    lst.Add(feature.Geometry);
                                    join.Add(streetName, lst);
                                }
                                else
                                    geom.Add(feature.Geometry);
                            }
                            Application.Current.Dispatcher.Invoke(() => LocationTools.Streets = join);
                        }
                        else
                        {
                            featureSource.LoadStatusChanged += async (sender, e) =>
                            {
                                if (e.Status == Esri.ArcGISRuntime.LoadStatus.Loaded)
                                {
                                    if (!featureSource.Fields.Any(fld => fld.Name == finderSettings.StreetNameField))
                                    {
                                        MessageBox.Show("Could not find " + finderSettings.StreetNameField + " Field in " + finderSettings.LayerName +
                                                        " Layer", "Street Intersection Finder");
                                        return;
                                    }
                                    var qf = new QueryParameters();
                                    qf.WhereClause = "1=1";
                                    var result = await featureSource.QueryFeaturesAsync(qf);
                                    LocationTools.FeatureSource = featureSource;
                                    LocationTools.StreetNameField = finderSettings.StreetNameField;
                                    var join = new Dictionary<String, List<Esri.ArcGISRuntime.Geometry.Geometry>>();
                                    foreach (var feature in result)
                                    {
                                        var streetName = feature.Attributes[finderSettings.StreetNameField]?.ToString();
                                        if (String.IsNullOrWhiteSpace(streetName))
                                            continue;
                                        List<Esri.ArcGISRuntime.Geometry.Geometry> geom;
                                        if (!join.TryGetValue(streetName, out geom))
                                        {
                                            var lst = new List<Esri.ArcGISRuntime.Geometry.Geometry>();
                                            lst.Add(feature.Geometry);
                                            join.Add(streetName, lst);
                                        }
                                        else
                                            geom.Add(feature.Geometry);
                                    }
                                    Application.Current.Dispatcher.Invoke(() => LocationTools.Streets = join);
                                }
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    var message = "Error setting up street intersections";
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                    Logging.LogMessage(Logging.LogType.Error, message, ex);
                }
            }));
            wait.IsBackground = true;
            wait.Start();
        }

        private void SetupAddressFinder()
        {
            Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
            var thread = new Thread(
                () =>
                {
                    try
                    {
                        LocationTools.ClearGeocoderDataSets();

                        Geodatabase cache = null;
                        foreach (GeocoderSettings geocoder in
                            Config.Geocoders.Where(s => LoadedCaches.TryGetValue(s.CacheName, out cache)))
                        {
                            FeatureTable featureSource =
                                cache.GeodatabaseFeatureTables.FirstOrDefault(
                                    lyr => lyr.ServiceLayerId.Equals(geocoder.LayerID) && (lyr.LayerInfo == null || string.Equals(lyr.LayerInfo.ServiceLayerName, geocoder.LayerName, StringComparison.OrdinalIgnoreCase)));
                            if (featureSource == null)
                                MessageBox.Show("Address finder layer (" + geocoder.LayerName + ") not found.");
                            else
                            {
                                if(featureSource.LoadStatus == Esri.ArcGISRuntime.LoadStatus.Loaded)
                                    LocationTools.AddGeocoderDataSet(featureSource, geocoder);
                                else
                                {
                                    featureSource.LoadStatusChanged += (sender, e) =>
                                    {
                                        if(e.Status == Esri.ArcGISRuntime.LoadStatus.Loaded)
                                            LocationTools.AddGeocoderDataSet(featureSource, geocoder);
                                    };
                                }
                            }
                        }

                        LocationTools.GeocodersLoaded = true;
                    }
                    catch (Exception ex)
                    {
                        var message = "Error setting up geocoders";
                        ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                        Logging.LogMessage(Logging.LogType.Error, message, ex);
                    }
                })
            {
                IsBackground = true
            };
            thread.Start();
        }

        private void GetSplashUpdateFileCompleted(object sender, GetSplashUpdateFileCompletedEventArgs e)
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                if (e.Error != null)
                {
                    if (!(e.Error.InnerException is WebException) ||
                        ((WebException)e.Error.InnerException).Status != WebExceptionStatus.RequestCanceled) return;
                    var sep = new Thread(
                        () =>
                        {
                            try
                            {
                                var tmpString = (string)e.UserState;
                                int repeats = 1;
                                if (tmpString.Contains('|'))
                                {
                                    var tmpSplit = tmpString.Split('|');
                                    repeats = int.Parse(tmpSplit[0]);
                                    tmpString = tmpSplit[1];
                                }
                                if (repeats++ >= 5)
                                    return;
                                var folder = new Uri(Directory.GetParent(Directory.GetCurrentDirectory()).FullName);
                                string s =
                                    Uri.UnescapeDataString(
                                        folder.MakeRelativeUri(new Uri(tmpString))
                                            .ToString()
                                            .Replace('/', Path.DirectorySeparatorChar));
                                ((VPMobileServiceClient)sender).GetSplashUpdateFileAsync(s, repeats + tmpString);
                            }
                            catch (Exception ex2)
                            {
                                Console.WriteLine(ex2.ToString());
                            }
                        });
                    sep.Start();
                }
                else
                {
                    if (e.Result == null) return;
                    var thread = new Thread(() =>
                    {
                        var written = false;
                        while (!written)
                        {
                            try
                            {
                                File.WriteAllBytes((string)e.UserState, e.Result);
                                written = true;
                            }
                            catch (IOException ex)
                            {
                                if (ex.Message.Contains("being used by another process")) continue;
                                var message = "Error getting update file for splash screen";
                                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                                Logging.LogMessage(Logging.LogType.Error, message, ex);
                                written = true;
                            }
                            catch (Exception ex)
                            {
                                var message = "Error getting update file for splash screen";
                                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                                Logging.LogMessage(Logging.LogType.Error, message, ex);
                                written = true;
                            }
                        }
                    })
                    { IsBackground = true };
                    thread.Start();
                }
            }
            catch (Exception ex)
            {
                var message = "Error getting update file for splash screen";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        private void GetSplashUpdateFileListCompleted(object sender, GetSplashUpdateFileListCompletedEventArgs e)
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                if (e.Error != null)
                {
                    var message = "Error getting update file list for splash screen";
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, e.Error);
                    Logging.LogMessage(Logging.LogType.Error, message, e.Error);
                    return;
                }
                UpdateFileInfo[] array = e.Result;
                string strPath = Directory.GetParent(Directory.GetCurrentDirectory()).FullName;
                if (array == null || !array.Any())
                    return;
                if (!Directory.Exists(strPath))
                    Directory.CreateDirectory(strPath);
                for (int i = 0; i <= array.Length - 1; i++)
                {
                    UpdateFileInfo f = array[i];
                    string filePath = Path.Combine(strPath, f.RelativeFilePath);
                    // if the file exists and the local file is later than the server file skip
                    if (File.Exists(filePath) && DateTime.Compare(File.GetLastWriteTime(filePath), f.FileDate) >= 0)
                        continue;
                    var client = (VPMobileServiceClient)sender;
                    client.GetSplashUpdateFileAsync(Guid.NewGuid().ToString(), f.RelativeFilePath, filePath);
                }
            }
            catch (Exception ex)
            {
                var message = "Error getting update file list for splash screen";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        private void GetAvlSettingsCompleted(object sender, GetAvlSettingsCompletedEventArgs e)
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                if (e.Error != null)
                {
                    var message = "Error pulling avl settings";
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, e.Error);
                    Logging.LogMessage(Logging.LogType.Error, message, e.Error);
                }
                if (AvlGraphicsLayer != null)
                    MapView.GraphicsOverlays.Remove(AvlGraphicsLayer);
                AvlGraphicsLayer = new GraphicsOverlay();
                AvlGraphicsLayer.RenderingMode = GraphicsRenderingMode.Dynamic;
                
                AvlGraphicsLayer.LabelDefinitions.Add(Labels.BuildLabelWithId(AvlViewModel.UNIT_LABEL, AvlViewModel.UNIT_ALIAS, AvlViewModel.LATITUDE, AvlViewModel.LONGITUDE, Config.AvlLocOption, Config.MapTextSize, Color.DarkOrange, Color.DarkSlateGray));                
                AvlGraphicsLayer.LabelsEnabled = UserSettings.AvlLabel;

                // Create a new unique value renderer
                UniqueValueRenderer avlGroupsRenderer = new UniqueValueRenderer();

                // Add the "SUB_REGION" field to the renderer
                avlGroupsRenderer.FieldNames.Add(AvlViewModel.GROUP_ID);

                // Set the default region fill symbol (transparent with no outline) for regions not explicitly defined in the renderer
                System.Windows.Media.Color colorRed = System.Windows.Media.Color.FromArgb(Color.Red.A, Color.Red.R, Color.Red.G, Color.Red.B);
                var defaultFillSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, colorRed, 25);
                avlGroupsRenderer.DefaultSymbol = defaultFillSymbol;
                //regionRenderer.DefaultLabel = "Other";

                foreach (var grp in e.Result.Groups.Where(grp => Config.AvlGroups.Contains(grp.GroupID)))
                {
                    var group = new AvlGroupViewModel(grp.GroupName, grp.GroupID, grp.GroupColor, grp.GroupSymbol, Config.MapIconSize);
                    Symbol smbl = PictureMarkerSymbol.FromJson(group.GroupImageJson);
                    Application.Current.Dispatcher.Invoke((Action)(() => AvlList.AvlGroups.Add(group)));
                    avlGroupsRenderer.UniqueValues.Add(
                        new UniqueValue(grp.GroupName, grp.GroupName + " Unit", smbl, grp.GroupID));
                }

                AvlGraphicsLayer.Renderer = avlGroupsRenderer;
                MapView.GraphicsOverlays.Add(AvlGraphicsLayer);

                _avlTimer = new System.Timers.Timer(Convert.ToDouble(UserSettings.IncidentRefreshInterval));
                _avlTimer.Elapsed += AvlTimer_Elapsed;
                _avlTimer.Start();
                AvlTimer_Elapsed(null, null);
            }
            catch (Exception ex)
            {
                var message = "Error setting up avl";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        private void AvlTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                if (_avlTimer.Interval != UserSettings.IncidentRefreshInterval)
                {
                    _avlTimer.Stop();
                    _avlTimer.Interval = Convert.ToDouble(UserSettings.IncidentRefreshInterval);
                    _avlTimer.Start();
                }

                VPService.GetAVLLastReportRecordDataAsync(Config.AvlID.ToString(), DateTime.Now);
            }
            catch (Exception ex)
            {
                var message = "Error pulling avl data";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        private void GetAVLLastReportRecordDataCompleted(object sender, GetAVLLastReportRecordDataCompletedEventArgs e)
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                if (_avlLast.CompareTo(e.UserState) > 0)
                    return;
                _avlLast = (DateTime)e.UserState;
                if (e.Error != null)
                {
                    NetworkDisconnect = true;
                    return;
                }
                NetworkDisconnect = false;

                foreach(var grphc in AvlGraphicsLayer.Graphics)
                {
                    if(!grphc.Attributes.ContainsKey("oldValue"))
                        grphc.Attributes.Add("oldValue", true);
                }

                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    foreach (var unitInfo in e.Result)
                    {
                        var unit = new AvlViewModel(unitInfo);
                        var group = AvlList.AvlGroups.FirstOrDefault(grp => grp.GroupID == unit.GroupID);
                        if (group == null || unit.UnitID.Equals(UserSettings.UnitNumber, StringComparison.CurrentCultureIgnoreCase)) continue;
                        unit.GroupImage = group.GroupImage;
                        var oldUnit = group.AvlUnits.FirstOrDefault(unt => unt.UnitID == unit.UnitID && unt.GroupID == unit.GroupID);
                        if (oldUnit != null)
                        {
                            group.AvlUnits.Remove(oldUnit);
                            AvlGraphicsLayer.Graphics.Remove(oldUnit.Graphic);
                        }
                        group.AvlUnits.Add(unit);
                        if (group.Visible)
                            AvlGraphicsLayer.Graphics.Add(unit.Graphic);
                    }
                }));

                var oldUnits = AvlGraphicsLayer.Graphics.Where(grphc => grphc.Attributes.ContainsKey("oldValue")).ToList();
                foreach (var old in oldUnits)
                {
                    AvlGraphicsLayer.Graphics.Remove(old);
                    Application.Current.Dispatcher.Invoke((Action)(() =>
                    {
                        var group = AvlList.AvlGroups.FirstOrDefault(grp => grp.GroupID == (int)old.Attributes[AvlViewModel.GROUP_ID]);
                        if (group == null) return;
                        var oldUnit = group.AvlUnits.FirstOrDefault(unt => unt.UnitID == old.Attributes[AvlViewModel.UNIT_ID].ToString());
                        group.AvlUnits.Remove(oldUnit);
                    }));
                }
            }
            catch (Exception ex)
            {
                var message = "Error pulling avl data";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        private void GetDispatchSettingsCompleted(object sender, GetDispatchSettingsCompletedEventArgs e)
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                if (e.Error != null)
                {
                    var message = "Error pulling incident settings";
                    ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, e.Error);
                    Logging.LogMessage(Logging.LogType.Error, message, e.Error);
                }
                if (DispatchGraphicsLayer != null)
                    MapView.GraphicsOverlays.Remove(DispatchGraphicsLayer);
                DispatchGraphicsLayer = new GraphicsOverlay();
                DispatchGraphicsLayer.RenderingMode = GraphicsRenderingMode.Dynamic;

                DispatchGraphicsLayer.LabelDefinitions.Add(Labels.BuildLabelWithId(IncidentViewModel.FirstDisplayItemKey, IncidentViewModel.FirstDisplayItemKey, IncidentViewModel.LATITUDE, IncidentViewModel.LONGITUDE, false, Config.MapTextSize, Color.Aqua, Color.DarkSlateGray));
                DispatchGraphicsLayer.LabelsEnabled = UserSettings.IncidentLabel;

                // Create a new unique value renderer
                UniqueValueRenderer callTypesRenderer = new UniqueValueRenderer();

                // Add the "SUB_REGION" field to the renderer
                callTypesRenderer.FieldNames.Add(IncidentViewModel.CALL_TYPE);

                // Set the default region fill symbol (transparent with no outline) for regions not explicitly defined in the renderer
                var callType = new CallTypeViewModel("", e.Result.DefaultIncidentSymbol, Config.MapIconSize);
                Symbol defaultFillSymbol = PictureMarkerSymbol.FromJson(callType.CallTypeImageJson);
                callTypesRenderer.DefaultSymbol = defaultFillSymbol;
                //regionRenderer.DefaultLabel = "Other";

                foreach (var type in e.Result.IncidentTypes.Where(typ => Config.DispatchGroups.Contains(typ.IncidentType)))
                {
                    var cllType = new CallTypeViewModel(type.IncidentType, type.Symbol, Config.MapIconSize);
                    Symbol smbl = PictureMarkerSymbol.FromJson(cllType.CallTypeImageJson);
                    Application.Current.Dispatcher.Invoke((Action)(() => IncidentsList.CallTypes.Add(cllType)));
                    callTypesRenderer.UniqueValues.Add(
                        new UniqueValue(type.IncidentType, type.IncidentType + " Incident", smbl, type.IncidentType));
                }
                                
                DispatchGraphicsLayer.Renderer = callTypesRenderer;
                if(MapView.GraphicsOverlays.Any(ovrly => ovrly == AvlGraphicsLayer))
                {
                    MapView.GraphicsOverlays.Remove(AvlGraphicsLayer);
                    MapView.GraphicsOverlays.Add(DispatchGraphicsLayer);
                    MapView.GraphicsOverlays.Add(AvlGraphicsLayer);
                }
                else
                    MapView.GraphicsOverlays.Add(DispatchGraphicsLayer);

                _incidentTimer = new System.Timers.Timer(Convert.ToDouble(UserSettings.IncidentRefreshInterval));
                _incidentTimer.Elapsed += IncidentTimer_Elapsed;
                _incidentTimer.Start();
                IncidentTimer_Elapsed(null, null);
            }
            catch (Exception ex)
            {
                var message = "Error setting up incident";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        private void IncidentTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                if (_incidentTimer.Interval != UserSettings.IncidentRefreshInterval)
                {
                    _incidentTimer.Stop();
                    _incidentTimer.Interval = Convert.ToDouble(UserSettings.IncidentRefreshInterval);
                    _incidentTimer.Start();
                }

                VPService.GetDispatchRecordDataAsync(Config.DispatchID.ToString(), DateTime.Now);
            }
            catch (Exception ex)
            {
                var message = "Error pulling incident data";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }

        private void GetDispatchRecordDataCompleted(object sender, GetDispatchRecordDataCompletedEventArgs e)
        {
            try
            {
                Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
                if (_incidentLast.CompareTo(e.UserState) > 0)
                    return;
                _incidentLast = (DateTime)e.UserState;
                if (e.Error != null)
                {
                    NetworkDisconnect = true;
                    return;
                }
                NetworkDisconnect = false;

                foreach (var incidentInfo in IncidentsList.Incidents)
                {
                    incidentInfo.isCurrent = false;
                }

                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    //var strDictionary = e.Result.Aggregate(String.Empty, (seed, diction) =>
                    // {
                    //     if (String.IsNullOrWhiteSpace(seed))
                    //     {
                    //         return "{" + diction.Aggregate(String.Empty, (sd, kval) => String.IsNullOrWhiteSpace(sd) ? "\r\n" + kval.Key + ": " + kval.Value.ToString() : sd + ",\r\n" + kval.Key + ": " + kval.Value.ToString()) + "\r\n}";
                    //     }
                    //     return seed + ",\r\n{" + diction.Aggregate(String.Empty, (sd, kval) => String.IsNullOrWhiteSpace(sd) ? "\r\n" + kval.Key + ": " + kval.Value.ToString() : sd + ",\r\n" + kval.Key + ": " + kval.Value.ToString()) + "\r\n}";
                    // });
                    //MessageBox.Show(strDictionary);
                    if (e.Result.Length > 0)
                    {
                        foreach (var incidentInfo in e.Result)
                        {
                            //var display = incidentInfo.Aggregate(String.Empty, (sd, kval) => String.IsNullOrWhiteSpace(sd) ? "\r\n" + kval.Key + ": " + kval.Value.ToString() : sd + ",\r\n" + kval.Key + ": " + kval.Value.ToString());
                            //MessageBox.Show(display);
                            var incident = new IncidentViewModel(incidentInfo);
                            var callType = IncidentsList.CallTypes.FirstOrDefault(type => type.CallTypeValue == incident.CallType);
                            if (callType == null) continue;
                            incident.CallTypeImage = callType.OriginalCallImage;
                            incident.AssignedIncident = !String.IsNullOrWhiteSpace(UserSettings.UnitNumber) && UserSettings.UnitNumber == incident.UnitID;
                            var oldIncident = IncidentsList.Incidents.FirstOrDefault(unt => unt.UniqueID.ToString().Equals(incident.UniqueID.ToString()));
                            if (oldIncident != null)
                            {
                                IncidentsList.Incidents.Remove(oldIncident);
                                if (!oldIncident.UnGeocoded)
                                    DispatchGraphicsLayer.Graphics.Remove(oldIncident.Graphic);
                                incident.Selected = oldIncident.Selected;
                            }
                            else if (UserSettings.AutoRoute && incident.AssignedIncident && !incident.UnGeocoded)
                            {
                                RouteTo(new Models.Point
                                {
                                    Latitude = incident.Latitude,
                                    Longitude = incident.Longitude
                                });
                            }
                            incident.isCurrent = true;
                            IncidentsList.Incidents.Add(incident);
                            if (callType.CallTypeActive && !incident.UnGeocoded)
                                DispatchGraphicsLayer.Graphics.Add(incident.Graphic);
                        }
                    }
                    else
                    {
                        // no incidents are active at the moment - clear the list and the graphics
                        IncidentsList.Incidents.Clear();
                        DispatchGraphicsLayer.Graphics.Clear();
                    }
                }));

                Application.Current.Dispatcher.Invoke((Action)(() =>
                {
                    var oldIncidents = IncidentsList.Incidents.Where(incident => !incident.isCurrent).ToList();
                    foreach (var oldIncident in oldIncidents)
                    {
                        if (oldIncident.Graphic != null)
                        {
                            DispatchGraphicsLayer.Graphics.Remove(oldIncident.Graphic);
                        }
                        IncidentsList.Incidents.Remove(oldIncident);
                    }
                }));
            }
            catch (Exception ex)
            {
                var message = "Error pulling incident data";
                ErrorHelper.OnError(MethodBase.GetCurrentMethod().DeclaringType.Name, message, ex);
                Logging.LogMessage(Logging.LogType.Error, message, ex);
            }
        }
        #endregion
    }
}
