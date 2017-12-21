using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;

namespace VideoController
{
    class Client
    {
        public string ID
        {
            get;
            private set;
        }

        public IPEndPoint EndPoint
        {
            get;
            private set;
        }

        Socket sck;
        public Client(Socket accepted)
        {
            sck = accepted;
            ID = Guid.NewGuid().ToString();
            EndPoint = (IPEndPoint)sck.RemoteEndPoint;
            sck.BeginReceive(new byte[] { 0 }, 0, 0, 0, callback, null);
        }

        void callback(IAsyncResult ar)
        {
            try
            {
                sck.EndReceive(ar);

                byte[] buf = new byte[1024];

                int rec = sck.Receive(buf, buf.Length, 0);

                if (rec <= 0)
                {
                    Close();

                    if (Disconnected != null)
                    {
                        Disconnected(this);
                    }
                }

                if (rec < buf.Length)
                {
                    Array.Resize<byte>(ref buf, byteArrayDefrag(buf)+1);
                }

                if (Received != null)
                {
                    Received(this, buf);
                }

                sck.BeginReceive(new byte[] { 0 }, 0, 0, 0, callback, null);
            }
            catch (SocketException se)
            {
                Console.WriteLine(se.Message.ToString());
                Close();

                switch (se.SocketErrorCode)
                {
                    case SocketError.ConnectionAborted:
                    case SocketError.ConnectionReset:
                        Close();

                        if (Disconnected != null)
                        {
                            Disconnected(this);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
                Close();

                if (Disconnected != null)
                {
                    Disconnected(this);
                }
            }
        }

        public void Close()
        {
            sck.Close();
            sck.Dispose();
        }

        public delegate void ClientReceivedHandler(Client sender, byte[] data);
        public event ClientReceivedHandler Received;

        public delegate void ClientDisconnectedHandler(Client sender);
        public event ClientDisconnectedHandler Disconnected;

        public void sendMessage(string msg)
        {            
            byte[] req = Encoding.UTF8.GetBytes(msg);
            if(sck.Connected)
                sck.Send(req);                      
        }

        public int byteArrayDefrag(byte[] sData)
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
