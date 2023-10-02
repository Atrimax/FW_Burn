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
        string username = string.Empty;

        public string DBconnection
        { get { return connectSQL; } set { connectSQL = value; } }

        public string UserN
        { get { return username; } set { username = value; } }


        public int FindMB_Status(string connectDB, string MBSerial)
        {
            
            int findmain = -1;
            bool mbflag = false;

            string query = "SELECT TOP(1) Final_Result FROM PCBA_Test WHERE Board_SN='" + MBSerial + "' ORDER BY Date DESC";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectDB))
                {
                    SqlCommand cmd = new SqlCommand(query, conn);
                    conn.Open();
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
            catch (Exception ex) { ex.Message.ToString(); findmain = -1; }

            return findmain;

        }
        public int FindMB_Pair(string connectDB, string MBSerial)
        {
            int pairstat = -1;
            
            string query = "SELECT Pair_Status FROM Parts_Pair WHERE MB_SN='" + MBSerial + "'";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectDB))
                {
                    SqlCommand cmd = new SqlCommand(query, conn);
                    conn.Open();
                    SqlDataReader dt = cmd.ExecuteReader();
                    if (dt.HasRows)
                    {
                        while (dt.Read())
                        {
                            pairstat = (int)dt["Pair_Status"];
                        }
                    }
                    else
                        pairstat = -1;
                    

                }

            }
            catch (Exception ex)
            { ex.Message.ToString(); }

            return pairstat;
        }

        public void UpdatePairing(string connectDB, string mbsn, string somsn, string somfwver, int pairstatus, string wifimac, string opername)
        {
            
            string query = "UPDATE Parts_Pair SET SOM_FW_VER=@SOM_FW_VER, SOM_SN=@SOM_SN, Pair_Status=@Pair_Status, Wifi_SN=@Wifi_SN, Operator_FW=@Operator_FW, Date_Pair=@Date_Pair WHERE MB_SN='" + mbsn + "'";
            using (SqlConnection conn = new SqlConnection(connectDB))
            {
                try
                {
                    //SqlDataAdapter adapter = new SqlDataAdapter();
                    conn.Open();
                    SqlCommand sc = new SqlCommand(query, conn);
                    {
                        sc.Parameters.AddWithValue("@SOM_SN", somsn);
                        sc.Parameters.AddWithValue("@SOM_FW_VER", somfwver);
                        sc.Parameters.AddWithValue("@Pair_Status", pairstatus);
                        sc.Parameters.AddWithValue("@Wifi_SN", wifimac);
                        sc.Parameters.AddWithValue(@"Operator_FW", SqlDbType.VarChar).Value = opername;
                        sc.Parameters.AddWithValue(@"Date_Pair", SqlDbType.SmallDateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        sc.ExecuteNonQuery();
                    }
                                        
                }
                catch (Exception ex)
                {
                    ex.Message.ToString();
                    throw ex;
                }
            }
        }

        public void SAVE_Pairing(string connectDB, string mbsn, string somsn, string somfwver, int pairstatus, string wifimac, string opername)
        {
            
            string query = "INSERT INTO Parts_Pair (MB_SN, SOM_FW_VER, SOM_SN, Pair_Status, Wifi_SN, Operator_FW, Date_Pair) VALUES (@MB_SN, @SOM_FW_VER, @SOM_SN, @Pair_Status, @Wifi_SN, @Operator_FW, @Date_Pair)";
            //@Wifi_SN, @Operator_FW, @Date_Pair)";
            try
            { 
                using (SqlConnection conn = new SqlConnection(connectDB))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                            
                        cmd.Parameters.AddWithValue(@"MB_SN", SqlDbType.VarChar).Value = mbsn;
                        cmd.Parameters.AddWithValue(@"SOM_FW_VER", SqlDbType.VarChar).Value = somfwver;
                        cmd.Parameters.AddWithValue(@"SOM_SN", SqlDbType.VarChar).Value = somsn;
                            
                        cmd.Parameters.AddWithValue(@"Pair_Status", SqlDbType.Int).Value = pairstatus;
                        cmd.Parameters.AddWithValue(@"Wifi_SN", SqlDbType.VarChar).Value = wifimac;
                        cmd.Parameters.AddWithValue(@"Operator_FW", SqlDbType.VarChar).Value = opername;
                        cmd.Parameters.AddWithValue(@"Date_Pair", SqlDbType.SmallDateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        cmd.ExecuteNonQuery();
                                                                     
                    }
                }

            }
            catch (Exception ex) { ex.Message.ToString(); throw ex; }
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

        //check login name and password vs db data, return status of user
        public int Login_check(string connectDB, string userl, string passl)
        {
            int status = -1;

            try
            {
                using (SqlConnection conn = new SqlConnection(connectDB))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("select * from Logins where Name='" + userl + "' and Password='" + passl + "'", conn);
                    SqlDataReader dt = cmd.ExecuteReader();
                    if (dt.HasRows)
                    {
                        while (dt.Read())
                        {
                            string namedb = dt["Name"].ToString();
                            string passdb = dt["Password"].ToString();
                            status = int.Parse(dt["Status"].ToString());
                        }

                    }
                    else
                    {
                        //MessageBox.Show("The name or password is NOT CORRECT!!!", "Warning");
                        //textBox1.Text = string.Empty; textBox2.Text = string.Empty;
                        status = 99;
                        //textBox1.Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString(), "Warning");
                status = 99; ex.Message.ToString();

            }

            return status;
        }
    }
}
