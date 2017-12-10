using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace VideoController
{
    public class ConnectionInfo
    {
        public Socket socket;
        public string ip;
        public string name;
        public string uuid;
        public bool isTeacher;
        
        public void setData(string name, string uuid, bool isTeacher)
        {
            this.name = name;
            this.uuid = uuid;
            this.isTeacher = isTeacher;
        }
    }
}
