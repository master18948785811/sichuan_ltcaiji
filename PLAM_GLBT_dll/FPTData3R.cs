using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using System.IO;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.Reflection;
namespace SC_PLAM_GLBT_DLL
{
    class FPTData3R
    {
        string all_data = "";
        //string []p_allfinger=new string [20];  //指位代码
        int Number = 1;                          //发送序号
        int count = 1;                           //读取指纹个数
        int Logical = 0;                         //逻辑记录长度
        Byte[] all = null;
        int i_count  = 0;
        int len = 0;
        int aaaa = 0;
        int bbb = 0;
        string zwbh;

        public enum TYPE
        {
            vchar_1 = 1, vchar_2, vchar_3, vchar_4,
            vchar_5, vchar_6, vchar_7, vchar_8,
            vchar_10 = 10, vchar_12 = 12, vchar_14 = 14, vchar_18 = 18,
            vchar_20 = 20, vchar_23 = 23, vchar_30 = 30, vchar_40 = 40,
            vchar_70 = 70, vchar_512 = 512, vchar_1024 = 1024, vchar_1800 = 1800
        }

        //外部接口
        public string packFPT()
        {
            string filePath="";
            try
            {
                len = File_length();
                all = new Byte[len];
                GreateFPT();
                filePath=GeneratedFile();
               // MessageBox.Show("生成FPT文件成功!");
            }
            catch (System.Exception ex)
            {
               MessageBox.Show(ex.ToString());
            }
            return filePath;
        }
        //获取创建FPT文件的数据
        private void GreateFPT()
        {
            /****************************GA 426-2003********************************/
            PackStr((int)TYPE.vchar_3, "FPT");                       //文件开头*
            PackStr((int)TYPE.vchar_4, "0400");                      //版本号和识别号*
            PackStr((int)TYPE.vchar_12, File_length().ToString());   //文件长度*
            PackStr((int)TYPE.vchar_2, "01");                        //逻辑记录类型*
            PackStr((int)TYPE.vchar_6, "1");                         //指纹信息记录数量,第2类逻辑记录*
            PackStr((int)TYPE.vchar_6, "");                         //第3类逻辑记录*
            PackStr((int)TYPE.vchar_6, "");                         //第4类逻辑记录*
            PackStr((int)TYPE.vchar_6, "");                         //第5类逻辑记录*
            PackStr((int)TYPE.vchar_6, "");                         //第6类逻辑记录*
            PackStr((int)TYPE.vchar_6, "");                         //第7类逻辑记录*
            PackStr((int)TYPE.vchar_6, "");                         //第8类逻辑记录*
            PackStr((int)TYPE.vchar_6, "");                         //第9类逻辑记录*
            PackStr((int)TYPE.vchar_6, "");                         //第10类逻辑记录*
            PackStr((int)TYPE.vchar_6, "");                         //第11类逻辑记录*
            PackStr((int)TYPE.vchar_6, "");                         //第12类逻辑记录*
            PackStr((int)TYPE.vchar_6, "");                         //第99类逻辑记录*
            PackStr((int)TYPE.vchar_14, System.DateTime.Now.ToString("yyyyMMddHHmmss"));           //发送时间 
            PackStr((int)TYPE.vchar_12, InformationData.nydw_gajgjgdm);             //接收单位代码
            PackStr((int)TYPE.vchar_12, InformationData.nydw_gajgjgdm);             //发送单位代码
            PackStr((int)TYPE.vchar_70, InformationData.nydw_gajgmc);             //发送单位名称 
            PackStr((int)TYPE.vchar_30, InformationData.nyry_xm);             //发送人 
            PackStr((int)TYPE.vchar_4, "2200");            //发送单位系统类型
            PackStr((int)TYPE.vchar_10, "");              //任务控制号 
            PackStr((int)TYPE.vchar_512, "");                      //备注 

            string str_F = Convert.ToString((char)28);
            PackStr((int)TYPE.vchar_1, str_F);                       //文件分割符“FS”*
            /*****************************GA 426.1-2008*******************************/
            if (0!=Logical_length())
            {
                PackStr((int)TYPE.vchar_8, Logical.ToString());      //逻辑长度记录* 
            }
            PackStr((int)TYPE.vchar_2, "02");                        //逻辑记录类型*   
            PackStr((int)TYPE.vchar_6, "1");                         //序号*  
            PackStr((int)TYPE.vchar_4, "2200");                         //系统类型*
            PackStr((int)TYPE.vchar_23, InformationData.ysxt_asjxgrybh);           //人员编号*
            zwbh = InformationData.ysxt_asjxgrybh;

            PackStr((int)TYPE.vchar_23, zwbh);  //卡号


            PackStr((int)TYPE.vchar_30, InformationData.xm);        //姓名*
                PackStr((int)TYPE.vchar_30, "");       //别名
                PackStr((int)TYPE.vchar_1, InformationData.xbdm);         //性别*
                PackStr((int)TYPE.vchar_8, InformationData.csrq);//出生日期*
                PackStr((int)TYPE.vchar_3, "156");        //国籍rs[15].ToString()
                PackStr((int)TYPE.vchar_2, InformationData.mzdm);        //名族rs[16].ToString()
                PackStr((int)TYPE.vchar_18, InformationData.zjhm);        //公民身份证号码
                PackStr((int)TYPE.vchar_3, "");                       //证件类型
                PackStr((int)TYPE.vchar_20, "");                      //证件号码

                PackStr((int)TYPE.vchar_6, InformationData.hjdz_xzqhdm);        //户籍地区化代码*
                PackStr((int)TYPE.vchar_70, InformationData.hjdz_dzmc);       //户籍地详址*
                PackStr((int)TYPE.vchar_6, InformationData.xzz_xzqhdm);        //现住址区划代码*
                PackStr((int)TYPE.vchar_70, InformationData.xzz_dzmc);       //现住址详址*
                PackStr((int)TYPE.vchar_2, "99");        //人员类别
                PackStr((int)TYPE.vchar_6, "");        //案件类别1
                PackStr((int)TYPE.vchar_6, "");        //案件类别2
                PackStr((int)TYPE.vchar_6, "");        //案件类别3
                PackStr((int)TYPE.vchar_1, "0");        //前科标识*
                PackStr((int)TYPE.vchar_1024, "无");     //前科情况
                PackStr((int)TYPE.vchar_12, InformationData.nydw_gajgjgdm);       //捺印单位代码*
                PackStr((int)TYPE.vchar_70, InformationData.nydw_gajgmc);       //捺印单位名称*
                PackStr((int)TYPE.vchar_30, InformationData.nyry_xm);       //捺印人姓名*
                PackStr((int)TYPE.vchar_8, System.DateTime.Now.ToString("yyyyMMdd"));        //捺印日期*rs[36].ToString()
                PackStr((int)TYPE.vchar_1, "9");        //协查级别
                PackStr((int)TYPE.vchar_6, "0");        //奖金
                PackStr((int)TYPE.vchar_5, "00000");        //协查目的
                PackStr((int)TYPE.vchar_23, "");       //相关人员编号

                PackStr((int)TYPE.vchar_23, "");       //相关案件编号
                PackStr((int)TYPE.vchar_1, "");       //协查有效时限*
                PackStr((int)TYPE.vchar_512, "");      //协查请求说明*
                PackStr((int)TYPE.vchar_12, "");       //协查单位代码*
                PackStr((int)TYPE.vchar_70, "");       //协查单位名称*
                PackStr((int)TYPE.vchar_8, "");        //协查日期*
                PackStr((int)TYPE.vchar_30, "");       //联系人*
                PackStr((int)TYPE.vchar_40, "");       //联系电话*
                PackStr((int)TYPE.vchar_30, "");       //审批人*
                PackStr((int)TYPE.vchar_512, "");      //备注
                PackStr((int)TYPE.vchar_1, "0");       //协查标识*

                string filePath =  getdburl()+"\\PortraitData\\portrait1" + InformationData.ysxt_asjxgrybh + ".jpeg";
                // string filePath = @"PortraitData\1.bmp";
                if (System.IO.File.Exists(filePath)) //如果不存在
                {
                    byte[] rx = System.IO.File.ReadAllBytes(filePath);
                    PackStr((int)TYPE.vchar_6, rx.Length.ToString());
                    PackStrNew(rx.Length, rx);
                }
                else
                {
                    PackStr((int)TYPE.vchar_6, "0");
                }
            //byte[] rx = InformationData.faceImage[0].rx_txsj;
            //if (null != rx)
            //{
            //    PackStr((int)TYPE.vchar_6, rx.Length.ToString());
            //    PackStrNew(rx.Length, rx);
            //}else
            //{
            //    PackStr((int)TYPE.vchar_6, "0");
            //}
            ///*************************指纹个数、长度及数据*****************************/
            PackStr((int)TYPE.vchar_2, "10");           //发送指纹个数*
            ReadFingerprint(Number);
            
        }
        //读取指纹标识号
        private void ReadFingerprint(int Flag)
        {
            bool mark = false;         //标记是否获取到指纹数据,没有获取到说明是断指
            byte[] fileBytes=null;
            int mes=0;//自定义信息长度
            int imagelength = 0;//指纹数据长度
            string str="";//指纹文件名称
            if (System.IO.File.Exists( getdburl()+"\\image\\" + Flag + ".wsq"))
            {
                byte[] ystp = System.IO.File.ReadAllBytes( getdburl()+"\\image\\" + Flag + ".wsq");
                //byte[] wtyuantu = new byte[409600];
                //    byte[] yssj = (byte[])rs[6];
                //    Array.Copy((byte[])rs[6], 1078, wtyuantu, 0, 409600);
                //     byte[] xzwtyuantu1 = xztp(wtyuantu);
                    fileBytes = ystp;
                    //fileBytes = GBMSByteToWsq(xzwtyuantu1, 640, 640);
                    imagelength = fileBytes.Length;
              
                     mark = true;       
                 }
            
            //string filePath = @"Palm\" + str + ".wsq";
            //if (System.IO.File.Exists(filePath))
            //    {
            //    FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            //    imagelength = (int)fileStream.Length;
            //    fileBytes = new byte[imagelength];
            //    fileStream.Read(fileBytes, 0, imagelength);
            //    fileStream.Close();
            //    }
            //str = null;
            int lengthFinger = 1884 + mes + 19 + imagelength;//201~221项总长度

            PackStr((int)TYPE.vchar_7, lengthFinger.ToString());         //指纹信息长度(201~221)*
            PackStr((int)TYPE.vchar_2, count.ToString());                //发送序号*
            //if (mark)
            //{
                if (Flag < 10)
                { PackStr((int)TYPE.vchar_2, Flag.ToString()); }         //01~09指位*
                else
                { PackStr((int)TYPE.vchar_2, Flag.ToString()); }      //11~20指位*
            //}else
            //{
            //    PackStr((int)TYPE.vchar_2, "00");                              //指位99*
            //}
            
            PackStr((int)TYPE.vchar_1, "O");                             //特征提取方式
            PackStr((int)TYPE.vchar_1, "6");                             //指纹纹型分类1
            PackStr((int)TYPE.vchar_1, "6");                             //指纹纹型分类2
            PackStr((int)TYPE.vchar_5, "");                          //指纹方向
            PackStr((int)TYPE.vchar_14, "");               //中心点
            PackStr((int)TYPE.vchar_14, "");               //副中心
            PackStr((int)TYPE.vchar_14, "");               //左三角
            PackStr((int)TYPE.vchar_14, "");               //右三角
            PackStr((int)TYPE.vchar_3, "0");                           //特征点个数*
            //特征点
            PackStr((int)TYPE.vchar_1800, "");
            
            /**********************************自定义信息*******************************************/
            //获取自定义信息长度
            PackStr((int)TYPE.vchar_6, "0");                            //自定义信息长度*
            if (0!=mes)
            {
                PackStr(mes, "");                                        //自定义信息数据
            }
            PackStr((int)TYPE.vchar_3, "640");                           //图像水平方向长度*
            PackStr((int)TYPE.vchar_3, "640");                           //图像垂直方向长度*
            PackStr((int)TYPE.vchar_3, "500");                           //图像分辨率*
            PackStr((int)TYPE.vchar_4, "2200");                          //图像压缩方法代码*
            if (null != fileBytes)
            {
                 PackStr((int)TYPE.vchar_6, imagelength.ToString());          //图像长度*
                 //ASCIIEncoding encoding = new ASCIIEncoding();
                 //PackStr(imagelength, encoding.GetString(fileBytes, 0, fileBytes.Length));     //图像数据*
                 PackStrNew(imagelength, fileBytes);//图像数据*
            }
            else 
            {
                PackStr((int)TYPE.vchar_6, "0");          //图像长度*
            }
            //出递归条件
            if (10== count)
            {
               // MessageBox.Show(Flag.ToString());
                string str_F = Convert.ToString((char)28);
                PackStr((int)TYPE.vchar_1,str_F);//结束符
                return;
            }else
            {
                //提取下一枚指纹
                string str_G = Convert.ToString((char)29);
                PackStr((int)TYPE.vchar_1,str_G);//分隔符
                count++;
                ReadFingerprint(++Flag);
            }
        }

