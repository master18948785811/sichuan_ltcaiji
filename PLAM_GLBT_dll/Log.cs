using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace SC_PLAM_GLBT_DLL
{
    class Log : PLAM_GLBT
    {
        private static Object _lock = new object();
        public static void WriteInfoLog(string logContent)
        {
            try
            {
                StreamWriter stream;
                //写入日志内容
                string path = getdburl1() + "//logs";
                //检查物理路径是否存在，不存在则创建路径
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                stream = new StreamWriter(path + $"\\log{DateTime.Now.ToString("yyyyMMdd")}.txt", true, Encoding.Default);
                //stream.Write(DateTime.Now.ToString() + ":" + logContent);
                stream.Write("\r\n");//追加写入
                stream.Write("****************************************" + DateTime.Now.ToString() + "****************************************");
                stream.Write("\r\n");
                stream.Write(logContent);
                stream.Flush();
                stream.Close();//一定要关闭流
            }
            catch (Exception ex)
            {
                MessageBox.Show("写入日志失败:" + ex.ToString());
                throw;
            }
        }

        public static string getdburl1()
        {
            string[] gtdb = Assembly.GetExecutingAssembly().Location.Split('\\');
            string getdb = "";

            for (int i = 0; i < gtdb.Length - 1; i++)
            {
                getdb = getdb + gtdb[i].ToString() + "\\";

            }
            //MessageBox.Show(getdb);
            return getdb;
        }
    }
}

