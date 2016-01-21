Raspberry Pi 2 with FEZ Hat on Raspbian Hands-on Lab
========================================

ConnectTheDots will help you get tiny devices connected to Microsoft Azure, and to implement great IoT solutions taking advantage of Microsoft Azure advanced analytic services such as Azure Stream Analytics and Azure Machine Learning.

> This lab is stand-alone, but is used at Microsoft to accompany a presentation about Azure, Windows 10 IoT Core, Raspbian, and our IoT services. If you wish to follow this on your own, you are encouraged to do so. If not, consider attending a Microsoft-led IoT lab in your area.


In this lab you will use a [Raspberry Pi 2](https://www.raspberrypi.org/products/raspberry-pi-2-model-b/) device running [Raspbian](https://www.raspberrypi.org/downloads/raspbian/) and a [FEZ HAT](https://www.ghielectronics.com/catalog/product/500) sensor hat. Using an Application, the sensors get the raw data and format it into a JSON string. That string is then shuttled off to the Azure Event Hub, where it gathers the data and will be consumed in different ways, such as displaying it as a chart using Power BI.

This lab includes the following tasks:

1. [Setup Environment](#Task1)
	1. [Software](#Task11)
	2. [Devices](#Task12)
	3. [Azure Account](#Task13)
2. [Creating an Application](#Task2)
	1. [Deploying a Simple Application](#Task21)
	2. [Reading the FEZ HAT sensors](#Task22)
	3. [Connecting the Application to Azure](#Task23)
3. [Consuming the Event Hub data](#Task3)
	1. [Creating a console application to read the Azure Event Hub](#Task31)
	2. [Using Power BI](#Task32)
	3. [Consuming the Event Hub data from a Website](#Task33)
4. [Summary](#Summary)

<a name="Task1" />
##Setup##
The following sections are intended to setup your environment to be able to create and run your solutions in Raspbian.

<a name="Task11" />
###Setting up your Software###
To setup your Raspbian development PC, you first need to install the following:

- Visual Studio 2015 or above – [Community Edition](http://www.visualstudio.com/downloads/download-visual-studio-vs) is sufficient.

- SSH Client, such as [Putty](http://www.chiark.greenend.org.uk/~sgtatham/putty/download.html).

- (optional) FTP Client of your choice. You can use Windows FTP Client.

<a name="Task12" />
###Setting up your Devices###

For this project, you will need the following:

- [Raspberry Pi 2 Model B](https://www.raspberrypi.org/products/raspberry-pi-2-model-b/)
- [GHI FEZ HAT](https://www.ghielectronics.com/catalog/product/500)

To setup your devices perform the following steps:

1. Plug the **GHI FEZ HAT** into the **Raspberry Pi 2**. 

	![fezhat-connected-to-raspberri-pi-2](Images/fezhat-connected-to-raspberri-pi-2.png?raw=true)
	
	_The FEZ hat connected to the Raspberry Pi 2 device_

2. Install the Raspbian Operating System. You can follow the [installation guide](https://www.raspberrypi.org/documentation/installation/installing-images/README.md) in the [raspberrypi.org](https://www.raspberrypi.org/) site.
	1. Download the lastest version of **Raspbian** from [here](https://www.raspberrypi.org/downloads/raspbian/).
	
		> **Note:** The following steps uses the official supported version of Raspbian Based on Debian Jessie (Kernel version: 4.1). To use a different version of the operating system you might need to follow additional steps.
	
	2. Download **Win32DiskImager** from the [Sourceforge Project page](http://sourceforge.net/projects/win32diskimager/).
	3. Copy the **Raspbian** image into the SD Card using **Win32DiskImager**. You can follow the instructions [here](https://www.raspberrypi.org/documentation/installation/installing-images/windows.md) 
	
		> **Note:** If you have a display, keyboard and mouse you are ready to go and you can skip the pre-installation steps and connect your PC to the device using SSH.
		> 
		> Eject the SD Card from your PC and insert it in the Raspberry Pi. Then Connect the Raspberry Pi to a power supply, keyboard, monitor, and Ethernet cable (or Wi-Fi dongle) with an Internet connection and find the IP address hovering the mouse on the upper right corner of the desktop. Then jump to the step #6 (you may also want to take a look at step #4 where remote access is explained).
		>
		> Otherwise follow the next instructions to set a fixed IP so it can be accessed with a remote console.
	
3. Setup a fixed IP address for your device and connect it directly to your development PC.
	1. Find the drive where the SD Card is mounted in your desktop PC.
	2. Edit the **cmdline.txt** file (make a backup copy first) and add the _fixed IP address_ at the end of the line (be sure not to add extra lines).
	
		> **Note:** For network settings where the IP address is obtained automatically, use an address in the range 169.254.X.X (169.254.0.0 – 169.254.255.255). For network settings where the IP address is fixed, use an address which matches the laptop/computer's address except the last octet.

		`ip=169.254.0.2`
		
	3. Connect the Raspberry Pi to a power supply and use the Ethernet cable to connect your device and your development PC. You can do it by plugging in one end of the spare Ethernet cable to the extra Ethernet port on your PC, and the other end of the cable to the Ethernet port on your Raspberry Pi device.	
	
5. To connect to your Raspberry Pi from your Desktop computer you will need a SSH (Secure SHell) client. PuTTY will be used for this section since it's for Windows plattform and it's free and open source. If you are using Mac OSX or another operating system, choose an appropriate SSH client, or use what is installed by default.

	1. Download PuTTY from [here](http://www.chiark.greenend.org.uk/~sgtatham/putty/download.html "Download PuTTY"). Downloading the **putty.exe** file is enough.

	1. PuTTY doesn't need to be installed. Just move the **putty.exe** file to some place in your file system.
	
	1. Open PuTTY and enter your device's IP in the **Host Name** field and select **SSH** as the **Connection Type**. Click on **Open**. Leave the other fields with their default values, including the **Port**
	
		![Open PuTTY](Images/open-putty.png?raw=true)

	1. Log in using the user you configured during the setup (usually **pi**):
	
		![Raspberry Pi Login](Images/raspberry-pi-login.png?raw=true)
	
		Find more information about using PuTTY in the [PuTTY Documentation Page](http://www.chiark.greenend.org.uk/~sgtatham/putty/docs.html "PuTTY Documentation Page").
	
5. Now, to connect your device to Internet you can either set up a wireless connection or change the device name so it can be easily discoverable in your wired network:
	
	- If you have a WiFi dongle you can set up your Internet connection as a wireless network. Connect your WiFi Dongle to the same network than your development PC. Follow instructions [here](https://www.raspberrypi.org/documentation/configuration/wireless/wireless-cli.md).
		1. Open a SSH console and execute the following command:
			
			````
			$ sudo iwlist wlan0 scan
			````
			
			which will list all available WiFi networks along with other useful information.
			
			![Scan available WiFi networks](Images/scan-available-wifi-networks.png?raw=true)
			
			Look out for:
			- ESSID:"XXX". This is the name of the WiFi network. 
			- IE: IEEE 802.11i/WPA2 Version 1. This is the authentication used; in this case it is WPA2, the newer and more secure wireless standard which replaces WPA1. **This guide should work for WPA or WPA2, but may not work for WPA2 enterprise**; for WEP hex keys see the last example here <http://netbsd.gw.com/cgi-bin/man-cgi?wpa_supplicant.conf+5+NetBSD-current>.
			
			You will also need the password for the WiFi network. For most home routers this is located on a sticker on the back of the router. For Microsoft-run labs, the WiFi information for the room will be provided to you.
			
		2. To add the network details to your Raspberry Pi open the **wpa-supplicant** configuration file. Run the following command to edit the file:
		
			````
			$ sudo nano /etc/wpa_supplicant/wpa_supplicant.conf 
			````
		
			Go to the bottom of the file and add the following:
			
			````
			network={ 
				ssid="The_ESSID_from_earlier" 
				psk="Your_wifi_password" 
			}
			````

			![Editting wpa-supplicant](Images/editting-wpa-supplicant.png?raw=true)
			
		3. Save the file by pressing **ctrl+x** then **y**, then finally press **Enter**. 
		
			At this point, wpa-supplicant will normally notice a change has occurred within a few seconds, and it will try and connect to the network. If it does not, either manually restart the interface with `sudo ifdown wlan0` and `sudo ifup wlan0`, or reboot your Raspberry Pi with `sudo reboot`. 
			
		4. Verify if it has successfully connected using `ifconfig wlan0`. If the **inet addr** field has an address beside it, the Raspberry Pi has connected to the network. If not, check your password and ESSID are correct
		
			![Verify wireless connection](Images/verify-wireless-connection.png?raw=true)
		
	- If you don't have a WiFi dongle, you can rename your device so it can be easily discoverable through your wired network.
		1. With the device connected to your desktop computer through the Ethernet cable, run the Raspberry Pi Software Configuration tool. Open a remote SSH console and type:
		
			````
			$ sudo raspi-config
			````		
		
			Note: start by extending the size of the partition in the main menu
		2. Select **Advanced Options** and hit Enter
		
			![Raspi-Config Menu](Images/raspi-config-menu.png?raw=true)
			
		3. Select **Host Name** and hit Enter
		
			![Raspi-Config Advanced Options](Images/raspi-config-advanced-options.png?raw=true)
			
		4. Dismiss the hostname label warning message by hitting Enter and Edit your device name:
		
			![Raspi-Config Edit the Hostname](Images/raspi-config-edit-the-hostname.png?raw=true)
		
		5. Once you're done, hit Enter to confirm the change. The tool will take you to the Raspi-Config home screen. Hit **Finish** to save the changes (Use the Tab key to move to the buttons section).
		
		6. Select **Yes** in the following screen to reboot the device
		
			![Raspi-Config Reboot message](Images/raspi-config-reboot-message.png?raw=true)
			
		7. After rebooting disconnect your ethernet cable from your desktop PC and plug it into your wired network.
		
		8. Open a new SSH Connection using the name you just configured as hostname to check that everything is working as expected:
		
			![Connect to MyRaspberryPi](Images/connect-to-myraspberrypi.png?raw=true)


9. Install the **Mono Framework** as stated [here](http://www.raspberry-sharp.org/eric-bezine/2012/10/mono-framework/installing-mono-raspberry-pi/). To do this, run the following commands.

	````
	$ sudo apt-get update
	$ sudo apt-get install mono-complete
	````
10.
		Get FileZilla Here on your development computer https://filezilla-project.org/ 

<a name="Task13" />
###Setting up your Azure Account###
You will need a Microsoft Azure subscription ([free trial subscription] (http://azure.microsoft.com/en-us/pricing/free-trial/) is sufficient)

####Creating an Event Hub and a Shared Access Policy####

1. Enter the Azure portal, by browsing to http://manage.windowsazure.com
2. Create a new Event Hub. To do this, click **NEW**, then click **APP SERVICES**, then click **SERVICE BUS**, then **EVENT HUB**, and finally click **CUSTOM CREATE**.

	![creating a new app service](Images/creating a new app service.png?raw=true)
	
	_Creating a new Event Hub_

3. In the **Add a new Event Hub** screen, enter an **EVENT HUB NAME** e.g. _Raspbian_, select a **REGION** such as _West US_, and enter a **NAMESPACE NAME**.

	![creating event hub](Images/creating event hub.png?raw=true)
	
	_New Event Hub Settings_

	> **Note:** If you already have a service bus namespace on your suscription, you can select that one in this step.
	
4. Write down the **EventHub Name** and **Event Hub Namespace Name** as yout will use these values later. Click the next arrow to continue.
5. In the **Configure Event Hub** screen, type _4_ in the **Partition Count** field; and in the **Message Retention** textbox, type _1_. Click the checkmark to continue.

	![configuring event hub](Images/configuring event hub.png?raw=true)
	
	_Configuring Event Hub_

6. Now, you will create a **Shared Access Policy**. To do this, after the service is activated, click on the newly created namespace. Then click the **Event Hubs** tab, and click the recently created service bus.
7. Click the **Configure** tab. In the **shared access policies** section, enter a **name** for the new policy, such as _manage_, in the **permissions** column, select **manage**.

	![creating a SAS](Images/creating a SAS.png?raw=true)

	_Creating a Shared Access Policy_
	
####Creating a Stream Analitycs Job####

To create a Stream Analytics Job, perform the following steps.

1. In the Azure Management Portal, click **NEW**, then click **DATA SERVICES**, then **STREAM ANALYTICS**, and finally **QUICK CREATE**.
2. Enter the **Job Name**, select a **Region**, such as _Central US_; and the enter a **NEW STORAGE ACCOUNT NAME** if this is the first storage account in the region used for Stream Analitycs; if not you have to select the one already used for that matter.
3. Click **CREATE A STREAM ANALITYCS JOB**.

	![Creating a Stream Analytics Job](Images/createStreamAnalytics.png?raw=true)
	
	_Creating a Stream Analytics Job_

	
4. After the _Stream Analytics_ is created, in the left pane click **STORAGE**, then click the account you used in the previous step, and click **MANAGE ACCESS KEYS**. Write down the **STORAGE ACCOUNT NAME** and the **PRIMARY ACCESS KEY** as you will use those value later.
	
	![manage access keys](Images/manage-access-keys.png?raw=true)

	_Managing Access Keys_

<a name="Task2" />
##Creating an Application##
Now that the device is configured, you will see how to create an application to read the value of the FEZ HAT sensors, and then send those values to an Azure Event Hub.

<a name="Task21" />
###Deploying a Simple Application###

There are several ways to deploy a .Net application to a Raspberry Pi running a Linux based operating system. For the purpose of this lab the simple copy approach will be used which implies the following steps:

- Write and Build the code using Visual Studio Community Edition
- Copy binary files to the device
- Run the app using Mono

<a name="Task211" />
#### Write the code ####
1. Open the Visual Studio 2015 (Community Edition is sufficient) and create a new Console Application.
 	1. Click on **File** / **New** / **Project...**


		![Raspbian File New Project](Images/raspbian-file-new-project.png?raw=true)

	1. Select a Visual C# Console Application:
	
		![Create a new Console Application](Images/create-a-new-console-application.png?raw=true)

1. Open the **Program.cs** file and replace the **Main** method for this one:

	````C#
	static void Main(string[] args)
	{
	    Console.WriteLine("Running on {0}", Environment.OSVersion);

	    Console.WriteLine("Press a key to continue");
	    Console.ReadKey(true);
	}
	````
	
1. To deploy the application just Build your solution by clicking on **Build / Rebuild** Solution

	![Build Hello World](Images/build-hello-world.png?raw=true)

	And once the app has been successfully rebuilt, find the generated binaries in the **bin\** folder inside the solution folder:
	
	![Find binary files](Images/find-binary-files.png?raw=true)
	
1. If you want you can run the application in your development machine, since it's a regular Windows console app. Just double-click in the **RaspbianFezHat.exe** file:

	![Run Hello World app in Windows](Images/run-hello-world-app-in-windows.png?raw=true)

<a name="Task212" />
#### Copy the binary files ####
1. Now, to run the app in the Raspberry Pi you need to copy the binaries to the device using the FTP service you configured in the Setup section. Copy all the files from the **bin\Debug** folder.

1. Open a new File Explorer, type **ftp://\<your-device-ip-or-name\>** in the address bar and hit enter. Enter a valid username and password if you are asked for your credential. 
	
	![FTP Ask for user credentials](Images/ftp-ask-for-user-credentials.png?raw=true)

	> **Note:** A FTP Client can also be used to transfer the files from your development PC to your Raspberry Pi.

	
1. After the connection with the FTP is established you can move the files to the device. Create a new folder **/files/HelloWorld** first and then paste the copied files there.

<a name="Task213" />
#### Run the App using Mono ####
1. Once the files were successfully transfered you are ready to run the application in the Raspberry Pi. Login in a Raspbian console. If you want to connect to the device with a remote computer use **PuTTY** or other SSH client.

1. Login in the device with the same user used to connect to the FTP server:

	![Raspberry Pi Login](Images/raspberry-pi-login.png?raw=true)
	
1. Change the current directory to the folder where you copied the files by running the following command:

	````Shell
	$ cd files/HelloWorld/
	````

1. To run the app you just copied execute the following command:

	````Shell
	$ sudo mono RaspbianFezHat.exe
	````

	![Run Hello World app in Raspbian](Images/run-hello-world-app-in-raspbian.png?raw=true)

<a name="Task22" />
###Reading the FEZ HAT sensors###
Now that you know how to deploy an application to a Raspbian device, you will see how it can be modified to read real data from one of the FEZ HAT sensors and show that information on the console.

<a name="Task221" />
#### Adding FEZ HAT driver references ####
Before you can write code to read data from the hat sensors you need to add the references to the FEZ HAT drivers which are included in the **Assets/FezHatDrivers** folder.

1. Copy all the assemblies in the **Assets/FezHatDrivers** folder to your file system.

1. Add a reference to every assembly in that folder. In the Visual Studio's Solution Explorer right click on the project's name and click in **Add** / **Reference...**

	![Add FEZ HAT References](Images/add-fez-hat-references.png?raw=true)
	
1. In the **Reference Manager** window click on the **Browse** button at the bottom of the popup and select all the files in your local folder.

	![Browse References](Images/browse-references.png?raw=true)
	![Select all assemblies](Images/select-all-assemblies.png?raw=true)
	
1. Click **OK** and the assemblies will be added to the project. 

<a name="Task222" />
#### Adding code to read sensor data ####
Now you will see how simple is to pull data out of the hat sensors using the assemblies you referenced in the previous step.

1. Open the **Program.cs** class and add the following line to the **Using** section:

	````C#
	using GHIElectronics.Mono.Shields;
	````

1. Declare the following variable in the **Program** class which will contain a reference to the FEZ HAT driver:

	````C#
	private static FEZHAT hat;
	````

1. Add the following method to initialize the driver:

	````C#
	private static async void SetupHat()
	{
	    // Initialize Fez Hat
	    hat = await FEZHAT.CreateAsync();
	}
````

	
1. Replace the **Main** method with the following code:
	<!-- mark:5,6,11 -->
	````C#
	static void Main(string[] args)
	{
	    Console.WriteLine("Running on {0}", Environment.OSVersion);

	    Program.SetupHat();	    
	    Console.WriteLine("The temperature is {0} °C", hat.GetTemperature().ToString("N2", System.Globalization.CultureInfo.InvariantCulture));
		
	    Console.WriteLine("Press a key to continue");
	    Console.ReadKey(true);
	    
	    hat.Dispose();
	}
	````

<a name="Task223" />
#### Changing the target .NET framework ####
Before the application can be deployed one last task needs to be done. As some of the .Net 4.6 framework components are yet to be implemented in Mono, the application won't run in that version. To fix this just change the project's Target Framework before building it.

1. Right-click on the project name and select **Properties**.

1. Select **Application** in the left pane

1. Select **.NET Framework 4.5** in the **Target Framework** dropdown list.

	![Changing Project Target Framework](Images/changing-project-target-framework.png?raw=true)

<a name="Task224" />
#### Deploying the app ####
Build and deploy the application to the Raspberry Pi following the same directions as the previous step ([Deploying a Simple Application](#Task21)). If you wish you can create a new remote folder to avoid overwritting the Simple application you deployed before. 

![Running Temperature Sensor](Images/running-temperature-sensor.png?raw=true)
	
<a name="Task23" />
###Connecting the Application to Azure###
In the following section you will learn how to use the ConnectTheDots architecture to send the information gathered from the FEZ HAT sensors to Azure.

<a name="Task231" />
#### Adding ConnectTheDots infrastructure ####
The fist thing you need to do to connect to Azure is to add the following classes:

- **ConnectTheDotsSensor.cs** which represents an individual ConnectTheDots sensor
- **ConnectTheDotsHelper.cs** which contains the methods needed to send sensor information to an Azure Event Hub.

Follow these instructions:

1. In Visual Studio right-click on the project name and select **Add** / **Class...**

	![Add new class](Images/add-new-class.png?raw=true)
	
1. Name it **ConnectTheDotsSensor** and click **Add**:

	![Adding ConnectTheDotsSensor class](Images/adding-connectthedotssensor-class.png?raw=true)
	
1. Replace the content of the class with the following code

	````C#
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
````

1. Following the same steps, create a new class named **ConnectTheDotsHelper** and replace its content with the following code:

	````C#
	namespace RaspbianFezHat
	{
		using System;
		using System.Collections.Generic;
		using System.Globalization;
		using System.Net;
		using System.Text;
		using System.Threading;
		using System.Web;
		using Amqp;
		using Amqp.Framing;
		using Amqp.Types;
		using Newtonsoft.Json;

		public class ConnectTheDotsHelper : IDisposable
		{
			private SimpleLogger logger;

			private string deviceId;
			private string deviceIP;
			private string eventHubMessageSubject;

			// We have several threads that will use the same SenderLink object
			// we will protect the access using InterLock.Exchange 0 for false, 1 for true. 
			private int sendingMessage = 0;

			// Variables for AMQPs connection
			private Connection connection = null;
			private Session session = null;
			private SenderLink sender = null;

			private Address appAMQPAddress;
			private string appAMQPAddressName;
			private string appEHTarget;

			private bool disposedValue = false; // To detect redundant calls (disposable pattern)

			public ConnectTheDotsHelper(
				SimpleLogger logger,
				string serviceBusNamespace = "",
				string eventHubName = "",
				string keyName = "",
				string key = "",
				string displayName = "",
				string organization = "",
				string location = "",
				string eventHubMessageSubject = "",
				List<ConnectTheDotsSensor> sensorList = null)
			{
				this.IsConnectionReady = false;
				this.logger = logger;
				this.DisplayName = displayName;
				this.Organization = organization;
				this.Location = location;
				this.Sensors = sensorList;
				this.eventHubMessageSubject = eventHubMessageSubject;

				// Get device IP
				IPHostEntry hostInfoIP = Dns.GetHostEntry(Dns.GetHostName());
				IPAddress address = hostInfoIP.AddressList[0];
				this.deviceIP = address.ToString();

				if (string.IsNullOrEmpty(displayName))
				{
					this.DisplayName = hostInfoIP.HostName;
				}

				var httpUtility = new HttpUtility();

				this.appAMQPAddressName = string.Format(CultureInfo.InvariantCulture, "amqps://{0}:{1}@{2}.servicebus.windows.net", keyName, HttpUtility.UrlEncode(key), serviceBusNamespace);
				this.appAMQPAddress = new Address(this.appAMQPAddressName);
				this.appEHTarget = eventHubName;

				this.ApplySettingsToSensors();

				this.InitAMQPConnection(false);
			}

			public ConnectTheDotsHelper(
				string serviceBusNamespace = "",
				string eventHubName = "",
				string keyName = "",
				string key = "",
				string displayName = "",
				string organization = "",
				string location = "",
				List<ConnectTheDotsSensor> sensorList = null) : this(
					new SimpleLogger(),
					serviceBusNamespace,
					eventHubName,
					keyName,
					key,
					displayName,
					organization,
					location)
			{
			}

			// For Disposable pattern
			~ConnectTheDotsHelper()
			{
				// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
				this.Dispose(false);
			}

			public bool IsConnectionReady { get; private set; }

			// App Settings variables
			public string DisplayName { get; set; }

			public string Organization { get; set; }

			public string Location { get; set; }

			public List<ConnectTheDotsSensor> Sensors { get; set; }

			public void SendSensorData(ConnectTheDotsSensor sensor)
			{
				sensor.TimeCreated = DateTime.UtcNow.ToString("o");
				this.SendAmqpMessage(sensor.ToJson());
			}

			/// <summary>
			///  Apply settings to sensors collection
			/// </summary>
			public void ApplySettingsToSensors()
			{
				foreach (ConnectTheDotsSensor sensor in this.Sensors)
				{
					sensor.DisplayName = this.DisplayName;
					sensor.Location = this.Location;
					sensor.Organization = this.Organization;
				}
			}

			#region IDisposable Support
			// This code added to correctly implement the disposable pattern.
			public void Dispose()
			{
				// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
				this.Dispose(true);
				GC.SuppressFinalize(this);
			}

			protected virtual void Dispose(bool disposing)
			{
				if (!this.disposedValue)
				{
					if (disposing)
					{
						// TODO: dispose managed state (managed objects).
					}

					if (this.sender != null) this.sender.Close(5000);
					if (this.session != null) this.session.Close();
					if (this.connection != null) this.connection.Close();

					this.disposedValue = true;
				}
			}
			#endregion

			/// <summary>
			/// Initialize AMQP connection
			/// we are using the connection to send data to Azure Event Hubs
			/// Connection information is retreived from the app configuration file
			/// </summary>
			/// <returns>
			/// true when successful
			/// false when unsuccessful
			/// </returns>
			private bool InitAMQPConnection(bool reset)
			{
				this.IsConnectionReady = false;

				if (reset)
				{
					// If the reset flag is set, we need to kill previous connection 
					try
					{
						this.logger.Info("Resetting connection to Azure Event Hub");
						this.logger.Info("Closing any existing senderLink, session and connection.");
						if (this.sender != null) this.sender.Close();
						if (this.session != null) this.session.Close();
						if (this.connection != null) this.connection.Close();
					}
					catch (Exception e)
					{
						this.logger.Error("Error closing AMQP connection to Azure Event Hub: {0}", e.Message);
					}
				}

				this.logger.Info("Initializing connection to Azure Event Hub");

				// Initialize AMQPS connection
				try
				{
					this.connection = new Connection(this.appAMQPAddress);
					this.session = new Session(this.connection);
					this.sender = new SenderLink(this.session, "send-link", this.appEHTarget);
				}
				catch (Exception e)
				{
					this.logger.Error("Error connecting to Azure Event Hub: {0}", e.Message);
					if (this.sender != null) this.sender.Close();
					if (this.session != null) this.session.Close();
					if (this.connection != null) this.connection.Close();
					return false;
				}

				this.IsConnectionReady = true;
				this.logger.Info("Connection to Azure Event Hub initialized.");
				return true;
			}

			/// <summary>
			/// Send a string as an AMQP message to Azure Event Hub
			/// </summary>
			/// <param name="valuesJson">
			/// String to be sent as an AMQP message to Event Hub
			/// </param>
			private void SendAmqpMessage(string valuesJson)
			{
				Message message = new Message();

				// If there is no value passed as parameter, do nothing
				if (valuesJson == null) return;

				try
				{
					// Deserialize Json message
					var sample = JsonConvert.DeserializeObject<Dictionary<string, object>>(valuesJson);
					if (sample == null)
					{
						this.logger.Info("Error parsing JSON message {0}", valuesJson);
						return;
					}
	#if DEBUG
					this.logger.Info("Parsed data from serial port: {0}", valuesJson);
					this.logger.Info("Device GUID: {0}", sample["guid"]);
					this.logger.Info("Subject: {0}", this.eventHubMessageSubject);
					this.logger.Info("dspl: {0}", sample["displayname"]);
	#endif

					// Convert JSON data in 'sample' into body of AMQP message
					// Only data added by gateway is time of message (since sensor may not have clock) 
					this.deviceId = Convert.ToString(sample["guid"]);      // Unique identifier from sensor, to group items in event hub

					message.Properties = new Properties()
					{
						Subject = this.eventHubMessageSubject,              // Message type (e.g. "wthr") defined in sensor code, sent in JSON payload
						CreationTime = DateTime.UtcNow, // Time of data sampling
					};

					message.MessageAnnotations = new MessageAnnotations();

					// Event Hub partition key: device id - ensures that all messages from this device go to the same partition and thus preserve order/co-location at processing time
					message.MessageAnnotations[new Symbol("x-opt-partition-key")] = this.deviceId;
					message.ApplicationProperties = new ApplicationProperties();
					message.ApplicationProperties["time"] = message.Properties.CreationTime;
					message.ApplicationProperties["from"] = this.deviceId; // Originating device
					message.ApplicationProperties["dspl"] = sample["displayname"] + " (" + this.deviceIP + ")";      // Display name for originating device defined in sensor code, sent in JSON payload

					if (sample != null && sample.Count > 0)
					{
	#if !SENDAPPPROPERTIES

						var outDictionary = new Dictionary<string, object>(sample);
						outDictionary["Subject"] = message.Properties.Subject; // Message Type
						outDictionary["time"] = message.Properties.CreationTime;
						outDictionary["from"] = this.deviceId; // Originating device
						outDictionary["dspl"] = sample["displayname"] + " (" + this.deviceIP + ")";      // Display name for originating device
						message.Properties.ContentType = "text/json";
						message.BodySection = new Data() { Binary = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(outDictionary)) };
	#else
			    foreach (var sampleProperty in sample)
					    {
						    message.ApplicationProperties [sample.Key] = sample.Value;
					    }
	#endif
					}
					else
					{
						// No data: send an empty message with message type "weather error" to help diagnose problems "from the cloud"
						message.Properties.Subject = "wthrerr";
					}
				}
				catch (Exception e)
				{
					this.logger.Error("Error when deserializing JSON data received over serial port: {0}", e.Message);
					return;
				}

				// Send to the cloud asynchronously
				// Obtain handle on AMQP sender-link object
				if (0 == Interlocked.Exchange(ref this.sendingMessage, 1))
				{
					bool amqpConnectionIssue = false;
					try
					{
						// Message send function is asynchronous, we will receive completion info in the SendOutcome function
						this.sender.Send(message, this.SendOutcome, null);
					}
					catch (Exception e)
					{
						// Something went wrong let's try and reset the AMQP connection
						this.logger.Error("Exception while sending AMQP message: {0}", e.Message);
						amqpConnectionIssue = true;
					}

					Interlocked.Exchange(ref this.sendingMessage, 0);

					// If there was an issue with the AMQP connection, try to reset it
					while (amqpConnectionIssue)
					{
						amqpConnectionIssue = !this.InitAMQPConnection(true);
						Thread.Sleep(200);
					}
				}

	#if LOG_MESSAGE_RATE
		    if (g_messageCount >= 500)
		    {
			float secondsElapsed = ((float)stopWatch.ElapsedMilliseconds) / (float)1000.0;
			if (secondsElapsed > 0)
			{
			    Console.WriteLine("Message rate: {0} msg/s", g_messageCount / secondsElapsed);
			    g_messageCount = 0;
			    stopWatch.Restart();
			}
		    }
	#endif
			}

			/// <summary>
			/// Callback function used to report on AMQP message send 
			/// </summary>
			/// <param name="message"></param>
			/// <param name="outcome"></param>
			/// <param name="state"></param>
			private void SendOutcome(Message message, Outcome outcome, object state)
			{
				if (outcome is Accepted)
				{
					////#if DEBUG
					this.logger.Info("Sent message at {0}", message.ApplicationProperties["time"]);
					////#endif
	#if LOG_MESSAGE_RATE
			g_messageCount++;
	#endif
				}
				else
				{
					this.logger.Error("Error sending message {0} - {1}, outcome {2}", message.ApplicationProperties["time"], message.Properties.Subject, outcome);
					this.logger.Error("Error sending to {0} at {1}", this.appEHTarget, this.appAMQPAddress);
				}
			}
		}
	}
	````

<a name="Task232" />
#### Adding a simple logger ####
In order to show the output (information or errors) of the application in the console you will implement a simple logger class. Create a class with the name **SimpleLogger** and replace its content with this code:

````C#
namespace RaspbianFezHat
{
    using System;

    public class SimpleLogger
    {
        public void Info(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }

        public void Error(string format, params object[] args)
        {
            Console.WriteLine("ERROR: " + format, args);
        }
    }
}
````

<a name="Task233" />
#### Adding required references ####
In this section you will add the references to the components required for the application to work:

1. **Newtonsoft.Json NuGet Package**: Open the NuGet Package Manager Console (**Tools** / **NuGet Package Manager** / **Package Manager Console**) and execute the following command:

	````PowerShell
	Install-Package Newtonsoft.Json
	````

	Which will install the library used to serialize the sensor information objects before send the information to Azure.
	
1. **AMQPNetLite**: Using the same NuGet Package Console, execute the following command to install the AMQPNetLite library which will be used to send messages to Azure using the [AMQP protocol](https://www.amqp.org/):

	````PowerShell
	Install-Package AMQPNetLite -version 1.1.1
	````

	> **Note:** We used the specific version 1.1.1. since currently the last version 1.2.1 doesn't work with Mono (it uses a Socket constructor not supported by Mono).
	
1. **System.Web**: Add the reference to this library by right-clicking in the Project name and selecting **Add** / **Reference**.

	Select **Assemblies** on the left pane and then select **System.Web** in the Assemblies list and click **OK**:
	
	![Referencing System.Web assembly](Images/referencing-systemweb-assembly.png?raw=true)

<a name="Task234" />
#### Adding code to Send data to Azure ####
Now you are ready to use the ConnectTheDots plattform to collect the data from the sensors and send them to an Azure Event Hub.

1. Open the **Program.cs** file and add the following code to the _using_ section:

	````C#
	using System.Globalization;
	using System.Timers;
	````

2. Add the following variable declarations to the class declaration section:

	````C#
	private static SimpleLogger logger = new SimpleLogger();
	private static ConnectTheDotsHelper ctdHelper;
	private static Timer timer;
	````
	
	The **timer** will be used to poll the hat sensors at regular basis. For every _tick_ of the timer the value of the sensors will be get and sent to Azure. **logger** and **ctdHelper** are instances of the recently created **SimpleLogger** and **ConnectTheDotsHelper** classes respectively.

1. Replace the **SetupHat** method with the following code:

	````C#
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
	````

	Which adds code to initialize the timer to tick every 500 miliseconds.
	
1. Replace the **Main** method with the following code:

	````C#
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
	````

	The first line initializes the ConnectTheDots sensor list. In this case two sensor objects are created (one for the temperature sensor and another for the light)
	<!-- mark:3,4 -->
	````C#
	// Hard coding guid for sensors. Not an issue for this particular application which is meant for testing and demos
	List<ConnectTheDotsSensor> sensors = new List<ConnectTheDotsSensor> {
		new ConnectTheDotsSensor("2298a348-e2f9-4438-ab23-82a3930662ab", "Light", "L"),
		new ConnectTheDotsSensor("d93ffbab-7dff-440d-a9f0-5aa091630201", "Temperature", "C"),
	};
	````

	The next statement initializes the ConnectTheDotsHelper object, which receives, among other parameters, the Event Hub connection settings. 
	
	````C#
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
	````

	In order to allow the application to send data to the **Event Hub**, the following information must be provided:

	- **Event Hub Name**: Is the name given to the Event Hub. In the **Azure Management Portal** go to the **Service Bus** service and select the **Namespace** under which the Event Hub was created. Then click the **Event Hubs** tab and you will see the name of the Event Hub.
	
		![Get Event Hub name](Images/get-event-hub-name.png?raw=true)
	
	- **Service Bus Namespace**: Is the name of the Service Bus namespace at which your Event Hub belongs.
	- **Shared Access Policy**: Is the policy created to grant access to the Event Hub. It can be obtained from the Event Hub **Configuration** tab of the **Azure Management Portal**. From the previous step, click **Event Hub** and then click the **Configuration** tab. 
		- **Key Name**: Shared Access Policy Name
		- **Key**: Shared Access Primary Key
	
		![Get Event Hub Shared Access Policy](Images/get-event-hub-shared-access-policy.png?raw=true)
		
		

	The following statement is the call to the FEZ HAT initializer
	
	````C#
	// Setup the FEZ HAT driver
	Program.SetupHat();
	````

	And the last piece of code is for disposing the objects used.
	
	````C#
	Console.WriteLine("Reading data from FEZ HAT sensors. Press Enter to Stop");
	Console.ReadLine();
	Console.WriteLine("Closing...");
	Program.timer.Enabled = false;
	Program.timer.Dispose();
	Program.hat.Dispose();
	ctdHelper.Dispose();
	````
	
1. Create a new method called **Time_Elapsed** with the following code:

	````C#
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
	````

	This method will be executed every time the timer ticks, and will poll the value of the hat's temperature and light sensors, send them to the Azure Event Hub and show the value obtained in the console.
	
	For every sensor, Light and Temperature, the following tasks are performed:
	
	- Gets the _ConnectTheDots_ sensor from the _sensors_ collection
	- The temperature is polled out from the hat's sensor using the driver object initialized in previous steps
	- The value obtained is sent to Azure using the _ConnectTheDots_'s helper object **ctdHelper** 

<a name="Task235" />
#### Syncing Trusted Root Certificates ####
In order to perform a connection using a secure protocol (SSL), Mono requires to trust in the remote site. To ensure that the connection with the Azure Event HUB is successfully stablished, run the following command in a Raspbian console, which will download the trusted root certificates from the Mozilla LXR web site into the Mono certificate store:

````Shell
$ sudo mozroots --import --sync
````

<a name="Task236" />
#### Deploying the app ####
Build and deploy the application to the Raspberry Pi following the same directions as [Deploying a Simple Application](#Task21) section. If you wish you can create a new remote folder to avoid overwritting the applications you deployed before. 

![Running Final Application](Images/running-final-application.png?raw=true)

<a name="Task3" />
##Consuming the Event Hub data##
In the following sections you will see different ways of consuming the sensor data that is being uploaded to the Azure Event Hub. You will use a simple Console App, a Website, and Power BI to consume this data and to generate meaningful information.

<a name="Task31" />
###Creating a Console Application to read the Azure Event Hub###
In the previous task you created an App that read data from the sensors and sent it to an Azure Event Hub. Now, you will create a simple console application that will read the information that is in the Event Hub.

> **Note:** This is an optional section. You can find the completed version of the solution created here in the _Code\EventHubReader_ folder. To make it work you will need to replace the placeholders in the **Program.cs** file with the corresponding values.

1. In Visual Studio, create a new solution by clicking **File** -> **New Project**.
2. In the Installed Templates pane, select **Visual C#**, and then choose **Console Application**. Enter a **Name** for the solution, select a **Location**, and click **OK**.

	![Creating a Console Application](Images/creating-a-console-application.png?raw=true)
	
	_Creating a new Console Application_

3. Right-click the project name, and click **Manage Nuget Packages**. Search for the **Microsoft.Azure.ServiceBus.EventProcessorHost** nuget package.

4. Select the nuget package and click **Install**. You will be prompted to accept the license agreement, click **I Accept** to do so. Wait until the nuget installation finishes.

	![Installing the EventProcessorHost Nuget Package](Images/installing-the-eventprocessorhost-nuget-packa.png?raw=true)
	
	_Installing the EventProcessorHost Nuget Package_

5. Add a new _EventProcessor_ class. To do this, right-click the project name, point to **Add**, and click **Class**. Enter _EventProcessor_ as the class **Name**, and click **Add**.

6. At the top of the class, add a the following using statements.

	````C#
	using System.Diagnostics;
	using Microsoft.ServiceBus.Messaging;
	using Newtonsoft.Json;
	````
7. Change the signature of the _EventProcessor_ class to be public and to implement the **IEventProcessor** interface. This is shown in the following code.

	````C#
	public class EventProcessor : IEventProcessor
	````

8.  Point to the **IEventProcessor** interface that should be underlined, click on the down arrow near the light bulb, and click **Implement interface**.

	![Implementing the IEventProcessor Interface](Images/implementing-the-ieventprocessor-interface.png?raw=true)
	
	_Implementing the IEventProcessor Interface_

9. Add the following variable definition inside the class body.

	````C#
	private Stopwatch stopWatch;
	````

	
10. Locate the **OpenAsync** method, and add the following code inside the method body.

	````C#
	public Task OpenAsync(PartitionContext context)
	{
	    Console.WriteLine("EventProcessor started");
	    this.stopWatch = new Stopwatch();
	    this.stopWatch.Start();
	    return Task.FromResult<object>(null);
	}
	````

	This logic will be run at the start of the EventProcessor and will instantiate a Stopwatch that will be used for saving 
	
11. Go to the **CloseAsync** method, and add the **async** keyword at the start of the method.

12. Add the following code inside this method to close the connection of the Event Processor.

	<!-- mark:3-7 -->
	````C#
	public async Task CloseAsync(PartitionContext context, CloseReason reason)
	{
	    Console.WriteLine("Shutting Down Event Processor");
	    if (reason == CloseReason.Shutdown)
	    {
	        await context.CheckpointAsync();
	    }
	}
	````
	
	This code will be run when the Event Processor is shut down, and will save checkpoint information, to resume from this point in a next session.

13.  Go to the **ProcessEventsAsync** method, and add the **async** keyword at the start of the method.

14. Add the following code inside this method to get the events from the event hub and write the information in the console.

	````C#
	public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
	{
		foreach (EventData eventData in messages)
		{
			dynamic eventBody = JsonConvert.DeserializeObject(Encoding.Default.GetString(eventData.GetBytes()));
			Console.WriteLine(
			    "Part.ID: {0}, Part.Key: {1}, Guid: {2}, Location: {3}, Measure Name: {4}, Unit of Measure: {5}, Value: {6}",
			    context.Lease.PartitionId, 
			    eventData.PartitionKey, 
			    eventBody.guid, 
			    eventBody.location, 
			    eventBody.measurename, 
			    eventBody.unitofmeasure, 
			    eventBody.value);
		}

		if (this.stopWatch.Elapsed > TimeSpan.FromMinutes(5))
		{
			await context.CheckpointAsync();
			this.stopWatch.Restart();
		}
	}
	````

	Messages are read by this method, deserialized, and sent to the console window. Additionally, there is logic that will set a checkpoint to resume processing from that point in case the worker stops. The checkpoint information will be saved in Azure Storage.
	
15. Open the _Program.cs_ file.
16. At the top of the file, add the following using statements.

	````C#
	using Microsoft.ServiceBus.Messaging;
	````
	
18. Inside the **Main** method, add the following code.
	<!-- mark:3-19 -->
	````C#
	public static void Main(string[] args)
	{
		string eventHubConnectionString = "Endpoint=sb://[EventHubNamespaceName].servicebus.windows.net/;SharedAccessKeyName=[SASKeyName];SharedAccessKey=[SASKey]";
		string eventHubName = "[EventHubName]";
		string storageAccountName = "[StorageAccountName]";
		string storageAccountKey = "[StorageAccountKey]";
		string storageConnectionString = string.Format(
			"DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}",
				storageAccountName, 
				storageAccountKey);

		string eventProcessorHostName = Guid.NewGuid().ToString();
		EventProcessorHost eventProcessorHost = new EventProcessorHost(eventProcessorHostName, eventHubName, EventHubConsumerGroup.DefaultGroupName, eventHubConnectionString, storageConnectionString);
		Console.WriteLine("Registering the EventProcessor");
		eventProcessorHost.RegisterEventProcessorAsync<EventProcessor>().Wait();

		Console.WriteLine("Listening... Press enter to stop.");
		Console.ReadLine();
		eventProcessorHost.UnregisterEventProcessorAsync().Wait();
	}
	````

	The preceding code sets the configuration values required to connect to the event hub and the storage account, register the **EventProcessor** created previously, and will start listening for the messages in the event hub.
	
19. Replace all the placeholders in the previous code, values inside the square brackets **[]**, with their corresponding values that you took note in the set up section. The placeholders are:
	- [EventHubNamespaceName]
	- [SASKeyName]
	- [SASKey]
	- [EventHubName]
	- [StorageAccountName]
	- [StorageAccountKey]
	
20. Press **F5** to run the solution.

	![Reading the Event Hub](Images/reading-the-event-hub.png?raw=true)
	
	_The console app reading the event hub_
	
	You will see in the console app, all the values that are read from the event hub. These values are parsed by the **EventProcessor** and will show the values read from the sensors of the _Raspeberri Pi2_, light and temperature.
	
<a name="Task32" />
###Using Power BI###

Another (and more interesting) way to use the information received from the connected device/s is to get near real-time analysis using the **Microsoft Power BI** tool. In this section you will see how to configure this tool to get an online dashoard showing summarized information about the different sensors.

<a name="Task321" />
#### Setting up a Power BI account ####
If you don't have a Power BI account already, you will need to create one (a free account is enough to complete this lab). If you already have an account set you can skip this step.

1. Go to the [Power BI website](https://powerbi.microsoft.com/) and follow the sign-up process.

	> **Note:** At the moment this lab was written, only users with corporate email accounts are allowed to sign up. Free consumer email accounts (like Outlook, Hotmail, Gmail, Yahoo, etc.) can't be used.

2. You will be asked to enter your email address. Then a confirmation email will be sent. After following the confirmation link, a form to enter your personal information will be displayed. Complete the form and click Start.

	The preparation of your account will take several minutes, and when it's ready you will see an screen similar to the following:

	![Power BI Welcome screen](Images/power-bi-welcome-screen.png?raw=true)
	
	_Power BI welcome screen_

Now that your account is set, you are ready to set up the data source that will feed the Power BI dashboard.

<a name="Task3220" />
##### Create a Service Bus Consumer Group #####
In order to allow several consumer applications to read data from the Event Hub independently at their own pace a Consumer Group must be configured for each one. If all of the consumer applications (the Console application, Stream Analytics / Power BI, the Web site you will configure in the next section) read the data from the default consumer group, one application will block the others.

To create a new Consumer Group for the Event Hub that will be used by the Stream Analytics job you are about to configure, follow these steps:

- Open the Azure Management Portal, and select Service Bus
- Select the Namespace you used for your solution
- From the top menu, select Event Hubs
- From the left menu, select the Event Hub
- From the top menu, select Consumer Groups
- Select "+" Create at the bottom to create a new Consumer Group
- Give it the name "CG4PBI" and click OK

![Create Consumer Group](Images/create-consumer-group.png?raw=true)

<a name="Task322" />
#### Setting the data source ####
In order to feed the Power BI reports with the information gathered by the hats and to get that information in near real-time, **Power BI** supports **Azure Stream Analytics** outputs as data source. The following section will show how to configure the Stream Analytics job created in the Setup section to take the input from the Event Hub and push that summarized information to Power BI.

<a name="Task3221" />
##### Stream Analytics Input Setup #####
Before the information can be delivered to **Power BI**, it must be processed by a **Stream Analytics Job**. To do so, an input for that job must be provided. As the Raspberry devices are sending information to an Event Hub, it will be set as the input for the job.

1. Go to the Azure management portal and select the **Stream Analytics** service. There you will find the Stream Analytics job created during the _Azure services setup_. Click on the job to enter the Stream Analytics configuration screen.

	![Stream Analytics configuration](Images/stream-analytics-configuration.png?raw=true)
	
	_Stream Analytics Configuration_

2. As you can see, the Start button is disabled since the job is not configured yet. To set the job input click on the **INPUTS** tab and then in the **Add an input** button.

3. In the **Add an input to your job** popup, select the **Data Stream** option and click **Next**. In the following step, select the option **Event Hub** and click **Next**. Lastly, in the **Event Hub Settings** screen, provide the following information:

	- **Input Alias:** _TelemetryHub_
	- **Subscription:** Use Event Hub from Current Subscription (you can use an Event Hub from another subscription too by selecting the other option)
	- **Choose a Namespace:** _Raspbian-ns_ (or the namespace name selected during the Event Hub creation)
	- **Choose an Event Hub:** _Raspbian_ (or the name used during the Event Hub creation)
	- **Event Hub Policy Name:** _RootManageSharedAccessKey_
	- **Choose a Consumer Group:** _cg4pbi_

	![Stream Analytics Input configuration](Images/stream-analytics-input-configuration.png?raw=true)
	
	_Stream Analytics Input Configuration_

4. Click **Next**, and then **Complete** (leave the Serialization settings as they are).

<a name="Task3222" />
##### Stream Analytics Output Setup #####
The output of the Stream Analytics job will be Power BI.

1. To set up the output, go to the Stream Analytics Job's **OUTPUTS** tab, and click the **ADD AN INPUT** link.

2. In the **Add an output to your job** popup, select the **POWER BI** option and the click the **Next button**.

3. In the following screen you will setup the credentials of your Power BI account in order to allow the job to connect and send data to it. Click the **Authorize Now** link.

	![Stream Analytics Output configuration](Images/steam-analytics-output-configuration.png?raw=true)
	
	_Stream Analytics Output Configuration_

	You will be redirected to the Microsoft login page.

4. Enter your Power BI account email and click **Continue**, then select your account type (Work, School account, or Microsoft account) and then enter your password. If the authorization is successful, you will be redirected back to the **Microsoft Power BI Settings** screen.

5. In this screen you will enter the following information:

	- **Output Alias**: _PowerBI_
	- **Dataset Name**: _Raspberry_
	- **Table Name**: _Telemetry_
	- **Group Name**: _My Workspace_

	![Power BI Settings](Images/power-bi-settings.png?raw=true)
	
	_Power BI Settings_

6. Click the checkmark button to create the output.

<a name="Task3223" />
##### Stream Analytics Query configuration #####
Now that the job's inputs and outputs are already configured, the Stream Analytics Job needs to know how to transform the input data into the output data source. To do so, you will create a new Query.

1. Go to the Stream Analytics Job **QUERY** tab and replace the query with the following statement:

	````SQL
	SELECT
	    displayname,
	    location,
	    guid,
	    measurename,
	    unitofmeasure,
	    Max(timecreated) timecreated,
	    Avg(value) AvgValue
	INTO
	    [PowerBI]
	FROM
	    [TelemetryHUB] TIMESTAMP by timecreated
	GROUP BY
	    displayname, location, guid, measurename, unitofmeasure,
	    TumblingWindow(Second, 10)
	````

	The query takes the data from the input (using the alias defined when the input was created **TelemetryHUB**) and inserts into the output (**PowerBI**, the alias of the output) after grouping it using 10 seconds chunks.

2. Click on the **SAVE** button and **YES** in the confirmation dialog.

<a name="Task3234" />
##### Starting the Stream Analytics Job #####
Now that the job is configured, the **START** button is enabled. Click the button to start the job and then select the **JOB START TIME** option in the **START OUTPUT** popup. After clicking **OK** the job will be started.

Once the job starts it creates the Power BI datasource associated with the given subscription.

<a name="Task324" />
#### Setting up the Power BI dashboard ####
1. Now that the datasource is created, go back to your Power BI session, and go to **My Workspace** by clicking the **Power BI** link.

	After some minutes of the job running you will see that the dataset that you configured as an output for the Job, is now displayed in the Power BI workspace Datasets section.

	![Power BI new datasource](Images/power-bi-new-datasource.png?raw=true)

	_Power BI: New Datasource_
	
	> **Note:** The Power BI dataset will only be created if the job is running and if it is receiving data from the Event Hub input, so check that the App is running and sending data to Azure to ensure that the dataset be created. To check if the Stream Analytics job is receiving and processing data you can check the Azure management Stream Analytics monitor.

2. Once the datasource becomes available you can start creating reports. To create a new Report click on the **Raspberry** datasource:

	![Power BI Report Designer](Images/power-bi-report-designer.png?raw=true)
	
	_Power BI: Report Designer_

	The Report designer will be opened showing the list of fields available for the selected datasource and the different visualizations supported by the tool.

3. To create the _Average Light by time_ report, select the following fields:

	- avgvalue
	- timecreated

	As you can see the **avgvalue** field is automatically set to the **Value** field and the **timecreated** is inserted as an axis. Now change the chart type to a **Line Chart**:

	![Select Line Chart](Images/select-line-chart.png?raw=true)
	
	_Selecting the Line Chart_

4. Then you will set a filter to show only the Light sensor data. To do so drag the **measurename** field to the **Filters** section and then select the **Light** value:

	![Select Report Filter](Images/select-report-filter.png?raw=true)
	![Select Light sensor values](Images/select-light-sensor-values.png?raw=true)
	
	_Selecting the Report Filters_

5. Now the report is almost ready. Click the **SAVE** button and set _Light by Time_ as the name for the report.

	![Light by Time Report](Images/light-by-time-report.png?raw=true)
	
	_Light by Time Report_

6. Now you will create a new Dashboard, and pin this report to it. Click the plus sign (+) next to the **Dashboards** section to create a new dashboard. Set _Raspberry Telemetry_ as the **Title** and press Enter. Now, go back to your report and click the pin icon to add the report to the recently created dashboard.

	![Pin a Report to the Dashboard](Images/pin-a-report-to-the-dashboard.png?raw=true)
	
	_Pinning a Report to the Dashboard_

1. To create a second chart with the information of the average Temperature follow these steps:
	1. Click on the **Raspberry** datasource to create a new report.
	2. Select the **avgvalue** field
	3. Drag the **measurename** field to the filters section and select **Temperature**
	4. Now change the visualization to a **gauge** chart:
	
		![Change Visualization to Gauge](Images/change-visualization-to-gauge.png?raw=true "Gauge visualization")
		
		_Gauge visualization_
	
	5. Change the **Value** from **Sum** to **Average**
	
		![Change Value to Average](Images/change-value-to-average.png?raw=true)
		
		_Change Value to Average_
		
		Now the Report is ready:
		
		![Gauge Report](Images/gauge-report.png?raw=true)
		
		_Gauge Report_
	
	6. Save and then Pin it to the Dashboard.

7. Following the same directions, create a _Temperature_ report and add it to the dashboard.
8. Lastly, edit the reports name in the dashboard by clicking the pencil icon next to each report.

	![Edit Report Title](Images/edit-report-title.png?raw=true)
	
	_Editing the Report Title_

	After renaming both reports you will get a dashboard similar to the one in the following screenshot, which will be automatically refreshed as new data arrives.

	![Final Power BI Dashboard](Images/final-power-bi-dashboard.png?raw=true)
	
	_Final Power BI Dashboard_

<a name="Task33" />
###Consuming the Event Hub data from a Website###
	
1. Download the Website project located [here](https://github.com/southworkscom/connectthedots/tree/master/Azure/WebSite).
2. Browse to the **Assets** folder and copy the _Web.config_ file inside the **ConnectTheDotsWebSite** folder of the Website.

	![Copying the web config to the website solution](Images/copying-the-web-config-to-the-website-solutio.png?raw=true)

	_Copying the **Web.config** to the WebSite solution_

3. Open the Web Site project (_ConnectTheDotsWebSite.sln_) in Visual Studio.
4. Edit the _Web.config_ file and add the corresponding values for the following keys:
	- **Microsoft.ServiceBus.EventHubDevices**: Name of the event hub.
	- **Microsoft.ServiceBus.ConnectionString**: Namespace endpoint connection string.
	- **Microsoft.ServiceBus.ConnectionStringDevices**: Event hub endpoint connection string.
	- **Microsoft.Storage.ConnectionString**: Storage account endpoint, in this case use the **storage account name** and **storage account primary key** to complete the endpoint.
5. Deploy the website to an Azure Web Site. To do this, perform the following steps.
	1. In Visual Studio, right-click on the project name and select **Publish**.
	2. Select **Microsoft Azure Web Apps**.
	
		![Selecting Publish Target](Images/selecting-publish-target.png?raw=true)
		
		_Selecting Publish target_
		
	3. Click **New** and use the following configuration.
	
		- **Web App name**: Pick something unique.
		- **App Service plan**: Select an App Service plan in the same region used for _Stream Analytics_ or create a new one using that region.
		- **Region**: Pick same region as you used for _Stream Analytics_.
		- **Database server**: No database.
		
	4. Click **Create**. After some time the website will be created in Azure.
	
		![Creating a New Web App](Images/creating-a-new-web-app.png?raw=true)
		
		_Creating a new Web App on Microsoft Azure_
		
	3. Click **Publish**.
	
		> **Note:** You might need to install **WebDeploy** extension if you are having an error stating that the Web deployment task failed. You can find WebDeploy [here](http://www.iis.net/downloads/microsoft/web-deploy).

	
6. After you deployed the site, it is required that you enable **Websockets**. To do this, perform the following steps:
	1. Browse to https://manage.windowsazure.com and select your _Azure Web Site_.
	2. Click the **Configure** tab.
	3. Then set **WebSockets** to **On** and click **Save**.
	
		![Enabling Web Sockets](Images/enabling-web-sockets.png?raw=true)
	
		_Enabling Web Sockets in your website_

7. Browse to your recently deployed Web Application. You will see something like in the following screenshot. There will be 2 real-time graphics representing the values read from the temperature and light sensors. Take into account that the App must be running and sending information to the Event Hub in order to see the graphics.

	![Web Site Consuming the Event Hub Data](Images/web-site-consuming-the-event-hub-data.png?raw=true)
	
	_Web Site Consuming the Event Hub data_
	
	> **Note:** At the bottom of the page you should see “**Connected**.”. If you see “**ERROR undefined**” you likely didn’t enable **WebSockets** for the Azure Web Site.

<a name="Summary" />
##Summary##
In this lab, you have learned how to setup a Raspberry Pi 2 device running Raspbian and connected to a FEZ hat. You also learned how to create a console application that reads from the FEZ hat sensors and sends that readings to an Azure Event Hub. Lastly, you have seen three different methods for consuming the Azure Event Hub data: a console application, PowerBI, and a website.
