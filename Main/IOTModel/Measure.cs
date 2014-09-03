using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace IOTModel
{
    public class Measure
    {
        [JsonProperty(PropertyName = "utcDate")]
        public DateTime UtcDate { get; set; }

        [JsonProperty(PropertyName = "humidity")]
        public double Humidity { get; set; }

        [JsonProperty(PropertyName = "temperature")]
        public double Temperature { get; set; }

        private static double GetRandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }
        public Measure BuildMeteoMessage()
        {
            return new Measure() 
            { UtcDate = DateTime.UtcNow, Humidity = GetRandomNumber(75, 90), Temperature = GetRandomNumber(22, 25) };         
        }
    }
}
