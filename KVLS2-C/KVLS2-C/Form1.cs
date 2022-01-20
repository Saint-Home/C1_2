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
using System.IO.Ports;
using System.Timers;
using Timer = System.Timers.Timer;





namespace KVLS2_C
{
    public partial class Form1 : Form
    {
        public enum DEVICE_IDX
        {
            SBC = 0,
            SSD,
            NETWORK,
            USB,
            COM
        }
        string[] DEVICE = new string[] { "SBC", "SSD", "NETWORK", "USB", "COM", "STATUS" };

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

        int preLen = 0;

        public Form1()
        {
            InitializeComponent();

            #region TreeListView Init
            Load += Form_Load;
            #endregion


            //data[0].Column1 = "Value change"; // head
            //data[0].Children[0].Column1 = "TEST2";  // child

            //treeListView.BeginUpdate();
            //treeListView.EndUpdate();
           
            _thread = new Thread(new ThreadStart(MonitoringStart));
            _thread.IsBackground = true;
            _thread.Start();

        }

        private void Form_Load(object sender, EventArgs e)
        {
            AddTree();
            InitializeData();
            FillTree();
        }

        public void MonitoringStart()
        {

            while (true)
            {

                Random rand = new Random();
                var value = rand.Next(1, 20);

                string[] comList = getComportList();

                if (this.InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(delegate ()
                    {
                        treeListView.BeginUpdate();

                        // SSD 
                        updateContent(1, 0, 1, SSD_MemoryUpdate());
                        updateContent(1, 1, 1, Convert.ToString(value));
                        updateContent(1, 2, 1, Convert.ToString(value));

                        // COM
                        if (comList.Length > 0)
                        {
                            preLen = comList.Length;
                            for (int i = 0; i < comList.Length; i++)
                            {
                                updateContent(4, i, 1, comList[i].ToString());
                            }
                        }
                        else
                        {
                            for (int i = 0; i < preLen; i++)
                            {
                                updateContent(4, i, 1, "-");
                            }
                        }
                
                        treeListView.EndUpdate();
                    }));
                }
                else
                {
                    treeListView.BeginUpdate();

                    updateContent(1, 0, 1, SSD_MemoryUpdate());
                    updateContent(1, 1, 1, Convert.ToString(value));
                    updateContent(1, 2, 1, Convert.ToString(value));

                    if (comList.Length > 0)
                    {
                        preLen = comList.Length;

                        for (int i = 0; i < comList.Length; i++)
                        {
                            updateContent(4, 0, 1, comList[i].ToString());
                        }
                    }
                    else
                    {
                        for (int i = 0; i < preLen; i++)
                        {
                            updateContent(4, i, 1, "-");
                        }
                    }

                    treeListView.EndUpdate();

                }

                Thread.Sleep(500);
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
            var nameCol = new BrightIdeasSoftware.OLVColumn("NAME", "Name");
            nameCol.AspectGetter = x => (x as Form1).Name;

            var col1 = new BrightIdeasSoftware.OLVColumn("CONTENT", "CONTENT");
            col1.AspectGetter = x => (x as Form1).Column1;

            var col2 = new BrightIdeasSoftware.OLVColumn("SATUS", "SATUS");
            col2.AspectGetter = x => (x as Form1).Column2;

            var col3 = new BrightIdeasSoftware.OLVColumn("BIT", "BIT");
            col3.AspectGetter = x => (x as Form1).Column3;

            // add the columns to the tree
            this.treeListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
            this.treeListView.Columns.Add(nameCol);
            this.treeListView.Columns.Add(col1);
            this.treeListView.Columns.Add(col2);
            this.treeListView.Columns.Add(col3);

            //listView1.Columns[0].AutoResize(ColumnHeaderAutoResizeStyle.HeaderSize);
            this.treeListView.Columns[0].Width = -2;
            this.treeListView.Columns[0].Width = 150;

            this.treeListView.Columns[1].Width = -2;
            this.treeListView.Columns[1].Width = 400;


            // set the tree roots
            this.treeListView.Roots = data;
        }

