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
using Excel = Microsoft.Office.Interop.Excel;

namespace Monitoring_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
		static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
		List<APP> apps = new List<APP>();
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
			Apps();
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
			HDD_temp();

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
				apps.Add(new APP() {Name = (string)item["Name"], Version = (string)item["Version"] });

				x++;
			}
			list.ItemsSource = apps;
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
			string load = "Load:\n";
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
							load += $"{sensor.Name} = {Math.Round(sensor.Value.Value, 1)}%\r\n";
							mem_prog.Value = Math.Round(sensor.Value.Value, 1);
						}
					}
				}
			}
			mem_load.Content = load;
		}

		public void HDD_temp()
        {
			string temp = "Temp:\n";
			foreach (var hardware in thisComputer.Hardware)
			{
				if (hardware.HardwareType == HardwareType.HDD)
				{
					hardware.Update();
					foreach (var sensor in hardware.Sensors)
					{
						if (sensor.SensorType == SensorType.Temperature)
						{
							temp += $"{sensor.Name} = {sensor.Value.Value}°C\r\n";
							hdd_prog.Value = Math.Round(sensor.Value.Value, 1);
						}
					}
				}
			}
			hdd_temp.Content = temp;
		}
		//----------------------------
		private void Button_Click(object sender, RoutedEventArgs e)
        {
			Excel.Application app = new Excel.Application();
			app.DisplayAlerts = false;
			app.Visible = false;
			Excel.Workbook wb = app.Workbooks.Add();
			wb.Worksheets.Add();
			Excel._Worksheet workSheet = app.Worksheets[app.Worksheets.Count - 1];
			workSheet.Name = "Applications";
			workSheet.Cells[1, 1].Value = "Name:";
			workSheet.Cells[1, 2].Value = "Version:";
			for (int i = 0; i < apps.Count; i++)
			{
				workSheet.Cells[i + 2, 1].Value = apps[i].Name;
				workSheet.Cells[i + 2, 2].Value = $"{apps[i].Version}";
			}
			workSheet.Columns.AutoFit();
			wb.SaveAs($@"{AppDomain.CurrentDomain.BaseDirectory}..\..\save\infos", Excel.XlFileFormat.xlWorkbookDefault,
			  Type.Missing, Type.Missing, Type.Missing,
			  Type.Missing, Excel.XlSaveAsAccessMode.xlExclusive,
			  Type.Missing, Type.Missing, Type.Missing,
			  Type.Missing, Type.Missing);
			wb.Close();
		}
       

    }

    public class APP
    {
		public string Name { get; set; }
		public string Version { get; set; }
	}
}
        
       

