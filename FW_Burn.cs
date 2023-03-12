using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;
using System.Management;
using System.Text.RegularExpressions;




namespace FW_Burn
{
    public partial class FW_Burn : Form
    {
        string connectSQLDB = System.Configuration.ConfigurationManager.ConnectionStrings["DBConnectionString"].ToString();
        
        //string ps_address = string.Empty;
        string pattern = @"\AFI-\d{4}-MB\d{5}\Z";
        string mac_pattern = @"\A[0-9A-Fa-f]{12}\Z"; //\A^[a-fA-F0-9]{12}\Z

        string[] SOM_Serial = new string[4];
        string[] MB_Serial = new string[4];
        int[] statflag = new int[4];

        string imagefile = System.Configuration.ConfigurationManager.AppSettings["imagefile"].ToString(); //@"C:\BurnImage\ight.img";
        string bootfile = System.Configuration.ConfigurationManager.AppSettings["bootfile"].ToString();

        //PS_Driver MainPS = new PS_Driver();
        SQL_Driver SQL_Manager = new SQL_Driver();
        
        public FW_Burn()
        {
            InitializeComponent();          


            //ps_address = System.Configuration.ConfigurationManager.AppSettings["ps_address"];

            
            var usbDevices = GetUSBDevices();
            /*
            foreach (var usbDevice in usbDevices)
            {
                listBox1.Items.Add(String.Format("Device ID: {0}, PNP Device ID: {1}, Description: {2}", usbDevice.DeviceID, usbDevice.PnpDeviceID, usbDevice.Description));                
            }*/
            //MainPS.GetAdressPS(ps_address);
            //MainPS.ConnectPowerSupply(ps_address);
            
            /*if (MainPS.PS_Init())
            {
                label2.ForeColor= System.Drawing.Color.Green;
                label2.Text = "CONNECTED";
            }
            else
            {
                label2.ForeColor = System.Drawing.Color.Red;
                label2.Text = "DISCONNECTED";
            }*/
            /*
            for (int i = 1; i <= 4; i++)
            {
                MainPS.PS_Setup_Current("1", i);
                MainPS.PS_Setup_Voltage("3.7", i);
            }*/
            if(File.Exists(imagefile) && File.Exists(bootfile))
            {
                label10.Text = imagefile;
            }
            else { MessageBox.Show("Image/Boot file not EXISTS!!!", "Warning"); }
            //MainPS.PS_CH_ONOFF(3, true);

            //System.Threading.Thread.Sleep(8000);
            //MainPS.PS_CH_ONOFF(3, false);
            bool connectflag = SQL_Manager.DBConnected(connectSQLDB);
            if(connectflag) 
            {
                label4.ForeColor = Color.Green;
                label4.Text = "CONNECTED";
            }
            else
            {
                label4.ForeColor = Color.Red;
                label4.Text = "DISCONNECTED";
                
            }                     
            
        }

        private void Cmd_Exit_Click(object sender, EventArgs e)
        {
            this.Dispose();
            Application.Exit(); 
        }      

        static List<USBDeviceInfo> GetUSBDevices()
        {
            List<USBDeviceInfo> devices = new List<USBDeviceInfo>();

            ManagementObjectCollection collection;
            using (var searcher = new ManagementObjectSearcher(@"Select * From Win32_USBHub"))
                collection = searcher.Get();

            foreach (var device in collection)
            {
                devices.Add(new USBDeviceInfo(
                (string)device.GetPropertyValue("DeviceID"),
                (string)device.GetPropertyValue("PNPDeviceID"),
                (string)device.GetPropertyValue("Description")
                ));
            }

            collection.Dispose();
            return devices;
        }

