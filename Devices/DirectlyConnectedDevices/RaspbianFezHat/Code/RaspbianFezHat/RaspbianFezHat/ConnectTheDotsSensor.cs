namespace RaspbianFezHat
{
    using Newtonsoft.Json;

    /// <summary>
    /// Class to manage sensor data and attributes 
    /// </summary>
    public class ConnectTheDotsSensor
    {
        /// <summary>
        /// Default parameterless constructor needed for serialization of the objects of this class
        /// </summary>
        public ConnectTheDotsSensor()
        {
        }

        /// <summary>
        /// Construtor taking parameters guid, measurename and unitofmeasure
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="measurename"></param>
        /// <param name="unitofmeasure"></param>
        public ConnectTheDotsSensor(string guid, string measurename, string unitofmeasure)
        {
            this.GUID = guid;
            this.MeasureName = measurename;
            this.UnitOfMeasure = unitofmeasure;
        }

        [JsonProperty("guid")]
        public string GUID { get; set; }

        [JsonProperty("displayname")]
        public string DisplayName { get; set; }

        [JsonProperty("organization")]
        public string Organization { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("measurename")]
        public string MeasureName { get; set; }

        [JsonProperty("unitofmeasure")]
        public string UnitOfMeasure { get; set; }

        [JsonProperty("timecreated")]
        public string TimeCreated { get; set; }

        [JsonProperty("value")]
        public double Value { get; set; }

        /// <summary>
        /// ToJson function is used to convert sensor data into a JSON string to be sent to Azure Event Hub
        /// </summary>
        /// <returns>JSon String containing all info for sensor data</returns>
        public string ToJson()
        {
            string json = JsonConvert.SerializeObject(this);

            return json;
        }
    }
}
