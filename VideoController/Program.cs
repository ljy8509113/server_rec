using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using System.Net;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;

namespace VideoController
{
    static class Program
    {
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(Common.KEY_URL);
            myReq.Method = "GET"; // 필요 없는듯?
            
            //HttpWebResponse 객체 받아옴
            HttpWebResponse wRes = (HttpWebResponse)myReq.GetResponse();
            
            // Response의 결과를 스트림을 생성합니다.
            Stream respGetStream = wRes.GetResponseStream();
            StreamReader readerGet = new StreamReader(respGetStream, Encoding.UTF8);
            
            // 생성한 스트림으로부터 string으로 변환합니다.
            string resultGet = readerGet.ReadToEnd();

            JObject obj = JObject.Parse(resultGet);
            Console.WriteLine("json : {0}", obj);
            Console.WriteLine();

            FileInfo keyFile = new FileInfo(Common.KEY_FILE);
            if (obj["key"].ToString().ToLower().Equals("t"))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
            else
            {
                MessageBox.Show("사용이 만료 되었습니다.");
                Application.Exit();
            }
        }
    }
}
