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
using System.Diagnostics;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using System.Security.Cryptography;

namespace FW_Burn
{
    public partial class FW_Burn : Form
    {
        string connectSQLDB = System.Configuration.ConfigurationManager.ConnectionStrings["DBConnectionString"].ToString();
        string imagefw = System.Configuration.ConfigurationManager.AppSettings["imageversion"].ToString();
        //string ps_address = string.Empty;
        string pattern = @"\AFI-\d{4}-MB\d{5}\Z";
        string mac_pattern = @"\A[0-9A-Fa-f]{12}\Z"; //\A^[a-fA-F0-9]{12}\Z

        string usr = string.Empty;
        string pwd = string.Empty;
        string[] SOM_Serial = new string[4];
        string[] MB_Serial = new string[4];
        int[] statflag = new int[4];

        string imagefile = System.Configuration.ConfigurationManager.AppSettings["imagefile"].ToString(); //@"C:\BurnImage\ight.img";
        string bootfile = System.Configuration.ConfigurationManager.AppSettings["bootfile"].ToString();

                

        delegate void SetProgressBarCallback(int countnum);
        delegate void SetLabelCallback(string countnum);

        delegate void SetProgressBarCallback2(int countnum2);
        delegate void SetLabelCallback2(string countnum2);

        delegate void SetProgressBarCallback3(int countnum3);
        delegate void SetLabelCallback3(string countnum3);

        delegate void SetProgressBarCallback4(int countnum4);
        delegate void SetLabelCallback4(string countnum4);

        

        public TaskCompletionSource<bool> eventHandled = new TaskCompletionSource<bool>();
        public TaskCompletionSource<bool> eventHandled2 = new TaskCompletionSource<bool>();
        public TaskCompletionSource<bool> eventHandled3 = new TaskCompletionSource<bool>();
        public TaskCompletionSource<bool> eventHandled4 = new TaskCompletionSource<bool>();

        SQL_Driver SQL_Manager = new SQL_Driver();        

