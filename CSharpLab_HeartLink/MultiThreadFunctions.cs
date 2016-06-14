using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using HeartLink_Lib;
using MySql.Data.MySqlClient;

namespace CSharpLab_HeartLink
{
    class MultiThreadFunctions
    {
        private static Person person;
        private static Socket socket = null;
        public static void ProcessServer(object obj)
        {
            socket = obj as Socket;
            if (socket.Connected)
            {
                Console.WriteLine("Connected");
                NetworkStream ns = new NetworkStream(socket);
                StreamWriter streamWriter = new StreamWriter(ns);
                StreamReader streamReader = new StreamReader(ns);
                person = new Person();
                int commandCode = -1;
                while (true)
                {
                    List<Packet> incommingPackets = new List<Packet>();
                    while (true)    //接收字节流（不定长）
                    {
                        try
                        {
                            byte[] receive = new byte[DataParser.PACKETDATALENGTH];
                            int readCount = socket.Receive(receive);
                            Packet p = new Packet(receive);
                            incommingPackets.Add(p);
                            if (DataParser.IsPacketLast(p))
                            {
                                break;
                            }
                        }
                        catch
                        {
                            socket.Close();
                            return;
                        }
                    }
                    commandCode = DataParser.GetPacketCommandCode(incommingPackets[0]);
                    switch ((CommandCode)commandCode)
                    {
                        #region Exit
                        case CommandCode.Exit:
                            Dictionary<string, string> exitPara = JsonParser.DeserializeObject(DataParser.Packets2Str(incommingPackets.ToArray()));
                            string exitType = exitPara["Type"];
                            exitPara.Remove("Type");
                            Dictionary<string, string> exitPair = new Dictionary<string, string>();
                            exitPair.Add("STATUS", "OFFLINE");
                            if (exitType.Equals("Stu"))
                            {
                                UpdateDB("students", exitPair, exitPara);
                            }
                            else
                            {
                                UpdateDB("supervisors", exitPair, exitPara);
                            }
                            socket.Close();
                            Console.WriteLine("Exit.");
                            return;
                        #endregion
                        #region Login
                        case CommandCode.Login:
                            Dictionary<string, string> loginPair = JsonParser.DeserializeObject(DataParser.Packets2Str(incommingPackets.ToArray()));
                            List<string> loginWanted = new List<string>();
                            loginWanted.Add("PASSWORD");
                            loginWanted.Add("STATUS");
                            Dictionary<string, string> loginPara = new Dictionary<string, string>();
                            loginPara.Add("ID", loginPair["ID"]);
                            List<Dictionary<string, string>> loginResult = null;
                            if (loginPair["Type"].Equals("Stu"))
                            {
                                loginResult = GetInfoFromDBWithPara("students", loginWanted, loginPara);
                            }
                            else
                            {
                                loginResult = GetInfoFromDBWithPara("supervisors", loginWanted, loginPara);
                            }
                            Packet[] loginInfo = null;
                            if (loginResult.Count == 0)
                            {
                                loginInfo = DataParser.Str2Packets(Convert.ToInt32(CommandCode.NotRegistered), "");
                            }
                            else
                            {
                                if (loginResult[0]["PASSWORD"].Equals(loginPair["Password"]))
                                {
                                    Dictionary<string, string> loginUpdate = new Dictionary<string, string>();
                                    loginUpdate.Add("STATUS", "ONLINE");
                                    List<string> needed = new List<string>();
                                    needed.Add("NAME");
                                    needed.Add("GENDER");
                                    needed.Add("CELLPHONE");
                                    if (loginPair["Type"].Equals("Stu"))
                                    {
                                        needed.Add("DORMITORY");
                                        needed.Add("CLASSNUMBER");
                                        needed.Add("DIRECTORNAME");
                                        needed.Add("DIRECTORCELLPHONE");
                                        UpdateDB("students", loginUpdate, loginPara);
                                        loginInfo = DataParser.Str2Packets(Convert.ToInt32(CommandCode.Succeed), JsonParser.SerializeObject(GetInfoFromDBWithPara("students", needed, loginPara)[0]));
                                    }
                                    else
                                    {
                                        UpdateDB("supervisors", loginUpdate, loginPara);
                                        needed.Add("BUILDING");
                                        loginInfo = DataParser.Str2Packets(Convert.ToInt32(CommandCode.Succeed), JsonParser.SerializeObject(GetInfoFromDBWithPara("supervisors", needed, loginPara)[0]));
                                    }
                                }
                                else if (loginResult[0]["STATUS"].Equals("ONLINE"))
                                {
                                    loginInfo = DataParser.Str2Packets(Convert.ToInt32(CommandCode.AlreadyLogin), "");
                                }
                                else
                                {
                                    loginInfo = DataParser.Str2Packets(Convert.ToInt32(CommandCode.WrongPsw), "");
                                }
                            }
                            SendPackets(loginInfo);
                            break;
                        #endregion
                        #region Register
                        case CommandCode.Register:
                            Dictionary<string, string> regPair = JsonParser.DeserializeObject(DataParser.Packets2Str(incommingPackets.ToArray()));
                            List<string> regWanted = new List<string>();
                            regWanted.Add("NAME");
                            Dictionary<string, string> regCheck = new Dictionary<string, string>();
                            regCheck.Add("ID", regPair["ID"]);
                            List<Dictionary<string, string>> regCheckResult = null;
                            string type = regPair["Type"];
                            if (type.Equals("Stu"))
                            {
                                regCheckResult = GetInfoFromDBWithPara("students", regWanted, regCheck);
                            }
                            else
                            {
                                regCheckResult = GetInfoFromDBWithPara("supervisors", regWanted, regCheck);
                            }
                            Packet[] regInfo = null;
                            if (regCheckResult.Count != 0)
                            {
                                regInfo = DataParser.Str2Packets(Convert.ToInt32(CommandCode.AlreadyRegistered), "");
                            }
                            else
                            {
                                regPair.Remove("Type");
                                //string psw = Encryption.Decrypt(regPair["PASSWORD"]);
                                //regPair.Remove("PASSWORD");
                                //regPair.Add("PASSWORD", psw);
                                int regLines = 0;
                                if (type.Equals("Stu"))
                                {
                                    regLines = InsertToDB("students", regPair);
                                }
                                else
                                {
                                    regLines = InsertToDB("supervisors", regPair);
                                }
                                if (regLines == 0)
                                {
                                    regInfo = DataParser.Str2Packets(Convert.ToInt32(CommandCode.RegisterFailed), "");
                                }
                                else
                                {
                                    regInfo = DataParser.Str2Packets(Convert.ToInt32(CommandCode.Succeed), "");
                                }
                            }
                            SendPackets(regInfo);
                            break;
                        #endregion

                        #region Logout
                        case CommandCode.Logout:
                            Dictionary<string, string> logoutPara = JsonParser.DeserializeObject(DataParser.Packets2Str(incommingPackets.ToArray()));
                            string logoutType = logoutPara["Type"];
                            logoutPara.Remove("Type");
                            Dictionary<string, string> logoutPair = new Dictionary<string, string>();
                            logoutPair.Add("STATUS", "OFFLINE");
                            if (logoutType.Equals("Stu"))
                            {
                                SendPackets(DataParser.Str2Packets(Convert.ToInt32(CommandCode.Logout), UpdateDB("students", logoutPair, logoutPara).ToString()));
                            }
                            else
                            {
                                SendPackets(DataParser.Str2Packets(Convert.ToInt32(CommandCode.Logout), UpdateDB("supervisors", logoutPair, logoutPara).ToString()));
                            }
                            break;
                        #endregion

                        #region SendContacter
                        case CommandCode.SendContacter:
                            Send("contacts", incommingPackets.ToArray(), CommandCode.ReturnContacters);
                            break;
                        #endregion
                        #region GetContacter
                        case CommandCode.GetContacters:
                            Dictionary<string, string> getContactorPair = JsonParser.DeserializeObject(DataParser.Packets2Str(incommingPackets.ToArray()));
                            string ID = getContactorPair["ID"];
                            List<string> contacterWanted = new List<string>();
                            contacterWanted.Add("CONTACTORID");
                            contacterWanted.Add("ISFORSTUDENT");
                            Dictionary<string, string> getContactorPara = new Dictionary<string, string>();
                            getContactorPara.Add("OWNERID", ID);
                            List<Dictionary<string, string>> contacters = GetInfoFromDBWithPara("contacts", contacterWanted, getContactorPara);
                            contacterWanted.Clear();
                            contacterWanted.Add("NAME");
                            contacterWanted.Add("CELLPHONE");
                            List<Dictionary<string, string>> stuContacter = new List<Dictionary<string, string>>();
                            List<Dictionary<string, string>> supContacter = new List<Dictionary<string, string>>();
                            Packet[] getContactersInfo = null;
                            try
                            {
                                foreach (Dictionary<string, string> con in contacters)
                                {
                                    Dictionary<string, string> temp = new Dictionary<string, string>();
                                    temp.Add("ID", con["CONTACTORID"]);
                                    try
                                    {
                                        if (con["ISFORSTUDENT"].Equals("1"))
                                        {
                                            stuContacter.Add(GetInfoFromDBWithPara("students", contacterWanted, temp)[0]);
                                        }
                                        else
                                        {
                                            supContacter.Add(GetInfoFromDBWithPara("supervisors", contacterWanted, temp)[0]);
                                        }
                                    }
                                    catch { }
                                }
                                stuContacter.AddRange(supContacter);
                                getContactersInfo = DataParser.Str2Packets(Convert.ToInt32(CommandCode.ReturnContacters), JsonParser.SerializeObject(stuContacter));
                            }
                            catch (Exception)
                            {
                                getContactersInfo = DataParser.Str2Packets(Convert.ToInt32(CommandCode.Error), "");
                            }
                            SendPackets(getContactersInfo);
                            break;
                        #endregion

                        #region SendMessageBox
                        case CommandCode.SendMessageBox:
                            Send("messageboard", incommingPackets.ToArray(), CommandCode.ReturnMessageBox);
                            break;
                        #endregion
                        #region GetMessageBox
                        case CommandCode.GetMessageBox:
                            List<string> messageBoxWanted = new List<string>();
                            messageBoxWanted.Add("SENDTIME");
                            messageBoxWanted.Add("ISFROMSTUDENT");
                            messageBoxWanted.Add("ID");
                            messageBoxWanted.Add("CONTENT");
                            List<Dictionary<string, string>> messageBox = GetInfoFromDB("messageboard", messageBoxWanted);

                            Packet[] messageBoxToSend = DataParser.Str2Packets(Convert.ToInt32(CommandCode.ReturnMessageBox), JsonParser.SerializeObject(messageBox));
                            SendPackets(messageBoxToSend);
                            break;
                        #endregion

                        #region SendDailyReminder
                        case CommandCode.SendDailyReminder:
                            Send("DAILYREMINDER", incommingPackets.ToArray(), CommandCode.ReturnDailyReminder);
                            break;
                        #endregion
                        #region GetDailyReminder
                        case CommandCode.GetDailyReminder:
                            List<string> dailyReminderWanted = new List<string>();
                            dailyReminderWanted.Add("REMINDTIME");
                            dailyReminderWanted.Add("CONTENT");
                            List<Dictionary<string, string>> dailyReminder = GetInfoFromDB("DAILYREMINDER", dailyReminderWanted);
                            if (dailyReminder.Count > 0)
                            {
                                Packet[] dailyReminderToSend = DataParser.Str2Packets(Convert.ToInt32(CommandCode.ReturnDailyReminder), JsonParser.SerializeObject(dailyReminder[0]));
                                SendPackets(dailyReminderToSend);
                            }
                            else
                            {
                                SendPackets(DataParser.Str2Packets(Convert.ToInt32(CommandCode.Error), ""));
                            }
                            break;
                        #endregion

                        #region SendLocation
                        case CommandCode.SendLocation:
                            Dictionary<string, string> locationPair = JsonParser.DeserializeObject(DataParser.Packets2Str(incommingPackets.ToArray()));
                            List<string> locationCheckWanted = new List<string>();
                            locationCheckWanted.Add("TIME");
                            Dictionary<string, string> locationCheck = new Dictionary<string, string>();
                            locationCheck.Add("ID", locationPair["ID"]);
                            if (GetInfoFromDBWithPara("studentslocation", locationCheckWanted, locationCheck).Count != 0)
                            {
                                Dictionary<string, string> locationPara = new Dictionary<string, string>();
                                locationPara.Add("ID", locationPair["ID"]);
                                locationPair.Remove("ID");
                                SendPackets(DataParser.Str2Packets(Convert.ToInt32(CommandCode.ReturnLocation), UpdateDB("studentslocation", locationPair, locationPara).ToString()));
                            }
                            else
                            {
                                SendPackets(DataParser.Str2Packets(Convert.ToInt32(CommandCode.ReturnLocation), InsertToDB("studentslocation", locationPair).ToString()));
                            }
                            break;
                        #endregion
                        #region GetLocation
                        case CommandCode.GetLocation:
                            List<string> locationWanted = new List<string>();
                            locationWanted.Add("ID");
                            locationWanted.Add("LONGITUDE");
                            locationWanted.Add("LATITUDE");
                            locationWanted.Add("ALTITUDE");
                            locationWanted.Add("TIME");
                            locationWanted.Add("POSITIONSOURCE");
                            GetLocationList("studentslocation", locationWanted);
                            break;
                        #endregion

                        #region SendAskOff
                        case CommandCode.SendAskOff:
                            Send("ASKOFF", incommingPackets.ToArray(), CommandCode.ReturnAskOff);
                            break;
                        #endregion
                        #region GetAskOff
                        case CommandCode.GetAskOff:
                            List<string> askOffWanted = new List<string>();
                            askOffWanted.Add("ID");
                            askOffWanted.Add("ASKOFFDATE");
                            askOffWanted.Add("ASKOFFTIME");
                            askOffWanted.Add("REASON");
                            GetList("ASKOFF", askOffWanted, CommandCode.ReturnAskOff);
                            break;
                        #endregion
                        #region SolveAskOff
                        case CommandCode.SolveAskOff:
                            Solve("ASKOFF", incommingPackets.ToArray());
                            break;
                        #endregion

                        #region SendDelivery
                        case CommandCode.SendDelivery:
                            Send("express", incommingPackets.ToArray(), CommandCode.ReturnDelivery);
                            break;
                        #endregion
                        #region GetDelivery
                        case CommandCode.GetDelivery:
                            List<string> getDeliveryWanted = new List<string>();
                            getDeliveryWanted.Add("CONTENT");
                            GetList("express", getDeliveryWanted, CommandCode.ReturnDelivery);
                            break;
                        #endregion
                        #region SolveDelivery:
                        case CommandCode.SolveDelivery:
                            Solve("express", incommingPackets.ToArray());
                            break;
                        #endregion

                        #region SendRepair
                        case CommandCode.SendRepair:
                            Send("repair", incommingPackets.ToArray(), CommandCode.ReturnRepair);
                            break;
                        #endregion
                        #region GetRepair
                        case CommandCode.GetRepair:
                            List<string> getRepairWanted = new List<string>();
                            getRepairWanted.Add("CONTENT");
                            GetList("repair", getRepairWanted, CommandCode.ReturnRepair);
                            break;
                        #endregion
                        #region SolveRepair
                        case CommandCode.SolveRepair:
                            Solve("repair", incommingPackets.ToArray());
                            break;
                        #endregion

                        #region SendWater
                        case CommandCode.SendWater:
                            Send("waterdelivery", incommingPackets.ToArray(), CommandCode.ReturnWater);
                            break;
                        #endregion
                        #region GetWater
                        case CommandCode.GetWater:
                            List<string> getWaterWanted = new List<string>();
                            getWaterWanted.Add("ADDRESS");
                            GetList("waterdelivery", getWaterWanted, CommandCode.ReturnWater);
                            break;
                        #endregion
                        #region SolveWater
                        case CommandCode.SolveWater:
                            Solve("waterdelivery", incommingPackets.ToArray());
                            break;
                        #endregion
                        default:
                            break;
                    }
                    incommingPackets.Clear();
                }
            }
        }

