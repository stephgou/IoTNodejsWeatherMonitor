using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace WeatherStation.Notifications
{
    /// <summary>
    ///     Represents a launch argument in toast notification
    /// </summary>
    public class NotificationArgs
    {
        public const string ToastType = "toast";
        public const string HumidityAlertType = "humidity";
        public const string TemperatureAlertType = "temperature";

        /// <summary>
        ///     Gets or sets the type : toast
        /// </summary>
        [JsonProperty(PropertyName="type")]
        public string Type { get; set; }

        /// <summary>
        ///     Gets or sets the alert type : humidity or temperature
        /// </summary>
        [JsonProperty(PropertyName = "alertType")]
        public string AlertType { get; set; }

        /// <summary>
        ///     Gets or sets the alert value
        /// </summary>
        [JsonProperty(PropertyName = "alertValue")]
        public string AlertValue { get; set; }

        /// <summary>
        ///     Creates an instance of NotificationArgs from its JSON representation
        /// </summary>
        /// <param name="jsonPayload">The JSON payload to deserialize</param>
        /// <returns></returns>
        public static NotificationArgs FromJson(string jsonPayload)
        {
            try
            {
                return JsonConvert.DeserializeObject<NotificationArgs>(jsonPayload);
            }
            catch(JsonException)
            {
                return null;
            }
        }
    }
}
