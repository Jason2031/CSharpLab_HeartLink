using System;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using System.Threading;

namespace CSharpLab_HeartLink
{
    class Server
    {
        public static string DBServer { get; set; }
        public static string DBUserName { get; set; }
        public static string DBPsw { get; set; }
        public static string DBSchema { get; set; }
        static void Main(string[] args)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(@"Settings.xml");
            XmlNode db = doc.SelectSingleNode("config").SelectSingleNode("database");
            DBServer = db.SelectSingleNode("dbserver").InnerText;
            DBUserName = db.SelectSingleNode("dbuserid").InnerText;
            DBPsw = db.SelectSingleNode("dbpassword").InnerText;
            DBSchema = db.SelectSingleNode("dbschema").InnerText;
            XmlNode xn = doc.SelectSingleNode("config").SelectSingleNode("ipendpoint");
            string ipAddress = xn.SelectSingleNode("ipaddress").InnerText;
            IPAddress IP = null;
            if (!IPAddress.TryParse(ipAddress, out IP))
            {
                Console.WriteLine("IP无效，无法启动服务器");
                Console.ReadKey();
                return;
            }
            Int32 port = 10000;     //default
            TcpListener tcpListener = null;
            try
            {
                port = Convert.ToInt32(xn.SelectSingleNode("port").InnerText);
                tcpListener = new TcpListener(IP, port);
                tcpListener.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                Console.ReadKey();
                return;
            }
            Console.WriteLine("服务器已启动");
            while (true)
            {
                Socket socketForClient = tcpListener.AcceptSocket();
                Thread t = new Thread(new ParameterizedThreadStart(MultiThreadFunctions.ProcessServer));
                t.Start(socketForClient);
            }

        }
    }
}
