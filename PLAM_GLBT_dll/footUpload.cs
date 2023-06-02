using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;

namespace SC_PLAM_GLBT_DLL
{
    class footUpload
    {
        public footUpload() 
        {
            
        }

        /// <summary>
        /// 上传足迹数据
        /// </summary>
        /// <param name="xmlString">足迹xml包</param>
        /// <param name="Url">上传地址</param>
        /// <param name="resultmsg">上传结果</param>
        public void sendMessage(string xmlString, string Url,ref string resultmsg)
        {
            //XmlDocument xmlDoc = GetOriginalData(filePath);
            //string xmlString = xmlDoc.InnerXml;
            byte[] byteRequest = System.Text.Encoding.UTF8.GetBytes(xmlString);
            // Create a request for the URL.
            string strURL = Url;
            //string receiveUrl = System.Configuration.ConfigurationManager.AppSettings["ReceiveSusInfoUrl"];
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(Url);
            myRequest.Method = "POST";
            myRequest.ContentType = "text/html";
            myRequest.ContentLength = byteRequest.Length;
            using (System.IO.Stream reqStream = myRequest.GetRequestStream())
            {
                reqStream.Write(byteRequest, 0, byteRequest.Length);
            }
            // Get the response.
            using (HttpWebResponse myResponse = (HttpWebResponse)myRequest.GetResponse())
            {
                // Get the stream containing content returned by the server.
                System.IO.Stream dataStream = myResponse.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                System.IO.StreamReader reader = new System.IO.StreamReader(dataStream, System.Text.Encoding.UTF8);
                // Read the content.
                resultmsg = reader.ReadToEnd();
                // Cleanup the streams and the response.
                reader.Close();
                dataStream.Close();
                myResponse.Close();
                //tbResponse.Text += "!是否成功，请查看源文件：<!--" + responseFromServer + "-->" + "\n ";

            }
        }

        private XmlDocument GetOriginalData(string filePath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);
            return xmlDoc;
        }

    }
}
