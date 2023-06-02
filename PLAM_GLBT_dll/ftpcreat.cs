using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

namespace SC_PLAM_GLBT_DLL
    {
    class ftpcreat
        {
        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="fileinfo">需要上传的文件</param>
        /// <param name="targetDir">目标路径</param>
        /// <param name="hostname">ftp地址</param>
        /// <param name="username">ftp用户名</param>
        /// <param name="password">ftp密码</param>
        public  void UploadFile(FileInfo fileinfo, string targetDir, string hostname, string username, string password)
            {
            //1. check target
            string target;
            target = fileinfo.Name.ToString();  //使用临时文件名
            string URI;
            if (targetDir.Trim() == "")
            {
                URI = "FTP://" + hostname + "/" + target;
            }
            else
            { 
            
             URI = "FTP://" + hostname + "/" + targetDir + "/" + target;
            }
           


           
            ///WebClient webcl = new WebClient();
            System.Net.FtpWebRequest ftp = GetRequest(URI, username, password);

            //设置FTP命令 设置所要执行的FTP命令，
            //ftp.Method = System.Net.WebRequestMethods.Ftp.ListDirectoryDetails;//假设此处为显示指定路径下的文件列表
            ftp.Method = System.Net.WebRequestMethods.Ftp.UploadFile;
            //指定文件传输的数据类型
            ftp.UseBinary = true;
            ftp.UsePassive = true;

            //告诉ftp文件大小
            ftp.ContentLength = fileinfo.Length;
            //缓冲大小设置为2KB
            const int BufferSize = 2048;
            byte[] content = new byte[BufferSize - 1 + 1];
            int dataRead;

            //打开一个文件流 (System.IO.FileStream) 去读上传的文件
            using (FileStream fs = fileinfo.OpenRead())
                {
                try
                    {
                    //把上传的文件写入流
                    using (Stream rs = ftp.GetRequestStream())
                        {
                        do
                            {
                            //每次读文件流的2KB
                            dataRead = fs.Read(content, 0, BufferSize);
                            rs.Write(content, 0, dataRead);
                            } while (!(dataRead < BufferSize));
                        rs.Close();
                        }

                    }
                catch (Exception ex) { }
                finally
                    {
                    fs.Close();

                    }

                }

            //ftp = null;
            ////设置FTP命令
            //ftp = GetRequest(URI, username, password);
            //ftp.Method = System.Net.WebRequestMethods.Ftp.Rename; //改名
            //ftp.RenameTo = fileinfo.Name;
            //try
            //    {
            //    ftp.GetResponse();
            //    }
            //catch (Exception ex)
            //    {
            //    ftp = GetRequest(URI, username, password);
            //    ftp.Method = System.Net.WebRequestMethods.Ftp.DeleteFile; //删除
            //    ftp.GetResponse(); 
            //    throw ex;
            //    }
            //finally
            //    {
            //    //fileinfo.Delete();
            //    }

            // 可以记录一个日志  "上传" + fileinfo.FullName + "上传到" + "FTP://" + hostname + "/" + targetDir + "/" + fileinfo.Name + "成功." );
            ftp = null;

            #region
            /*****
             *FtpWebResponse
             * ****/
            //FtpWebResponse ftpWebResponse = (FtpWebResponse)ftp.GetResponse();
            #endregion
            }

        private  FtpWebRequest GetRequest(string URI, string username, string password)
            {
            //根据服务器信息FtpWebRequest创建类的对象
            FtpWebRequest result = (FtpWebRequest)FtpWebRequest.Create(URI);
            //提供身份验证信息
            result.Credentials = new System.Net.NetworkCredential(username, password);
            //设置请求完成之后是否保持到FTP服务器的控制连接，默认值为true
            result.KeepAlive = false;
            return result;
            }

        //获取ftp上面的文件和文件夹
        public static string[] GetFileList(string dir, string username, string password)
        {
            string[] downloadFiles;
            StringBuilder result = new StringBuilder();
            FtpWebRequest request;
            try
            {
                request = (FtpWebRequest)FtpWebRequest.Create(new Uri(dir));
                request.UseBinary = true;
                request.Credentials = new NetworkCredential(username, password);//设置用户名和密码
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.UseBinary = true;
                request.UsePassive = false; //选择主动还是被动模式 , 这句要加上的。
                request.KeepAlive = false;//一定要设置此属性，否则一次性下载多个文件的时候，会出现异常。
                WebResponse response = request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());

                string line = reader.ReadLine();
                while (line != null)
                {
                    result.Append(line);
                    result.Append("\n");
                    line = reader.ReadLine();
                }

                result.Remove(result.ToString().LastIndexOf('\n'), 1);
                reader.Close();
                response.Close();
                return result.ToString().Split('\n');
            }
            catch (Exception ex)
            {
                //LogHelper.writeErrorLog("获取ftp上面的文件和文件夹：" + ex.Message);
                downloadFiles = null;
                return downloadFiles;
            }
        }

        /// <summary>
        /// 从ftp服务器上获取文件并将内容全部转换成string返回
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static string GetFileStr(string targetDir, string hostname, string target, string username, string password)
        {
            FtpWebRequest reqFTP;
            string URI = "FTP://" + hostname + "/" + targetDir + "/" + target;
            try
            {
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(URI));
                reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(username, password);
                reqFTP.UsePassive = false; //选择主动还是被动模式 , 这句要加上的。
                reqFTP.KeepAlive = false;//一定要设置此属性，否则一次性下载多个文件的时候，会出现异常。
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(ftpStream);
                string fileStr = reader.ReadToEnd();

                reader.Close();
                ftpStream.Close();
                response.Close();
                return fileStr;
            }
            catch (Exception ex)
            {
                //LogHelper.writeErrorLog("获取ftp文件并读取内容失败：" + ex.Message);

                return null;
            }
        }


        /// <summary>
        /// 从ftp服务器上获得文件夹列表
        /// </summary>
        /// <param name="RequedstPath">服务器下的相对路径</param>
        /// <returns></returns>
        public static List<string> GetDirctory(string targetDir, string hostname, string target, string username, string password)
        {
            List<string> strs = new List<string>();
            try
            {
                string URI = "FTP://" + hostname + "/" + targetDir + "/"; //目标路径 path为服务器地址
                FtpWebRequest reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(URI));
                // ftp用户名和密码
                reqFTP.Credentials = new NetworkCredential(username, password);
                reqFTP.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                WebResponse response = reqFTP.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());//中文文件名

                string line = reader.ReadLine();
                while (line != null)
                {
                    if (line.Contains("<DIR>"))
                    {
                        string msg = line.Substring(line.LastIndexOf("<DIR>") + 5).Trim();
                        strs.Add(msg);
                    }
                    line = reader.ReadLine();
                }
                reader.Close();
                response.Close();
                return strs;
            }
            catch (Exception ex)
            {
                Console.WriteLine("获取目录出错：" + ex.Message);
            }
            return strs;
        }


        /// <summary>
        /// 读取文件目录下所有的文件名称，包括文件夹名称
        /// </summary>
        /// <param name="ftpAdd">传过来的文件夹路径</param>
        /// <returns>返回的文件或文件夹名称</returns>
        public static string[] GetFtpFileList(string targetDir, string hostname, string target, string username, string password)
        {

            string url = "FTP://" + hostname + "/" + targetDir + "/"; //目标路径 path为服务器地址
            FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(new Uri(url));
            ftpRequest.UseBinary = true;
            ftpRequest.Credentials = new NetworkCredential(username, password);

            if (ftpRequest != null)
            {
                StringBuilder fileListBuilder = new StringBuilder();
                //ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;//该方法可以得到文件名称的详细资源，包括修改时间、类型等这些属性
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;//只得到文件或文件夹的名称
                try
                {

                    WebResponse ftpResponse = ftpRequest.GetResponse();
                    StreamReader ftpFileListReader = new StreamReader(ftpResponse.GetResponseStream(), Encoding.Default);

                    string line = ftpFileListReader.ReadLine();
                    while (line != null)
                    {
                        fileListBuilder.Append(line);
                        fileListBuilder.Append("@");//每个文件名称之间用@符号隔开，便于前端调用的时候解析
                        line = ftpFileListReader.ReadLine();
                    }
                    ftpFileListReader.Close();
                    ftpResponse.Close();
                    fileListBuilder.Remove(fileListBuilder.ToString().LastIndexOf("@"), 1);
                    return fileListBuilder.ToString().Split('@');//返回得到的数组
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 读取文件目录下所有的文件名称，包括文件夹名称
        /// </summary>
        /// <param name="ftpAdd">传过来的文件夹路径</param>
        /// <returns>返回的文件或文件夹名称</returns>
        public static string GetFtpFileList1(string targetDir, string hostname, string target, string username, string password)
        {

            string url = "FTP://" + hostname + "/" + targetDir + "/"; //目标路径 path为服务器地址
            FtpWebRequest ftpRequest = (FtpWebRequest)WebRequest.Create(new Uri(url));
            ftpRequest.UseBinary = true;
            ftpRequest.Credentials = new NetworkCredential(username, password);

            if (ftpRequest != null)
            {
                StringBuilder fileListBuilder = new StringBuilder();
                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;//该方法可以得到文件名称的详细资源，包括修改时间、类型等这些属性
                //ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;//只得到文件或文件夹的名称
                try
                {

                    WebResponse ftpResponse = ftpRequest.GetResponse();
                    StreamReader ftpFileListReader = new StreamReader(ftpResponse.GetResponseStream(), Encoding.Default);

                    string line = ftpFileListReader.ReadLine();
                    while (line != null)
                    {
                        fileListBuilder.Append(line);
                        fileListBuilder.Append("@");//每个文件名称之间用@符号隔开，便于前端调用的时候解析
                        line = ftpFileListReader.ReadLine();
                    }
                    ftpFileListReader.Close();
                    ftpResponse.Close();
                    fileListBuilder.Remove(fileListBuilder.ToString().LastIndexOf("@"), 1);
                    return fileListBuilder.ToString();//返回得到的数组
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 获取当前目录下明细(包含文件和文件夹)
        /// </summary>
        /// <returns></returns>
        public string[] GetFilesDetailList(string targetDir, string hostname, string target, string username, string password)
        {
            string[] downloadFiles;
            try
            {
                string url = "FTP://" + hostname + "/" + targetDir + "/"; //目标路径 path为服务器地址
                StringBuilder result = new StringBuilder();
                FtpWebRequest ftp;
                ftp = (FtpWebRequest)FtpWebRequest.Create(new Uri(url));
                ftp.Credentials = new NetworkCredential(username, password);
                ftp.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                WebResponse response = ftp.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.Default);

                //while (reader.Read() > 0)
                //{

                //}
                string line = reader.ReadLine();
                //line = reader.ReadLine();
                //line = reader.ReadLine();

                while (line != null)
                {
                    result.Append(line);
                    result.Append("\n");
                    line = reader.ReadLine();
                }
                result.Remove(result.ToString().LastIndexOf("\n"), 1);
                reader.Close();
                response.Close();
                return result.ToString().Split('\n');
            }
            catch (Exception ex)
            {
                downloadFiles = null;
                return downloadFiles;
            }
        }
        /// <summary>
        /// 获取当前目录下所有的文件夹列表(仅文件夹)
        /// </summary>
        /// <returns></returns>
        public string[] GetDirectoryList(string[] GetFilesDetailList)
        {
            string[] drectory = GetFilesDetailList;
            string m = string.Empty;
            foreach (string str in drectory)
            {
                int dirPos = str.IndexOf("<DIR>");
                if (dirPos > 0)
                {
                    /*判断 Windows 风格*/
                    m += str.Substring(dirPos + 5).Trim() + "\n";
                }
                else if (str.Trim().Substring(0, 1).ToUpper() == "D")
                {
                    /*判断 Unix 风格*/
                    string dir = str.Substring(54).Trim();
                    if (dir != "." && dir != "..")
                    {
                        m += dir + "\n";
                    }
                }
            }

            char[] n = new char[] { '\n' };
            return m.Split(n);
        }
        /// <summary>
        /// 新建目录 上一级必须先存在
        /// </summary>
        /// <param name="dirName">服务器下的相对路径</param>
        public void MakeDir(string targetDir, string hostname, string target, string username, string password)
        {
            try
            {
                string url = "FTP://" + hostname + "/" + targetDir + "/"; //目标路径 path为服务器地址
                FtpWebRequest reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(url));
                // 指定数据传输类型
                reqFTP.UseBinary = true;
                // ftp用户名和密码
                reqFTP.Credentials = new NetworkCredential(username, password);
                reqFTP.Method = WebRequestMethods.Ftp.MakeDirectory;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                response.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("创建目录出错：" + ex.Message);
            }
        }

        }
    }
