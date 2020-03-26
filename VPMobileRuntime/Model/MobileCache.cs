using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VPMobileRuntime100_1_0.Model
{
    public static class MobileCache
    {
        private static Regex _mapServerNameRegex;

        public static String CachePath
        {
            get
            {
                return Path.Combine(Assembly.GetExecutingAssembly().Location, "Caches");
            }
        }

        private static String PullArcgisServiceName(String serviceUrl)
        {
            if (_mapServerNameRegex == null)
                _mapServerNameRegex = new Regex("/([^/]*)/[^/]*(?:/[^/]*)?$", RegexOptions.Compiled & RegexOptions.IgnoreCase);
            var mtch = _mapServerNameRegex.Match(serviceUrl);
            if (!mtch.Success)
                return null;
            return mtch.Groups[1].Value;
        }

        public static async Task<TileCache> DownloadOrUpdateTileService(String serviceUrl, Extent extent, Action<double, double> ProgressUpdate)
        {
            var task = new TaskCompletionSource<TileCache>();
            // Create a task for generating a geodatabase (GeodatabaseSyncTask)
            var etcTask = await ExportTileCacheTask.CreateAsync(new Uri(serviceUrl));
            var serviceName = PullArcgisServiceName(serviceUrl);
            var tileCacheFilePath = Path.Combine(CachePath, serviceName + ".tpk");

            if (File.Exists(tileCacheFilePath))
            {
                // need to figure out how to update a tile cache

                TileCache tileCache = new TileCache(tileCacheFilePath);
                task.SetResult(tileCache);
            }
            else
            {
                var envelope = new Envelope(extent.MinX, extent.MinY, extent.MaxX, extent.MaxY, SpatialReference.Create(extent.WKID));
                // Get the default parameters for the generate geodatabase task
                var etcParams = await etcTask.CreateDefaultExportTileCacheParametersAsync(envelope, 0, 0);

                // Create a generate geodatabase job
                var etcJob = etcTask.ExportTileCache(etcParams, tileCacheFilePath);
                // Handle the job changed event
                etcJob.JobChanged += (async (object sender, EventArgs e) =>
                {
                    var job = sender as ExportTileCacheJob;
                    switch(job?.Status)
                    {
                        case JobStatus.Succeeded:
                            TileCache tileCache = await etcJob.GetResultAsync();
                            
                            task.SetResult(tileCache);
                            break;
                        case JobStatus.Failed:
                            if (etcJob.Error != null)
                            {
                                task.SetException(etcJob.Error);
                            }
                            else
                            {
                                String message = string.Empty;

                                var m = from msg in etcJob.Messages select msg.Message;
                                message += ": " + string.Join<string>("\n", m);
                                task.SetException(new Exception(message));
                            }
                            task.SetResult(null);
                            break;
                    }
                });

                // Handle the progress changed event (to show progress bar)
                etcJob.ProgressChanged += ((object sender, EventArgs e) =>
                {
                    ProgressUpdate?.Invoke(etcJob.Progress, 1.0);
                });

                // Start the job
                etcJob.Start();
            }
            return await task.Task;
        }

        public static async Task<Geodatabase> DownloadOrUpdateFeatureService(String serviceUrl, Extent extent, Action<double, double> ProgressUpdate)
        {
            var task = new TaskCompletionSource<Geodatabase>();
            var syncTask = await GeodatabaseSyncTask.CreateAsync(new Uri(serviceUrl));
            var serviceName = PullArcgisServiceName(serviceUrl);
            var geodatabaseFilePath = Path.Combine(CachePath, serviceName + ".gdb");
            Geodatabase ret;
            if (File.Exists(geodatabaseFilePath))
            {
                //add service to existing mobile geodatabase
                ret = await Geodatabase.OpenAsync(geodatabaseFilePath);

                // create sync parameters
                var taskParameters = new SyncGeodatabaseParameters()
                {
                    RollbackOnFailure = true,
                    GeodatabaseSyncDirection = SyncDirection.Download
                };

                // create a synchronize geodatabase job, pass in the parameters and the geodatabase
                SyncGeodatabaseJob syncGdbJob = syncTask.SyncGeodatabase(taskParameters, ret);

                syncGdbJob.JobChanged += (async(object sender, EventArgs e) =>
                {
                    var job = sender as SyncGeodatabaseJob;
                    switch (job?.Status)
                    {
                        case JobStatus.Succeeded:
                            await syncTask.UnregisterGeodatabaseAsync(ret);

                            task.SetResult(ret);
                            break;
                        case JobStatus.Failed:
                            if (job.Error != null)
                            {
                                task.SetException(job.Error);
                            }
                            else
                            {
                                String message = string.Empty;

                                var m = from msg in job.Messages select msg.Message;
                                message += ": " + string.Join<string>("\n", m);
                                task.SetException(new Exception(message));
                            }
                            task.SetResult(null);
                            break;
                    }
                });

                syncGdbJob.ProgressChanged += ((object sender, EventArgs e) =>
                {
                    ProgressUpdate?.Invoke(syncGdbJob.Progress, 1.0);
                });

                // Start the job
                var result = await syncGdbJob.GetResultAsync();
            }
            else
            {
                var envelope = new Envelope(extent.MinX, extent.MinY, extent.MaxX, extent.MaxY, SpatialReference.Create(extent.WKID));
                // Get the default parameters for the generate geodatabase task
                GenerateGeodatabaseParameters generateParams = await syncTask.CreateDefaultGenerateGeodatabaseParametersAsync(envelope);

                // Create a generate geodatabase job
                var generateGdbJob = syncTask.GenerateGeodatabase(generateParams, geodatabaseFilePath);

                // Handle the job changed event
                generateGdbJob.JobChanged += (async (object sender, EventArgs e) =>
                {
                    var job = sender as GenerateGeodatabaseJob;
                    switch (job?.Status)
                    {
                        case JobStatus.Succeeded:
                            ret = await job.GetResultAsync();

                            await syncTask.UnregisterGeodatabaseAsync(ret);

                            task.SetResult(ret);
                            break;
                        case JobStatus.Failed:
                            if (job.Error != null)
                            {
                                task.SetException(job.Error);
                            }
                            else
                            {
                                String message = string.Empty;

                                var m = from msg in job.Messages select msg.Message;
                                message += ": " + string.Join<string>("\n", m);
                                task.SetException(new Exception(message));
                            }
                            task.SetResult(null);
                            break;
                    }
                });

                // Handle the progress changed event (to show progress bar)
                generateGdbJob.ProgressChanged += ((object sender, EventArgs e) =>
                {
                    ProgressUpdate?.Invoke(generateGdbJob.Progress, 1.0);
                });

                // Start the job
                generateGdbJob.Start();
            }
            return await task.Task;
        }
    }
}