        public FW_Burn()
        {
            InitializeComponent();          
                        
            var usbDevices = GetUSBDevices();
            labelSOM1.Text = string.Empty;
            this.ActiveControl = textBox1;
            textBox1.Focus();
            /*
            foreach (var usbDevice in usbDevices)
            {
                listBox1.Items.Add(String.Format("Device ID: {0}, PNP Device ID: {1}, Description: {2}", usbDevice.DeviceID, usbDevice.PnpDeviceID, usbDevice.Description));                
            }*/                       
            
            if(File.Exists(imagefile) && File.Exists(bootfile))
            {
                label10.Text = imagefile;
                label12.Text = imagefw;
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

        private void ShowLabel(int labelnum, int stat)
        {
            switch(labelnum)
            {
                case 0:
                    if (stat == 0)
                    {
                        lbl_Info1.Text = "CONNECT USB TO THE FIRST UNIT";
                    }
                    else if (stat == 1)
                    {
                        lbl_Info1.Text = "DISCONNECT USB FROM THE FIRST UNIT";
                    }
                    break;
                case 1:
                    if (stat == 0)
                    {
                        lbl_Info2.Text = "CONNECT USB TO THE SECOND UNIT";
                    }
                    else if (stat == 1)
                    {
                        lbl_Info2.Text = "DISCONNECT USB FROM THE SECOND UNIT";
                    }
                    break;
                case 2:
                    if (stat == 0)
                    {
                        lbl_Info3.Text = "CONNECT USB TO THE THIRD UNIT";
                    }
                    else if (stat == 1)
                    {
                        lbl_Info3.Text = "DISCONNECT USB FROM THE THIRD UNIT";
                    }
                    break;
                case 3:
                    if (stat == 0)
                    {
                        lbl_Info4.Text = "CONNECT USB TO THE FOURTH UNIT";
                    }
                    else if (stat == 1)
                    {
                        lbl_Info4.Text = "DISCONNECT USB FROM THE FOURTH UNIT";
                    }
                    break;
            }

            
        }

        //first progressbar
        private void SetProgressBar(int cnum)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.progressBar1.InvokeRequired)
            {
                SetProgressBarCallback d = new SetProgressBarCallback(SetProgressBar);
                this.Invoke(d, new object[] { cnum });
            }
            else
            {
                
                this.progressBar1.Value = cnum;
            }
        }
        //second progressbar
        private void SetProgressBar2(int cnum2)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.progressBar2.InvokeRequired)
            {
                SetProgressBarCallback2 d2 = new SetProgressBarCallback2(SetProgressBar2);
                this.Invoke(d2, new object[] { cnum2 });
            }
            else
            {

                this.progressBar2.Value = cnum2;
            }
        }

        private void SetProgressBar3(int cnum3)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.progressBar3.InvokeRequired)
            {
                SetProgressBarCallback3 d3 = new SetProgressBarCallback3(SetProgressBar3);
                this.Invoke(d3, new object[] { cnum3 });
            }
            else
            {

                this.progressBar3.Value = cnum3;
            }
        }
        private void SetProgressBar4(int cnum4)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.progressBar4.InvokeRequired)
            {
                SetProgressBarCallback4 d4 = new SetProgressBarCallback4(SetProgressBar4);
                this.Invoke(d4, new object[] { cnum4 });
            }
            else
            {

                this.progressBar4.Value = cnum4;
            }
        }

        private void SetLabel(string cnum)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.Status1.InvokeRequired)
            {
                SetLabelCallback d = new SetLabelCallback(SetLabel);
                this.Invoke(d, new object[] { cnum });
            }
            else
            {
                if (cnum.Contains("DONE"))
                {
                    this.Status1.Text = cnum;
                    this.button1.Enabled = true;
                }
                else
                    this.Status1.Text = cnum + "%";
            }
        }
        private void SetLabel2(string cnum2)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.Status2.InvokeRequired)
            {
                SetLabelCallback2 d2 = new SetLabelCallback2(SetLabel2);
                this.Invoke(d2, new object[] { cnum2 });
            }
            else
            {
                if (cnum2.Contains("DONE"))
                {
                    this.Status2.Text = cnum2;
                    this.button2.Enabled = true;
                }
                else
                    this.Status2.Text = cnum2 + "%";
            }
        }

        private void SetLabel3(string cnum3)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.Status3.InvokeRequired)
            {
                SetLabelCallback3 d3 = new SetLabelCallback3(SetLabel3);
                this.Invoke(d3, new object[] { cnum3 });
            }
            else
            {
                if (cnum3.Contains("DONE"))
                {
                    this.Status3.Text = cnum3;
                    this.button3.Enabled = true;
                }
                else
                    this.Status3.Text = cnum3 + "%";
            }
        }

        private void SetLabel4(string cnum4)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.Status4.InvokeRequired)
            {
                SetLabelCallback4 d4 = new SetLabelCallback4(SetLabel4);
                this.Invoke(d4, new object[] { cnum4 });
            }
            else
            {
                if (cnum4.Contains("DONE"))
                {
                    this.Status4.Text = cnum4;
                    this.button4.Enabled = true;
                }
                else
                    this.Status4.Text = cnum4 + "%";
            }
        }

        private void ShowMAIN(int mainnum, int state)
        {
            switch (mainnum)
            {
                case 0:
                    if (state == 0)
                    {
                        labelMAIN1.ForeColor= Color.Red;
                        labelMAIN1.Text = "FAILED";
                    }
                    else if (state == 1)
                    {
                        labelMAIN1.ForeColor = Color.Green;
                        labelMAIN1.Text = "PASSED";
                    }
                    else
                    {
                        labelMAIN1.ForeColor = Color.Blue;
                        labelMAIN1.Text = "NOT TESTED";
                    }
                    break;
                case 1:
                    if (state == 0)
                    {
                        labelMAIN2.ForeColor = Color.Red;
                        labelMAIN2.Text = "FAILED";
                    }
                    else if (state == 1)
                    {
                        labelMAIN2.ForeColor = Color.Green;
                        labelMAIN2.Text = "PASSED";
                    }
                    else
                    {
                        labelMAIN2.ForeColor = Color.Blue;
                        labelMAIN2.Text = "NOT TESTED";
                    }
                    break;
                case 2:
                    if (state == 0)
                    {
                        labelMAIN3.ForeColor = Color.Red;
                        labelMAIN3.Text = "FAILED";
                    }
                    else if (state == 1)
                    {
                        labelMAIN3.ForeColor = Color.Green;
                        labelMAIN3.Text = "PASSED";
                    }
                    else
                    {
                        labelMAIN3.ForeColor = Color.Blue;
                        labelMAIN3.Text = "NOT TESTED";
                    }
                    break;
                case 3:
                    if (state == 0)
                    {
                        labelMAIN4.ForeColor = Color.Red;
                        labelMAIN4.Text = "FAILED";
                    }
                    else if (state == 1)
                    {
                        labelMAIN4.ForeColor = Color.Green;
                        labelMAIN4.Text = "PASSED";
                    }
                    else
                    {
                        labelMAIN4.ForeColor = Color.Blue;
                        labelMAIN4.Text = "NOT TESTED";
                    }
                    break;
            }
        }

        private void ShowSOM(int somnum, int state)
        {
            switch(somnum)
            {
                case 0:
                    if (state == 0)
                    {
                        labelSOM1.ForeColor= Color.Red;
                        labelSOM1.Text = "Not Verified";
                    }
                    else if (state == 1)
                    {
                        labelSOM1.ForeColor= Color.Green;
                        labelSOM1.Text = "Verified";
                    }
                    break;
                case 1:
                    if (state == 0)
                    {
                        labelSOM2.ForeColor= Color.Red;
                        labelSOM2.Text = "Not Verified";
                    }
                    else if (state == 1)
                    {
                        labelSOM2.ForeColor = Color.Green;
                        labelSOM2.Text = "Verified";
                    }
                    break;
                case 2:
                    if (state == 0)
                    {
                        labelSOM3.ForeColor = Color.Red;
                        labelSOM3.Text = "Not Verified";
                    }
                    else if (state == 1)
                    {
                        labelSOM3.ForeColor = Color.Green;
                        labelSOM3.Text = "Verified";
                    }
                    break;
                case 3:
                    if (state == 0)
                    {
                        labelSOM4.ForeColor = Color.Red;
                        labelSOM4.Text = "Not Verified";
                    }
                    else if (state == 1)
                    {
                        labelSOM4.ForeColor = Color.Green;
                        labelSOM4.Text = "Verified";
                    }
                    break;
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
                    ShowSOM(0, 1);
                    textMAIN1.Focus();
                    
                }
                else
                {
                    MessageBox.Show("The SOM Serial is Wrong", "Warning");
                    textSOM1.Clear(); textSOM1.Focus(); labelSOM1.Text = "";
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
                    ShowSOM(1, 1);
                    textMAIN2.Focus();

                }
                else
                {
                    MessageBox.Show("The SOM Serial is Wrong", "Warning");
                    textSOM2.Clear(); textSOM2.Focus(); labelSOM2.Text = "";
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
                    ShowSOM(2, 1);
                    textMAIN3.Focus();

                }
                else
                {
                    MessageBox.Show("The SOM Serial is Wrong", "Warning");
                    textSOM3.Clear(); textSOM3.Focus(); labelSOM3.Text = "";
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
                    ShowSOM(3, 1);
                    textMAIN4.Focus();

                }
                else
                {
                    MessageBox.Show("The SOM Serial is Wrong", "Warning");
                    textSOM4.Clear(); textSOM4.Focus(); labelSOM4.Text = "";
                }
            }
        }
               

        static void cmd_Error(object sender, DataReceivedEventArgs e)
        {
            //Console.WriteLine("Error from other process");
            //Console.WriteLine(e.Data);
        }

        private async Task RunWithRedirect(string arguments)
        {
            Process proc = new Process();
        
            int exitCode;
            
            ProcessStartInfo startInfo = new ProcessStartInfo();
            proc.StartInfo = startInfo;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = arguments;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.StandardOutputEncoding = Encoding.UTF8;
            startInfo.CreateNoWindow = true;

            proc.ErrorDataReceived += proc_DataReceived;
            proc.OutputDataReceived += proc_DataReceived;
            proc.Exited += new EventHandler(proc_Exited);
            proc.Start();

            proc.BeginOutputReadLine();

            
            await Task.WhenAny(eventHandled.Task);
            //exitCode = proc.ExitCode;
           
        }

        private async Task RunWithRedirect2(string arguments2)
        {
            Process proc2 = new Process();
        
            int exitCode2;
            
            ProcessStartInfo startInfo2 = new ProcessStartInfo();
            proc2.StartInfo = startInfo2;
            startInfo2.FileName = "cmd.exe";
            startInfo2.Arguments = arguments2;
            startInfo2.UseShellExecute = false;
            startInfo2.RedirectStandardOutput = true;
            startInfo2.StandardOutputEncoding = Encoding.UTF8;
            startInfo2.CreateNoWindow = true;

            proc2.ErrorDataReceived += proc2_DataReceived;
            proc2.OutputDataReceived += proc2_DataReceived;
            proc2.Exited += new EventHandler(proc2_Exited);
            proc2.Start();

            proc2.BeginOutputReadLine();


            await Task.WhenAny(eventHandled2.Task);
            exitCode2 = proc2.ExitCode;
            
        }

        private async Task RunWithRedirect3(string arguments3)
        {
            Process proc3 = new Process();
            int exitCode3;
            
            ProcessStartInfo startInfo3 = new ProcessStartInfo();
            proc3.StartInfo = startInfo3;
            startInfo3.FileName = "cmd.exe";
            startInfo3.Arguments = arguments3;
            startInfo3.UseShellExecute = false;
            startInfo3.RedirectStandardOutput = true;
            startInfo3.StandardOutputEncoding = Encoding.UTF8;
            startInfo3.CreateNoWindow = true;

            proc3.ErrorDataReceived += proc3_DataReceived;
            proc3.OutputDataReceived += proc3_DataReceived;
            proc3.Exited += new EventHandler(proc3_Exited);
            proc3.Start();

            proc3.BeginOutputReadLine();


            await Task.WhenAny(eventHandled3.Task);
            exitCode3 = proc3.ExitCode;
            
        }

        private async Task RunWithRedirect4(string arguments4)
        {
            Process proc4 = new Process();
            int exitCode4;
            
            ProcessStartInfo startInfo4 = new ProcessStartInfo();
            proc4.StartInfo = startInfo4;
            startInfo4.FileName = "cmd.exe";
            startInfo4.Arguments = arguments4;
            startInfo4.UseShellExecute = false;
            startInfo4.RedirectStandardOutput = true;
            startInfo4.StandardOutputEncoding = Encoding.UTF8;
            startInfo4.CreateNoWindow = true;

            proc4.ErrorDataReceived += proc4_DataReceived;
            proc4.OutputDataReceived += proc4_DataReceived;
            proc4.Exited += new EventHandler(proc4_Exited);
            proc4.Start();

            proc4.BeginOutputReadLine();


            await Task.WhenAny(eventHandled4.Task);
            exitCode4 = proc4.ExitCode;
            
        }
        private void ParseOutString(string datastring)
        {
            if(datastring != string.Empty)
            {
                if (datastring.Contains("1:181>Start Cmd:FB: ucmd mmc partconf ${emmc_dev} ${emmc_ack} 1 0"))
                {
                    SetLabel("DONE");
                    
                }
                else if (datastring.Contains("1:181>Start Cmd:FB: done"))
                {
                    SetLabel("DONE");
                    
                }
            }
        }

        private void ParseOutString2(string datastring2)
        {
            if (datastring2 != string.Empty)
            {
                if (datastring2.Contains("1:182>Start Cmd:FB: ucmd mmc partconf ${emmc_dev} ${emmc_ack} 1 0"))
                {
                    SetLabel2("DONE");
                    //proc.Kill();
                    //textSOM1.Clear();
                    //textMAIN1.Clear();
                }
                else if (datastring2.Contains("1:182>Start Cmd:FB: done"))
                {
                    SetLabel2("DONE");

                }
            }
        }

        private void ParseOutString3(string datastring3)
        {
            if (datastring3 != string.Empty)
            {
                if (datastring3.Contains("1:183>Start Cmd:FB: ucmd mmc partconf ${emmc_dev} ${emmc_ack} 1 0"))
                {
                    SetLabel3("DONE");
                    
                }
                else if (datastring3.Contains("1:183>Start Cmd:FB: done"))
                {
                    SetLabel3("DONE");

                }
            }
        }

        private void ParseOutString4(string datastring4)
        {
            if (datastring4 != string.Empty)
            {
                if (datastring4.Contains("1:181>Start Cmd:FB: ucmd mmc partconf ${emmc_dev} ${emmc_ack} 1 0"))
                {
                    SetLabel4("DONE");
                    
                }
                else if (datastring4.Contains("1:184>Start Cmd:FB: done"))
                {
                    SetLabel4("DONE");

                }
            }
        }

        void proc_DataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                string oot = Environment.NewLine + e.Data;
                if(oot.Contains("%"))
                {
                    string[] kt = oot.Split('%');
                    SetProgressBar(int.Parse(kt[0]));
                    SetLabel(kt[0]);
                    
                }
                else if(oot.Contains("1:181>Start Cmd:FB: ucmd mmc partconf ${emmc_dev} ${emmc_ack} 1 0"))
                {
                    ParseOutString(oot);
                    
                }
                else if(oot.Contains("1:181>Start Cmd:FB: done"))
                {
                    ParseOutString(oot);
                }
            }
                //BeginInvoke(new Action(() => richTextBox1.Text += (Environment.NewLine + e.Data))); //textOut
        }

        void proc2_DataReceived(object sender, DataReceivedEventArgs e2)
        {
            if (e2.Data != null)
            {
                string oot2 = Environment.NewLine + e2.Data;
                if (oot2.Contains("%"))
                {
                    string[] kt2 = oot2.Split('%');
                    SetProgressBar2(int.Parse(kt2[0]));
                    SetLabel2(kt2[0]);

                }
                else if (oot2.Contains("1:182>Start Cmd:FB: ucmd mmc partconf ${emmc_dev} ${emmc_ack} 1 0"))
                {
                    ParseOutString2(oot2);

                }
                else if (oot2.Contains("1:182>Start Cmd:FB: done"))
                {
                    ParseOutString2(oot2);
                }
            }
            
        }

        void proc3_DataReceived(object sender, DataReceivedEventArgs e3)
        {
            if (e3.Data != null)
            {
                string oot3 = Environment.NewLine + e3.Data;
                if (oot3.Contains("%"))
                {
                    string[] kt3 = oot3.Split('%');
                    SetProgressBar3(int.Parse(kt3[0]));
                    SetLabel3(kt3[0]);

                }
                else if (oot3.Contains("1:183>Start Cmd:FB: ucmd mmc partconf ${emmc_dev} ${emmc_ack} 1 0"))
                {
                    ParseOutString3(oot3);

                }
                else if (oot3.Contains("1:183>Start Cmd:FB: done"))
                {
                    ParseOutString3(oot3);
                }
            }

        }

        void proc4_DataReceived(object sender, DataReceivedEventArgs e4)
        {
            if (e4.Data != null)
            {
                string oot4 = Environment.NewLine + e4.Data;
                if (oot4.Contains("%"))
                {
                    string[] kt4 = oot4.Split('%');
                    SetProgressBar4(int.Parse(kt4[0]));
                    SetLabel4(kt4[0]);

                }
                else if (oot4.Contains("1:184>Start Cmd:FB: ucmd mmc partconf ${emmc_dev} ${emmc_ack} 1 0"))
                {
                    ParseOutString4(oot4);

                }
                else if (oot4.Contains("1:184>Start Cmd:FB: done"))
                {
                    ParseOutString4(oot4);
                }
            }

        }


        // Handle Exited event and display process information.
        private void proc_Exited(object sender, System.EventArgs e)
        {
            //string stpak = String.Format("Exit time: {0}, Exit code: {1}, Elapsed time: {2}", proc.ExitTime, proc.ExitCode, Math.Round((proc.ExitTime - proc.StartTime).TotalMilliseconds));
                
            bool tty = eventHandled.TrySetResult(true);
            
        }

        private void proc2_Exited(object sender, System.EventArgs e)
        {
            //string stpak = String.Format("Exit time: {0}, Exit code: {1}, Elapsed time: {2}", proc.ExitTime, proc.ExitCode, Math.Round((proc.ExitTime - proc.StartTime).TotalMilliseconds));

            bool rtt2 = eventHandled2.TrySetResult(true);
            
        }
        private void proc3_Exited(object sender, System.EventArgs e)
        {
            //string stpak = String.Format("Exit time: {0}, Exit code: {1}, Elapsed time: {2}", proc.ExitTime, proc.ExitCode, Math.Round((proc.ExitTime - proc.StartTime).TotalMilliseconds));

            bool rtt3 = eventHandled3.TrySetResult(true);

        }
        private void proc4_Exited(object sender, System.EventArgs e)
        {
            //string stpak = String.Format("Exit time: {0}, Exit code: {1}, Elapsed time: {2}", proc.ExitTime, proc.ExitCode, Math.Round((proc.ExitTime - proc.StartTime).TotalMilliseconds));

            bool rtt4 = eventHandled4.TrySetResult(true);

        }
        private void textMAIN1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                Regex gr = new Regex(pattern);
                if (gr.IsMatch(textMAIN1.Text))
                {
                    MB_Serial[0] = textMAIN1.Text;
                    statflag[0] = SQL_Manager.FindMB_Status(connectSQLDB, MB_Serial[0]);
                    if (statflag[0] == 1)
                    {
                        textSOM2.Focus(); ShowMAIN(0, statflag[0]); //verified serial number 
                        ShowLabel(0, 0);
                        Cmd_Burn1.Enabled = true;
                    }
                    else if (statflag[0] == 0)
                    {
                        MessageBox.Show("THIS MAINBOARD PCBA TEST LAST FINAL RESULT IS FAIL!!", "Warning");
                    }
                    else if (statflag[0] == 2)
                    {
                        DialogResult mflag = MessageBox.Show("THIS MAIN BOARD NOT TESTED! DO YOU WANT TO PAIR IT WITH SOM?", "Warning", MessageBoxButtons.YesNo);
                        if (mflag == DialogResult.Yes)
                        {
                            //statflag[0] = 1;
                            ShowMAIN(0, statflag[0]); //verified serial number
                            ShowLabel(0, 0);
                            Cmd_Burn1.Enabled = true;
                            Cmd_Burn1.Focus();
                        }
                        else
                            statflag[0] = 0;

                    }


                }
                else
                {
                    MessageBox.Show("The MAIN BOARD Serial is Wrong", "Warning");
                    textMAIN1.Clear(); textMAIN1.Focus(); lbl_Info1.Text = ""; labelMAIN1.Text = "";
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
                    statflag[1] = SQL_Manager.FindMB_Status(connectSQLDB, MB_Serial[1]);
                    if (statflag[1] == 1)
                    {
                        textSOM3.Focus(); ShowMAIN(1, statflag[1]); //verified serial number 
                        ShowLabel(1, 0);
                        Cmd_Burn2.Enabled = true;
                    }
                    else if (statflag[1] == 0)
                    {
                        MessageBox.Show("THIS MAINBOARD PCBA TEST LAST FINAL RESULT IS FAIL!!", "Warning");
                    }
                    else if (statflag[1] == 2)
                    {
                        DialogResult mflag = MessageBox.Show("THIS MAIN BOARD NOT TESTED! DO YOU WANT TO PAIR IT WITH SOM?", "Warning", MessageBoxButtons.YesNo);
                        if (mflag == DialogResult.Yes)
                        {
                            //statflag[0] = 1;
                            ShowMAIN(1, statflag[1]); //verified serial number
                            ShowLabel(1, 0);
                            Cmd_Burn2.Enabled = true;
                            Cmd_Burn2.Focus();
                        }
                        else
                            statflag[1] = 0;

                    }


                }
                else
                {
                    MessageBox.Show("The MAIN BOARD Serial is Wrong", "Warning");
                    textMAIN2.Clear(); textMAIN2.Focus(); lbl_Info2.Text = ""; labelMAIN2.Text = "";
                }
            }
        }
        private void textMAIN3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                Regex gr = new Regex(pattern);
                if (gr.IsMatch(textMAIN3.Text))
                {
                    MB_Serial[2] = textMAIN3.Text;
                    statflag[2] = SQL_Manager.FindMB_Status(connectSQLDB, MB_Serial[2]);
                    if (statflag[2] == 1)
                    {
                        textSOM4.Focus(); ShowMAIN(2, statflag[2]); //verified serial number 
                        ShowLabel(2, 0);
                        Cmd_Burn3.Enabled = true;
                    }
                    else if (statflag[2] == 0)
                    {
                        MessageBox.Show("THIS MAINBOARD PCBA TEST LAST FINAL RESULT IS FAIL!!", "Warning");
                    }
                    else if (statflag[2] == 2)
                    {
                        DialogResult mflag = MessageBox.Show("THIS MAIN BOARD NOT TESTED! DO YOU WANT TO PAIR IT WITH SOM?", "Warning", MessageBoxButtons.YesNo);
                        if (mflag == DialogResult.Yes)
                        {
                            ShowMAIN(2, statflag[2]); //verified serial number
                            ShowLabel(2, 0);
                            Cmd_Burn3.Enabled = true;
                            Cmd_Burn3.Focus();
                        }
                        else
                            statflag[2] = 0;
                    }
                }
                else
                {
                    MessageBox.Show("The MAIN BOARD Serial is Wrong", "Warning");
                    textMAIN3.Clear(); textMAIN3.Focus(); lbl_Info3.Text = ""; labelMAIN3.Text = "";
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
                    statflag[3] = SQL_Manager.FindMB_Status(connectSQLDB, MB_Serial[3]);
                    if (statflag[3] == 1)
                    {
                        //textSOM2.Focus();
                        ShowMAIN(3, statflag[3]); //verified serial number 
                        ShowLabel(3, 0);
                        Cmd_Burn4.Enabled = true;
                    }
                    else if (statflag[3] == 0)
                    {
                        MessageBox.Show("THIS MAINBOARD PCBA TEST LAST FINAL RESULT IS FAIL!!", "Warning");
                    }
                    else if (statflag[3] == 2)
                    {
                        DialogResult mflag = MessageBox.Show("THIS MAIN BOARD NOT TESTED! DO YOU WANT TO PAIR IT WITH SOM?", "Warning", MessageBoxButtons.YesNo);
                        if (mflag == DialogResult.Yes)
                        {
                            ShowMAIN(3, statflag[3]); //verified serial number
                            ShowLabel(3, 0);
                            Cmd_Burn4.Enabled = true;
                            Cmd_Burn4.Focus();
                        }
                        else
                            statflag[3] = 0;
                    }
                }
                else
                {
                    MessageBox.Show("The MAIN BOARD Serial is Wrong", "Warning");
                    textMAIN4.Clear(); textMAIN4.Focus(); lbl_Info4.Text = ""; labelMAIN4.Text = "";
                }
            }
        }

        private string cmdtext(int unitnum, string boot, string image)
        {
            string cmdtextstring = string.Empty;
            switch(unitnum)
            {
                case 1:
                    cmdtextstring = @"/C C:\BurnImage\uuu.exe -m 1:21 -m 1:181 -b emmc_all " + boot + " " + image;
                    break;
                case 2:
                    cmdtextstring = @"/C C:\BurnImage\uuu.exe -m 1:22 -m 1:182 -b emmc_all " + boot + " " + image;
                    break;
                case 3:
                    cmdtextstring = @"/C C:\BurnImage\uuu.exe -m 1:23 -m 1:183 -b emmc_all " + boot + " " + image;
                    break;
                case 4:
                    cmdtextstring = @"/C C:\BurnImage\uuu.exe -m 1:24 -m 1:184 -b emmc_all " + boot + " " + image;
                    break;
            }


            return cmdtextstring;
        }
        private void Cmd_Burn1_Click(object sender, EventArgs e)
        {
            
            string cmdline = string.Empty;
            int fmb = SQL_Manager.FindMB_Pair(connectSQLDB, MB_Serial[0]);
            if (fmb == 1)
            {
                DialogResult m1 = MessageBox.Show("THIS MAINBOARD ALREADY PAIRED, DO YOU WANT TO UPDATE PAIRING?","INFO",MessageBoxButtons.YesNo);
                if(m1 == DialogResult.Yes) 
                {
                    Cmd_Burn1.Enabled = false;
                    
                    //cmdline = @"/C C:\BurnImage\uuu.exe -m 1:21 -m 1:181 -b emmc_all C:\BurnImage\imx-boot-sd.bin-mainboard C:\BurnImage\scanner-scanner_image-0.1.2.img";
                    cmdline = cmdtext(1, bootfile, imagefile);
                    _=RunWithRedirect(cmdline);
                   
                    
                }


            }
            else if(fmb == -1) 
            {
                Cmd_Burn1.Enabled = false;
                cmdline = cmdtext(1, bootfile, imagefile);
                _=RunWithRedirect(cmdline);
            }
            
        }
        //username
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == 13)
            {
                usr = textBox1.Text;
                textBox2.Focus();
            }
        }
        //password
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 13)
            {
                pwd = textBox2.Text;
                cmd_Login.Focus();
            }
        }

        private void cmd_Login_Click(object sender, EventArgs e)
        {
            int sd = -1;
            if (textBox1.Text != string.Empty && textBox2.Text != string.Empty)
            {
                sd = SQL_Manager.login_check(connectSQLDB, usr, pwd);

                if (sd < 99 && sd > -1)
                {
                    groupBox1.Enabled = true;
                    textBox1.Enabled = false;
                    textBox2.Enabled = false;
                    cmd_Login.Enabled = false;
                    textSOM1.Focus();
                }
                else
                {
                    MessageBox.Show("The login or password is INCORRECT!!!", "Warning");
                    textBox1.Clear(); textBox2.Clear(); textBox1.Focus();
                }
                
            }
            else
            {
                MessageBox.Show("No data entered", "Warning");
                sd = -1;
                textBox1.Clear(); textBox2.Clear(); textBox1.Focus();
            }
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
                        
            if(Status1.Text == "DONE")
            {
                int  p1 = SQL_Manager.FindMB_Pair(connectSQLDB, MB_Serial[0]);
                if(p1 == 1)
                {
                    SQL_Manager.UpdatePairing(connectSQLDB, MB_Serial[0], SOM_Serial[0], imagefw, 1, usr);
                    Status1.Text = "";
                    ShowLabel(0, 1);
                    textSOM1.Clear();
                    textMAIN1.Clear();
                    progressBar1.Value = 0;
                    labelSOM1.Text = "";
                    labelMAIN1.Text = "";
                }
                else if(p1 == -1)
                {
                    SQL_Manager.SAVE_Pairing(connectSQLDB, MB_Serial[0], SOM_Serial[0], imagefw, 1, usr);
                    Status1.Text = "";
                    ShowLabel(0, 1);
                    textSOM1.Clear();
                    textMAIN1.Clear();
                    progressBar1.Value = 0;
                    labelSOM1.Text = "";
                    labelMAIN1.Text = "";
                }
                button1.Enabled = false;
                
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
            if (Status2.Text == "DONE")
            {
                int p2 = SQL_Manager.FindMB_Pair(connectSQLDB, MB_Serial[1]);
                if (p2 == 1)
                {
                    SQL_Manager.UpdatePairing(connectSQLDB, MB_Serial[1], SOM_Serial[1], imagefw, 1, usr);
                    Status2.Text = "";
                    ShowLabel(1, 1);
                    textSOM2.Clear();
                    textMAIN2.Clear();
                    progressBar2.Value = 0;
                    labelSOM2.Text = "";
                    labelMAIN2.Text = "";
                }
                else if (p2 == -1)
                {
                    SQL_Manager.SAVE_Pairing(connectSQLDB, MB_Serial[1], SOM_Serial[1], imagefw, 1, usr); //version sql driver v18.12.9
                    Status2.Text = "";
                    ShowLabel(1, 1);
                    textSOM2.Clear();
                    textMAIN2.Clear();
                    progressBar2.Value = 0;
                    labelSOM2.Text = "";
                    labelMAIN2.Text = "";
                }
                button2.Enabled = false;

            }
        }

        private void Cmd_Burn2_Click(object sender, EventArgs e)
        {
            string cmdline2 = string.Empty;
            int fmb = SQL_Manager.FindMB_Pair(connectSQLDB, MB_Serial[1]);
            if (fmb == 1)
            {
                DialogResult m1 = MessageBox.Show("THIS MAINBOARD ALREADY PAIRED, DO YOU WANT TO UPDATE PAIRING?", "INFO", MessageBoxButtons.YesNo);
                if (m1 == DialogResult.Yes)
                {
                    Cmd_Burn2.Enabled = false;
                    
                    //cmdline = @"/C C:\BurnImage\uuu.exe -m 1:21 -m 1:181 -b emmc_all C:\BurnImage\imx-boot-sd.bin-mainboard C:\BurnImage\scanner-scanner_image-0.1.2.img";
                    cmdline2 = cmdtext(2, bootfile, imagefile);
                    _=RunWithRedirect2(cmdline2);
                   
                }


            }
            else if (fmb == -1)
            {
                Cmd_Burn2.Enabled = false;
                //MessageBox.Show("CONNECT FIRST STAND USB TO MAINBOARD","INFO");

                //int pair1 = BurnTest(bootfile, imagefile);
                //await BurnTest1(bootfile, imagefile);
                //cmdline = @"/C C:\BurnImage\uuu.exe -m 1:21 -m 1:181 -b emmc_all C:\BurnImage\imx-boot-sd.bin-mainboard C:\BurnImage\scanner-scanner_image-0.1.2.img";
                cmdline2 = cmdtext(2, bootfile, imagefile);
                _=RunWithRedirect2(cmdline2);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            
            if (Status3.Text == "DONE")
            {
                int p3 = SQL_Manager.FindMB_Pair(connectSQLDB, MB_Serial[2]);
                if (p3 == 1)
                {
                    SQL_Manager.UpdatePairing(connectSQLDB, MB_Serial[2], SOM_Serial[2], imagefw, 1, usr);
                    Status3.Text = "";
                    ShowLabel(2, 1);
                    textSOM3.Clear();
                    textMAIN3.Clear();
                    progressBar3.Value = 0;
                    labelSOM3.Text = "";
                    labelMAIN3.Text = "";
                }
                else if (p3 == -1)
                {
                    SQL_Manager.SAVE_Pairing(connectSQLDB, MB_Serial[2], SOM_Serial[2], imagefw, 1, usr);
                    Status3.Text = "";
                    ShowLabel(2, 1);
                    textSOM3.Clear();
                    textMAIN3.Clear();
                    progressBar3.Value = 0;
                    labelSOM3.Text = "";
                    labelMAIN3.Text = "";
                }
                button3.Enabled = false;

            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            
            if (Status4.Text == "DONE")
            {
                int p4 = SQL_Manager.FindMB_Pair(connectSQLDB, MB_Serial[3]);
                if (p4 == 1)
                {
                    SQL_Manager.UpdatePairing(connectSQLDB, MB_Serial[3], SOM_Serial[3], imagefw, 1, usr);
                    Status4.Text = "";
                    ShowLabel(3, 1);
                    textSOM4.Clear();
                    textMAIN4.Clear();
                    progressBar4.Value = 0;
                    labelSOM4.Text = "";
                    labelMAIN4.Text = "";
                }
                else if (p4 == -1)
                {
                    SQL_Manager.SAVE_Pairing(connectSQLDB, MB_Serial[3], SOM_Serial[3], imagefw, 1, usr);
                    Status4.Text = "";
                    ShowLabel(3, 1);
                    textSOM4.Clear();
                    textMAIN4.Clear();
                    progressBar4.Value = 0;
                    labelSOM4.Text = "";
                    labelMAIN4.Text = "";
                }
                button4.Enabled = false;

            }
        }

        private void Cmd_Burn3_Click(object sender, EventArgs e)
        {
            string cmdline3 = string.Empty;
            int fmb = SQL_Manager.FindMB_Pair(connectSQLDB, MB_Serial[2]);
            if (fmb == 1)
            {
                DialogResult m1 = MessageBox.Show("THIS MAINBOARD ALREADY PAIRED, DO YOU WANT TO UPDATE PAIRING?", "INFO", MessageBoxButtons.YesNo);
                if (m1 == DialogResult.Yes)
                {
                    Cmd_Burn3.Enabled = false;
                    
                    //cmdline = @"/C C:\BurnImage\uuu.exe -m 1:21 -m 1:181 -b emmc_all C:\BurnImage\imx-boot-sd.bin-mainboard C:\BurnImage\scanner-scanner_image-0.1.2.img";
                    cmdline3 = cmdtext(3, bootfile, imagefile);
                    _=RunWithRedirect3(cmdline3);
                    
                }


            }
            else if (fmb == -1)
            {
                Cmd_Burn3.Enabled = false;
                
                //cmdline = @"/C C:\BurnImage\uuu.exe -m 1:21 -m 1:181 -b emmc_all C:\BurnImage\imx-boot-sd.bin-mainboard C:\BurnImage\scanner-scanner_image-0.1.2.img";
                cmdline3 = cmdtext(3, bootfile, imagefile);
               _= RunWithRedirect3(cmdline3);
            }
        }

        private void Cmd_Burn4_Click(object sender, EventArgs e)
        {
            string cmdline4 = string.Empty;
            int fmb = SQL_Manager.FindMB_Pair(connectSQLDB, MB_Serial[3]);
            if (fmb == 1)
            {
                DialogResult m1 = MessageBox.Show("THIS MAINBOARD ALREADY PAIRED, DO YOU WANT TO UPDATE PAIRING?", "INFO", MessageBoxButtons.YesNo);
                if (m1 == DialogResult.Yes)
                {
                    Cmd_Burn4.Enabled = false;
                    //MessageBox.Show("CONNECT FIRST STAND USB TO MAINBOARD","INFO");

                    //int pair1 = BurnTest(bootfile, imagefile);
                    //await BurnTest1(bootfile, imagefile);
                    //cmdline = @"/C C:\BurnImage\uuu.exe -m 1:21 -m 1:181 -b emmc_all C:\BurnImage\imx-boot-sd.bin-mainboard C:\BurnImage\scanner-scanner_image-0.1.2.img";
                    cmdline4 = cmdtext(4, bootfile, imagefile);
                    _=RunWithRedirect4(cmdline4);
                    //SQL_Manager.UpdatePairing(connectSQLDB, MB_Serial[0], SOM_Serial[0], imagefw, pair1);
                }


            }
            else if (fmb == -1)
            {
                Cmd_Burn4.Enabled = false;
                //MessageBox.Show("CONNECT FIRST STAND USB TO MAINBOARD","INFO");

                //int pair1 = BurnTest(bootfile, imagefile);
                //await BurnTest1(bootfile, imagefile);
                //cmdline = @"/C C:\BurnImage\uuu.exe -m 1:21 -m 1:181 -b emmc_all C:\BurnImage\imx-boot-sd.bin-mainboard C:\BurnImage\scanner-scanner_image-0.1.2.img";
                cmdline4 = cmdtext(4, bootfile, imagefile);
                _=RunWithRedirect4(cmdline4);
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
 * https://stackoverflow.com/questions/20997771/run-a-given-number-of-the-same-process-concurrently
 * 
 * 
 ------------------------------------------------------------------------------------------------------------------------ 
100%1:181>Okay (350.8s)
1:181>Start Cmd:FB: flash -scanterm -scanlimited 0x800000 bootloader C:\BurnImage\imx-boot-sd.bin-mainboard
0x400000001:181>Okay (0.207s)
1:181>Start Cmd:FB: ucmd if env exists emmc_ack; then ; else setenv emmc_ack 0; fi;
1:181>Okay (0.011s)
1:181>Start Cmd:FB: ucmd mmc partconf ${emmc_dev} ${emmc_ack} 1 0
1:181>Okay (0.014s)
1:181>Start Cmd:FB: done
1:181>Okay (0s)
--------------------------------------------------------------------------------------------------------------------------
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