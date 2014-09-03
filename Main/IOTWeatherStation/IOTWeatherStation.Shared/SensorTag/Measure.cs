using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WeatherStation.SensorTag
{
    /// <summary>
    ///     Represents a sensor measure
    /// </summary>
    public class Measure
    {
        [JsonProperty(PropertyName = "utcDate")]
        public DateTime UtcDate { get; set; }

        [JsonProperty(PropertyName = "humidity")]
        public double Humidity { get; set; }

        [JsonProperty(PropertyName = "temperature")]
        public double Temperature { get; set; }
    }
}