        //获取数据库数据
        //private void SQLToString(int total_length, string sqlstr)
        //{
        //    string data=gj.getback(sqlstr);
        //    PackStr(total_length, data);
        //}

        //写入每条数据
        private void PackStr(int total_length,string data)
        {
            string temp = data;
            int length1 = System.Text.Encoding.Default.GetBytes(temp).Length;

            //int length1=System.Text.Encoding.UTF8.GetBytes(temp).Length;
            if (total_length > length1)
            {
                for (int i = 0; i < total_length - length1; i++)
                {
                    //data +=(char)0;
                    data += Convert.ToString((char)0);
                }
                all_data += data;
            }
            else if (total_length < length1)
            {
                data = "";
                for (int i = 0; i < total_length; i++)
                {
                    //data +=(char)0;
                    data += Convert.ToString((char)0);
                }
                all_data += data;
            }
            else if (total_length == length1)
            {
                 all_data += data;
            }
            byte[] byteArray = System.Text.Encoding.Default.GetBytes(data);//有效长度
            int j = 0;
            try
            {
                for (j = 0; j < byteArray.Length; j++)
                {
                    all[i_count] = byteArray[j];
                    i_count++;
                }
            }
            catch (System.Exception ex)
            {
                
            }
            aaaa += total_length;
            bbb++;
            //MessageBox.Show("当前长度:" + i_count.ToString() + "总长度：" + aaaa.ToString());
            if (aaaa != i_count)
            {
                //MessageBox.Show(bbb.ToString());
            }
        }

