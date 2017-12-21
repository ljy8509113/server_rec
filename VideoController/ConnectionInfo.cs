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
        //public Socket socket;
        public object client;
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

        public bool isDownloading = false;
        public int current = 0;
        public int progress = 0;
        public int max = 0;
        public string fileName;

        public void sendFile(bool isDown, string fileName)
        {
            this.isDownloading = isDown;
            this.fileName = fileName;
        }

        public void progressData(int current, int progress, int max)
        {
            this.current = current;
            this.progress = progress;
            this.max = max;
        }

        public void endDownLoad()
        {
            isDownloading = false;
            current = 0;
            progress = 0;
            max = 0;
            fileName = "";
        }

        public string getDownMsg()
        {
            if (isDownloading)
            {
                if (current == max && progress == 100)
                    return "전송완료";
                else
                    return "전송중 (" + current + "/" + max + " : " + progress + "%)";
            }
            else
            {
                return "접속중";
            }
            
        }
    }
}


//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Net.Sockets;

//namespace VideoController
//{
//    public class ConnectionInfo
//    {
//        public Socket socket;
//        public string ip;
//        public string name;
//        public string uuid;
//        public bool isTeacher;

//        public void setData(string name, string uuid, bool isTeacher)
//        {
//            this.name = name;
//            this.uuid = uuid;
//            this.isTeacher = isTeacher;
//        }
//    }
//}
