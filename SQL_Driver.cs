using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace FW_Burn
{
    internal class SQL_Driver
    {
        string connectSQL = string.Empty;

        public string DBconnection
        { get { return connectSQL; } set { connectSQL = value; } }


        public int FindMB_Status(string connectDB, string MBSerial)
        {
            string query = string.Empty;
            int findmain = -1;
            bool mbflag = false;

            query = "SELECT TOP(1) Final_Result FROM PCBA_Test WHERE Board_SN='" + MBSerial + "' ORDER BY Date DESC";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectDB))
                {
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataReader dt = cmd.ExecuteReader();
                    if (dt.HasRows)
                    {
                        while (dt.Read())
                        {
                            mbflag = (bool)dt["Final_Result"];
                            if (!mbflag)
                            {
                                //MessageBox.Show();
                                findmain = 0; //mainboard last test in pcba is failed 
                            }
                            else
                            {
                                findmain = 1; // mainboard last test in pcba is passed
                            }

                        }
                    }
                    else
                    {
                        //DialogResult mflag = MessageBox.Show("THIS MAIN BOARD NOT TESTED! DO YOU WANT TO PAIR IT WITH SOM?", "Warning", MessageBoxButtons.YesNo);
                        findmain = 2; //STILL SAVE THIS BOARD TO DB
                    }

                    
                }
            

            }
            catch (Exception ex) { ex.Message.ToString();  findmain = -1; }

            return findmain;
            
        }

        public bool DBConnected(string connectionSQL)
        {
            bool status = false;
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionSQL))
                {
                    conn.Open();


                    if (conn.State == ConnectionState.Open)
                        status = true;
                    else
                        status = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Warning");
                status = false;
            }
            return status;
        }
    }
}
