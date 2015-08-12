//  ---------------------------------------------------------------------------------
//  Copyright (c) Microsoft Open Technologies, Inc.  All rights reserved.
// 
//  The MIT License (MIT)
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//  ---------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel.Web;
using System.Text;
using System.Web.Http;
using Historical;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;


namespace ConnectTheDotsWebSite
{
    public class HistoricalController : ApiController
    {
        [HttpGet]
        [AcceptVerbs()] 
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/GetAllProducts?start={start}&intervalMins={intervalMins}")]
        public IEnumerable<IDictionary<string, object>> GetAllProducts(DateTime start, int intervalMins)
        {
            var res = GetIntervalData(start, intervalMins);
            return res;
        }

        [HttpGet]
        [AcceptVerbs()]
        [WebGet(BodyStyle = WebMessageBodyStyle.Bare, UriTemplate = "/GetAllProducts?start={start}&intervalMins={intervalMins}")]
        public IEnumerable<IDictionary<string, object>> GetLast(int intervalMins)
        {
            var res = GetLastData(intervalMins);
            return res;
        }
        public class CloudBlockCompareByDate : IComparer<CloudBlockBlob>
        {
            int IComparer<CloudBlockBlob>.Compare(CloudBlockBlob x, CloudBlockBlob y)
            {
                if (x.Properties.LastModified == y.Properties.LastModified) return 0;
                return (x.Properties.LastModified < y.Properties.LastModified) ? -1 : 1;
            }
        }

