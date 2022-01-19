using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Diagnostics;
using System.Management;
using OpenHardwareMonitor;
using OpenHardwareMonitor.Hardware;
using System.Threading;
using System.IO;
using System.Timers;
using Timer = System.Timers.Timer;
using Newtonsoft.Json;



namespace KVLS2_C
{
    public partial class Form1 : Form
    {
        private Thread _thread;
        static Computer _thisComputer;
        public const Int64 GB = (1024 * 1024 * 1024);
        //private IPlayEssentialPluginContext _context;

        public string Name { get; private set; }
        public string Column1 { get; private set; }
        public string Column2 { get; private set; }
        public string Column3 { get; private set; }
        public List<Form1> Children { get; private set; }
        public Form1(string name, string col1, string col2, string col3)
        {
            this.Name = name;
            this.Column1 = col1;
            this.Column2 = col2;
            this.Column3 = col3;
            this.Children = new List<Form1>();
        }
        private List<Form1> data;
        private BrightIdeasSoftware.TreeListView treeListView;

        public Form1()
        {

            InitializeComponent();
            AddTree();
            InitializeData();
            FillTree();

            _thread = new Thread(new ThreadStart(MonitoringStart));
            _thread.Start();

        }
        public void MonitoringStart()
        {
            while (true)
            {
                

                //_context.Bridge.Publish(JsonConvert.SerializeObject(_monitor.Tick()));
                Thread.Sleep(1000);
            }

        }


        private void AddTree()
        {
            treeListView = new BrightIdeasSoftware.TreeListView();
            treeListView.Dock = DockStyle.Fill;
            this.Controls.Add(treeListView);
            
        }

        private void FillTree()
        {
            // set the delegate that the tree uses to know if a node is expandable
            this.treeListView.CanExpandGetter = x => (x as Form1).Children.Count > 0;
            // set the delegate that the tree uses to know the children of a node
            this.treeListView.ChildrenGetter = x => (x as Form1).Children;

            // create the tree columns and set the delegates to print the desired object proerty
            var nameCol = new BrightIdeasSoftware.OLVColumn("Name", "Name");
            nameCol.AspectGetter = x => (x as Form1).Name;

            var col1 = new BrightIdeasSoftware.OLVColumn("Column1", "Column1");
            col1.AspectGetter = x => (x as Form1).Column1;

            var col2 = new BrightIdeasSoftware.OLVColumn("Column2", "Column2");
            col2.AspectGetter = x => (x as Form1).Column2;

            var col3 = new BrightIdeasSoftware.OLVColumn("Column3", "Column3");
            col3.AspectGetter = x => (x as Form1).Column3;

            // add the columns to the tree
            this.treeListView.Columns.Add(nameCol);
            this.treeListView.Columns.Add(col1);
            this.treeListView.Columns.Add(col2);
            this.treeListView.Columns.Add(col3);
            

            // set the tree roots
            this.treeListView.Roots = data;
        }

        private void InitializeData()
        {
      
            // create fake nodes
            var parent1 = new Form1("SBC", "-", "-", "-");
            parent1.Children.Add(new Form1("CHILD_1_1", SBC_CPUInfoUpdate(), "X", "1"));
            parent1.Children.Add(new Form1("CHILD_1_2", "A", "Y", "2"));
            parent1.Children.Add(new Form1("CHILD_1_3", "A", "Z", "3"));
            parent1.Children.Add(new Form1("CHILD_1_1", "A", "X", "1"));
            parent1.Children.Add(new Form1("CHILD_1_2", "A", "Y", "2"));
            parent1.Children.Add(new Form1("CHILD_1_3", "A", "Z", "3"));

            var parent2 = new Form1("SDD", "-", "-", "-");
            parent2.Children.Add(new Form1("CHILD_2_1", SSD_MemoryUpdate(), "-","-"));
            parent2.Children.Add(new Form1("CHILD_2_2", "B", "Z", "8"));
            parent2.Children.Add(new Form1("CHILD_2_3", "B", "J", "9"));
            
            var parent3 = new Form1("NETWORK", "-", "-", "-");
            parent3.Children.Add(new Form1("CHILD_3_1", "C", "R", "10"));
            parent3.Children.Add(new Form1("CHILD_3_2", "C", "T", "12"));
            parent3.Children.Add(new Form1("CHILD_3_3", "C", "H", "14"));

            var parent4 = new Form1("USB", "-", "-", "-");
            parent4.Children.Add(new Form1("CHILD_3_1", "C", "R", "10"));
            parent4.Children.Add(new Form1("CHILD_3_2", "C", "T", "12"));
            parent4.Children.Add(new Form1("CHILD_3_3", "C", "H", "14"));

            data = new List<Form1> { parent1, parent2, parent3, parent4 };
        }

        private string SSD_MemoryUpdate()
        {
            ManagementObject myDisk = new ManagementObject("Win32_LogicalDisk.DeviceID='C:'");
            string myProperty = myDisk.GetPropertyValue("Size").ToString();
            var tmp = Convert.ToInt64(myProperty);
            var SSD_size = tmp / GB;
            string result = "";

            if (this.treeListView.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(delegate ()
                {
                    result = Convert.ToString(SSD_size) + " GB";
                }));
            }
            else
            {
                result = Convert.ToString(SSD_size) + " GB";
            }

            return result;
        }


        private string SBC_CPUInfoUpdate()
        {
            var result = "";
            _thisComputer = new Computer() { CPUEnabled = true, GPUEnabled = true, MainboardEnabled = true, HDDEnabled = true };
            _thisComputer.Open();

            StringBuilder sb = new StringBuilder();

            foreach (var hardwareItem in _thisComputer.Hardware)
            {
                switch (hardwareItem.HardwareType)
                {
                    case HardwareType.CPU:
                    case HardwareType.GpuNvidia:
                    case HardwareType.HDD:
                    case HardwareType.Mainboard:
                    case HardwareType.RAM:
                        AddCpuInfo(sb, hardwareItem);
                        break;
                }
            }

            result = sb.ToString();
            return result;
        }

        private static void AddCpuInfo(StringBuilder sb, IHardware hardwareItem)
        {
            hardwareItem.Update();
            foreach (IHardware subHardware in hardwareItem.SubHardware)
                subHardware.Update();

            string text;

            foreach (var sensor in hardwareItem.Sensors)
            {
                string name = sensor.Name;
                string value = sensor.Value.HasValue ? sensor.Value.Value.ToString() : "-1";

                switch (sensor.SensorType)
                {
                    case SensorType.Temperature:
                        text = $"{name} Temperature = {value}";
                        sb.AppendLine(text);
                        break;

                    case SensorType.Voltage:
                        text = $"{name} Voltage = {value}";
                        sb.AppendLine(text);
                        break;

                    case SensorType.Fan:
                        text = $"{name} RPM = {value}";
                        sb.AppendLine(text);
                        break;

                    case SensorType.Load:
                        text = $"{name} % = {value}";
                        sb.AppendLine(text);
                        break;
                }
            }
        }



    }
}
