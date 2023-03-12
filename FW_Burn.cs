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
        string imagefw = System.Configuration.ConfigurationManager.AppSettings["imageversion"].ToString();
        //string ps_address = string.Empty;
        string pattern = @"\AFI-\d{4}-MB\d{5}\Z";
        string mac_pattern = @"\A[0-9A-Fa-f]{12}\Z"; //\A^[a-fA-F0-9]{12}\Z

        string[] SOM_Serial = new string[4];
        string[] MB_Serial = new string[4];
        int[] statflag = new int[4];

        string imagefile = System.Configuration.ConfigurationManager.AppSettings["imagefile"].ToString(); //@"C:\BurnImage\ight.img";
        string bootfile = System.Configuration.ConfigurationManager.AppSettings["bootfile"].ToString();

        
        SQL_Driver SQL_Manager = new SQL_Driver();
        
        public FW_Burn()
        {
            InitializeComponent();          
                        
            var usbDevices = GetUSBDevices();
            label11.Text = string.Empty;
            /*
            foreach (var usbDevice in usbDevices)
            {
                listBox1.Items.Add(String.Format("Device ID: {0}, PNP Device ID: {1}, Description: {2}", usbDevice.DeviceID, usbDevice.PnpDeviceID, usbDevice.Description));                
            }*/
                       
            
            if(File.Exists(imagefile) && File.Exists(bootfile))
            {
                label10.Text = imagefile;
            }
            else { MessageBox.Show("Image/Boot file not EXISTS!!!", "Warning"); }
            
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

        private void ShowLabel(int stat)
        {
            if(stat == 0)
            {
                label11.Text = "CONNECT USB TO THE FIRST UNIT";
            }
            else if(stat == 1) 
            {
                label11.Text = "DISCONNECT USB FROM THE FIRST UNIT";
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
                        MessageBox.Show("THIS MAINBOARD PCBA TEST LAST FINAL RESULT IS FAIL!!", "Warning");
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
        private int BurnTest(string bootf, string imagef)
        {
            try
            {
                string commt = @"/C C:\BurnImage\uuu.exe -b -emmc_all " + bootf + " " + imagef;
                // create the ProcessStartInfo using "cmd" as the program to be run, and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows, and then exit.
                System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", commt);
                // The following commands are needed to redirect the standard output. 
                //This means that it will be redirected to the Process.StandardOutput StreamReader.
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.WorkingDirectory = @"C:\BurnImage\";
                procStartInfo.UseShellExecute = false;
                // Do not create the black window.
                procStartInfo.CreateNoWindow = false;
                // Now we create a process, assign its ProcessStartInfo and start it
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                   
                
                proc.Start();

                // Get the output into a string
                string result = proc.StandardOutput.ReadToEnd();

                // Display the command output.
                Console.WriteLine(result);
            }
            catch (Exception objException)
            {
                // Log the exception
                Console.WriteLine("ExecuteCommandSync failed" + objException.Message);
            }

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
            int fmb = SQL_Manager.FindMB_Pair(connectSQLDB, MB_Serial[0]);
            if (fmb == 1)
            {
                DialogResult m1 = MessageBox.Show("THIS MAINBOARD ALREADY PAIRED, DO YOU WANT TO UPDATE PAIRING?","INFO",MessageBoxButtons.YesNo);
                if(m1 == DialogResult.Yes) 
                {
                    //MessageBox.Show("CONNECT FIRST STAND USB TO MAINBOARD","INFO");
                    ShowLabel(0);
                    int pair1 = BurnTest(bootfile, imagefile);
                    SQL_Manager.UpdatePairing(connectSQLDB, MB_Serial[0], SOM_Serial[0], imagefw, pair1);
                }

            }
            else if(fmb == -1) 
            {
                if (statflag[0] == 1)
                {
                    ShowLabel(0);
                    //MessageBox.Show("CONNECT FIRST STAND USB TO MAIN BOARD", "INFO");
                    int pair1 = BurnTest(bootfile, imagefile);
                    SQL_Manager.SAVE_Pairing(connectSQLDB, MB_Serial[0], SOM_Serial[0],imagefw, pair1);


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
 * https://stackoverflow.com/questions/5367557/how-to-parse-command-line-output-from-c
 * https://stackoverflow.com/questions/206323/how-to-execute-command-line-in-c-get-std-out-results
 * https://www.codeproject.com/Articles/170017/Solving-Problems-of-Monitoring-Standard-Output-and
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
 * 
 * 
 * 
 * public static void ExecuteCommandSync(object command)
    {
        try
        {
            // create the ProcessStartInfo using "cmd" as the program to be run, and "/c " as the parameters.
            // Incidentally, /c tells cmd that we want it to execute the command that follows, and then exit.
            System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("cmd", "/c " + command);
            // The following commands are needed to redirect the standard output. 
            //This means that it will be redirected to the Process.StandardOutput StreamReader.
            procStartInfo.RedirectStandardOutput =  true;
            procStartInfo.UseShellExecute = false;
            // Do not create the black window.
            procStartInfo.CreateNoWindow = true;
            // Now we create a process, assign its ProcessStartInfo and start it
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo = procStartInfo;
            proc.Start();

            // Get the output into a string
            string result = proc.StandardOutput.ReadToEnd();

            // Display the command output.
            Console.WriteLine(result);
        }
        catch (Exception objException)
        {
            // Log the exception
            Console.WriteLine("ExecuteCommandSync failed" + objException.Message);
        }
    }

    /// <summary>
    /// Execute the command Asynchronously.
    /// </summary>
    /// <param name="command">string command.</param>
    public static void ExecuteCommandAsync(string command)
    {
        try
        {
            //Asynchronously start the Thread to process the Execute command request.
            Thread objThread = new Thread(new ParameterizedThreadStart(ExecuteCommandSync));
            //Make the thread as background thread.
            objThread.IsBackground = true;
            //Set the Priority of the thread.
            objThread.Priority = ThreadPriority.AboveNormal;
            //Start the thread.
            objThread.Start(command);
        }
        catch (ThreadStartException )
        {
            // Log the exception
        }
        catch (ThreadAbortException )
        {
            // Log the exception
        }
        catch (Exception )
        {
            // Log the exception
        }
    }

}
 * */