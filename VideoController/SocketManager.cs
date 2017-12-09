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

        //public static Socket Server, Client;
        public static byte[] getByte = new byte[1024];
        public static byte[] setByte = new byte[1024];

        int port = 0;
        Thread threadServer = null;
        public bool _isOn = false;

        Listener _listener;
        List<ConnectionInfo> _listConnection;

        public Form1 f = null;

        public void init(int port)
        {
            this.port = port;
            
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
            //IPAddress serverIP = IPAddress.Parse(GetLocalIP());
            //IPEndPoint serverEndPoint = new IPEndPoint(serverIP, port);
            Console.WriteLine("1 try");

            _listener = new Listener(Convert.ToInt32(port));
            _listConnection = new List<ConnectionInfo>();
            
            _listener.SocketAccepted += new Listener.SocketAcceptedHandler(l_SocketAccepted);
            _listener.Start(GetLocalIP());
            
            Console.ReadLine();
            _isOn = true;
            
            /*
            try
            {
                Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Server.Bind(serverEndPoint);
                Server.Listen(10);
                
                Client = Server.Accept();
                if (Client.Connected)
                {
                    Console.WriteLine("Client.Connected in ============================");
                    string stringbyte = null;
                    while (!isEndThread)
                    {
                        Client.Receive(getByte, 0, getByte.Length, SocketFlags.None);
                        stringbyte = Encoding.UTF7.GetString(getByte);

                        if (stringbyte != String.Empty)
                        {
                            int getValueLength = 0;
                            getValueLength = byteArrayDefrag(getByte);

                            stringbyte = Encoding.UTF7.GetString(getByte, 0, getValueLength + 1);
                            Console.WriteLine("수신데이터:{0} | 길이:{1}", stringbyte, getValueLength + 1);

                            JObject obj = JObject.Parse(stringbyte);
                            Console.WriteLine("json : {0}", obj);
                            Console.WriteLine(obj["identifier"].ToString());

                            if (obj["identifier"].ToString().Equals("user_info"))
                            {
                                string name = obj["name"].ToString();
                                bool isTeacher = obj["user"].ToString().Equals("T") ? true : false;
                                string uuid = obj["uuid"].ToString();
                                Console.WriteLine("name : " + name + "  //  isTeacher : " + isTeacher + "  //  uuid : "+uuid);
                            }

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
            */
        }

        void l_SocketAccepted(Socket e)
        {
            Console.WriteLine("New Connection: {0}\n{1}\n=========================", e.RemoteEndPoint.ToString(), DateTime.Now.ToString());

            if (e != null)
            {
                bool isAdd = true;
                foreach (ConnectionInfo info in _listConnection)
                {
                    if(info.socket == e)
                    {
                        if (info.socket.Connected == false)
                        {
                            info.socket = e;
                        }
                        isAdd = false;
                    }
                }

                if (isAdd)
                {
                    ConnectionInfo info = new ConnectionInfo();
                    info.socket = e;
                    _listConnection.Add(info);
                }
                    
            }
            
            int index = 1;
            Console.WriteLine("Connected socket list\n=========================");

            foreach (ConnectionInfo i in _listConnection)
            {
                Console.WriteLine("{0} : {1} : socket handle {2}", index, i.socket.RemoteEndPoint.ToString(), i.socket.Handle.ToString());
                index++;

                i.socket.Receive(getByte, 0, getByte.Length, SocketFlags.None);
                string stringbyte = Encoding.UTF8.GetString(getByte);

                if (stringbyte != String.Empty)
                {
                    int getValueLength = 0;
                    getValueLength = byteArrayDefrag(getByte);

                    stringbyte = Encoding.UTF8.GetString(getByte, 0, getValueLength + 1);
                    
                    Console.WriteLine("1. 수신데이터:{0} | 길이:{1}", stringbyte, getValueLength + 1);
                    
                    JObject obj = JObject.Parse(stringbyte);
                    Console.WriteLine("json : {0}", obj);
                    Console.WriteLine(obj["identifier"].ToString());

                    if (obj["identifier"].ToString().Equals("user_info"))
                    {
                        string name = obj["name"].ToString();
                        bool isTeacher = obj["user"].ToString().Equals("T") ? true : false;
                        string uuid = obj["device_id"].ToString();
                        Console.WriteLine("name : " + name + "  //  isTeacher : " + isTeacher + "  //  uuid : " + uuid);

                        i.setData(name, uuid, isTeacher);
                        f.addConnectionInfo(name, uuid, isTeacher, i.socket);

                        JObject json = new JObject();
                        json.Add("id", "user_info");
                        json.Add("result", "success");

                        i.socket.Send(Encoding.UTF8.GetBytes(json.ToString()));
                    }
                    
                }

                getByte = new byte[1024];
                setByte = new byte[1024];

            }
            Console.WriteLine("");
        }

        public void onEnd()
        {
            if (_listener != null)
                _listener.Stop();
            if(_listConnection != null)
            {
                foreach(ConnectionInfo i in _listConnection)
                {
                    i.socket.Close();
                }
                _listConnection.Clear();
            }
            threadServer.Abort();
        }

        public void sendMessage(string text, string uuid)
        {
            foreach (ConnectionInfo info in _listConnection)
            {
                if (info.socket.Connected)
                {
                    
                    if (uuid != null)
                    {
                        if (uuid.Equals(info.uuid))
                        {
                            try
                            {
                                info.socket.Send(Encoding.UTF8.GetBytes(text));
                            }
                            catch (SocketException e)
                            {
                                _listConnection.Remove(info);
                            }

                        }
                    }
                    else
                    {
                        try
                        {
                            info.socket.Send(Encoding.UTF8.GetBytes(text));
                        }
                        catch (SocketException e)
                        {
                            _listConnection.Remove(info);
                        }
                        
                    }
                }
                else
                {

                }
                
            }
        }
    }
}
