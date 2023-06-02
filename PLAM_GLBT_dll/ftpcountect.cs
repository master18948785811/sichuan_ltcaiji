using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace SC_PLAM_GLBT_DLL
{
    class ftpcountect
    {

        private static ManualResetEvent timeoutObject;
        private static Socket socket = null;
        private static bool isConn = false;
        /// <summary>
        /// 通过socket判断ftp是否通畅(异步socket连接,同步发送接收数据)
        /// </summary> 
        /// <returns></returns>
        public  bool CheckFtp(string ip, string ftpuser, string ftppas, out string errmsg, int port, int timeout = 10000)
        {
            #region 输入数据检查
            if (ftpuser.Trim().Length == 0)
            {
                errmsg = "FTP用户名不能为空,请检查设置!";
                return false;
            }
            if (ftppas.Trim().Length == 0)
            {
                errmsg = "FTP密码不能为空,请检查设置!";
                return false;
            }
            IPAddress address;
            try
            {
                address = IPAddress.Parse(ip);
            }
            catch
            {
                errmsg = string.Format("FTP服务器IP:{0}解析失败,请检查是否设置正确!", ip);
                return false;
            }
            #endregion
            isConn = false;

            bool ret = false;
            byte[] result = new byte[1024];
            int pingStatus = 0, userStatus = 0, pasStatus = 0, exitStatus = 0; //连接返回,用户名返回,密码返回,退出返回
            timeoutObject = new ManualResetEvent(false);
            try
            {
                int receiveLength;

                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.SendTimeout = timeout;
                socket.ReceiveTimeout = timeout;//超时设置成2000毫秒

                try
                {
                    socket.BeginConnect(new IPEndPoint(address, port), new AsyncCallback(callBackMethod), socket); //开始异步连接请求
                    if (!timeoutObject.WaitOne(timeout, false))
                    {
                        socket.Close();
                        socket = null;
                        pingStatus = -1;
                    }
                    if (isConn)
                    {
                        pingStatus = 200;
                    }
                    else
                    {
                        pingStatus = -1;
                    }
                }
                catch (Exception ex)
                {
                    pingStatus = -1;
                }

                if (pingStatus == 200) //状态码200 - TCP连接成功
                {
                    receiveLength = socket.Receive(result);
                    pingStatus = getFtpReturnCode(result, receiveLength); //连接状态
                    if (pingStatus == 220)//状态码220 - FTP返回欢迎语
                    {
                        socket.Send(Encoding.Default.GetBytes(string.Format("{0}{1}", "USER " + ftpuser, Environment.NewLine)));
                        receiveLength = socket.Receive(result);
                        userStatus = getFtpReturnCode(result, receiveLength);
                        if (userStatus == 331)//状态码331 - 要求输入密码
                        {
                            socket.Send(Encoding.Default.GetBytes(string.Format("{0}{1}", "PASS " + ftppas, Environment.NewLine)));
                            receiveLength = socket.Receive(result);
                            pasStatus = getFtpReturnCode(result, receiveLength);
                            if (pasStatus == 230)//状态码230 - 登入因特网
                            {
                                errmsg = string.Format("FTP:{0}@{1}登陆成功", ip, port);
                                ret = true;
                                socket.Send(Encoding.Default.GetBytes(string.Format("{0}{1}", "QUIT", Environment.NewLine))); //登出FTP
                                receiveLength = socket.Receive(result);
                                exitStatus = getFtpReturnCode(result, receiveLength);
                            }
                            else
                            { // 状态码230的错误
                                errmsg = string.Format("FTP:{0}@{1}登陆失败,用户名或密码错误({2})", ip, port, pasStatus);
                            }
                        }
                        else
                        {// 状态码331的错误 
                            errmsg = string.Format("使用用户名:'{0}'登陆FTP:{1}@{2}时发生错误({3}),请检查FTP是否正常配置!", ftpuser, ip, port, userStatus);
                        }
                    }
                    else
                    {// 状态码220的错误 
                        errmsg = string.Format("FTP:{0}@{1}返回状态错误({2}),请检查FTP服务是否正常运行!", ip, port, pingStatus);
                    }
                }
                else
                {// 状态码200的错误
                    errmsg = string.Format("无法连接FTP服务器:{0}@{1},请检查FTP服务是否启动!", ip, port);
                }
            }
            catch (Exception ex)
            { //连接出错 
                errmsg = string.Format("FTP:{0}@{1}连接出错:", ip, port) + ex.Message;
                //Common.Logger(errmsg);
                ret = false;
            }
            finally
            {
                if (socket != null)
                {
                    socket.Close(); //关闭socket
                    socket = null;
                }
            }
            return ret;
        }
        private static void callBackMethod(IAsyncResult asyncResult)
        {
            try
            {
                socket = asyncResult.AsyncState as Socket;
                if (socket != null)
                {
                    socket.EndConnect(asyncResult);
                    isConn = true;
                }
            }
            catch (Exception ex)
            {
                isConn = false;
            }
            finally
            {
                timeoutObject.Set();
            }
        }
        /// <summary>
        /// 传递FTP返回的byte数组和长度,返回状态码(int)
        /// </summary>
        /// <param name="retByte"></param>
        /// <param name="retLen"></param>
        /// <returns></returns>
        private static int getFtpReturnCode(byte[] retByte, int retLen)
        {
            try
            {
                string str = Encoding.ASCII.GetString(retByte, 0, retLen).Trim();
                return int.Parse(str.Substring(0, 3));
            }
            catch
            {
                return -1;
            }
        }    
    }
}
