using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace VideoController
{
    class SocketManager
    {
        public static SocketManager instance = null;
        public static SocketManager getInstance()
        {
            if (instance == null)
            {
                instance = new SocketManager();              
            }
                
            return instance;
        }

        public static Socket Server, Client;
        public static byte[] getByte = new byte[1024];
        public static byte[] setByte = new byte[1024];

        int port = 0;
        Thread threadServer = null;
        public bool isEndThread = false;

        public void init(int port)
        {
            this.port = port;

            if (threadServer != null)
            {
                if (Server != null)
                    Server.Close();
                if (Client != null)
                    Client.Close();

                threadServer.Join();
                isEndThread = true;
            }
            
            Console.WriteLine("thread start");
            threadServer = new Thread(new ThreadStart(serverProc));

            threadServer.IsBackground = true;
            threadServer.Start();
                      
        }

        public string GetLocalIP()
        {
            string localIP = "Not available, please check your network seetings!";
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            Console.WriteLine("1 ip : " + localIP);

            return localIP;
        }

        public static int byteArrayDefrag(byte[] sData)
        {
            int endLength = 0;

            for (int i = 0; i < sData.Length; i++)
            {
                if ((byte)sData[i] != (byte)0)
                {
                    endLength = i;
                }
            }
            return endLength;
        }

        void serverProc()
        {
            IPAddress serverIP = IPAddress.Parse(GetLocalIP());
            IPEndPoint serverEndPoint = new IPEndPoint(serverIP, port);
            isEndThread = false;
            Console.WriteLine("1 try");
            try
            {
                Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Console.WriteLine("1");
                Server.Bind(serverEndPoint);
                Console.WriteLine("2");
                Server.Listen(10);
                Console.WriteLine("3");

                Client = Server.Accept();
                Console.WriteLine("try");
                if (Client.Connected)
                {
                    Console.WriteLine("Client.Connected in ============================");
                    string stringbyte = null;
                    while (!isEndThread)
                    {
                        Console.WriteLine("============================");
                        Client.Receive(getByte, 0, getByte.Length, SocketFlags.None);
                        stringbyte = Encoding.UTF7.GetString(getByte);

                        if (stringbyte != String.Empty)
                        {
                            int getValueLength = 0;
                            getValueLength = byteArrayDefrag(getByte);

                            stringbyte = Encoding.UTF7.GetString(getByte, 0, getValueLength + 1);
                            Console.WriteLine("수신데이터:{0} | 길이:{1}", stringbyte, getValueLength + 1);

                            setByte = Encoding.UTF7.GetBytes(stringbyte);
                            Client.Send(setByte, 0, setByte.Length, SocketFlags.None);
                        }

                        getByte = new byte[1024];
                        setByte = new byte[1024];
                    }
                }
                else
                {
                    Console.WriteLine("Client.Connected else ============================");
                }
            }
            catch (System.Net.Sockets.SocketException socketEx)
            {
                Console.WriteLine("[Error 1]:{0}", socketEx.Message);
            }
            catch (System.Exception commonEx)
            {
                Console.WriteLine("[Error 2]:{0}", commonEx.Message);
            }
            finally
            {
                Console.WriteLine("finally");
                if (Server != null)
                    Server.Close();

                if (Client != null)
                    Client.Close();
                isEndThread = true;
            }                       
        }
    }
}
