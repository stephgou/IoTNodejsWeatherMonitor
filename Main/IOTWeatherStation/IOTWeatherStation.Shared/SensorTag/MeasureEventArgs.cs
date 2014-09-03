using System;
using System.Collections.Generic;
using System.Text;

namespace WeatherStation.SensorTag
{
    /// <summary>
    ///     Event args when a new data event is fired
    /// </summary>
    public class MeasureEventArgs : EventArgs
    {
        /// <summary>
        ///     Creates an instance of MeasureEventArgs
        /// </summary>
        /// <param name="measure">The measure to provide</param>
        public MeasureEventArgs(Measure measure)
        {
            Measure = measure;
        }

        /// <summary>
        ///     Gets the measure
        /// </summary>
        public Measure Measure { get; private set; }
    }
}