        private IEnumerable<IDictionary<string, object>> GetIntervalData(DateTime start, int intervalMins)
        {
            const int MIN_READ_SIZE = 2 * 16384;

            var sw = new Stopwatch();
            sw.Start();
            long responseTime;

            string containerName = SelectBlobContainerByTimeInterval(intervalMins);
            CloudBlobContainer startCloudBlobContainer = SetUpContainer(Global.eventHubDevicesSettings.storageConnectionString, containerName);

            start = start.ToUniversalTime();
            DateTime end = start.AddMinutes(intervalMins);
            
            var res = new List<IDictionary<string, object>>();

            CloudBlockBlob[] allBlobs = startCloudBlobContainer.ListBlobs(null, true, BlobListingDetails.Copy).OfType<CloudBlockBlob>().ToArray();
            Array.Sort(allBlobs, new CloudBlockCompareByDate());

            for (int currentBlobIndex = 0, blobsViewed = 0; currentBlobIndex < allBlobs.Length; ++currentBlobIndex)
            {
                CloudBlockBlob blob = allBlobs[currentBlobIndex];
                if (blob.Properties.LastModified != null && blob.Properties.LastModified.Value.ToUniversalTime() < start)
                {
                    continue;
                }

                if (blobsViewed == 2)
                {
                    break;
                }
                blobsViewed++;

                if (currentBlobIndex == allBlobs.Length - 1)
                {
                    try
                    {
                        //blob.Delete(DeleteSnapshotsOption.DeleteSnapshotsOnly);
                        blob = blob.CreateSnapshot();    
                    }
                    catch (Exception) { }
                }

                blob.StreamMinimumReadSizeInBytes = MIN_READ_SIZE;
                Stream blobStream = blob.OpenRead();

                IDictionary<string, object> messagePayload;

                long maxPos = (blob.Properties.Length - 1) / MIN_READ_SIZE + 1, minPos = 0, resPos = -1;
                char []buffer = new char[MIN_READ_SIZE];

                StreamReader reader;
                if (blobsViewed == 2)
                {
                    reader = new StreamReader(blobStream, Encoding.UTF8);
                    var firstMessageJSON = reader.ReadLine();

                    messagePayload = PrepareMessagePayload(firstMessageJSON);
                    DateTime firstMessageTime = DateTime.Parse(messagePayload["time"].ToString()).ToUniversalTime();
                    //if (firstMessageTime < end)
                    //{
                    //    continue;
                    //}
                    //TODO: delete firstMessageTime conditions when blob will be fully sorted
                    if(firstMessageTime >= start)
                        maxPos = minPos;
                }

                for (; maxPos - minPos > 1; )
                {
                    long curPos = (maxPos + minPos) / 2;
                    blobStream.Seek(curPos * MIN_READ_SIZE, SeekOrigin.Begin);

                    try
                    {
                        reader = new StreamReader(blobStream, Encoding.UTF8);
                        reader.Read(buffer, 0, MIN_READ_SIZE);

                        string jsonData = new string(buffer);
                        
                        string[] values = jsonData.Split(new char[] { '\n' });
                        int index = LookForDateInString(values, start);

                        if (index < 0)
                        {
                            minPos = curPos;
                        }
                        else
                        {
                            if (index <= values[0].Length)
                                maxPos = curPos;
                            else
                            {
                                resPos = index;
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        break;
                        //minPos = curPos;
                    }
                }
                if (resPos >= 0)
                {
                    minPos = ((minPos + maxPos)/2)*MIN_READ_SIZE + resPos;
                }
                else minPos *= MIN_READ_SIZE;
                long binsearchTime = sw.ElapsedMilliseconds;
                blob.StreamMinimumReadSizeInBytes = 16384*4;
                reader = new StreamReader(blobStream, Encoding.UTF8);
                blobStream.Seek(minPos, SeekOrigin.Begin);
                string responseJSON = reader.ReadLine();
                for (; ; )
                {
                    try
                    {
                        responseJSON = reader.ReadLine();
                        messagePayload = PrepareMessagePayload(responseJSON);
                        DateTime time = DateTime.Parse(messagePayload["time"].ToString()).ToUniversalTime();
                        if (time > end)
                        {
                            break;
                        }
                        if (res.Count >= HistoricalConstants.MAX_RESULT_SIZE)
                        {
                            responseTime = sw.ElapsedMilliseconds;
                            return res;
                        }
                        if(time >= start)
                            res.Add(messagePayload);
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
                //blob.Delete(DeleteSnapshotsOption.DeleteSnapshotsOnly);
            }
            responseTime = sw.ElapsedMilliseconds;
            return res;
        }

        private string SelectBlobContainerByTimeInterval(int intervalMins)
        {
            for (int i = HistoricalConstants.TUMBLING_WINDOW_SIZES_SEC.Length - 1; i >= 1; --i)
            {
                double dots = 60 * intervalMins / (double)HistoricalConstants.TUMBLING_WINDOW_SIZES_SEC[i];
                if (dots >= HistoricalConstants.MAX_RESULT_SIZE) return HistoricalConstants.BLOB_CONTAINER_NAME_PREFIX + HistoricalConstants.TUMBLING_WINDOW_SIZES_SEC[i];
            }
            return HistoricalConstants.BLOB_CONTAINER_NAME_PREFIX + HistoricalConstants.TUMBLING_WINDOW_SIZES_SEC[0];
        }

        private IEnumerable<IDictionary<string, object>> GetLastData(int intervalMins)
        {
            const int MIN_READ_SIZE = 8 * 16384;
            var sw = new Stopwatch();
            sw.Start();
            long responseTime;

            DateTime end = DateTime.Now.ToUniversalTime();
            DateTime start = end.AddMinutes(-intervalMins);//.ToUniversalTime();

            string containerName = SelectBlobContainerByTimeInterval(intervalMins);
            if (Global.lastHistoricalData.ContainsKey(containerName) &&
                Global.lastHistoricalData[containerName].Key > end.AddMinutes(-2))
            {
                return Global.lastHistoricalData[containerName].Value;
            }

            CloudBlobContainer startCloudBlobContainer = SetUpContainer(Global.eventHubDevicesSettings.storageConnectionString, containerName);

            var res = new List<IDictionary<string, object>>();

            CloudBlockBlob[] allBlobs = startCloudBlobContainer.ListBlobs(null, true, BlobListingDetails.Copy).OfType<CloudBlockBlob>().ToArray();
            Array.Sort(allBlobs, new CloudBlockCompareByDate());

            for (int currentBlobIndex = allBlobs.Length - 1; currentBlobIndex >= 0; --currentBlobIndex)
            {
                CloudBlockBlob blob = allBlobs[currentBlobIndex];
                
                if (blob.Properties.LastModified != null && blob.Properties.LastModified.Value.ToUniversalTime() < start)
                {
                    break;
                }

                if (currentBlobIndex == allBlobs.Length - 1)
                {
                    try
                    {
                        //blob.Delete(DeleteSnapshotsOption.DeleteSnapshotsOnly);
                        blob = blob.CreateSnapshot();
                    }
                    catch (Exception) { }
                    
                }

                blob.StreamMinimumReadSizeInBytes = MIN_READ_SIZE;
                Stream blobStream = blob.OpenRead();

                char[] buffer = new char[MIN_READ_SIZE];

                long maxLoadsCount = blob.Properties.Length/MIN_READ_SIZE + (blob.Properties.Length % MIN_READ_SIZE == 0 ? 0 : 1);

                for (long loadsCount = 1; loadsCount <= maxLoadsCount; loadsCount++)
                {
                    long curPos = Math.Max(blob.Properties.Length - loadsCount * MIN_READ_SIZE, 0);
                    blobStream.Seek(curPos, SeekOrigin.Begin);
                    bool wasLastLoad = false;

                    try
                    {
                        StreamReader reader = new StreamReader(blobStream, Encoding.UTF8);
                        reader.Read(buffer, 0, MIN_READ_SIZE);
                        
                        string jsonData = new string(buffer);

                        string[] values = jsonData.Split(new char[] { '\n' });

                        if (values.Length >= 3)
                        {
                            var messagePayload = PrepareMessagePayload(values[1]);
                            DateTime firstMessageTime = DateTime.Parse(messagePayload["time"].ToString());//.ToUniversalTime();

                            if (firstMessageTime < start)
                            {
                                wasLastLoad = true;
                            }

                            for(int i = 1; i < values.Length - 1; ++i)
                            {
                                try
                                {
                                    messagePayload = PrepareMessagePayload(values[i]);
                                    DateTime time = DateTime.Parse(messagePayload["time"].ToString());
                                    if (time > start)
                                        res.Add(messagePayload);
                                }
                                catch (Exception) { }
                            }
                            if (wasLastLoad)
                            {
                                responseTime = sw.ElapsedMilliseconds;
                                Global.lastHistoricalData[containerName] = new KeyValuePair<DateTime, IEnumerable<IDictionary<string, object>>>(end, res);
                                return res;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
            }
            responseTime = sw.ElapsedMilliseconds;

            Global.lastHistoricalData[containerName] = new KeyValuePair<DateTime, IEnumerable<IDictionary<string, object>>>(end, res);
            return res;
        }

        private IDictionary<string, object> PrepareMessagePayload(string json)
        {
            var messagePayload = (IDictionary<string, object>)
                            JsonConvert.DeserializeObject(json, typeof(IDictionary<string, object>));

            //messagePayload["timecreated"] = DateTime.Parse(messagePayload["timecreated"].ToString()).AddDays(30).ToString();

            if (messagePayload.ContainsKey("timecreated"))
            {
                messagePayload["time"] = messagePayload["timecreated"];
            }
            if (messagePayload.ContainsKey("timearrived"))
            {
                messagePayload["time"] = messagePayload["timearrived"];
            }
            return messagePayload;
        }

        private CloudBlobContainer SetUpContainer(string storageConnectionString,
            string containerName)
        {
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(storageConnectionString);
            CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            CloudBlobContainer cloudBlobContainer = cloudBlobClient.GetContainerReference(containerName);
            return cloudBlobContainer;
        }

        private void FillTestData(IEnumerable<IDictionary<string, object>> data)
        {
            DateTime first = DateTime.UtcNow;
            foreach (var seconds in HistoricalConstants.TUMBLING_WINDOW_SIZES_SEC)
            {
                try
                {
                    string containerName = HistoricalConstants.BLOB_CONTAINER_NAME_PREFIX + seconds;
                    CloudBlobContainer startCloudBlobContainer = SetUpContainer(Global.eventHubDevicesSettings.storageConnectionString, containerName);
                    startCloudBlobContainer.CreateIfNotExists();

                    var gtrg = startCloudBlobContainer.GetBlockBlobReference(HistoricalConstants.BLOB_NAME_PREFIX + seconds);
                    gtrg.DeleteIfExists(DeleteSnapshotsOption.IncludeSnapshots);
                    //continue;
                    Stream writes = gtrg.OpenWrite();

                    foreach (var item in data)
                    {
                        item["time"] = first;
                        item["timecreated"] = first;
                        first = first.AddSeconds(-seconds);

                        string json = JsonConvert.SerializeObject(item) + '\n';
                        var tr = System.Text.Encoding.UTF8.GetBytes(json);
                        writes.Write(tr, 0, tr.Length);
                    }
                    writes.Flush();
                    writes.Close();
                }
                catch (Exception)
                {

                }
            }
        }

        private bool CheckDateLess(string s, DateTime start)
        {
            var messagePayload = (IDictionary<string, object>)
                            JsonConvert.DeserializeObject(s, typeof(IDictionary<string, object>));

            DateTime time = DateTime.Parse(messagePayload["timecreated"].ToString()).ToUniversalTime();
            return time < start;
        }

        private int LookForDateInString(string[] values, DateTime start)
        {
            int[] sums = new int[1000];
            for (int i = 0, curLen = 0; i < values.Length; ++i)
            {
                sums[i] = curLen;
                curLen += values[i].Length;
            }

            int l = 1, r = values.Length - 2;
            for (; r - l > 1; )
            {
                int c = (l + r) / 2;

                bool val = true;
                for (; c < r; ++c)
                {
                    try
                    {
                        val = CheckDateLess(values[c], start);
                        break;
                    }
                    catch (Exception)
                    {

                    }
                }
                if (val)
                {
                    l = c;
                }
                else
                {
                    r = c;
                }
            }
            if (l > 0 && !CheckDateLess(values[l], start)) return sums[l];
            if (r > 0 && !CheckDateLess(values[r], start)) return sums[r];
            return -1;
        }
    }

}