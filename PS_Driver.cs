using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ivi.Visa.Interop;

namespace FW_Burn
{
    internal class PS_Driver
    {
        Ivi.Visa.Interop.ResourceManager rMgr = new ResourceManager();
        FormattedIO488 src = new FormattedIO488();

        private string sendstring = string.Empty;
        private string src_address = string.Empty;

        public string GetAdressPS(string resourceName)
        {
            return src_address = resourceName;
        }

        public void getAvailableResources()//function that find the available USBTMC resources and populates combobox1
        {
            try
            {
                //USB VISA resources
                string[] resources = rMgr.FindRsrc("USB?*");//rMgr.FindRsrc("USB?*"); 
                src_address = resources[0];

            }
            catch (Exception)
            {
                throw;

            }
        }

        //Close power supply resource
        public void PS_Close()
        {
            try
            {
                src.IO.Close();
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }
        }

        public void ConnectPowerSupply(string SrcAddress)
        {
            try 
            {
                src.IO = (IMessage)rMgr.Open(SrcAddress, AccessMode.NO_LOCK, 2000, "");
                src.IO.Timeout = 2000;
            }
            catch (Exception ex) { ex.Message.ToString(); }
            

        }

        public bool PS_Init()
        {
            bool PS_INIT_Flag = false; 
            try 
            {
                src.WriteString("*IDN?\n");
                System.Threading.Thread.Sleep(200);

                string p_answer = src.ReadString();
                PS_INIT_Flag = p_answer.Contains("");
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
                PS_INIT_Flag = false;
            }
            
            return PS_INIT_Flag;
                        
        }

        //PS Channel number[1-4] , status - ON/OFF [1/0]
        public bool PS_CH_ONOFF(int channel, bool status)
        {
            bool psonoff = false;
            
            string command = "SOUR:CHAN:OUTP " + status.ToString()+"\n";

            string b_command = "INST:NSEL " + channel.ToString() + "\n";
            try 
            {
                src.WriteString(b_command);
                System.Threading.Thread.Sleep(200);
                src.WriteString(command);
                System.Threading.Thread.Sleep(200);
                psonoff = true;
            }
            catch(Exception ex) 
            {
                MessageBox.Show(ex.Message, "Warning");
                psonoff = false;
            }
            
            return psonoff;
        }

        //PS must be configured to for 3.7VDC
        public bool PS_Setup_Voltage(string voltage)
        {
            bool ps_volt = false;
            string command = "SOUR:APPL:VOLT "+voltage.ToString()+"," + voltage.ToString() + "," + voltage.ToString() + "," + voltage.ToString() + "\n";
            try
            {                
                src.WriteString(command);
                System.Threading.Thread.Sleep(200);
                ps_volt = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Warning");
                ps_volt = false;
            }

            return ps_volt;
        }

        public bool PS_Setup_Current(string current)
        {
            bool ps_curr = false;
            string command = "SOUR:APPL:CURR " + current.ToString() + "," + current.ToString() + "," + current.ToString() + "," + current.ToString() + "\n";

            try
            {
                
                src.WriteString(command);
                System.Threading.Thread.Sleep(200);
                ps_curr = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Warning");
                ps_curr = false;
            }
            return ps_curr;
        }
    }
}