        private void InitializeData()
        {

            ImageList imgList = new ImageList();

        

            // create fake nodes
            Form1 parent1 = new Form1(DEVICE[(int)DEVICE_IDX.SBC], "-", "-", "-");
            parent1.Children.Add(new Form1(DEVICE[(int)DEVICE_IDX.SBC], "-", "-", "-"));
            parent1.Children.Add(new Form1(DEVICE[(int)DEVICE_IDX.SBC], "-", "-", "-"));
            parent1.Children.Add(new Form1(DEVICE[(int)DEVICE_IDX.SBC], "-", "-", "-"));
            parent1.Children.Add(new Form1(DEVICE[(int)DEVICE_IDX.SBC], "-", "-", "-"));
            parent1.Children.Add(new Form1(DEVICE[(int)DEVICE_IDX.SBC], "-", "-", "-"));
            parent1.Children.Add(new Form1(DEVICE[(int)DEVICE_IDX.SBC], "-", "-", "-"));


            var parent2 = new Form1(DEVICE[(int)DEVICE_IDX.SSD], "-", "-", "-");
            parent2.Children.Add(new Form1(DEVICE[(int)DEVICE_IDX.SSD], "-", "-", "-"));
            parent2.Children.Add(new Form1(DEVICE[(int)DEVICE_IDX.SSD], "-", "-", "-"));
            parent2.Children.Add(new Form1(DEVICE[(int)DEVICE_IDX.SSD], "-", "-", "-"));


            var parent3 = new Form1(DEVICE[(int)DEVICE_IDX.NETWORK], "-", "-", "-");
            parent3.Children.Add(new Form1(DEVICE[(int)DEVICE_IDX.NETWORK], "-", "-", "-"));
            parent3.Children.Add(new Form1(DEVICE[(int)DEVICE_IDX.NETWORK], "-", "-", "-"));
            parent3.Children.Add(new Form1(DEVICE[(int)DEVICE_IDX.NETWORK], "-", "-", "-"));


            var parent4 = new Form1(DEVICE[(int)DEVICE_IDX.USB], "-", "-", "-");
            parent4.Children.Add(new Form1(DEVICE[(int)DEVICE_IDX.USB], "-", "-", "-"));
            parent4.Children.Add(new Form1(DEVICE[(int)DEVICE_IDX.USB], "-", "-", "-"));
            parent4.Children.Add(new Form1(DEVICE[(int)DEVICE_IDX.USB], "-", "-", "-"));

            var parent5 = new Form1(DEVICE[(int)DEVICE_IDX.COM], "-", "-", "-");
            parent5.Children.Add(new Form1(DEVICE[(int)DEVICE_IDX.COM], "-", "-", "-"));
            parent5.Children.Add(new Form1(DEVICE[(int)DEVICE_IDX.COM], "-", "-", "-"));
            parent5.Children.Add(new Form1(DEVICE[(int)DEVICE_IDX.COM], "-", "-", "-"));

            data = new List<Form1> { parent1, parent2, parent3, parent4, parent5 };
            
        }

        private void updateContent(int row, int row_Index, int col, string str)
        {
            this.treeListView.BeginUpdate();            

            switch(col)
            {
                case 1:
                    data[row].Children[row_Index].Column1 = str;
                    if( (str != null) || (str != "-") || (str !="\0") )
                    {
                        data[row].Children[row_Index].Column2 = "Nomal";
                    }
                    else
                    {
                        data[row].Children[row_Index].Column2 = "-";
                    }
                    break;

                case 2:
                    data[row].Children[row_Index].Column2 = str;
                    break;

                case 3:
                    data[row].Children[row_Index].Column3 = str;
                    break;

            }      
            this.treeListView.EndUpdate();
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
        private string[] getComportList()
        {

            List<string> comList = new List<string>();
            var search = new ManagementObjectSearcher("Win32_SerialPort");

            string[] p = System.IO.Ports.SerialPort.GetPortNames();
            string[] ports = new string[p.Length];

            for (int i=0; i<p.Length; i++)
            {
                ports[i] = p[i];

            }
            //MessageBox.Show(ports.ToString());

            return ports;
        }


        private void button1_Click(object sender, EventArgs e)
        {
            treeListView.ExpandAll();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            treeListView.CollapseAll();
        }
    }
}
