using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;

namespace WeatherStation.SensorTag
{
    public class DataProvider
    {
        private static readonly string TISensorId = "TI BLE Sensor Tag";
        private static readonly object DataLockSync = new object();

        private static readonly Guid HumidityServiceUUID = Guid.Parse("F000AA20-0451-4000-B000-000000000000");
        private static readonly Guid HumidityCharacUUID = Guid.Parse("F000AA21-0451-4000-B000-000000000000");
        private static readonly Guid HumidityConfigUUID = Guid.Parse("F000AA22-0451-4000-B000-000000000000");

        private static readonly Guid TemperatureServiceUUID = Guid.Parse("F000AA00-0451-4000-B000-000000000000");
        private static readonly Guid TemperatureCharacUUID  = Guid.Parse("F000AA01-0451-4000-B000-000000000000");
        private static readonly Guid TemperatureConfigUUID = Guid.Parse("F000AA02-0451-4000-B000-000000000000");

        private readonly bool _useFakeDataIfJeromeBonaldiEffect;
        private readonly Timer _dataTimer;
        private Measure _lastMeasure; 
        private DateTime _lastTouch;
        private bool _capturing;

        private GattCharacteristic _temperatureCharacteristic;
        private GattCharacteristic _temperatureConfiguration;

        private GattCharacteristic _humidityCharacteristic;
        private GattCharacteristic _humidityConfiguration;

        /// <summary>
        ///     Events fired when a new measure is available
        /// </summary>
        public event EventHandler<MeasureEventArgs> NewMeasureAvailable;

        public DataProvider(bool useFakeDataIfJeromeBonaldiEffect = false)
        {
            _useFakeDataIfJeromeBonaldiEffect = useFakeDataIfJeromeBonaldiEffect;
            _dataTimer = new Timer(OnDataTimerTick, null, -1, 5000);
        }

        public async Task InitializeSensorAsync()
        {
            if (!_useFakeDataIfJeromeBonaldiEffect)
            {
                var tasks = new Task[2];
                tasks[0] = InitializeHumiditySensorAsync();
                tasks[1] = InitializeTemperatureSensorAsync();

                await Task.WhenAll(tasks);
            }

            _capturing = true;
            _lastTouch = DateTime.UtcNow;
            _dataTimer.Change(0, 5000);
        }

        private async Task InitializeHumiditySensorAsync()
        {
            if (_useFakeDataIfJeromeBonaldiEffect)
                return;

            var devices = await DeviceInformation.FindAllAsync(GattDeviceService.GetDeviceSelectorFromUuid(HumidityServiceUUID));
            var device = devices.FirstOrDefault(d => d.Name == TISensorId);

            if(device == null)
            {
                throw new InvalidOperationException("No humidity device found");
            }

            var service = await GattDeviceService.FromIdAsync(device.Id);

            if (service == null)
                return;

            _humidityCharacteristic = service.GetCharacteristics(HumidityCharacUUID)[0];
            _humidityCharacteristic.ValueChanged += OnHumidityValueChanged;

            await _humidityCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);

            _humidityConfiguration = service.GetCharacteristics(HumidityConfigUUID)[0];
            await _humidityConfiguration.WriteValueAsync((new byte[] { 1 }).AsBuffer());
        }

        private async void OnHumidityValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            int a = await ShortUnsignedAtOffset(sender, 2);
            a = a - (a % 4);
            double humidityValue = ((-6f) + 125f * (a / 65535f));

            lock (DataLockSync)
            {
                if (_lastMeasure == null)
                {
                    _lastMeasure = new Measure();
                }

                _lastMeasure.Humidity = humidityValue;
            }
        }

        private async Task InitializeTemperatureSensorAsync()
        {
            var devices = await DeviceInformation.FindAllAsync(GattDeviceService.GetDeviceSelectorFromUuid(TemperatureServiceUUID));
            var device = devices.FirstOrDefault(d => d.Name == TISensorId);

            if (device == null)
            {
                throw new InvalidOperationException("No temperature device found");
            }

            var service = await GattDeviceService.FromIdAsync(device.Id);

            if (service == null)
                return;

            _temperatureCharacteristic = service.GetCharacteristics(TemperatureCharacUUID)[0];
            _temperatureCharacteristic.ValueChanged += OnTemperatureValueChanged;

            await _temperatureCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);

            _temperatureConfiguration = service.GetCharacteristics(TemperatureConfigUUID)[0];
            await _temperatureConfiguration.WriteValueAsync((new byte[] { 1 }).AsBuffer());
        }

        private async void OnTemperatureValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            int a = await ShortUnsignedAtOffset(sender, 2);
            double temperatureValue = a / 128.0;

            lock (DataLockSync)
            {
                if(_lastMeasure == null)
                {
                    _lastMeasure = new Measure();
                }

                _lastMeasure.Temperature = temperatureValue;
            }
        }

        private static async Task<int> ShortUnsignedAtOffset(GattCharacteristic characteristic, int offset)
        {
            var values = (await characteristic.ReadValueAsync()).Value.ToArray();
            uint lowerByte = values[offset];
            uint upperByte = values[offset + 1]; ; // Note: interpret MSB as signed.

            var longValue = (upperByte << 8) + lowerByte;
            return Convert.ToInt32(longValue);
        }

        private async void OnDataTimerTick(object state)
        {
            if (!_capturing)
                return;

            if(_useFakeDataIfJeromeBonaldiEffect)
            {
                _lastMeasure = new Measure();
                _lastMeasure.UtcDate = DateTime.UtcNow;
                _lastMeasure.Humidity = GetRandomNumber(70, 100);
                _lastMeasure.Temperature = GetRandomNumber(20, 25);

                if(NewMeasureAvailable != null)
                {
                    NewMeasureAvailable(this, new MeasureEventArgs(_lastMeasure));
                    _lastMeasure = null;
                }

                return;
            }

            if (_lastMeasure == null || _lastMeasure.Humidity == 0 || _lastMeasure.Temperature == 0)
            {
                if (DateTime.UtcNow >= _lastTouch.AddSeconds(10))
                {
                    ////await _temperatureCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                    //await _temperatureConfiguration.WriteValueAsync((new byte[] { 1 }).AsBuffer());

                    ////await _humidityCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                    //await _humidityConfiguration.WriteValueAsync((new byte[] { 1 }).AsBuffer());

                    //_lastTouch = DateTime.Now;
                    await StopSensorMeasureAsync();
                    await InitializeSensorAsync();
                }

                return;
            }

            lock (DataLockSync)
            {
                _lastMeasure.UtcDate = DateTime.UtcNow;
                _lastTouch = DateTime.UtcNow;

                if (NewMeasureAvailable != null)
                {
                    NewMeasureAvailable(this, new MeasureEventArgs(_lastMeasure));
                    _lastMeasure = null;
                }
            }
        }

        private static double GetRandomNumber(double minimum, double maximum)
        {
            Random random = new Random();
            return random.NextDouble() * (maximum - minimum) + minimum;
        }

        public async Task StopSensorMeasureAsync()
        {
            _capturing = false;
            _dataTimer.Change(-1, 5000);

            //if (_temperatureCharacteristic != null)
            //{
            //    _temperatureCharacteristic.ValueChanged -= OnTemperatureValueChanged;
            //    await _temperatureCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
            //}

            //if(_humidityCharacteristic != null)
            //{
            //    _humidityCharacteristic.ValueChanged -= OnHumidityValueChanged;
            //    await _humidityCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
            //}
        }
    }
}
