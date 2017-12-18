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


        static void l_SocketAccepted(Socket e)
        {
            Console.WriteLine("New Connection: {0}\n{1}\n=========================", e.RemoteEndPoint.ToString(), DateTime.Now.ToString());
            if (e != null)
            {

                string p = ":";
                ConnectionInfo info = new ConnectionInfo();
                info.socket = e;
                string strIp = e.RemoteEndPoint.ToString();

                info.ip = strIp.Substring(0, strIp.IndexOf(p));
                listSocket.Add(info);
            }
            instance.checkSockets();

            int index = 1;
            Console.WriteLine("Connected socket list\n=========================");

            foreach (ConnectionInfo i in listSocket)
            {
                Console.WriteLine("{0} : {1} : socket handle {2}", index, i.socket.RemoteEndPoint.ToString(), i.socket.Handle.ToString());
                index++;
                if (e == i.socket)
                {
                    int res = i.socket.Receive(getByte, 0, getByte.Length, SocketFlags.None);
                    int dataType = int.Parse(Encoding.UTF8.GetString(getByte, 0, 1));//BitConverter.ToInt32(getByte, 0);

                    if (dataType == 2)
                    {
                        //file
                        int getValueLength = 0;
                        getValueLength = byteArrayDefrag(getByte);

                        string sDirPath;
                        sDirPath = Application.StartupPath + ".\\movie\\" + i.name + "\\";
                        DirectoryInfo di = new DirectoryInfo(sDirPath);
                        if (di.Exists == false)
                        {
                            di.Create();
                        }
                        Console.WriteLine("file in");
                        //int receivedBytesLen = i.socket.Receive(getByte);

                        int fileNameLen = BitConverter.ToInt32(getByte, 0);
                        string fileName = Encoding.ASCII.GetString(getByte, 4, fileNameLen);

                        Console.WriteLine("Client:{0} connected & File {1} started received.", i.socket.RemoteEndPoint, fileName);

                        BinaryWriter bWrite = new BinaryWriter(File.Open(sDirPath + fileName, FileMode.Append)); ;
                        bWrite.Write(getByte, 4 + fileNameLen, res - 4 - fileNameLen);

                        Console.WriteLine("File: {0} received & saved at path: {1}", fileName, sDirPath);

                    }
                    else
                    {
                        //string
                        string stringbyte = Encoding.UTF8.GetString(getByte, 0, getByte.Length);
                        if (stringbyte != String.Empty)
                        {
                            int getValueLength = 0;
                            getValueLength = byteArrayDefrag(getByte);

                            stringbyte = Encoding.UTF8.GetString(getByte, 1, getValueLength + 1);

                            Console.WriteLine("1. 수신데이터:{0} | 길이:{1}", stringbyte, getValueLength + 1);

                            if (getValueLength + 1 > 1)
                            {
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
                                    instance.f.addConnectionInfo(listSocket);

                                    JObject json = new JObject();
                                    json.Add("id", "user_info");
                                    json.Add("result", "success");

                                    //i.socket.Send(Encoding.UTF8.GetBytes(json.ToString()));
                                }else if (obj["identifier"].ToString().Equals("progress"))
                                {
                                    int persent = int.Parse(obj["persent"].ToString());
                                    string fileName = obj["name"].ToString();
                                    string uuid = obj["device_id"].ToString();
                                    Console.WriteLine("progress : " + persent + " // " + fileName + "  //  uuid : " + uuid);
                                }else if (obj["identifier"].ToString().Equals("downEnd"))
                                {
                                    int max = int.Parse(obj["max"].ToString());
                                    int current = int.Parse(obj["current"].ToString());
                                    string uuid = obj["device_id"].ToString();
                                    Console.WriteLine("current : " + current + " // max : " + max);
                                }
                            }

                        }
                    }

                    getByte = new byte[1024];
                    setByte = new byte[1024];
                }
            }

            Console.WriteLine("");
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
                    i.socket.Close();
                }
                listSocket.Clear();
            }
        }

        public void sendMessage(string text, string uuid)
        {
            foreach (ConnectionInfo info in listSocket)
            {
                if (info.socket.Connected)
                {
                    try
                    {
                        info.socket.Send(Encoding.UTF8.GetBytes(text));
                    }
                    catch (SocketException e)
                    {

                    }

                }
            }
        }

        public void checkSockets()
        {
            if (listSocket.Count > 0)
            {
                for (int i = listSocket.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        int r = listSocket[i].socket.Send(Encoding.UTF8.GetBytes("1"));
                        Console.WriteLine("r : " + r);
                    }
                    catch (Exception e)
                    {
                        listSocket.RemoveAt(i);
                    }

                }
            }
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

    }
}
