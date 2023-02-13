using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ivi.Visa.Interop;

namespace FW_Burn
{
    internal class PS_Driver
    {
        Ivi.Visa.Interop.ResourceManager rMgr = new ResourceManager();
        FormattedIO488 src = new FormattedIO488();

        private string sendstring = string.Empty;
        private string src_address = string.Empty;

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

        public bool PS_Init(string resourses)
        {

            return true;
        }

        //PS Channel number[1-4] , status - ON/OFF [1/0]
        public bool PS_CH_ONOFF(int channel, bool status)
        {
            string instrumentinit = "INST:SEL "+channel.ToString()+"\n";
            string command = "SOUR:CHAN:OUTP " + status.ToString()+"\n";
            return false;
        }

        //PS must be configured to for 3.7VDC
        public bool PS_Setup_Voltage(string voltage)
        {
            string command = "SOUR:APPL:VOLT "+voltage.ToString()+"," + voltage.ToString() + "," + voltage.ToString() + "," + voltage.ToString() + "\n";
            return false;
        }

        public bool PS_Setup_Current(string current)
        {
            string command = "SOUR:APPL:CURR " + current.ToString() + "," + current.ToString() + "," + current.ToString() + "," + current.ToString() + "\n";
            return false;
        }
    }
}