        //写入新的每天数据
        private void PackStrNew(int total_length, byte[] data) 
        {
            int i=0;
            try
            {
                for (i = 0; i < data.Length; i++)
                {
                    all[i_count] = data[i];
                    i_count++;
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            aaaa += total_length;
            bbb++;
            //MessageBox.Show("当前长度:" + i_count.ToString() + "总长度：" + aaaa.ToString());
            if (aaaa != i_count)
            {
             //   MessageBox.Show(bbb.ToString());
            }
        }
        //获取FPT整个文件长度
        private int File_length()
        {
            int All_length = Logical = Logical_length();
            All_length += 758;//(任务描述记录格式+识别符版本编号)
            return All_length;
        }

        //获取逻辑记录长度
        private int Logical_length()
        {
            int All_length = 0;
            //人像长度

            string filePath1 =  getdburl()+"\\image\\" + InformationData.ysxt_asjxgrybh + "\\f1.JPG";
            if (System.IO.File.Exists(filePath1)) //如果存在
            {
                Bitmap bs = new Bitmap(@filePath1);
                KiSaveAsJPEG(bs,  getdburl()+"\\PortraitData\\portrait1" + InformationData.ysxt_asjxgrybh + ".jpeg", 70);
                bs.Dispose();
                byte[] rx = System.IO.File.ReadAllBytes( getdburl()+"\\PortraitData\\portrait1" + InformationData.ysxt_asjxgrybh + ".jpeg");
                All_length = All_length + rx.Length;
            }
              All_length = All_length + 2778+2;//201项前的所有长度
            //所有指纹长度
            for (int i = 1; i <= 10;i++ )
            {
                //自定义信息长度
                int Custom_information_length = 0;

                string FINGPATH = getdburl()+"\\image\\" + i + ".wsq";
                if (System.IO.File.Exists(FINGPATH))
                {

                         byte[] ystp = System.IO.File.ReadAllBytes(FINGPATH);
                        //byte[] wtyuantu = new byte[409600];
                        //Array.Copy((byte[])rs[6], 1078, wtyuantu, 0, 409600);
                        //byte[] xzwtyuantu1 = xztp(wtyuantu);
                        byte[] fileBytes = ystp;
                         //byte[] fileBytes= GBMSByteToWsq(xzwtyuantu1, 640, 640);
                        int temp = fileBytes.Length;
                        All_length += temp;
                    
                }
               
                All_length = All_length + Custom_information_length + 1904;//201~221项总长度
            }
            return All_length;
        }
        //生成FPT文件
        private string GeneratedFile()
        {
            string fileName =  getdburl()+"\\FPT\\" + zwbh+ ".fpt";
            if (System.IO.File.Exists(fileName))
            {
                File.Delete(fileName);
            }
            Thread.Sleep(1000);
            //byte[] myByte = System.Text.Encoding.UTF8.GetBytes(all_data);
           // MessageBox.Show(i_count.ToString() + " " + len.ToString());

            using (FileStream fsWrite = new FileStream(fileName, FileMode.Append))
            {
                fsWrite.Write(all, 0, all.Length);
                fsWrite.Close();
            }
            return fileName;
        }
        //获取图片路径
        private void ImagePath(string filePath)
        {
            if (!System.IO.File.Exists(filePath)) //如果不存在
                return;
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            int byteLength = (int)fileStream.Length;
            byte[] fileBytes = new byte[byteLength];
            fileStream.Read(fileBytes, 0, byteLength);

            //文件流关闭,文件解除锁定
            fileStream.Close();
        }
        private byte[] xztp(byte[] bitmap1)  //旋转图片
            {
            byte[] mm = new byte[409600];

            for (int i = 0; i < 640; i++)
                {
                for (int j = 0; j < 640; j++)
                    {
                    if (i == 0)
                        {
                        mm[408960 + j] = bitmap1[j];

                        }
                    else
                        {
                        mm[408960 - (640 * i) + j] = bitmap1[640 * i + j];

                        }

                    }

                }

            return mm;
            }
        public static bool KiSaveAsJPEG(Bitmap bmp, string FileName, int Qty)//压缩图片
            {
            try
                {
                EncoderParameter p;
                EncoderParameters ps;

                ps = new EncoderParameters(1);

                p = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, Qty);
                ps.Param[0] = p;

                bmp.Save(FileName, GetCodecInfo("image/jpeg"), ps);
                

                return true;
                }
            catch
                {
                return false;
                }

            }
        private static ImageCodecInfo GetCodecInfo(string mimeType)
            {
            ImageCodecInfo[] CodecInfo = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo ici in CodecInfo)
                {
                if (ici.MimeType == mimeType) return ici;
                }
            return null;
            }
        //格林比特指纹压缩
        private byte[] GBMSByteToWsq(byte[] RawBuf, int nWidth, int nHeight)
        {
            byte[] encoded = null;
            try
            {
                WSQPACK_NET_WRAPPER.NW_WSQPACK_Compress WSQC = new WSQPACK_NET_WRAPPER.NW_WSQPACK_Compress(RawBuf, nWidth, nHeight);
                WSQC.CompressionRate = 0.75f;
                encoded = WSQC.Encoded;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("压缩失败:" + ex.ToString());
            }
            return encoded;
        }
       
        /// 获取运行根目录
        /// </summary>
        /// <returns></returns>
        public string getdburl()
        {
            string[] gtdb = Assembly.GetExecutingAssembly().Location.Split('\\');
            string getdb = "";

            for (int i = 0; i < gtdb.Length - 1; i++)
            {
                getdb = getdb + gtdb[i].ToString() + "\\";

            }

            return getdb;
        }
    }
}
