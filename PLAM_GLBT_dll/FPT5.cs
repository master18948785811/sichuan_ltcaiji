using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Windows.Forms;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;

namespace SC_PLAM_GLBT_DLL
    {
    class FPT5
        {
        string alldata = "";
        int Flag = 1;
        int plams = 31;
        int four = 21;
        int picture = 1;
        public void packFPT()
            {
                try
                {
                    GreateFPT();
                    GeneratedFile();
                    //MessageBox.Show("生成FPT文件成功!");
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        private void GreateFPT()
            {
            alldata = alldata + "<?xml version=\"1.0\" encoding=\"utf-8\" ?><package>";
            gethead();
            getFPackage();
            alldata = alldata +"</package>";
            }
     
        private void gethead()
            {
                alldata = alldata + "<packageHead>";
                alldata = alldata + " <version>FPT0500</version>";
                alldata = alldata + " <createTime>" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + "</createTime>";
                alldata = alldata + " <originSystem>CJ</originSystem>";
                alldata = alldata + " <fsdw_gajgjgdm>" + gj.getback("select Address from admin where username='" + Program.user + "'") + "</fsdw_gajgjgdm>";
                alldata = alldata + " <fsdw_gajgmc>" + gj.getback("select UnitName from admin where username='" + Program.user + "'") + "</fsdw_gajgmc>";
                alldata = alldata + " <fsdw_xtlx>1419</fsdw_xtlx>";
                alldata = alldata + " <fsr_xm>" + gj.getback("select PersonalName from admin where username='" + Program.user + "'") + "</fsr_xm>";
                alldata = alldata + " <fsr_gmsfhm>" + gj.getback("select IDcardnumber from admin where username='" + Program.user + "'") + "</fsr_gmsfhm>";
                alldata = alldata + " <fsr_lxdh>" + gj.getback("select Telnumber from admin where username='" + Program.user + "'") + "</fsr_lxdh>";
                alldata = alldata + "</packageHead>";

            }
        private void getFPackage()
            {

            alldata = alldata + "<fingerprintPackage>";
            getDMsg();
            getCMsg();
            alldata = alldata + "<fingers>";
            getfigers();
            alldata = alldata + "</fingers>";
            //alldata = alldata + "<palms>";
            //getpalms();
            //alldata = alldata + "</palms>";
            //alldata = alldata + "<fourprints>";
            //getfourprints();
            //alldata = alldata + "</fourprints>";
            alldata = alldata + "<faceImages>";
            getpicture();
            alldata = alldata + "</faceImages>";

            alldata = alldata + "</fingerprintPackage>";
          
            }
     
        private void getDMsg()
            {
                string sqlinf = "select * from PersonnelInformation where PersonID='" + helpuser.userid + "'";
                foreach (DataRow rs in gj.gettable(sqlinf).Rows)
                {
                    alldata = alldata + "<descriptiveMsg>";
                    alldata = alldata + " <ysxt_asjxgrybh>" + helpuser.userid + "</ysxt_asjxgrybh>";
                    alldata = alldata + "<jzrybh></jzrybh>";
                    alldata = alldata + "<asjxgrybh></asjxgrybh>";
                    alldata = alldata + "<zzhwkbh></zzhwkbh>";
                    alldata = alldata + "<collectingReasonSet><cjxxyydm>" + rs[52].ToString() + "</cjxxyydm></collectingReasonSet>";
                    alldata = alldata + "<xm>" + rs[2].ToString() + "</xm>";
                    alldata = alldata + "<bmch>" + rs[13].ToString() + "</bmch>";
                    alldata = alldata + "<xbdm>" + rs[7].ToString() + "</xbdm>";
                    alldata = alldata + "<csrq>" + rs[8].ToString().Replace("-", "") + "</csrq>";
                    alldata = alldata + "<gjdm>156</gjdm>";
                    alldata = alldata + "<mzdm>" + rs[16].ToString() + "</mzdm>";
                    alldata = alldata + "<cyzjdm>111</cyzjdm>";
                    alldata = alldata + "<zjhm>" + rs[4].ToString() + "</zjhm>";
                    alldata = alldata + "<hjdz_xzqhdm>" + rs[18].ToString() + "</hjdz_xzqhdm>";
                    alldata = alldata + "<hjdz_dzmc>" + rs[19].ToString() + "</hjdz_dzmc>";
                    alldata = alldata + "<xzz_xzqhdm>" + rs[31].ToString() + "</xzz_xzqhdm>";
                    alldata = alldata + "<xzz_dzmc>" + rs[32].ToString() + "</xzz_dzmc>";
                    alldata = alldata + "<bz></bz>";

                    alldata = alldata + "</descriptiveMsg>";

                }

            }
        private void getCMsg()
            {
                alldata = alldata + "<collectInfoMsg>";
                alldata = alldata + "<zwbdxtlxms>1419</zwbdxtlxms>";
                alldata = alldata + "<nydw_gajgjgdm>" + gj.getback("select Address from admin where username='" + Program.user + "'") + "</nydw_gajgjgdm>";
                alldata = alldata + "<nydw_gajgmc>" + gj.getback("select UnitName from admin where username='" + Program.user + "'") + "</nydw_gajgmc>";
                alldata = alldata + "<nyry_xm>" + gj.getback("select PersonalName from admin where username='" + Program.user + "'") + "</nyry_xm>";
                alldata = alldata + "<nyry_gmsfhm>" + gj.getback("select IDcardnumber from admin where username='" + Program.user + "'") + "</nyry_gmsfhm>";
                alldata = alldata + "<nyry_lxdh>" + gj.getback("select Telnumber from admin where username='" + Program.user + "'") + "</nyry_lxdh>";
                alldata = alldata + "<nysj>" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + "</nysj>";
                alldata = alldata + "</collectInfoMsg>";

            }
        private void getfigers()
            {
            bool mark = false;         //标记是否获取到指纹数据,没有获取到说明是断指
            string str = "";//指纹文件名称

            byte[] MntBuff = null;               //特征点缓冲区
            int MntLen = 0;                        //指纹缓冲区长度
            string FingerState = "0";            //指纹状态0 正常 1 残缺 2 系统设置不采集 3 受伤未采集 9 其他缺失情况 

            string sqlfig = "select * from FingerPalm where PersonID='" + helpuser.userid + "'and Flag='" + Flag.ToString() + "'";
            foreach (DataRow rs in gj.gettable(sqlfig).Rows)
                {
                    //获取指纹状态
                    FingerState = rs[16].ToString();
                    if ("" != rs[5].ToString())
                    {


                        str = rs[3].ToString() + helpuser.userid + ".wsq";
                        MntLen = int.Parse(rs[13].ToString());//获取特征点长度
                        MntBuff = new byte[MntLen];
                        MntBuff = (byte[])rs[12];
                        mark = true;
                    }
                    else
                    {

                        mark = false;
                    }
                }
            alldata = alldata + "<fingerMsg>";
          
                if (Flag < 10)
                    alldata = alldata + "<zwzwdm>0" + Flag.ToString() + "</zwzwdm>";     //01~09指位*
                else
                    alldata = alldata + "<zwzwdm>" + Flag.ToString() + "</zwzwdm>";                 //10~20指位*
         

                    alldata = alldata + "<zzhwqsqkdm>" + FingerState + "</zzhwqsqkdm>";   //指纹其它状态

               
           
            alldata = alldata + "<zw_txspfxcd>640</zw_txspfxcd>";
            alldata = alldata + "<zw_txczfxcd>640</zw_txczfxcd>";
            alldata = alldata + "<zw_txfbl>500</zw_txfbl>";
            alldata = alldata + "<zw_txysffms>1419</zw_txysffms>";

            if (mark)
                {
                alldata = alldata + "<zw_txzl>60</zw_txzl>";
                alldata = alldata + " <zw_txsj>" + Convert.ToBase64String(System.IO.File.ReadAllBytes(@"Palm\" + str)) + "</zw_txsj>";
                }

            alldata = alldata + "</fingerMsg>";

            if (Flag < 20)
                {
                Flag++;
                getfigers();

                }
            }
        private void getpalms()
            {

            bool mark = false;         //标记是否获取到指纹数据,没有获取到说明是断指
            byte[] fileBytes = new byte[2398922];
            int mes = 0;//自定义信息长度
            int imagelength = 0;//指纹数据长度
            int gd = 0;
            string str = "";//指纹文件名称
            switch (plams)
            { 
                case 31:
                    gd=201;
                    break;
                case 32:
                    gd = 301;
                    break;
                case 33:
                    gd = 202;
                    break;
                case 34:
                    gd = 302;
                    break;
                
             }


            string sqlfig = "select * from FingerPalm where PersonID='" + helpuser.userid + "'and Flag='" + gd.ToString() + "'";
            foreach (DataRow rs in gj.gettable(sqlfig).Rows)
                {

                if ("" != rs[5].ToString())
                    {


                    str = rs[3].ToString() + helpuser.userid + ".wsq";
                    mark = true;
                    }
                }
            alldata = alldata + "<palmMsg>";
            if (mark)
                {

                alldata = alldata + "<zhwzhwdm>" + plams.ToString() + "</zhwzhwdm>";
                }
            else
                {
                alldata = alldata + "<zhwzhwdm>99</zhwzhwdm>";
                }
            if (mark)
                {
                alldata = alldata + "<zhw_zzhwqsqkdm>0</zhw_zzhwqsqkdm>";
                }
            else
                {

                alldata = alldata + "<zhw_zzhwqsqkdm>1</zhw_zzhwqsqkdm>";

                }
            alldata = alldata + "<zhw_txspfxcd>1600</zhw_txspfxcd>";
            alldata = alldata + "<zhw_txczfxcd>1500</zhw_txczfxcd>";
            alldata = alldata + "<zhw_txfbl>500</zhw_txfbl>";
            alldata = alldata + "<zhw_txysffms>1419</zhw_txysffms>";
            
            if (mark)
                {
                alldata = alldata + "<zhw_txzl>60</zhw_txzl>";
                alldata = alldata + " <zhw_txsj>" + Convert.ToBase64String(System.IO.File.ReadAllBytes(@"Palm\" + str)) + "</zhw_txsj>";
                }
            alldata = alldata + "</palmMsg>";

            if (plams < 34)
                {
                plams++;
                 getpalms();

                }

            }
        private void getfourprints()
            {

            bool mark = false;         //标记是否获取到指纹数据,没有获取到说明是断指

            int gd = 0;
            string str = "";//指纹文件名称
            string sqlfig = "select * from FingerPalm where PersonID='" + helpuser.userid + "'and Flag='" + four.ToString() + "'";
            foreach (DataRow rs in gj.gettable(sqlfig).Rows)
                {

                if ("" != rs[5].ToString())
                    {


                    str = rs[3].ToString() + helpuser.userid + ".wsq";
                    mark = true;
                    }
                }
            alldata = alldata + "<fourprintMsg>";
            if (mark)
                {

                alldata = alldata + "<slz_zwzwdm>" + four.ToString() + "</slz_zwzwdm>";
                }
            else
                {
                alldata = alldata + "<slz_zwzwdm>99</slz_zwzwdm>";
                }
            if (mark)
                {
                alldata = alldata + "<slz_zzhwqsqkdm>0</slz_zzhwqsqkdm>";
                }
            else
                {

                alldata = alldata + "<slz_zzhwqsqkdm>1</slz_zzhwqsqkdm>";

                }
            alldata = alldata + "<slz_txspfxcd>1600</slz_txspfxcd>";
            alldata = alldata + "<slz_txczfxcd>1500</slz_txczfxcd>";
            alldata = alldata + "<slz_txfbl>500</slz_txfbl>";
            alldata = alldata + "<slz_txysffms>1419</slz_txysffms>";

            if (mark)
                {
                alldata = alldata + "<slz_txzl>60</slz_txzl>";
                alldata = alldata + " <slz_txsj>" + Convert.ToBase64String(System.IO.File.ReadAllBytes(@"Palm\" + str)) + "</slz_txsj>";
                }
            alldata = alldata + "</fourprintMsg>";

            if (four < 22)
                {
                four++;
                getfourprints();

                }
          
            }
        private void getpicture()
            {
              
              string filePath1="";
            switch (picture)
            { 
                
                
                
                case 1:
                     filePath1 = @"PortraitData\portrait1" + helpuser.userid + ".jpg";
                     if (System.IO.File.Exists(filePath1)) //如果不存在
                         {
                     Bitmap bs = new Bitmap(filePath1);
                     KiSaveAsJPEG(bs, @"PortraitData\portrait1" + helpuser.userid + ".jpeg", 70);
                      bs.Dispose();
                      alldata = alldata + "<faceImage>";
                      alldata = alldata + "<rxzplxdm>1</rxzplxdm>";
                      alldata = alldata + "<rx_dzwjgs>JEPG</rx_dzwjgs>";
                      alldata = alldata + " <rx_txsj>" + Convert.ToBase64String(System.IO.File.ReadAllBytes(@"PortraitData\portrait1" + helpuser.userid + ".jpeg")) + "</rx_txsj>";
                      alldata = alldata + "</faceImage>";
                         }
                     break;
                case 2:
                     filePath1 = @"PortraitData\portrait2" + helpuser.userid + ".jpg";
                     if (System.IO.File.Exists(filePath1)) //如果不存在
                         {
                         Bitmap bs = new Bitmap(filePath1);
                         KiSaveAsJPEG(bs, @"PortraitData\portrait2" + helpuser.userid + ".jpeg", 70);
                         bs.Dispose();
                         alldata = alldata + "<faceImage>";
                         alldata = alldata + "<rxzplxdm>2</rxzplxdm>";
                         alldata = alldata + "<rx_dzwjgs>JEPG</rx_dzwjgs>";
                         alldata = alldata + " <rx_txsj>" + Convert.ToBase64String(System.IO.File.ReadAllBytes(@"PortraitData\portrait2" + helpuser.userid + ".jpeg")) + "</rx_txsj>";
                         alldata = alldata + "</faceImage>";
                         }
                     break;
                case 3:
                     filePath1 = @"PortraitData\portrait3" + helpuser.userid + ".jpg";
                     if (System.IO.File.Exists(filePath1)) //如果不存在
                         {
                         Bitmap bs = new Bitmap(filePath1);
                         KiSaveAsJPEG(bs, @"PortraitData\portrait3" + helpuser.userid + ".jpeg", 70);
                         bs.Dispose();
                         alldata = alldata + "<faceImage>";
                         alldata = alldata + "<rxzplxdm>4</rxzplxdm>";
                         alldata = alldata + "<rx_dzwjgs>JEPG</rx_dzwjgs>";
                         alldata = alldata + " <rx_txsj>" + Convert.ToBase64String(System.IO.File.ReadAllBytes(@"PortraitData\portrait3" + helpuser.userid + ".jpeg")) + "</rx_txsj>";
                         alldata = alldata + "</faceImage>";
                         }
                     break;
                default: break;

                }
            if (picture < 4)
                {
                picture++;
                getpicture();
                }
            }
        
        private void GeneratedFile()
            {
                string fileName = System.IO.Directory.GetCurrentDirectory() + "\\FPT\\" + helpuser.userid + ".fptx";
            if (System.IO.File.Exists(fileName))
                {
                File.Delete(fileName);
                }
            Thread.Sleep(1000);
            //byte[] myByte = System.Text.Encoding.UTF8.GetBytes(all_data);
            // MessageBox.Show(i_count.ToString() + " " + len.ToString());

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileName, true))
                {
                file.Write(alldata);
                file.Close();


                }
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
        }
    }