        private static void SendPackets(Packet[] packets)
        {
            foreach (Packet p in packets)
            {
                socket.Send(p.ToBytes());
            }
        }

        private static MySqlConnection GetConn()
        {
            return new MySqlConnection(String.Format("Persist Security Info=False;database={0};server={1};user id={2};pwd={3}", Server.DBSchema, Server.DBServer, Server.DBUserName, Server.DBPsw));
        }

        private static List<Dictionary<string, string>> GetInfoFromDBWithPara(string tableName, List<string> wantedFields, Dictionary<string, string> parameter)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string wanted in wantedFields)
            {
                sb.Append(wanted);
                sb.Append(',');
            }
            sb.Remove(sb.Length - 1, 1);    //去掉最后一个逗号
            StringBuilder sqlBuilder = new StringBuilder(String.Format("select {0} from {1} where ", sb.ToString(), tableName));
            foreach (string key in parameter.Keys)
            {
                sqlBuilder.Append(String.Format("{0}=?{0}", key));
                sqlBuilder.Append(" and ");
            }
            sqlBuilder.Remove(sqlBuilder.Length - 5, 5);    //去掉最后一个" and "

            MySqlConnection conn = GetConn();
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sqlBuilder.ToString(), conn);

            for (int i = 0; i < parameter.Count; ++i)
            {
                cmd.Parameters.Add(new MySqlParameter(String.Format("?{0}", parameter.ElementAt(i).Key), parameter.ElementAt(i).Value));
            }

            MySqlDataReader DBReader = cmd.ExecuteReader();

            List<Dictionary<string, string>> output = new List<Dictionary<string, string>>();

            try
            {
                while (DBReader.Read())
                {
                    Dictionary<string, string> temp = new Dictionary<string, string>();
                    foreach (string wanted in wantedFields)
                    {
                        temp[wanted] = DBReader[wanted].ToString();
                    }
                    output.Add(temp);
                }
            }
            finally
            {
                DBReader.Close();
                conn.Close();
            }
            return output;
        }

        private static List<Dictionary<string, string>> GetInfoFromDB(string tableName, List<string> wantedFields)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string wanted in wantedFields)
            {
                sb.Append(wanted);
                sb.Append(',');
            }
            sb.Remove(sb.Length - 1, 1);    //去掉最后一个逗号
            StringBuilder sqlBuilder = new StringBuilder(String.Format("select {0} from {1}", sb.ToString(), tableName));

            MySqlConnection conn = GetConn();
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sqlBuilder.ToString(), conn);

            MySqlDataReader DBReader = cmd.ExecuteReader();

            List<Dictionary<string, string>> output = new List<Dictionary<string, string>>();

            try
            {
                while (DBReader.Read())
                {
                    Dictionary<string, string> temp = new Dictionary<string, string>();
                    foreach (string wanted in wantedFields)
                    {
                        temp[wanted] = DBReader[wanted].ToString();
                    }
                    output.Add(temp);
                }
            }
            finally
            {
                DBReader.Close();
                conn.Close();
            }
            return output;
        }

        private static int InsertToDB(string tableName, Dictionary<string, string> record)
        {
            StringBuilder sqlBuilder = new StringBuilder(String.Format("insert into {0} (", tableName));
            foreach (string key in record.Keys)
            {
                sqlBuilder.Append(key);
                sqlBuilder.Append(',');
            }
            sqlBuilder.Remove(sqlBuilder.Length - 1, 1);
            sqlBuilder.Append(") values (");
            foreach (string key in record.Keys)
            {
                sqlBuilder.Append(String.Format("?{0}", key));
                sqlBuilder.Append(',');
            }
            sqlBuilder.Remove(sqlBuilder.Length - 1, 1);
            sqlBuilder.Append(')');
            MySqlConnection conn = GetConn();
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sqlBuilder.ToString(), conn);
            for (int i = 0; i < record.Count; ++i)
            {
                cmd.Parameters.Add(new MySqlParameter(String.Format("?{0}", record.ElementAt(i).Key), record.ElementAt(i).Value));
            }
            int affectedLines = 0;
            try
            {
                affectedLines = cmd.ExecuteNonQuery();
            }
            finally
            {
                conn.Close();
            }
            return affectedLines;
        }

        private static int UpdateDB(string tableName, Dictionary<string, string> updateRecord, Dictionary<string, string> parameter)
        {
            StringBuilder sqlBuilder = new StringBuilder(String.Format("update {0} set ", tableName));
            foreach (string key in updateRecord.Keys)
            {
                sqlBuilder.Append(String.Format("{0}=?{0}", key));
                sqlBuilder.Append(',');
            }
            sqlBuilder.Remove(sqlBuilder.Length - 1, 1);
            sqlBuilder.Append(" where ");
            foreach (string key in parameter.Keys)
            {
                sqlBuilder.Append(String.Format("{0}=?{0}", key));
                sqlBuilder.Append(" and ");
            }
            sqlBuilder.Remove(sqlBuilder.Length - 5, 5);

            MySqlConnection conn = GetConn();
            conn.Open();
            MySqlCommand cmd = new MySqlCommand(sqlBuilder.ToString(), conn);

            for (int i = 0; i < updateRecord.Count; ++i)
            {
                cmd.Parameters.Add(new MySqlParameter(String.Format("?{0}", updateRecord.ElementAt(i).Key), updateRecord.ElementAt(i).Value));
            }

            for (int i = 0; i < parameter.Count; ++i)
            {
                cmd.Parameters.Add(new MySqlParameter(String.Format("?{0}", parameter.ElementAt(i).Key), parameter.ElementAt(i).Value));
            }

            int affectedLines = 0;
            try
            {
                affectedLines = cmd.ExecuteNonQuery();
            }
            finally
            {
                conn.Close();
            }
            return affectedLines;
        }

        private static void Send(string tableName, Packet[] packets, CommandCode code)
        {
            Dictionary<string, string> sendPara = JsonParser.DeserializeObject(DataParser.Packets2Str(packets));
            SendPackets(DataParser.Str2Packets(Convert.ToInt32(code), InsertToDB(tableName, sendPara).ToString()));
        }

        private static void Solve(string tableName, Packet[] packets)
        {
            Dictionary<string, string> solvePara = JsonParser.DeserializeObject(DataParser.Packets2Str(packets));
            Dictionary<string, string> solvePair = new Dictionary<string, string>();
            solvePair.Add("ISSOLVED", "1");
            Packet[] solveInfo = null;
            if (UpdateDB(tableName, solvePair, solvePara) != 0)
            {
                solveInfo = DataParser.Str2Packets(Convert.ToInt32(CommandCode.Succeed), "");
            }
            else
            {
                solveInfo = DataParser.Str2Packets(Convert.ToInt32(CommandCode.Error), "");
            }
            SendPackets(solveInfo);
        }

        private static void GetList(string tableName, List<string> wanted, CommandCode code)
        {
            Dictionary<string, string> para = new Dictionary<string, string>();
            para.Add("ISSOLVED", "0");
            List<Dictionary<string, string>> toSend = GetInfoFromDBWithPara(tableName, wanted, para);
            SendPackets(DataParser.Str2Packets(Convert.ToInt32(code), JsonParser.SerializeObject(toSend)));
        }

        private static void GetLocationList(string tableName, List<string> wanted)
        {
            List<Dictionary<string, string>> toSend = GetInfoFromDBWithPara(tableName, wanted, new Dictionary<string, string>());
            SendPackets(DataParser.Str2Packets(Convert.ToInt32(CommandCode.ReturnLocation), JsonParser.SerializeObject(toSend)));
        }

    }
}
