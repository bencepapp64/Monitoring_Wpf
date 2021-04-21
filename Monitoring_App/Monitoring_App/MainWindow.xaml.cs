﻿using System;
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

namespace Monitoring_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

			Timer();
			CPU();
			GPU();
        }

		public void Timer()
		{
			//Timer--------------
			DispatcherTimer timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromMilliseconds(1);
			timer.Tick += timer_Tick;
			timer.Start();
			//-------------------


		}

		void timer_Tick(object sender, EventArgs e)
		{
			

		}

		//Monitoring--------------------
		public void CPU()
		{
			ManagementObjectSearcher cpu = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
			foreach (var item in cpu.Get())
			{
				cpu_name.Content = $"CPU: {item["Name"]}";
				cpu_cores.Content = $"CPU cores: {item["ThreadCount"]}";
				cpu_.Content = $"CPU: {item["Manufacturer"]}";
			}
		}
		public void GPU()
        {
			ManagementObjectSearcher gpu = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
            foreach (var item in gpu.Get())
            {
				gpu_name.Content = $"GPU: {item["Name"]}";
				gpu_ram.Content = $"CPU: {item["AdapterRAM"]}";
			}
		}


		//----------------------------
	}	
}
        
       