        private void textSOM1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == 13)
            {
                Regex mc = new Regex(mac_pattern);
                if (mc.IsMatch(textSOM1.Text))
                {
                    SOM_Serial[0] = textSOM1.Text;
                    
                    textMAIN1.Focus();
                    
                }
                else
                {
                    MessageBox.Show("The SOM Serial is Wrong", "Warning");
                    textSOM1.Clear();
                    textSOM1.Focus();
                }                
            }
        }

        private void textSOM2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                Regex mc = new Regex(mac_pattern);
                if (mc.IsMatch(textSOM2.Text))
                {
                    SOM_Serial[1] = textSOM2.Text;
                    textMAIN2.Focus();

                }
                else
                {
                    MessageBox.Show("The SOM Serial is Wrong", "Warning");
                    textSOM2.Clear(); textSOM2.Focus();
                }
            }
        }

        private void textSOM3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                Regex mc = new Regex(mac_pattern);
                if (mc.IsMatch(textSOM3.Text))
                {
                    SOM_Serial[2] = textSOM3.Text;
                    textMAIN3.Focus();

                }
                else
                {
                    MessageBox.Show("The SOM Serial is Wrong", "Warning");
                    textSOM3.Clear(); textSOM3.Focus();
                }
            }
        }
        private void textSOM4_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                Regex mc = new Regex(mac_pattern);
                if (mc.IsMatch(textSOM4.Text))
                {
                    SOM_Serial[3] = textSOM4.Text;
                    textMAIN4.Focus();

                }
                else
                {
                    MessageBox.Show("The SOM Serial is Wrong", "Warning");
                    textSOM4.Clear(); textSOM4.Focus();
                }
            }
        }

        private void textMAIN1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == 13) 
            {
                Regex gr = new Regex(pattern);
                if (gr.IsMatch(textMAIN1.Text))
                {
                    MB_Serial[0] = textMAIN1.Text;
                    statflag[0] = SQL_Manager.FindMB_Status(connectSQLDB, MB_Serial[0]);
                    if (statflag[0] == 1)
                    {
                        textSOM2.Focus(); 
                        Cmd_Burn1.Enabled = true;
                    }
                    else if (statflag[0] == 0)
                    {
                        MessageBox.Show("THIS MAINBOARD PCBA TEST LAST FINAL RESULT IS FAIL", "Warning");
                    }
                    else if (statflag[0] == 2)
                    {
                        DialogResult mflag = MessageBox.Show("THIS MAIN BOARD NOT TESTED! DO YOU WANT TO PAIR IT WITH SOM?", "Warning", MessageBoxButtons.YesNo);
                        if(mflag == DialogResult.Yes)
                            statflag[0] = 1;
                        else
                            statflag[0] = 0;
                        
                    }


                }
                else
                {
                    MessageBox.Show("The MAIN BOARD Serial is Wrong", "Warning");
                    textMAIN1.Clear(); textMAIN1.Focus();
                }
            }
        }

        private void textMAIN2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                Regex gr = new Regex(pattern);
                if (gr.IsMatch(textMAIN2.Text))
                {
                    MB_Serial[1] = textMAIN2.Text;
                    textSOM3.Focus();
                    Cmd_Burn2.Enabled = true;
                }
                else
                {
                    MessageBox.Show("The MAIN BOARD Serial is Wrong", "Warning");
                    textMAIN2.Clear(); textMAIN2.Focus();
                }
            }
        }
        private int BurnTest(int status, string bootf, string imagef)
        {
            return 0;
        }

        private void textMAIN3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                Regex gr = new Regex(pattern);
                if (gr.IsMatch(textMAIN3.Text))
                {
                    MB_Serial[2] = textMAIN3.Text;
                    textSOM3.Focus();
                    Cmd_Burn3.Enabled = true;
                }
                else
                {
                    MessageBox.Show("The MAIN BOARD Serial is Wrong", "Warning");
                    textMAIN3.Clear(); textMAIN3.Focus();
                }
            }
        }

        private void textMAIN4_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                Regex gr = new Regex(pattern);
                if (gr.IsMatch(textMAIN4.Text))
                {
                    MB_Serial[3] = textMAIN4.Text;
                    
                    Cmd_Burn4.Enabled = true;
                }
                else
                {
                    MessageBox.Show("The MAIN BOARD Serial is Wrong", "Warning");
                    textMAIN4.Clear(); textMAIN4.Focus();
                }
            }
        }

        private void Cmd_Burn1_Click(object sender, EventArgs e)
        {
            if (statflag[0] == 1)
            {

                DialogResult dialogResult = MessageBox.Show("CONNECT FIRST STAND USB TO MAIN BOARD", "Warning", MessageBoxButtons.OKCancel);
                if (dialogResult == DialogResult.OK)
                {

                }
                else
                {
                    textMAIN1.Clear();
                    textSOM1.Clear();
                    Cmd_Burn1.Enabled = false;
                }
            }
        }
            
    }

        
}



/*
 * 
 * 
 * DeviceInformation.FindAllAsync()
 * 
 * https://stackoverflow.com/questions/6416931/can-the-physical-usb-port-be-identified-programmatically-for-a-device-in-windows
 * 
 * 
 * 
 * 
 * 
 * namespace ConsoleApplication1
{
  using System;
  using System.Collections.Generic;
  using System.Management; // need to add System.Management to your project references.

  class Program
  {
    static void Main(string[] args)
    {
      var usbDevices = GetUSBDevices();

      foreach (var usbDevice in usbDevices)
      {
        Console.WriteLine("Device ID: {0}, PNP Device ID: {1}, Description: {2}",
            usbDevice.DeviceID, usbDevice.PnpDeviceID, usbDevice.Description);
      }

      Console.Read();
    }

    static List<USBDeviceInfo> GetUSBDevices()
    {
      List<USBDeviceInfo> devices = new List<USBDeviceInfo>();

      ManagementObjectCollection collection;
      using (var searcher = new ManagementObjectSearcher(@"Select * From Win32_USBHub"))
        collection = searcher.Get();      

      foreach (var device in collection)
      {
        devices.Add(new USBDeviceInfo(
        (string)device.GetPropertyValue("DeviceID"),
        (string)device.GetPropertyValue("PNPDeviceID"),
        (string)device.GetPropertyValue("Description")
        ));
      }

      collection.Dispose();
      return devices;
    }
  }

  class USBDeviceInfo
  {
    public USBDeviceInfo(string deviceID, string pnpDeviceID, string description)
    {
      this.DeviceID = deviceID;
      this.PnpDeviceID = pnpDeviceID;
      this.Description = description;
    }
    public string DeviceID { get; private set; }
    public string PnpDeviceID { get; private set; }
    public string Description { get; private set; }
  }
}
 * 
 * */