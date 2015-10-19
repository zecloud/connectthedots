namespace RaspbianFezHat
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Timers;
    using GHIElectronics.Mono.Shields;

    public class Program
    {
        private static SimpleLogger logger = new SimpleLogger();
        private static FEZHAT hat;
        private static ConnectTheDotsHelper ctdHelper;
        private static Timer timer;

        public static void Main(string[] args)
        {
            // Hard coding guid for sensors. Not an issue for this particular application which is meant for testing and demos
            List<ConnectTheDotsSensor> sensors = new List<ConnectTheDotsSensor> {
                new ConnectTheDotsSensor("2298a348-e2f9-4438-ab23-82a3930662ab", "Light", "L"),
                new ConnectTheDotsSensor("d93ffbab-7dff-440d-a9f0-5aa091630201", "Temperature", "C"),
            };

            Program.ctdHelper = new ConnectTheDotsHelper(
                logger: Program.logger,
                serviceBusNamespace: "SERVICE_BUS_NAMESPACE",
                eventHubName: "EVENT_HUB_NAME",
                keyName: "SHARED_ACCESS_POLICY_KEY_NAME",
                key: "SHARED_ACCESS_POLICY_KEY",
                displayName: string.Empty,
                organization: "YOUR_ORGANIZATION_OR_SELF",
                location: "YOUR_LOCATION",
                eventHubMessageSubject: "Raspberry PI",
                sensorList: sensors);

            if (ctdHelper.IsConnectionReady)
            {
                // Setup the FEZ HAT driver
                Program.SetupHat();

                Console.WriteLine("Reading data from FEZ HAT sensors. Press Enter to Stop");
                Console.ReadLine();
                Console.WriteLine("Closing...");
                Program.timer.Enabled = false;
                Program.timer.Dispose();
                Program.hat.Dispose();
                Program.ctdHelper.Dispose();
            }
            else
            {
                Console.WriteLine("An error ocurred while connecting to Azure. See previous errors for details.");
                Console.WriteLine("Press Enter to exit");
                Program.ctdHelper.Dispose();
            }
        }

        private static async void SetupHat()
        {
            // Initialize Fez Hat
            logger.Info("Initializing FEZ HAT");
            hat = await FEZHAT.CreateAsync();

            // Initialize Timer
            timer = new Timer(2000); // (TimerCallback, null, 0, 500);
            timer.Elapsed += Timer_Elapsed;
            timer.Enabled = true;

            Program.logger.Info("FEZ HAT Initialized");
        }

        private static void Timer_Elapsed(object source, ElapsedEventArgs e)
        {
            // Light Sensor
            ConnectTheDotsSensor lightSensor = ctdHelper.Sensors.Find(item => item.MeasureName == "Light");
            lightSensor.Value = hat.GetLightLevel();

            Program.ctdHelper.SendSensorData(lightSensor);

            // Temperature Sensor
            var tempSensor = ctdHelper.Sensors.Find(item => item.MeasureName == "Temperature");
            tempSensor.Value = hat.GetTemperature();
            Program.ctdHelper.SendSensorData(tempSensor);

            Program.logger.Info("Temperature: {0} °C, Light {1}", tempSensor.Value.ToString("N2", CultureInfo.InvariantCulture), lightSensor.Value.ToString("P2", CultureInfo.InvariantCulture));
        }
    }
}
