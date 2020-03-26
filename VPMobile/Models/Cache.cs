using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using GTG.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VPMobileObjects;

namespace VP_Mobile
{
    public class TileCacheLoadedEventArgs : EventArgs
    {
        public Exception Error { get; private set; }
        public TileCache Cache { get; private set; }
        public String CacheName { get; private set; }

        public TileCacheLoadedEventArgs(String cacheName, TileCache cache, Exception error)
        {
            CacheName = cacheName;
            Error = error;
            Cache = cache;
        }
    }

    public class GeodatabaseLoadedEventArgs : EventArgs
    {
        public Exception Error { get; private set; }
        public Geodatabase Database { get; private set; }
        public String CacheName { get; private set; }

        public GeodatabaseLoadedEventArgs(String cacheName, Geodatabase database, Exception error)
        {
            CacheName = cacheName;
            Error = error;
            Database = database;
        }
    }

    public class CacheUpdatingEventArgs : EventArgs
    {
        public String Status { get; private set; }

        public double Loaded { get; private set; }

        public double Total { get; private set; }

        public CacheUpdatingEventArgs(String status, double loaded, double total)
        {
            Status = status;
            Loaded = loaded;
            Total = total;
        }
    }

    public static class Cache
    {
        public const String ARCGIS_LITE_LICENSE_KEY = "runtimelite,1000,rud3102912412,none,6PB3LNBHPDMF8YAJM245";

        // Job used to generate the geodatabase
        private static GenerateGeodatabaseJob gdbJob;

        public static String GeodatabaseFilePath(String name, bool old, bool done)
        {
            Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name, () => new Dictionary<String, Object> {
                        { nameof(name), name },
                        { nameof(old), old },
                        { nameof(done), done }
                    });
            var dir = Path.Combine(ConfigHandler.AssemblyDirectory, old ? "FinishedCaches" : "BuildingCaches");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return Path.Combine(dir, name + (done ? "-Done" : String.Empty) + ".geodatabase");
        }

