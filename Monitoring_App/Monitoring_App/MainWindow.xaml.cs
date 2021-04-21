using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Windows.Threading;
using System.Management;
using OpenHardwareMonitor;
using OpenHardwareMonitor.Hardware;
using System.IO;

namespace Monitoring_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
		static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
		Computer thisComputer;
		public MainWindow()
        {
            InitializeComponent();

			thisComputer = new Computer() { CPUEnabled = true, GPUEnabled = true, MainboardEnabled = true, RAMEnabled = true, HDDEnabled = true };
			thisComputer.Open();


			Timer();
			CPU();
			GPU();
			RAM();
			MothBoard();
			Disk();
			Drives();
			//Apps();
        }

		public void Timer()
		{
			//Timer--------------
			DispatcherTimer timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromSeconds(1.5);
			timer.Tick += timer_Tick;
			timer.Start();
			//-------------------


		}

		void timer_Tick(object sender, EventArgs e)
		{
			CPU_temp();
			GPU_temp();
			MEM_temp();


		}

		static string SizeSuffix(Int64 value, int decimalPlaces = 1)
		{
			if (value < 0) { return "-" + SizeSuffix(-value); }
			if (value < 0) { return "-" + SizeSuffix(-value, decimalPlaces); }

			int i = 0;
			decimal dValue = (decimal)value;
			while (Math.Round(dValue, decimalPlaces) >= 1000)
			{
				dValue /= 1024;
				i++;
			}

			return string.Format("{0:n" + decimalPlaces + "} {1}", dValue, SizeSuffixes[i]);
		}

		//Monitoring--------------------
		public void CPU()
		{
			ManagementObjectSearcher cpu = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
			foreach (var item in cpu.Get())
			{
				cpu_name.Content = $"CPU: {item["Name"]}";
				cpu_cores.Content = $"CPU cores: {item["ThreadCount"]}";
				cpu_.Content = $"CPU Manufacturer: {item["Manufacturer"]}";
			}
		}
		public void GPU()
        {
			ManagementObjectSearcher gpu = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
            foreach (var item in gpu.Get())
            {
				gpu_name.Content = $"GPU: {item["Name"]}";
				gpu_ram.Content = $"VRAM: {SizeSuffix(Convert.ToInt64(item["AdapterRAM"]))}";
			}
		}
		
		public void RAM()
        {
			ManagementObjectSearcher ram = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory");
			string ddr = "";
			foreach (var item in ram.Get())
            {
				
				if (Convert.ToInt32(item["MemoryType"]) == 20) ddr = $"DDR";
				else if (Convert.ToInt32(item["MemoryType"]) == 21) ddr = $"DDR2";
				else if (Convert.ToInt32(item["MemoryType"]) == 24) ddr = $"DDR3";
				else if (Convert.ToInt32(item["MemoryType"]) == 26) ddr = $"DDR4";
				ram_list.Items.Add($" {Convert.ToString(ddr)} {item["Manufacturer"]} {item["Tag"]} {SizeSuffix(Convert.ToInt64(item["Capacity"]))} {item["Speed"]} MHz");
			}
		}

		public void MothBoard()
        {
			ManagementObjectSearcher ram = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");
            foreach (var item in ram.Get())
            {
				mboard.Content = $"Motherboard: {item["Manufacturer"]}";
            }
		}

		public void Disk()
        {
			ManagementObjectSearcher ram = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
            foreach (var item in ram.Get())
            {
				disk_model.Content = $"Disk: {item["Model"]}";
				disk_int.Content = $"Disk interface: {item["InterfaceType"]}";
				disk_size.Content = $"Disk size: {SizeSuffix(Convert.ToInt64(item["Size"]))}";
			}
		}

		public void Drives()
        {
			DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach (var item in allDrives)
            {
				drive.Items.Add($"{item.Name}: \r\nSize: {SizeSuffix(item.TotalSize)}, \r\nFree: {SizeSuffix(item.AvailableFreeSpace)}, \r\nFormat: {item.DriveFormat}");
				
			}
		}
		//---------------------------
		//Apps-----------------------
		public void Apps()
        {
			int x = 0;
			ManagementObjectSearcher app = new ManagementObjectSearcher("SELECT * FROM Win32_Product");
			foreach (var item in app.Get())
			{
				listbox.Items.Add($"{item["Name"]}, Version: {item["Version"]}");
				x++;
			}
			app_num.Content = $"{x} apps installed";
		}
		//----------------------------
		//Monitoring------------------
		public void CPU_temp()
        {
			string temp = "Temp:\n";
			string load = "Load:\n";
			string clock = "Speed:\n";
          
			foreach (var hardware in thisComputer.Hardware)
			{
				if (hardware.HardwareType == HardwareType.CPU)
				{
					hardware.Update();

					foreach (var sensor in hardware.Sensors)
					{
						if (sensor.SensorType == SensorType.Temperature)
						{
							temp += $"{sensor.Name} = {sensor.Value.Value}°C\r\n";
						}
						else if (sensor.SensorType == SensorType.Load)
						{
							load += $"{sensor.Name} = {Math.Round(sensor.Value.Value, 1)}%\r\n";
						}
						else if (sensor.SensorType == SensorType.Clock)
						{
							clock += $"{sensor.Name} = {Math.Round(sensor.Value.Value, 1)}Mhz\r\n";
						}
					}
				}
			}
			cpu_temp.Content = temp;
			cpu_load.Content = load;
			cpu_clock.Content = clock;

		}

		public void GPU_temp()
        {
			string temp = "Temp:\n";
			string load = "Load:\n";
			string clock = "Speed:\n";

			foreach (var hardware in thisComputer.Hardware)
			{
				if (hardware.HardwareType == HardwareType.GpuNvidia || hardware.HardwareType == HardwareType.GpuAti)
				{
					hardware.Update();

					foreach (var sensor in hardware.Sensors)
					{
						if (sensor.SensorType == SensorType.Temperature)
						{
							temp += $"{sensor.Name} = {sensor.Value.Value}°C\r\n";
						}
						else if (sensor.SensorType == SensorType.Load)
						{
							load += $"{sensor.Name} = {Math.Round(sensor.Value.Value, 1)}%\r\n";
						}
						else if (sensor.SensorType == SensorType.Clock)
						{
							clock += $"{sensor.Name} = {Math.Round(sensor.Value.Value, 1)}Mhz\r\n";
						}
					}
				}
			}
			gpu_temp.Content = temp;
			gpu_load.Content = load;
			gpu_clock.Content = clock;
		}

		public void MEM_temp()
        {
			string loadContent = "Load:\n";
			foreach (var hardware in thisComputer.Hardware)
			{
				if (hardware.HardwareType == HardwareType.RAM)
				{
					hardware.Update();
					foreach (IHardware subHardware in hardware.SubHardware)
						subHardware.Update();

					foreach (var sensor in hardware.Sensors)
					{
						if (sensor.SensorType == SensorType.Load)
						{
							loadContent += $"{sensor.Name} = {Math.Round(sensor.Value.Value, 1)}%\r\n";
							mem_prog.Value = Math.Round(sensor.Value.Value, 1);
						}
					}
				}
			}
			mem_load.Content = loadContent;
		}
		//----------------------------
	}	
}
        
       

