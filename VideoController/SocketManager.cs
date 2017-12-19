using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using System.IO;

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

        int port = 0;
        static Listener l;
        static List<ConnectionInfo> listSocket;
        public bool _isOn = false;
        public Form1 f = null;
        string ip = "";

        public static byte[] getByte = new byte[1024];
        public static byte[] setByte = new byte[1024];

        public void init(string ip, int port)
        {
            this.port = port;
            this.ip = ip;

            Console.WriteLine("thread start");

            l = new Listener(Convert.ToInt32(port));

            listSocket = new List<ConnectionInfo>();

            l.SocketAccepted += new Listener.SocketAcceptedHandler(l_SocketAccepted);
            l.Start(this.ip);

            Console.ReadLine();
            _isOn = true;
        }


        void l_SocketAccepted(Socket e)
        {
            Client client = new Client(e);
            client.Received += new Client.ClientReceivedHandler(client_Received);
            client.Disconnected += new Client.ClientDisconnectedHandler(client_Disconnected);
            
            Console.WriteLine("New Connection: {0}\n{1}\n=========================", e.RemoteEndPoint.ToString(), DateTime.Now.ToString());
            if (e != null)
            {
                bool isAdd = true;
                if (listSocket.Count != 0)
                {
                    foreach (ConnectionInfo i in listSocket)
                    {
                        Client c = i.client as Client;
                        if (c.ID == client.ID)
                            isAdd = false;
                    }
                }
                if (isAdd)
                {
                    string p = ":";
                    ConnectionInfo info = new ConnectionInfo();
                    info.client = client;
                    string strIp = e.RemoteEndPoint.ToString();

                    info.ip = strIp.Substring(0, strIp.IndexOf(p));
                    listSocket.Add(info);
                }
            }
            Console.WriteLine("");
        }

        void client_Disconnected(Client sender)
        {
            for (int i = listSocket.Count - 1; i > -1; i--)
            {
                Client client = listSocket[i].client as Client;
                if (client.ID == sender.ID)
                {
                    listSocket.RemoveAt(i);
                    break;
                }
            }

        }

        void client_Received(Client sender, byte[] data)
        {
            for (int i = 0; i < listSocket.Count; i++)
            {
                Client client = listSocket[i].client as Client;

                if (client.ID == sender.ID)
                {
                    string stringbyte = Encoding.UTF8.GetString(data, 0, data.Length);

                    JObject obj = JObject.Parse(stringbyte);
                    Console.WriteLine("json : {0}", obj);
                    Console.WriteLine(obj["identifier"].ToString());

                    if (obj["identifier"].ToString().Equals("user_info"))
                    {
                        string name = obj["name"].ToString();
                        bool isTeacher = obj["user"].ToString().Equals("T") ? true : false;
                        string uuid = obj["device_id"].ToString();
                        Console.WriteLine("name : " + name + "  //  isTeacher : " + isTeacher + "  //  uuid : " + uuid);

                        listSocket[i].setData(name, uuid, isTeacher);
                        instance.f.addConnectionInfo(listSocket);
                    }
                    else if (obj["identifier"].ToString().Equals("progress"))
                    {
                        int persent = int.Parse(obj["persent"].ToString());
                        int max  = int.Parse(obj["max"].ToString());
                        int current = int.Parse(obj["current"].ToString());
                        string fileName = obj["name"].ToString();
                        string uuid = obj["device_id"].ToString();
                        Console.WriteLine("progress : " + persent + " // " + fileName + "  //  uuid : " + uuid);
                        f.updateProgress(uuid, persent, current, max);
                    }
                    else if (obj["identifier"].ToString().Equals("downEnd"))
                    {
                        int max = int.Parse(obj["max"].ToString());
                        int current = int.Parse(obj["current"].ToString());
                        string uuid = obj["device_id"].ToString();
                        string fileName = obj["name"].ToString();
                        Console.WriteLine("current : " + current + " // max : " + max);
                        f.updateProgress(uuid, 100, current, max);
                    }

                    break;
                }
            }

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

            return localIP;
        }

        public void onEnd()
        {
            if (l != null)
                l.Stop();
            if (listSocket != null)
            {
                foreach (ConnectionInfo i in listSocket)
                {
                    Client c = i.client as Client;
                    client_Disconnected(c);
                }
                listSocket.Clear();
            }
        }

        public void sendMessage(string text, string uuid)
        {
            foreach (ConnectionInfo info in listSocket)
            {
                Client c = info.client as Client;
                c.sendMessage(text);
            }
        }

        //public void checkSockets()
        //{
        //    if (listSocket.Count > 0)
        //    {
        //        for (int i = listSocket.Count - 1; i >= 0; i--)
        //        {
        //            try
        //            {
        //                int r = listSocket[i].socket.Send(Encoding.UTF8.GetBytes("1"));
        //                Console.WriteLine("r : " + r);
        //            }
        //            catch (Exception e)
        //            {
        //                listSocket.RemoveAt(i);
        //            }

        //        }
        //    }
        //}

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

    }
}