        public static String CacheFilePath(bool isTileCache, String name, bool old, bool done)
        {
            Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name, () => new Dictionary<String, Object> {
                        { nameof(isTileCache), isTileCache },
                        { nameof(name), name },
                        { nameof(old), old },
                        { nameof(done), done }
                    });
            var dir = Path.Combine(ConfigHandler.AssemblyDirectory, old ? "FinishedCaches" : "BuildingCaches");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            return Path.Combine(dir, name + (done ? "-Done" : String.Empty) + (isTileCache ? ".tpk" : ".geodatabase"));
        }

        public static void License()
        {
            Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name);
            Esri.ArcGISRuntime.ArcGISRuntimeEnvironment.SetLicense(ARCGIS_LITE_LICENSE_KEY);
        }

        public static event EventHandler<CacheUpdatingEventArgs> CacheUpdating;

        public static event EventHandler<TileCacheLoadedEventArgs> TileCacheLoaded;

        public static event EventHandler<GeodatabaseLoadedEventArgs> GeodatabaseLoaded;

        public static async void LoadCache(CacheSettings cache)
        {
            Logging.LogMethodCall(MethodBase.GetCurrentMethod().DeclaringType.Name, () => new Dictionary<String, Object> {
                        { nameof(cache), cache }
                    });
            if (cache == null)
                throw new ArgumentNullException("cache");
            try
            {
                if(File.Exists(CacheFilePath(cache.IsBaseMap, cache.Name, false, true)))
                {
                    if (File.Exists(CacheFilePath(cache.IsBaseMap, cache.Name, true, true)))
                        File.Delete(CacheFilePath(cache.IsBaseMap, cache.Name, true, true));
                    if (cache.IsBaseMap || true)//handle not purge on sync
                        File.Move(CacheFilePath(cache.IsBaseMap, cache.Name, false, true), CacheFilePath(cache.IsBaseMap, cache.Name, true, true));
                    else
                        File.Copy(CacheFilePath(cache.IsBaseMap, cache.Name, false, true), CacheFilePath(cache.IsBaseMap, cache.Name, true, true));
                }

                if (File.Exists(CacheFilePath(cache.IsBaseMap, cache.Name, true, true)))
                {
                    try
                    {
                        if (cache.IsBaseMap)
                            TileCacheLoaded.Invoke(null, new TileCacheLoadedEventArgs(cache.Name, new TileCache(CacheFilePath(cache.IsBaseMap, cache.Name, true, true)), null));
                        else
                            GeodatabaseLoaded.Invoke(null, new GeodatabaseLoadedEventArgs(cache.Name, await Geodatabase.OpenAsync(CacheFilePath(cache.IsBaseMap, cache.Name, true, true)), null));
                    }
                    catch (Exception ex)
                    {
                        if (cache.IsBaseMap)
                            TileCacheLoaded.Invoke(null, new TileCacheLoadedEventArgs(cache.Name, null, ex));
                        else
                            GeodatabaseLoaded.Invoke(null, new GeodatabaseLoadedEventArgs(cache.Name, null, ex));
                    }

                    if (cache.SyncType == CacheSyncTypes.AdminSync && File.GetLastWriteTime(CacheFilePath(cache.IsBaseMap, cache.Name, true, true)) >= cache.LastUpdate)
                        return;
                }

                if (cache.SyncType == CacheSyncTypes.NeverSync)
                    return;

                if(cache.IsBaseMap)
                {
                    // Create a task for generating a geodatabase (GeodatabaseSyncTask)
                    var etcTask = await ExportTileCacheTask.CreateAsync(new Uri(cache.URL));
                    
                    // Get the default parameters for the generate geodatabase task
                    var etcParams = await etcTask.CreateDefaultExportTileCacheParametersAsync(etcTask.ServiceInfo.FullExtent, etcTask.ServiceInfo.MinScale, etcTask.ServiceInfo.MaxScale);

                    // Create a generate geodatabase job
                    var etcJob = etcTask.ExportTileCache(etcParams, CacheFilePath(cache.IsBaseMap, cache.Name, false, false));

                    // Handle the job changed event
                    etcJob.JobChanged += (object sender, EventArgs e) =>
                    {
                        JobStatus status = etcJob.Status;

                        // If the job completed successfully, add the geodatabase data to the map
                        if (status == JobStatus.Succeeded)
                        {
                            File.Move(CacheFilePath(cache.IsBaseMap, cache.Name, false, false), CacheFilePath(cache.IsBaseMap, cache.Name, false, true));

                            TileCacheLoaded.Invoke(null, new TileCacheLoadedEventArgs(cache.Name, new TileCache(CacheFilePath(cache.IsBaseMap, cache.Name, false, true)), null));
                        }

                        // See if the job failed
                        if (status == JobStatus.Failed)
                        {
                            // Create a message to show the user
                            string message = "Generate tile cache job failed";

                            // Show an error message (if there is one)
                            if (etcJob.Error != null)
                            {
                                message += ": " + etcJob.Error.Message;
                            }
                            else
                            {
                                // If no error, show messages from the job
                                var m = from msg in etcJob.Messages select msg.Message;
                                message += ": " + string.Join<string>("\n", m);
                            }

                            TileCacheLoaded.Invoke(null, new TileCacheLoadedEventArgs(cache.Name, null, new Exception(message)));
                        }
                    };

                    // Handle the progress changed event (to show progress bar)
                    etcJob.ProgressChanged += ((object sender, EventArgs e) =>
                    {
                        String message = String.Format("Loading {0}", cache.Name);
                        CacheUpdating.Invoke(null, new CacheUpdatingEventArgs(message, etcJob.Progress, 100));
                    });

                    // Start the job
                    etcJob.Start();
                }
                else
                {
                    // Create a task for generating a geodatabase (GeodatabaseSyncTask)
                    var gdbSyncTask = await GeodatabaseSyncTask.CreateAsync(new Uri(cache.URL));

                    // Get the default parameters for the generate geodatabase task
                    GenerateGeodatabaseParameters generateParams = await gdbSyncTask.CreateDefaultGenerateGeodatabaseParametersAsync(gdbSyncTask.ServiceInfo.FullExtent);

                    // Create a generate geodatabase job
                    gdbJob = gdbSyncTask.GenerateGeodatabase(generateParams, CacheFilePath(cache.IsBaseMap, cache.Name, false, false));

                    // Handle the job changed event
                    gdbJob.JobChanged += (async (object sender, EventArgs e) =>
                    {
                        var job = sender as GenerateGeodatabaseJob;

                        // See if the job failed
                        if (job.Status == JobStatus.Failed)
                        {
                            // Create a message to show the user
                            string message = "Generate geodatabase job failed";

                            // Show an error message (if there is one)
                            if (job.Error != null)
                            {
                                message += ": " + job.Error.Message;
                            }
                            else
                            {
                                // If no error, show messages from the job
                                var m = from msg in job.Messages select msg.Message;
                                message += ": " + string.Join<string>("\n", m);
                            }

                            GeodatabaseLoaded.Invoke(null, new GeodatabaseLoadedEventArgs(cache.Name, null, new Exception(message)));
                        }

                        if (job.Status == JobStatus.Succeeded)
                        {
                            // Get the new geodatabase
                            var _currentGeodatabase = await job.GetResultAsync();

                            // Best practice is to unregister the geodatabase
                            await gdbSyncTask.UnregisterGeodatabaseAsync(_currentGeodatabase);
                            _currentGeodatabase.Close();

                            File.Move(CacheFilePath(cache.IsBaseMap, cache.Name, false, false), CacheFilePath(cache.IsBaseMap, cache.Name, false, true));
                            var old = await Geodatabase.OpenAsync(CacheFilePath(cache.IsBaseMap, cache.Name, false, true));

                            GeodatabaseLoaded.Invoke(null, new GeodatabaseLoadedEventArgs(cache.Name, old, null));
                        }
                    });

                    // Handle the progress changed event (to show progress bar)
                    gdbJob.ProgressChanged += ((object sender, EventArgs e) =>
                    {
                        String message = String.Format("Loading {0}", cache.Name);
                        CacheUpdating.Invoke(null, new CacheUpdatingEventArgs(message, gdbJob.Progress, 100));
                    });

                    // Start the job
                    gdbJob.Start();
                }
            }
            catch (Exception ex)
            {
                if (cache.IsBaseMap)
                    TileCacheLoaded.Invoke(null, new TileCacheLoadedEventArgs(cache.Name, null, ex));
                else
                    GeodatabaseLoaded.Invoke(null, new GeodatabaseLoadedEventArgs(cache.Name, null, ex));
            }
        }

        //public static async void DownloadTileCache(CacheSyncTypes syncType, DateTime syncDate, String name, Uri tileCacheUri, Esri.ArcGISRuntime.Geometry.Envelope extent, double minScale, double maxScale, Action<double, double> updateProgress, Action<String, ArcGISTiledLayer> jobDone)
        //{
        //    if (File.Exists(TileCacheFilePath(name, false, true)))
        //    {
        //        if (File.Exists(TileCacheFilePath(name, true, true)))
        //            File.Delete(TileCacheFilePath(name, true, true));
        //        File.Move(TileCacheFilePath(name, false, true), TileCacheFilePath(name, true, true));
        //    }

        //    if (File.Exists(TileCacheFilePath(name, true, true)))
        //    {
        //        try
        //        {
        //            // Create the corresponding layer based on the tile cache
        //            var old = new ArcGISTiledLayer(new TileCache(TileCacheFilePath(name, true, true)));
        //            updateProgress?.Invoke(1.0, 1.0);
        //            jobDone?.Invoke(null, old);
        //            updateProgress = null;
        //            jobDone = null;
        //        }
        //        catch (Exception ex)
        //        {
        //            jobDone?.Invoke(ex.ToString(), null);
        //        }

        //        if (syncType == CacheSyncTypes.AdminSync && File.GetLastWriteTime(TileCacheFilePath(name, true, true)) >= syncDate)
        //            return;
        //    }

        //    if (syncType == CacheSyncTypes.NeverSync)
        //        return;
        //}

        //public static async void SyncService(CacheSyncTypes syncType, DateTime syncDate, String name, Uri serviceUri, Esri.ArcGISRuntime.Geometry.Envelope extent, Action<double, double> updateProgress, Action<String, Geodatabase> jobDone)
        //{
        //    if(File.Exists(GeodatabaseFilePath(name, false, true)))
        //    {
        //        if (File.Exists(GeodatabaseFilePath(name, true, true)))
        //            File.Delete(GeodatabaseFilePath(name, true, true));
        //        File.Move(GeodatabaseFilePath(name, false, true), GeodatabaseFilePath(name, true, true));

        //        ////add service to existing mobile geodatabase
        //        //if(_currentGeodatabase == null)
        //        //    _currentGeodatabase = await Geodatabase.OpenAsync(GeodatabaseFilePath);

        //        //// create sync parameters
        //        //var taskParameters = new SyncGeodatabaseParameters()
        //        //{
        //        //    RollbackOnFailure = true,
        //        //    GeodatabaseSyncDirection = SyncDirection.Download
        //        //};

        //        //// create a sync task with the URL of the feature service to sync
        //        //var syncTask = await GeodatabaseSyncTask.CreateAsync(serviceUri);

        //        //// create a synchronize geodatabase job, pass in the parameters and the geodatabase
        //        //SyncGeodatabaseJob syncGdbJob = syncTask.SyncGeodatabase(taskParameters, _currentGeodatabase);

        //        //syncGdbJob.JobChanged += ((object sender, EventArgs e) =>
        //        //{
        //        //    var job = sender as SyncGeodatabaseJob;

        //        //    // See if the job failed
        //        //    if (job.Status == JobStatus.Failed)
        //        //    {
        //        //        // Create a message to show the user
        //        //        string message = "Generate geodatabase job failed";

        //        //        // Show an error message (if there is one)
        //        //        if (job.Error != null)
        //        //        {
        //        //            message += ": " + job.Error.Message;
        //        //        }
        //        //        else
        //        //        {
        //        //            // If no error, show messages from the job
        //        //            var m = from msg in job.Messages select msg.Message;
        //        //            message += ": " + string.Join<string>("\n", m);
        //        //        }

        //        //        jobDone?.Invoke(message, null);
        //        //    }

        //        //    if (job.Status == JobStatus.Succeeded)
        //        //    {
        //        //        // Best practice is to unregister the geodatabase
        //        //        MobileGeodatabase.UnregisterGeodatabaseAsync(_currentGeodatabase);

        //        //        jobDone?.Invoke(null, _currentGeodatabase);
        //        //    }
        //        //});

        //        //syncGdbJob.ProgressChanged += ((object sender, EventArgs e) =>
        //        //{
        //        //    updateProgress?.Invoke(syncGdbJob.Progress, 1.0);
        //        //});

        //        //// Start the job
        //        //var result = await syncGdbJob.GetResultAsync();
        //    }

        //    if(File.Exists(GeodatabaseFilePath(name, true, true)))
        //    {
        //        try
        //        {
        //            var old = await Geodatabase.OpenAsync(GeodatabaseFilePath(name, true, true));
        //            updateProgress?.Invoke(1.0, 1.0);
        //            jobDone?.Invoke(null, old);
        //            updateProgress = null;
        //            jobDone = null;
        //        }
        //        catch (Exception ex)
        //        {
        //            jobDone?.Invoke(ex.ToString(), null);
        //        }
        //        if (syncType == CacheSyncTypes.AdminSync && File.GetLastWriteTime(GeodatabaseFilePath(name, true, true)) >= syncDate)
        //            return;
        //    }

        //    if (syncType == CacheSyncTypes.NeverSync)
        //        return;

        //    // Create a task for generating a geodatabase (GeodatabaseSyncTask)
        //    var gdbSyncTask = await GeodatabaseSyncTask.CreateAsync(serviceUri);

        //    // Get the default parameters for the generate geodatabase task
        //    GenerateGeodatabaseParameters generateParams = await gdbSyncTask.CreateDefaultGenerateGeodatabaseParametersAsync(extent);

        //    // Create a generate geodatabase job
        //    _generateGdbJob = gdbSyncTask.GenerateGeodatabase(generateParams, GeodatabaseFilePath(name, false, false));

        //    // Handle the job changed event
        //    _generateGdbJob.JobChanged += (async (object sender, EventArgs e) =>
        //    {
        //        var job = sender as GenerateGeodatabaseJob;

        //        // See if the job failed
        //        if (job.Status == JobStatus.Failed)
        //        {
        //            // Create a message to show the user
        //            string message = "Generate geodatabase job failed";

        //            // Show an error message (if there is one)
        //            if (job.Error != null)
        //            {
        //                message += ": " + job.Error.Message;
        //            }
        //            else
        //            {
        //                // If no error, show messages from the job
        //                var m = from msg in job.Messages select msg.Message;
        //                message += ": " + string.Join<string>("\n", m);
        //            }

        //            jobDone?.Invoke(message, null);
        //        }

        //        if(job.Status == JobStatus.Succeeded)
        //        {
        //            // Get the new geodatabase
        //            var _currentGeodatabase = await job.GetResultAsync();

        //            // Best practice is to unregister the geodatabase
        //            await gdbSyncTask.UnregisterGeodatabaseAsync(_currentGeodatabase);
        //            _currentGeodatabase.Close();

        //            File.Move(GeodatabaseFilePath(name, false, false), GeodatabaseFilePath(name, false, true));
        //            var old = await Geodatabase.OpenAsync(GeodatabaseFilePath(name, false, true));

        //            jobDone?.Invoke(null, old);
        //        }
        //    });

        //    // Handle the progress changed event (to show progress bar)
        //    _generateGdbJob.ProgressChanged += ((object sender, EventArgs e) =>
        //    {
        //        updateProgress?.Invoke(_generateGdbJob.Progress, 1.0);
        //    });

        //    // Start the job
        //    _generateGdbJob.Start();
        //}

        //public static void Sync()
        //{

        //    // Create parameters for the sync task
        //    SyncGeodatabaseParameters parameters = new SyncGeodatabaseParameters()
        //    {
        //        GeodatabaseSyncDirection = SyncDirection.Bidirectional,
        //        RollbackOnFailure = false
        //    };

        //    // Get the layer Id for each feature table in the geodatabase, then add to the sync job
        //    foreach (GeodatabaseFeatureTable table in _currentGeodatabase.GeodatabaseFeatureTables)
        //    {
        //        // Get the ID for the layer
        //        long id = table.ServiceLayerId;

        //        // Create the SyncLayerOption
        //        SyncLayerOption option = new SyncLayerOption(id);

        //        // Add the option
        //        parameters.LayerOptions.Add(option);
        //    }

        //    // Create job
        //    SyncGeodatabaseJob job = _gdbSyncTask.SyncGeodatabase(parameters, _currentGeodatabase);

        //    // Subscribe to status updates
        //    //job.JobChanged += Job_JobChanged;

        //    //// Subscribe to progress updates
        //    //job.ProgressChanged += Job_ProgressChanged;

        //    // Start the sync
        //    job.Start();
        //}
    }
}
