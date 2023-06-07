using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FOOT_HR
{
    internal class Tool
    {
        #region 读取程序运行目录
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
        #endregion

        #region 声明读写INI文件的API函数
        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string section, string key, string defVal, StringBuilder retVal, int size, string filePath);
        #endregion

        #region 读取INI文件
        /// <summary>
        /// 读取INI文件
        /// </summary>
        /// <param name="section">段落</param>
        /// <param name="key">键</param>
        /// <returns>返回的键值</returns>
        public static string IniReadValue(string section, string key, string filePath)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(section, key, "", temp, 255, filePath);
            return temp.ToString();
        }
        #endregion

        #region 图片转image
        public static Image ReturnPhoto(string imagepath)
        {
            List<byte> listbit = new List<byte>();
            FileStream FileStream = new FileStream(imagepath, FileMode.Open);
            byte[] byData = new byte[FileStream.Length];
            FileStream.Read(byData, 0, byData.Length);
            FileStream.Close();
            listbit.AddRange(byData);
            byte[] bit = listbit.ToArray();

            System.IO.MemoryStream ms = new System.IO.MemoryStream(bit);
            System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
            return img;
        }
        #endregion

        #region image转base64
        public static string ChangeImageToString(Image image)
        {
            try
            {
                MemoryStream ms = new MemoryStream();
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                byte[] arr = new byte[ms.Length];
                ms.Position = 0;
                ms.Read(arr, 0, (int)ms.Length);
                ms.Close();
                string pic = Convert.ToBase64String(arr);
                return pic;
            }
            catch (Exception ex)
            {
                Log.WriteInfoLog(ex.ToString());
                return "Fail to change bitmap to string!";
            }
        }
        #endregion

        # region 解码base64
        public static string DecodeBase64(string code_type, string code)
        {
            string decode = "";
            byte[] bytes = Convert.FromBase64String(code);
            try
            {
                decode = Encoding.GetEncoding(code_type).GetString(bytes);
            }
            catch (Exception ex)
            {
                decode = code;
                Log.WriteInfoLog(ex.ToString());
            }
            return decode;
        }
        #endregion

        #region base64转image
        public static Image ChangeStringToImage(string pic)
        {
            try
            {
                byte[] imageBytes = Convert.FromBase64String(pic);
                //读入MemoryStream对象
                MemoryStream memoryStream = new MemoryStream(imageBytes, 0, imageBytes.Length);
                memoryStream.Write(imageBytes, 0, imageBytes.Length);
                //转成图片
                Image image = Image.FromStream(memoryStream);

                return image;
            }
            catch (Exception ex)
            {
                Image image = null;
                Log.WriteInfoLog(ex.ToString());
                return image;
            }
        }
        #endregion
    }
}
