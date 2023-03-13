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
            string query = string.Empty;
            query = "SELECT Pair_Status FROM Parts_Pair WHERE MB_SN='" + MBSerial + "'";
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
                    {
                        pairstat = -1;
                    }

                }

            }
            catch (Exception ex)
            { ex.Message.ToString(); return pairstat = -1; }

            return pairstat;
        }

        public void UpdatePairing(string connectDB, string mbsn, string somsn, string somfwver, int pairstatus)
        {
            string query = string.Empty;
            query  = query = "UPDATE Parts_Pair SET SOM_FW_VER=@SOM_FW_VER, SOM_SN=@SOM_SN, Pair_Status=@Pair_Status WHERE MB_SN='" + mbsn + "'";
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
                        sc.ExecuteNonQuery();
                    }

                    //adapter.UpdateCommand = sc;
                    //conn.Open();
                    //adapter.UpdateCommand.ExecuteNonQuery();

                }
                catch (Exception ex)
                {
                    ex.Message.ToString();
                    throw ex;
                }
            }
        }

        public void SAVE_Pairing(string connectDB, string mbsn, string somsn, string somfwver, int pairstatus)
        {
            string query = string.Empty;
            query = "INSERT INTO Parts_Pair (MB_SN, SOM_FW_VER, SOM_SN, Pair_Status, Operator_FW, Date_Pair) VALUES (MB_SN, SOM_FW_VER, SOM_SN, Pair_Status, Operator_FW, Date_Pair)";
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
                            
                            cmd.Parameters.AddWithValue(@"Operator_FW", SqlDbType.VarChar).Value = "admin";
                            cmd.Parameters.AddWithValue(@"Date_Pair", SqlDbType.SmallDateTime).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                            int rt = cmd.ExecuteNonQuery();
                            //saveflag = true;
                                             
                    }
                }

            }
            catch (Exception ex) { ex.Message.ToString(); }
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
