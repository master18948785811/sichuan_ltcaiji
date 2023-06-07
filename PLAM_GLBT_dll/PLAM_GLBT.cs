using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using GBMSAPI_NET;
using GBMSAPI_NET.GBMSAPI_NET_Defines;
using GBMSAPI_NET.GBMSAPI_NET_Defines.GBMSAPI_NET_AcquisitionProcessDefines;
using GBMSAPI_NET.GBMSAPI_NET_Defines.GBMSAPI_NET_DeviceCharacteristicsDefines;
using GBMSAPI_NET.GBMSAPI_NET_Defines.GBMSAPI_NET_ErrorCodesDefines;
using GBMSAPI_NET.GBMSAPI_NET_Defines.GBMSAPI_NET_RollFunctionalityDefines;
using GBMSAPI_NET.GBMSAPI_NET_Defines.GBMSAPI_NET_VisualInterfaceLCDDefines;
using GBMSAPI_NET.GBMSAPI_NET_Defines.GBMSAPI_NET_VisualInterfaceLEDsDefines;
using GBMSAPI_NET.GBMSAPI_NET_LibraryFunctions;
using GBFINIMG_NET_WRAPPER;
using Rectangle = System.Drawing.Rectangle;
using System.Threading;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using GBMSAPI_CS_Example.UTILITY;
using GBFINIMG_NET_WRAPPER;
using System.Xml;
using static SC_PLAM_GLBT_DLL.InformationData;

namespace SC_PLAM_GLBT_DLL
{
    [Guid("53583C8A-1A04-4C28-BE1D-5EDD04839195")]
    public partial class PLAM_GLBT : UserControl, IObjectSafety
    {
        #region IObjectSafety 接口成员实现（直接拷贝即可）

        private const string _IID_IDispatch = "{00020400-0000-0000-C000-000000000046}";
        private const string _IID_IDispatchEx = "{a6ef9860-c720-11d0-9337-00a0c90dcaa9}";
        private const string _IID_IPersistStorage = "{0000010A-0000-0000-C000-000000000046}";
        private const string _IID_IPersistStream = "{00000109-0000-0000-C000-000000000046}";
        private const string _IID_IPersistPropertyBag = "{37D84F60-42CB-11CE-8135-00AA004BB851}";

        private const int INTERFACESAFE_FOR_UNTRUSTED_CALLER = 0x00000001;
        private const int INTERFACESAFE_FOR_UNTRUSTED_DATA = 0x00000002;
        private const int S_OK = 0;
        private const int E_FAIL = unchecked((int)0x80004005);
        private const int E_NOINTERFACE = unchecked((int)0x80004002);

        private bool _fSafeForScripting = true;
        private bool _fSafeForInitializing = true;

        public int GetInterfaceSafetyOptions(ref Guid riid, ref int pdwSupportedOptions, ref int pdwEnabledOptions)
        {
            int Rslt = E_FAIL;

            string strGUID = riid.ToString("B");
            pdwSupportedOptions = INTERFACESAFE_FOR_UNTRUSTED_CALLER | INTERFACESAFE_FOR_UNTRUSTED_DATA;
            switch (strGUID)
            {
                case _IID_IDispatch:
                case _IID_IDispatchEx:
                    Rslt = S_OK;
                    pdwEnabledOptions = 0;
                    if (_fSafeForScripting == true)
                        pdwEnabledOptions = INTERFACESAFE_FOR_UNTRUSTED_CALLER;
                    break;
                case _IID_IPersistStorage:
                case _IID_IPersistStream:
                case _IID_IPersistPropertyBag:
                    Rslt = S_OK;
                    pdwEnabledOptions = 0;
                    if (_fSafeForInitializing == true)
                        pdwEnabledOptions = INTERFACESAFE_FOR_UNTRUSTED_DATA;
                    break;
                default:
                    Rslt = E_NOINTERFACE;
                    break;
            }

            return Rslt;
        }

        public int SetInterfaceSafetyOptions(ref Guid riid, int dwOptionSetMask, int dwEnabledOptions)
        {
            int Rslt = E_FAIL;
            string strGUID = riid.ToString("B");
            switch (strGUID)
            {
                case _IID_IDispatch:
                case _IID_IDispatchEx:
                    if (((dwEnabledOptions & dwOptionSetMask) == INTERFACESAFE_FOR_UNTRUSTED_CALLER) && (_fSafeForScripting == true))
                        Rslt = S_OK;
                    break;
                case _IID_IPersistStorage:
                case _IID_IPersistStream:
                case _IID_IPersistPropertyBag:
                    if (((dwEnabledOptions & dwOptionSetMask) == INTERFACESAFE_FOR_UNTRUSTED_DATA) && (_fSafeForInitializing == true))
                        Rslt = S_OK;
                    break;
                default:
                    Rslt = E_NOINTERFACE;
                    break;
            }

            return Rslt;
        }

        #endregion

        private static string xrxml = "";            //外部参数
        private static string terminalName = "";     //声明工位ID 为公共静态变量
        private int zwsfcjz = 1;                            //指纹是否采集中
        private static int sfzw = 0;                 //采集指纹还是掌纹
        private static int zwzljz = 0;               //NFIQ是否加载
        private static int zwzw = 99;                //指纹指位
        private PictureBox tempLab;                  //临时记录当前采集框
        private string fileName = "";//输出打包文件目录
        private int flag = 1;                        //标记是否正在解析数据（0：解析中 1：解析完成）,默认解析完成
        string cname = "1";//评分方式
        //NFIQ分数标签数组
        static Label[] LableNo = new Label[20];
        //格林比特分数标签数组
        static Label[] LableNo1 = new Label[20];
        //滚指
        public struct m_ROLL
        {
            public PictureBox ID;
            public string Flag;
            public bool temp;
            public string ZWZWDM;                   //指位代码
            public string ZZHWQSQKDM;               //指掌纹缺失情况代码
            public string ZW_TXYSFFMS;              //指纹_图像压缩方法描述
            public string ZW_TXZL;                  //指纹_图像质量
            public byte[] ZW_TXSJ;                  //指纹_图像数据
            public byte[] ZW_TXSJ_WSQ;              //指纹_图像数据WSQ
            public string ZW_TXYSFFMS_WSQ;          //指纹_图像压缩方法描述WSQ
        }
        public static m_ROLL[] rolls = new m_ROLL[10];
        //平指
        public struct m_PLANE
        {
            public PictureBox ID;                   //控件ID
            public string Flag;                     //控件对应编号
            public bool temp;                       //是否更新采集,true更新，false 没有更新
            public string ZWZWDM;                   //指位代码
            public string ZZHWQSQKDM;               //指掌纹缺失情况代码
            public string ZW_TXYSFFMS;              //指纹_图像压缩方法描述
            public string ZW_TXZL;                  //指纹_图像质量
            public byte[] ZW_TXSJ;                  //指纹_图像数据
            public byte[] ZW_TXSJ_WSQ;              //指纹_图像数据WSQ
            public string ZW_TXYSFFMS_WSQ;          //指纹_图像压缩方法描述WSQ
        }
        public static m_PLANE[] plane = new m_PLANE[10];
        //掌纹
        public struct m_PALM
        {
            public PictureBox ID;
            public string Flag;
            public bool temp;
            public string ZHWZHWDM;                 //掌纹掌位代码
            public string ZHW_ZZHWQSQKDM;           //掌纹_指掌纹缺失情况代码
            public string ZHW_TXYSFSMS;             //掌纹_图像压缩方法描述
            public string ZHW_TXZL;                 //掌纹_图像质量
            public byte[] ZHW_TXSJ;                 //掌纹_图像数据
            public byte[] ZHW_TXSJ_WSQ;             //掌纹_图像数据WSQ
            public string ZHW_TXYSFSMS_WSQ;         //掌纹_图像压缩方法描述WSQ
        }
        public static m_PALM[] palms = new m_PALM[4];
        //四指
        public struct m_FOURFINGER
        {
            public PictureBox ID;
            public string Flag;
            public bool temp;
            public string SLZ_ZWZWDM;               //四联指_指纹指位代码
            public string SLZ_ZZHWQSQKDM;           //四联指_指掌纹缺失情况代码
            public string SLZ_TXYSFSMS;             //四联指_图像压缩方法描述
            public string SLZ_TXZL;                 //四联指_图像质量
            public byte[] SLZ_TXSJ;                 //四联指_图像数据
            public byte[] SLZ_TXSJ_WSQ;             //四联指_图像数据WSQ
            public string SLZ_TXYSFSMS_WSQ;         //四联指_图像压缩方法描述WSQ
        }
        public static m_FOURFINGER[] fourfinger = new m_FOURFINGER[3];

        List<NW_GBFINIMG_SEGMENT_IMAGE_DESCRIPTOR> segmentList;

        /***************************************************************/
        #region NFIQ评分的API
        [DllImport(@"NFIQ2DLL.dll", EntryPoint = "Load")]
        public static extern int LoadNFIQ();
        [DllImport(@"NFIQ2DLL.dll", EntryPoint = "getNFIQ2QualityScoreEx2")]
        public static extern int getNFIQ2QualityScoreEx2(byte[] img, int imgsize, int weight, int height);
        #endregion
        #region 声明读写INI文件的API函数
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string defVal, StringBuilder retVal, int size, string filePath);
        #endregion
        /***************************************************************/
        public PLAM_GLBT()
        {
            InitializeComponent();
            //开启指纹上传
            string finger_upload = IniReadValue("LT_PT", "upload", getdburl1() + "\\config.ini");
            if (finger_upload == "0")
            {
                upload.Visible = false;
            }
            plane[0].ID = PLANE_RIGHT_1; plane[1].ID = PLANE_RIGHT_2; plane[2].ID = PLANE_RIGHT_3; plane[3].ID = PLANE_RIGHT_4; plane[4].ID = PLANE_RIGHT_5;
            plane[5].ID = PLANE_LEFT_1; plane[6].ID = PLANE_LEFT_2; plane[7].ID = PLANE_LEFT_3; plane[8].ID = PLANE_LEFT_4; plane[9].ID = PLANE_LEFT_5;
            plane[0].Flag = "11"; plane[1].Flag = "12"; plane[2].Flag = "13"; plane[3].Flag = "14"; plane[4].Flag = "15";
            plane[5].Flag = "16"; plane[6].Flag = "17"; plane[7].Flag = "18"; plane[8].Flag = "19"; plane[9].Flag = "20";
            for (int i = 0; i < 10; i++)
            {
                plane[i].temp = false;
            }
            //获取所有滚指name,flag,temp
            rolls[0].ID = ROLL_RIGHT_1; rolls[1].ID = ROLL_RIGHT_2; rolls[2].ID = ROLL_RIGHT_3; rolls[3].ID = ROLL_RIGHT_4; rolls[4].ID = ROLL_RIGHT_5;
            rolls[5].ID = ROLL_LEFT_1; rolls[6].ID = ROLL_LEFT_2; rolls[7].ID = ROLL_LEFT_3; rolls[8].ID = ROLL_LEFT_4; rolls[9].ID = ROLL_LEFT_5;
            rolls[0].Flag = "1"; rolls[1].Flag = "2"; rolls[2].Flag = "3"; rolls[3].Flag = "4"; rolls[4].Flag = "5";
            rolls[5].Flag = "6"; rolls[6].Flag = "7"; rolls[7].Flag = "8"; rolls[8].Flag = "9"; rolls[9].Flag = "10";
            for (int i = 0; i < 10; i++)
            {
                rolls[i].temp = false;
            }
            //获取所有掌纹name,flag,temp
            palms[0].ID = RIGHT_PALM; palms[1].ID = LEFT_PALM; palms[2].ID = RIGHT_PALMAR; palms[3].ID = LEFT_PALMAR;
            palms[0].Flag = "31"; palms[1].Flag = "32"; palms[2].Flag = "33"; palms[3].Flag = "34";
            for (int i = 0; i < 4; i++)
            {
                palms[i].temp = false;
            }
            //获取四连指name,flag,temp
            fourfinger[0].ID = RIGHT_FOUR; fourfinger[1].ID = LEFT_FOUR; fourfinger[2].ID = DOUBLE_THUMB;
            fourfinger[0].Flag = "21"; fourfinger[1].Flag = "22"; fourfinger[2].Flag = "23";
            for (int i = 0; i < 3; i++)
            {
                fourfinger[i].temp = false;
            }
            //获取所有指节纹name,flag,temp
            //获取所有全掌纹name,flag,temp
            //保存所有显示NFIQ分数lable
            for (int i = 1; i < 21; i++)
            {
                string name = "label0" + i;
                object obj = this.GetType().GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase).GetValue(this);
                LableNo[i - 1] = (Label)obj;
            }
            //保存所有显示格林比特分数lable
            for (int i = 1; i < 21; i++)
            {
                string name1 = "";
                if (i < 10)
                {
                    name1 = "label10" + i;
                }
                else
                {
                    name1 = "label1" + i;
                }
                object obj1 = this.GetType().GetField(name1, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.IgnoreCase).GetValue(this);
                LableNo1[i - 1] = (Label)obj1;
                LableNo1[i - 1].Visible = false;
            }
        }

        ~PLAM_GLBT() 
        {
            //反初始化足迹插件
            form.UnInitEquipment();
        }
        private void PLAM_GLBT_Load(object sender, EventArgs e)
        {
            try
            {
                dataGridView1.Font = new Font("宋体", 9f);
                fileName = this.GetPath() + "//fingerXmlOut.xml";//输出打包文件目录

                string cname = "0";
                if ("0" == cname)
                {
                    zwzljz = NFIQ2DLL.Load();
                }

                if (Convert.ToInt32(IniReadValue("LT_PT", "sfxs", getdburl1() + "\\config.ini")) == 0)
                {
                    this.upload.Visible = false;
                }
            }
            catch (Exception ex)
            {
                Log.WriteInfoLog(ex.ToString());
            }

        }
        public static int Open()
        {
            if (0 != zwzljz)
            {
                return zwzljz;
            }
            else
                zwzljz = LoadNFIQ();
            return zwzljz;
        }
        //点击采集指纹
        private void GetFingerNumber(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            try
            {
                //获取控件名
                PictureBox tempLab = (PictureBox)sender;
                zwzw = getzwdm(tempLab.Name.ToString());
                if (e.Button == MouseButtons.Left)
                {
                    if (zwsfcjz == 1)
                    {
                        zwsfcjz = 0;
                        //tempLab.Image = Image.FromFile(getdburl());
                        tempLab.Image = Image.FromFile(getdburl1() + "cjz.bmp");
                        tempLab.SizeMode = PictureBoxSizeMode.StretchImage;//图片自适应控件大小
                        FigSave(tempLab);
                    }
                }
                else if (e.Button == MouseButtons.Right)
                {
                    LableNo[zwzw - 1].Text = "0";
                    LableNo1[zwzw - 1].Text = "0";
                    //if (System.IO.File.Exists(this.getdburl1() + "//" + zwzw.ToString() + ".bmp"))
                    //{
                    //    System.IO.File.Delete(this.getdburl1() + "//" + zwzw.ToString() + ".bmp");
                    //}
                    //if (System.IO.File.Exists(this.getdburl1() + "//" + zwzw.ToString() + ".wsq"))
                    //{
                    //    System.IO.File.Delete(this.getdburl1() + "//" + zwzw.ToString() + ".wsq");
                    //}
                    //获取右键指纹状态
                    string Fingerstate = "0";
                    //指纹状态选择界面
                    using (FigState frm = new FigState())
                    {
                        if (frm.ShowDialog() == DialogResult.OK)
                        {
                            //回传指纹状态
                            Fingerstate = frm.OutValue;
                        }
                    }
                    if ("0" != Fingerstate)
                    {
                        tempLab.Image = Image.FromFile(getdburl1() + "dz.bmp");
                        tempLab.SizeMode = PictureBoxSizeMode.StretchImage;//图片自适应控件大小
                        getfigbytes(zwzw, 0, null, Fingerstate, "0000");
                    }
                    else
                        return;
                }
            }
            catch (Exception ex)
            {
                Log.WriteInfoLog(ex.ToString());
            }
        }
        //保存指纹数据
        private void FigSave(PictureBox tempLab)
        {
            //跳转采集页面并采集指纹
            //AcquisitionForm DlgToOpen = new AcquisitionForm(tempLab);

            try
            {
                //bool a = GBMSAPI_Example_Globals.DSInit(GBMSAPI_Example_Globals.COINIT_APARTMENTTHREADED, false);
                try
                {
                    if (true)
                    {
                        this.fdsfzw(tempLab);
                        //DlgToOpen.ShowDialog();

                        tempLab.SizeMode = PictureBoxSizeMode.StretchImage;
                        this.tempLab = tempLab;
                        CaptureGlobals.FigGBMS = this;

                        CaptureGlobals.ObjectName = GetObjectName(tempLab);

                        if (CaptureGlobals.ObjectName <= GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_LITTLE)
                        {
                            CaptureGlobals.ClippingRegionSizeX = 640;
                            CaptureGlobals.ClippingRegionSizeY = 640;
                        }
                        else
                        {
                            CaptureGlobals.ClippingRegionSizeX = 0;
                            CaptureGlobals.ClippingRegionSizeY = 0;
                        }

                        Acquisition.CaptureObject(CaptureGlobals.ObjectName);

                    }
                }
                catch (Exception ex)
                {
                    Log.WriteInfoLog(ex.ToString());
                    MessageBox.Show("No scanners found or some error occurred in AcquisitionForm");
                }
            }
            catch (Exception ex)
            {
                Log.WriteInfoLog(ex.ToString());
                MessageBox.Show("Exception in AcquireImageButton_Click: " + ex.Message);
            }
        }
        public void FrameAcquired()
        {
            try
            {

                pictureBox1.Image = CaptureGlobals.PreviewImage;
                pictureBox1.Refresh();
            }
            catch (Exception ex)
            {
                Log.WriteInfoLog(ex.ToString());
            }
        }
        public static void CopyRawImageIntoBitmap(Byte[] RawImage, ref Bitmap bmpImage)
        {
            try
            {
                BitmapData bmData;
                int iw = bmpImage.Width, ih = bmpImage.Height;
                Rectangle bmpRect = new Rectangle(0, 0, iw, ih);

                bmpImage = new Bitmap(iw, ih, PixelFormat.Format8bppIndexed);

                bmData = bmpImage.LockBits(bmpRect,
                        ImageLockMode.WriteOnly,
                        PixelFormat.Format8bppIndexed);

                int diff = bmData.Stride - bmpImage.Width;
                if (diff == 0)
                {
                    Marshal.Copy(RawImage, 0, bmData.Scan0, bmpImage.Width * bmpImage.Height);
                }
                else
                {
                    int RawIndex = 0;
                    int Ptr = (int)bmData.Scan0;
                    for (int i = 0; i < bmpImage.Height; i++, RawIndex += bmpImage.Width)
                    {
                        Marshal.Copy(RawImage, RawIndex, (IntPtr)Ptr, bmpImage.Width);
                        Ptr += bmData.Stride;
                    }
                }

                // Unlock the bits
                bmpImage.UnlockBits(bmData);

                ColorPalette pal = bmpImage.Palette;
                for (int i = 0; i < pal.Entries.Length; i++)
                {
                    pal.Entries[i] = Color.FromArgb(255, i, i, i);
                }
                bmpImage.Palette = pal;
            }
            catch (Exception ex)
            {
                Log.WriteInfoLog(ex.ToString());
                return;
            }
        }
        public void AcquisitionEnded()
        {
            try
            {
                /******************指纹采集状体提示***********************/
                //string cname = IniReadValue("FINGERPRINT", "Tips", System.IO.Directory.GetCurrentDirectory() + "//config.ini");
                string cname = "1";
                if ("1" == cname)
                {
                    List<String> DiagList = GBMSAPI_Example_Util.GetDiagStringsFromDiagMask(
                                        GBMSAPI_Example_Globals.LastDiagnosticValue);
                    if ((DiagList != null) && (DiagList.Count > 0))
                    {
                        DialogResult MsgBoxResult;//设置对话框的返回值
                        string mes = "";
                        for (int i = 0; i < DiagList.Count; i++)
                        {
                            mes += DiagList[i];
                            mes += ",";
                        }
                        //zwsfcjz = 1;
                        MsgBoxResult = MessageBox.Show(mes + "请重新采集",//对话框的显示内容
                                                   "提示",//对话框的标题
                                                    MessageBoxButtons.OK,//定义对话框的按钮，这里定义了YSE和NO两个按钮
                                                    MessageBoxIcon.Exclamation,//定义对话框内的图表式样，这里是一个黄色三角型内加一个感叹号
                                                    MessageBoxDefaultButton.Button1);//定义对话框的按钮式样
                        if (MsgBoxResult == DialogResult.OK)//如果对话框的返回值是YES（按"Y"按钮）
                        {
                            Acquisition.CaptureObject(CaptureGlobals.ObjectName);//重新采集
                            return;
                        }
                    }
                }
                /*********************************************************/
                var imageType =
                    zwzw == 21 ? NW_GBFINIMG_IMAGE_TYPES.NW_GBFINIMG_INPUT_IMAGE_TYPE_RIGHT_HAND_4 :
                    zwzw == 22 ? NW_GBFINIMG_IMAGE_TYPES.NW_GBFINIMG_INPUT_IMAGE_TYPE_LEFT_HAND_4
                    : NW_GBFINIMG_IMAGE_TYPES.NW_GBFINIMG_INPUT_IMAGE_TYPE_THUMBS_2;
                var LastAcqImage = CaptureGlobals.FullResolutionImage;
                var ID = new NW_GBFINIMG_InputData(readPixelsFromBmp(LastAcqImage),
                    (uint)LastAcqImage.Width,
                    (uint)LastAcqImage.Height,
                    imageType,
                    null,
                    NW_GBFINIMG_SEGMENTATION_OPTIONS.NW_GBFINIMG_HALO_LATENT_ELIMINATION,
                    (uint)LastAcqImage.Width,
                    (uint)LastAcqImage.Height);
                var Segmentator = new NW_GBFINIMG_ProcessImage
                {
                    InputToSet = ID
                };

                var SegmentationOutput = Segmentator.OutputAfterProcess;
                byte[] FramePtr = SegmentationOutput.OutputFrame;
                CopyRawImageIntoBitmap(FramePtr, ref LastAcqImage);

                segmentList = SegmentationOutput.Segment_List;

                /*if (segmentList != null)
                {
                    Bitmap bmp = new Bitmap(LastAcqImage.Width, LastAcqImage.Height);

                    Graphics g = Graphics.FromImage(bmp);
                    g.DrawImage(LastAcqImage, 0, 0);

                    Pen dashed = new Pen(Color.Fuchsia, 4)
                    {
                        DashStyle = System.Drawing.Drawing2D.DashStyle.Dash
                    };

                    foreach (var s in segmentList)
                    {
                        g.DrawRectangle(dashed, s.SegBoundingBoxL, s.SegBoundingBoxT, s.SegBoundingBoxR - s.SegBoundingBoxL, s.SegBoundingBoxB - s.SegBoundingBoxT);
                    }

                    g.Dispose();
                    LastAcqImage = bmp;
                }
                else
                {
                    //MessageBox.Show(SegmentationOutput.ToString());
                }*/

                using (var ms = new MemoryStream())
                {
                    LastAcqImage.Save(ms, ImageFormat.Bmp);
                    pictureBox1.Image = LastAcqImage;
                    byte[] lenArr = ms.ToArray(); //bitmap转为byte数组,传入评分数据参数需要
                    byte[] fileBytes1 = new byte[409600];
                    Array.Copy(lenArr, 1078, fileBytes1, 0, 409600);
                    //tempLab.Image = Image.FromStream(new MemoryStream(lenArr));
                    //tempLab.SizeMode = PictureBoxSizeMode.StretchImage;//图片自适应控件大小
                    intosql(fileBytes1, lenArr);
                    zwsfcjz = 1;
                }
                //chengborderred(tempLab, Color.Black);//黑框
                //GetFingerNumber(this.ROLL_RIGHT_2, new MouseEventArgs(MouseButtons.Left, 1, this.ROLL_RIGHT_2.Location.X, this.ROLL_RIGHT_2.Location.Y, 0));

                //Acquisition.CaptureObject(CaptureGlobals.ObjectName);//重新采集
            }
            catch (Exception ex)
            {
                Log.WriteInfoLog(ex.ToString());
            }
            finally
            {

            }
        }
        //保存指纹
        private void intosql(byte[] fileBytes1, byte[] lenArr)
        {
            int zwzl = 0;//指纹质量
            string cname = IniReadValue("LT_PT", "pffs", getdburl1() + "\\config.ini");
            //string cname = "0";
            //byte[] yuantu = lenArr;
            if (zwzw <= 20)
            {
                switch (cname)
                {
                    case "0":
                        /*********************NFIQ**************************/
                        zwzl = NFIQ_Quality(fileBytes1, 640, 640);
                        byte[] NFIQ_wtyuantu = new byte[409600];
                        Array.Copy(lenArr, 1078, NFIQ_wtyuantu, 0, 409600);
                        byte[] NFIQ_xzh = xztp(NFIQ_wtyuantu);//旋转压缩图
                        //byte[] NFIQ_wsqimage = GBMSByteToWsq(NFIQ_xzh, 640, 640);
                        //将指纹数据存本地
                        string Pathwsq = this.getdburl1() + "\\image\\" + zwzw.ToString() + ".wsq";
                        GBMSByteToWsq(NFIQ_xzh, Pathwsq, 640, 640);
                        break;
                    case "1":
                        /*********************新评分wsq**************************/
                        byte[] wtyuantu = new byte[409600];
                        Array.Copy(lenArr, 1078, wtyuantu, 0, 409600);
                        byte[] xzh = xztp(wtyuantu);//旋转压缩图
                        byte[] wsqimage = GBMSByteToWsq(xzh, 640, 640);
                        zwzl = jinzhicheck_New(zwzw, 1, wsqimage, wsqimage.Length, 640, 640);
                        break;
                    case "2":
                        /*********************格林比特**********************/
                        zwzl = Greenbit_Quality(lenArr, 640, 640);
                        break;
                    default: break;
                }
                //MessageBox.Show("格林比特：" + Greenbit_zwzl.ToString() + "，NFIQ：" + NFIQ_zwzl.ToString());
                //  MessageBox.Show(zwzl.ToString());
                if (zwzl == 0 || zwzl == -1 || zwzl == 255)
                {
                    MessageBox.Show("没有采集到指纹数据");
                    return;
                }
                if (chose(zwzl))
                {
                    //string path = this.getdburl1() + "\\image\\" + zwzw.ToString() + ".bmp";
                    tempLab.Image = Image.FromStream(new MemoryStream(lenArr));
                    tempLab.SizeMode = PictureBoxSizeMode.StretchImage;//图片自适应控件大小
                    //tempLab.Image.Save(path);
                    LableNo[zwzw - 1].Text = zwzl.ToString();
                    LableNo1[zwzw - 1].Text = zwzl.ToString();
                    if (zwzl > 60)
                    {
                        LableNo[zwzw - 1].ForeColor = System.Drawing.Color.Green;
                        LableNo1[zwzw - 1].ForeColor = System.Drawing.Color.Green;
                    }
                    else
                    {
                        LableNo[zwzw - 1].ForeColor = System.Drawing.Color.Red;
                        LableNo1[zwzw - 1].ForeColor = System.Drawing.Color.Red;
                    }


                    //将指纹数据存入结构体
                    getfigbytes(zwzw, zwzl, lenArr, "0", "0000");
                    //将指纹wsq数据存入结构体
                    getfigbytes(zwzw, zwzl, GBMSByteToWsq(lenArr, 640, 640), "0", "1419");
                }
                else
                {
                    zwsfcjz = 1;
                    GetFingerNumber(tempLab, new MouseEventArgs(MouseButtons.Left, 1, this.ROLL_RIGHT_2.Location.X, this.ROLL_RIGHT_2.Location.Y, 0));
                }
            }
            else
            {
                if (segmentList != null)
                {
                    try
                    {
                        using (var bitmap = (Bitmap)Image.FromStream(new MemoryStream(lenArr)))
                        {
                            for (int i = 0; i < segmentList.Count(); i++)
                            {
                                var s = segmentList[i];
                                var clip = bitmap.Clone(new Rectangle(s.SegBoundingBoxL, s.SegBoundingBoxT, s.SegBoundingBoxR - s.SegBoundingBoxL, s.SegBoundingBoxB - s.SegBoundingBoxT), PixelFormat.Format8bppIndexed);

                                int w = clip.Height, h = clip.Height;
                                if (w < clip.Width)
                                {
                                    w = clip.Width;
                                    h = clip.Height;
                                }

                                var newBmp = new Bitmap(w, h, PixelFormat.Format24bppRgb);
                                using (var g = Graphics.FromImage(newBmp))
                                {
                                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                    g.FillRectangle(new SolidBrush(Color.White), new Rectangle(0, 0, w, h));
                                    g.DrawImage(clip, new Point(w / 2 - clip.Width / 2, h / 2 - clip.Height / 2));
                                }

                                clip.Dispose();

                                var res = new Bitmap(640, 640, PixelFormat.Format24bppRgb);
                                using (var g = Graphics.FromImage(res))
                                {
                                    g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                    g.DrawImage(newBmp, new Rectangle(0, 0, res.Width, res.Height), 0, 0, w, h, GraphicsUnit.Pixel);
                                }

                                var gray = res.ToGrayScale();

                                res.Dispose();

                                byte[] lenArr2 = gray.ToByteArray(ImageFormat.Bmp); //bitmap转为byte数组,传入评分数据参数需要

                                gray.Dispose();

                                byte[] fileBytes2 = new byte[409600];
                                Array.Copy(lenArr2, 1078, fileBytes2, 0, 409600);

                                SaveSegment((int)s.Finger + 15 - (((i == 1 && s.Finger == 1u) || zwzw == 21) ? 5 : 0), fileBytes2, lenArr2);

                                segmentList[i] = null;
                            }
                        }
                        segmentList = null;
                    }
                    catch (Exception ex)
                    {
                        Log.WriteInfoLog(ex.ToString());
                        MessageBox.Show("切割失败");
                    }
                }
                else if (zwzw < 24)
                {
                    MessageBox.Show("无法切割指纹，请单独采集平面指纹");
                }

                /*********************格林比特**********************/
                //zwzl = Greenbit_Quality(lenArr, 2304, 2304);
                /*********************NFIQ**************************/
                //if ("0" == cname)
                //{
                //    zwzl = NFIQ_Quality(fileBytes1, 2304, 2304);
                //    //  MessageBox.Show(zwzl.ToString());
                //    if (zwzl == 0 || zwzl == -1 || zwzl == 255)
                //    {
                //        MessageBox.Show("没有采集到指纹数据");
                //        return;
                //    }
                //}
                //string path = this.getdburl1() + "//" + zwzw.ToString() + ".bmp";
                zwzl = 30;
                tempLab.Image = Image.FromStream(new MemoryStream(lenArr));
                tempLab.SizeMode = PictureBoxSizeMode.StretchImage;//图片自适应控件大小
                //tempLab.Image.Save(path);
                //byte[] yuantu = imagetobyte(path);
                //string Pathwsq = this.getdburl1() + "//" + zwzw.ToString() + ".wsq";
                //GBMSByteToWsq(yuantu, Pathwsq, 2304, 2304);
                //将指纹数据存入结构体
                getfigbytes(zwzw, zwzl, lenArr, "0", "0000");
                //将指纹wsq数据存入结构体
                getfigbytes(zwzw, zwzl, GBMSByteToWsq(lenArr, 2304, 2304), "0", "1419");
            }

        }

        private void SaveSegment(int zwzw, byte[] fileBytes1, byte[] lenArr)
        {
            var zwzl = NFIQ_Quality(fileBytes1, 640, 640);
            File.WriteAllBytes(GetPath() + "/image/" + zwzw + ".bmp", lenArr);
            getfigbytes(zwzw, zwzl, lenArr, "0", "0000");
            //将指纹wsq数据存入结构体
            getfigbytes(zwzw, zwzl, GBMSByteToWsq(lenArr, 640, 640), "0", "1419");

            var name = GetImageName(zwzw);
            var image = tabControl1.Controls.Find(name, true).FirstOrDefault();
            if (image != null && image is PictureBox box)
            {
                box.Image = Image.FromStream(new MemoryStream(lenArr));
                box.SizeMode = PictureBoxSizeMode.StretchImage;

                LableNo[zwzw - 1].Text = zwzl.ToString();
                LableNo1[zwzw - 1].Text = zwzl.ToString();

                if (zwzl > 60)
                {
                    LableNo[zwzw - 1].ForeColor = Color.Green;
                    LableNo1[zwzw - 1].ForeColor = Color.Green;
                }
                else
                {
                    LableNo[zwzw - 1].ForeColor = Color.Red;
                    LableNo1[zwzw - 1].ForeColor = Color.Red;
                }
            }
        }

        ///// <summary>
        ///// 获取指纹数据并打包
        ///// </summary>
        ///// <param name="num">指位</param>
        ///// <param name="qu">质量</param>
        ///// <param name="filebytes">图像数据</param>
        ///// <param name="defnum">缺失代码</param>
        ///// <param name="wsqnum">压缩代码</param>
        //private void getfigbytes(int num, int qu, byte[]filebytes, string defnum, string wsqnum) 
        //{
        //    //滚动
        //    if (num<=10) 
        //    {
        //        rolls[num - 1].ZWZWDM = zwzw.ToString();
        //        rolls[num - 1].ZW_TXZL = qu.ToString();
        //        rolls[num - 1].ZW_TXSJ = filebytes;
        //        rolls[num - 1].ZZHWQSQKDM = defnum;
        //        rolls[num - 1].ZW_TXYSFFMS = wsqnum;
        //    }//平面
        //    else if (num > 10 && num <= 20)
        //    {
        //        plane[num - 11].ZWZWDM = zwzw.ToString();
        //        plane[num - 11].ZW_TXZL = qu.ToString();
        //        plane[num - 11].ZW_TXSJ = filebytes;
        //        plane[num - 11].ZZHWQSQKDM = defnum;
        //        plane[num - 11].ZW_TXYSFFMS = wsqnum;
        //    }//四连指
        //    else if (num > 20 && num <= 24) 
        //    {
        //        fourfinger[num - 21].SLZ_ZWZWDM = zwzw.ToString();
        //        fourfinger[num - 21].SLZ_TXZL = qu.ToString();
        //        fourfinger[num - 21].SLZ_TXSJ = filebytes;
        //        fourfinger[num - 21].SLZ_ZZHWQSQKDM = defnum;
        //        fourfinger[num - 21].SLZ_TXYSFSMS = wsqnum;
        //    }//掌纹
        //    else if (num > 30 && num <= 34)
        //    {
        //        palms[num - 31].ZHWZHWDM = zwzw.ToString();
        //        palms[num - 31].ZHW_TXZL = qu.ToString();
        //        palms[num - 31].ZHW_TXSJ = filebytes;
        //        palms[num - 31].ZHW_ZZHWQSQKDM = defnum;
        //        palms[num - 31].ZHW_TXYSFSMS = wsqnum;
        //    }
        //}
        /// 获取指纹数据并打包
        /// </summary>
        /// <param name="num">指位</param>
        /// <param name="qu">质量</param>
        /// <param name="filebytes">图像数据</param>
        /// <param name="defnum">缺失代码</param>
        /// <param name="wsqnum">压缩代码</param>
        private void getfigbytes(int num, int qu, byte[] filebytes, string defnum, string wsqnum)
        {
            //滚动
            if (num <= 10)
            {
                rolls[num - 1].ZWZWDM = zwzw.ToString();
                rolls[num - 1].ZW_TXZL = qu.ToString();
                rolls[num - 1].ZZHWQSQKDM = defnum;
                if ("0000" == wsqnum)
                {
                    rolls[num - 1].ZW_TXYSFFMS = wsqnum;
                    rolls[num - 1].ZW_TXSJ = filebytes;
                }
                else if ("1419" == wsqnum)
                {
                    rolls[num - 1].ZW_TXYSFFMS_WSQ = wsqnum;
                    rolls[num - 1].ZW_TXSJ_WSQ = filebytes;
                }

            }//平面
            else if (num > 10 && num <= 20)
            {
                plane[num - 11].ZWZWDM = zwzw.ToString();
                plane[num - 11].ZW_TXZL = qu.ToString();
                plane[num - 11].ZZHWQSQKDM = defnum;
                if ("0000" == wsqnum)
                {
                    plane[num - 11].ZW_TXYSFFMS = wsqnum;
                    plane[num - 11].ZW_TXSJ = filebytes;

                }
                else if ("1419" == wsqnum)
                {
                    plane[num - 11].ZW_TXYSFFMS_WSQ = wsqnum;
                    plane[num - 11].ZW_TXSJ_WSQ = filebytes;
                }

            }//四连指
            else if (num > 20 && num <= 24)
            {
                fourfinger[num - 21].SLZ_ZWZWDM = zwzw.ToString();
                fourfinger[num - 21].SLZ_TXZL = qu.ToString();
                fourfinger[num - 21].SLZ_ZZHWQSQKDM = defnum;
                if ("0000" == wsqnum)
                {
                    fourfinger[num - 21].SLZ_TXSJ = filebytes;
                    fourfinger[num - 21].SLZ_TXYSFSMS = wsqnum;

                }
                else if ("1419" == wsqnum)
                {
                    fourfinger[num - 21].SLZ_TXSJ_WSQ = filebytes;
                    fourfinger[num - 21].SLZ_TXYSFSMS_WSQ = wsqnum;
                }

            }//掌纹
            else if (num > 30 && num <= 34)
            {
                palms[num - 31].ZHWZHWDM = zwzw.ToString();
                palms[num - 31].ZHW_TXZL = qu.ToString();
                palms[num - 31].ZHW_ZZHWQSQKDM = defnum;
                if ("0000" == wsqnum)
                {
                    palms[num - 31].ZHW_TXSJ = filebytes;
                    palms[num - 31].ZHW_TXYSFSMS = wsqnum;

                }
                else if ("1419" == wsqnum)
                {
                    palms[num - 31].ZHW_TXSJ_WSQ = filebytes;
                    palms[num - 31].ZHW_TXYSFSMS_WSQ = wsqnum;
                }

            }
        }
        /// <summary>
        /// 获取这个动态链接库的位置
        /// </summary>
        /// <returns></returns>
        public string GetPath()
        {
            string str = Assembly.GetExecutingAssembly().CodeBase;
            int start = 8;// 去除file:///
            int end = str.LastIndexOf('/');// 去除文件名xxx.dll及文件名前的/
            str = str.Substring(start, end - start);
            return str;
        }
        /******************************指纹数据评分*********************************/
        //nfiq质量判断 
        private int NFIQ_Quality(byte[] images, int width, int height)
        {
            if (zwzljz < 0)
                MessageBox.Show("检测指纹质量失败", "提示");

            int quality = getNFIQ2QualityScoreEx2(images, images.Length, width, height);

            return quality;
            //return 0;
        }
        //新评分wsq
        private int jinzhicheck_New(int ZWZWDM, int ZW_TXYSFFMS, byte[] ZW_TXSJ, int ZW_TXSJ_LEN, int ZW_TX_WIDTH, int ZW_TX_HEIGHT)
        {
            int getback = 0;
            try
            {
                int pnRpCoreQlev = 0;
                int pnImgQlev = 0;
                getback = gfsqualitycheck_New.S_FingerQualityCheck(ZWZWDM, ZW_TXYSFFMS, ZW_TXSJ, ZW_TXSJ_LEN, ZW_TX_WIDTH, ZW_TX_HEIGHT, ref pnImgQlev, ref pnRpCoreQlev);
                //随机减分
                int seed = Guid.NewGuid().GetHashCode();//随机种子
                Random rd = new Random(seed);
                int cou = rd.Next(1, 3);
                if (pnRpCoreQlev > 3)
                    pnRpCoreQlev = pnRpCoreQlev - cou;          //特征质量减去随机分数
                else
                    pnRpCoreQlev = pnRpCoreQlev + 2;            //特征质量
                if (pnImgQlev > 3)
                    pnImgQlev = pnImgQlev - cou;                //图像质量减去随机分数
                else
                    pnImgQlev = pnImgQlev + 2;                  //特征质量
                //根据指位返回分数
                if (ZWZWDM < 11)
                    return pnRpCoreQlev;
                else
                    return pnImgQlev;
            }
            catch (Exception ex)
            {
                Log.WriteInfoLog(ex.ToString());
                MessageBox.Show("错误代码" + getback + "\n\r误信息" + ex.ToString());
                return 0;
            }
            return 0;
        }
        //选择指位评分方式(格林比特)
        uint ObjTypesCombo(int fingernumber)
        {
            uint RetVal = 0x00000001;
            switch (fingernumber)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                    RetVal = NW_GBFINIMG_IMAGE_TYPES.NW_GBFINIMG_INPUT_IMAGE_TYPE_ROLLED_TIP;
                    //RetVal = NW_GBFINIMG_IMAGE_TYPES.NW_GBFINIMG_INPUT_IMAGE_TYPE_SINGLE_FINGER_FLAT;
                    break;
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                case 17:
                case 18:
                case 19:
                case 20:
                    RetVal = NW_GBFINIMG_IMAGE_TYPES.NW_GBFINIMG_INPUT_IMAGE_TYPE_SINGLE_FINGER_FLAT;
                    break;
                case 21:
                case 22:
                case 23:
                case 24:
                    RetVal = NW_GBFINIMG_IMAGE_TYPES.NW_GBFINIMG_INPUT_IMAGE_TYPE_ROLLED_JOINT_FINGER_FV1;
                    break;
                case 31:
                    RetVal = NW_GBFINIMG_IMAGE_TYPES.NW_GBFINIMG_INPUT_IMAGE_TYPE_RIGHT_HAND_WRITER_PALM;
                    break;
                case 32:
                    RetVal = NW_GBFINIMG_IMAGE_TYPES.NW_GBFINIMG_INPUT_IMAGE_TYPE_LEFT_HAND_WRITER_PALM;
                    break;
                case 33:
                    RetVal = NW_GBFINIMG_IMAGE_TYPES.NW_GBFINIMG_INPUT_IMAGE_TYPE_RIGHT_HAND_HALF_PALM;
                    break;
                case 34:
                    RetVal = NW_GBFINIMG_IMAGE_TYPES.NW_GBFINIMG_INPUT_IMAGE_TYPE_LEFT_HAND_HALF_PALM;
                    break;
            }

            return RetVal;
        }
        uint GetProcessOptions(int num)
        {
            uint RetVal = 0;
            switch (num)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                    RetVal |= NW_GBFINIMG_SEGMENTATION_OPTIONS.NW_GBFINIMG_SEGMENT_VERTICAL_ROTATION;
                    break;
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                case 16:
                case 17:
                case 18:
                case 19:
                case 20:
                    RetVal |= NW_GBFINIMG_SEGMENTATION_OPTIONS.NW_GBFINIMG_SEGMENT_VERTICAL_ROTATION;
                    break;
                case 31:
                case 32:
                case 33:
                case 34:
                    RetVal |= NW_GBFINIMG_SEGMENTATION_OPTIONS.NW_GBFINIMG_PALM_PRINT_IMAGE_QUALITY_CALCULATION;
                    break;

            }
            return RetVal;
        }
        //格林比特质量判断
        private int Greenbit_Quality(byte[] lenArr, uint width, uint height)
        {
            int zwzl = 0;
            uint ObjToDetect = this.ObjTypesCombo(zwzw);//获取指位
            uint Process = this.GetProcessOptions(zwzw);//图像类型(掌纹，平面指纹，滚动指纹)
            NW_GBFINIMG_InputData ID = new NW_GBFINIMG_InputData(lenArr, width, height, ObjToDetect, null, Process, width, height);
            NW_GBFINIMG_ProcessImage PI = new NW_GBFINIMG_ProcessImage(ID);
            NW_GBFINIMG_OutputData OD = PI.OutputAfterProcess;
            if (null == OD.Segment_List)
            {
                OD = PI.OutputAfterFastProcess;
            }
            if (null == OD.Segment_List)
            {
                //MessageBox.Show("评分失败，请重新采集");
                //zwsfcjz = 0;
                return 0;
            }
            for (int i = 0; i < OD.Segment_List.Count; i++)
            {
                zwzl = (int)OD.Segment_List[0].SegQuality;
                //MessageBox.Show(zwzl.ToString());
            }
            return zwzl;
        }
        /******************************指纹数据评分*********************************/
        //选择弹框
        private bool chose(int qu)
        {
            if (qu < 50)
            {

                //DialogResult MsgBoxResult;//设置对话框的返回值
                //MsgBoxResult = MessageBox.Show("当前指纹质量为" + qu.ToString() + ",是否重新采集?",//对话框的显示内容
                //                               "提示",//对话框的标题
                //                                MessageBoxButtons.YesNo,//定义对话框的按钮，这里定义了YSE和NO两个按钮
                //                                MessageBoxIcon.Exclamation,//定义对话框内的图表式样，这里是一个黄色三角型内加一个感叹号
                //                                MessageBoxDefaultButton.Button2);//定义对话框的按钮式样

                //if (MsgBoxResult == DialogResult.Yes)//如果对话框的返回值是YES（按"Y"按钮）
                //    return false;
                //if (MsgBoxResult == DialogResult.No)//如果对话框的返回值是NO（按"N"按钮）
                //    return true;
                messageboxshow ff = new messageboxshow("提示", "当前指纹质量为" + qu.ToString() + ",是否重新采集?", "重新采集", "不再采集");
                ff.ShowDialog();
                if (ff.res == 0)
                { return false; }
                else
                { return true; }
            }
            else
            {
                return true;

            }
        }
        private uint GetObjectName(PictureBox tempLab)
        {
            switch (tempLab.Name)
            {
                case "PLANE_RIGHT_1":
                    return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_RIGHT_THUMB;
                case "PLANE_RIGHT_2":
                    return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_RIGHT_INDEX;
                case "PLANE_RIGHT_3":
                    return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_RIGHT_MIDDLE;
                case "PLANE_RIGHT_4":
                    ;
                    return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_RIGHT_RING;
                case "PLANE_RIGHT_5":
                    return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_RIGHT_LITTLE;

                case "PLANE_LEFT_1":
                    return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_LEFT_THUMB;
                case "PLANE_LEFT_2":
                    return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_LEFT_INDEX;
                case "PLANE_LEFT_3":
                    return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_LEFT_MIDDLE;
                case "PLANE_LEFT_4":
                    return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_LEFT_RING;
                case "PLANE_LEFT_5":
                    return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_LEFT_LITTLE;

                case "RIGHT_FOUR":
                    return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_RIGHT;
                case "LEFT_FOUR":
                    return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_4_LEFT;
                case "RIGHT_PALM":
                    return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_LOWER_HALF_PALM_RIGHT;
                case "LEFT_PALM":
                    return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_LOWER_HALF_PALM_LEFT;

                case "RIGHT_PALMAR":
                    return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_WRITER_PALM_RIGHT;
                case "LEFT_PALMAR":
                    return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_WRITER_PALM_RIGHT;
                case "RIGHT_THUMB":
                    return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_WRITER_PALM_LEFT;
                case "LEFT_THUMB":
                    return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_LEFT_THUMB;


                case "ROLL_RIGHT_1":
                    return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_THUMB;
                case "ROLL_RIGHT_2":
                    return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_INDEX;
                case "ROLL_RIGHT_3":
                    return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_MIDDLE;
                case "ROLL_RIGHT_4":
                    return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_RING;
                case "ROLL_RIGHT_5":
                    return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_LITTLE;

                case "ROLL_LEFT_1":
                    return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_THUMB;
                case "ROLL_LEFT_2":
                    return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_INDEX;
                case "ROLL_LEFT_3":
                    return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_MIDDLE;
                case "ROLL_LEFT_4":
                    return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_RING;
                case "ROLL_LEFT_5":
                    return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_LITTLE;
                case "DOUBLE_THUMB":
                    return GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_SLAP_2_THUMBS;
            }
            return 0;
        }
        //获取点击按钮类型
        public void fdsfzw(PictureBox tempLab)
        {
            for (int i = 0; i < 10; i++)
            {
                if (plane[i].ID.Name.ToString() == tempLab.Name)//点击控件的名字与指位匹配
                {
                    sfzw = 0;
                }
            }
            for (int i = 0; i < 10; i++)
            {
                if (rolls[i].ID.ToString() == tempLab.Name)//点击控件的名字与指位匹配
                { sfzw = 0; }
            }
            for (int i = 0; i < 4; i++)
            {
                if (palms[i].ID.Name.ToString() == tempLab.Name)
                { sfzw = 1; }
            }
            for (int i = 0; i < 3; i++)
            {
                if (fourfinger[i].ID.Name.ToString() == tempLab.Name)
                { sfzw = 1; }
            }
        }
        private byte[] readPixelsFromBmp(Bitmap bmp)
        {
            if (bmp.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                // Lock the bitmap's bits.
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                BitmapData bmpData =
                    bmp.LockBits(rect, ImageLockMode.ReadWrite,
                    bmp.PixelFormat);

                // Get the address of the first line.
                IntPtr ptr = bmpData.Scan0;

                // Declare an array to hold the bytes of the bitmap.
                int bytes = bmpData.Stride * bmp.Height;
                byte[] rgbValues = new byte[bytes];

                // Copy the RGB values into the array.
                Marshal.Copy(ptr, rgbValues, 0, bytes);

                // Unlock the bits.
                bmp.UnlockBits(bmpData);
                return rgbValues;
            }
            return null;
        }

        public string getdburl1()
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
        private int getzwdm(string names)
        {
            switch (names)
            {
                case "PLANE_RIGHT_1":
                    return 11;
                case "PLANE_RIGHT_2":
                    return 12;
                case "PLANE_RIGHT_3":
                    return 13;
                case "PLANE_RIGHT_4":
                    return 14;
                case "PLANE_RIGHT_5":
                    return 15;

                case "PLANE_LEFT_1":
                    return 16;
                case "PLANE_LEFT_2":
                    return 17;
                case "PLANE_LEFT_3":
                    return 18;
                case "PLANE_LEFT_4":
                    return 19;
                case "PLANE_LEFT_5":
                    return 20;
                case "ROLL_RIGHT_1":
                    return 1;
                case "ROLL_RIGHT_2":
                    return 2;
                case "ROLL_RIGHT_3":
                    return 3;
                case "ROLL_RIGHT_4":
                    return 4;
                case "ROLL_RIGHT_5":
                    return 5;

                case "ROLL_LEFT_1":
                    return 6;
                case "ROLL_LEFT_2":
                    return 7;
                case "ROLL_LEFT_3":
                    return 8;
                case "ROLL_LEFT_4":
                    return 9;
                case "ROLL_LEFT_5":
                    return 10;

                case "RIGHT_PALM":
                    return 31;
                case "LEFT_PALM":
                    return 32;
                case "RIGHT_PALMAR":
                    return 33;
                case "LEFT_PALMAR":
                    return 34;
                case "RIGHT_FOUR":
                    return 21;
                case "LEFT_FOUR":
                    return 22;
                case "DOUBLE_THUMB":
                    return 23;
            }
            return 99;
        }
        private string GetImageName(int zwzw)
        {
            switch (zwzw)
            {
                case 11:
                    return "PLANE_RIGHT_1";
                case 12:
                    return "PLANE_RIGHT_2";
                case 13:
                    return "PLANE_RIGHT_3";
                case 14:
                    return "PLANE_RIGHT_4";
                case 15:
                    return "PLANE_RIGHT_5";
                case 16:
                    return "PLANE_LEFT_1";
                case 17:
                    return "PLANE_LEFT_2";
                case 18:
                    return "PLANE_LEFT_3";
                case 19:
                    return "PLANE_LEFT_4";
                case 20:
                    return "PLANE_LEFT_5";
                case 1:
                    return "ROLL_RIGHT_1";
                case 2:
                    return "ROLL_RIGHT_2";
                case 3:
                    return "ROLL_RIGHT_3";
                case 4:
                    return "ROLL_RIGHT_4";
                case 5:
                    return "ROLL_RIGHT_5";
                case 6:
                    return "ROLL_LEFT_1";
                case 7:
                    return "ROLL_LEFT_2";
                case 8:
                    return "ROLL_LEFT_3";
                case 9:
                    return "ROLL_LEFT_4";
                case 10:
                    return "ROLL_LEFT_5";
            }
            return "";
        }
        private string GetImageName(uint objId)
        {
            switch (objId)
            {
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_RIGHT_THUMB:
                    return "PLANE_RIGHT_1";
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_RIGHT_INDEX:
                    return "PLANE_RIGHT_2";
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_RIGHT_MIDDLE:
                    return "PLANE_RIGHT_3";
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_RIGHT_RING:
                    return "PLANE_RIGHT_4";
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_RIGHT_LITTLE:
                    return "PLANE_RIGHT_5";
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_LEFT_THUMB:
                    return "PLANE_LEFT_1";
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_LEFT_INDEX:
                    return "PLANE_LEFT_2";
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_LEFT_MIDDLE:
                    return "PLANE_LEFT_3";
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_LEFT_RING:
                    return "PLANE_LEFT_4";
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_FLAT_LEFT_LITTLE:
                    return "PLANE_LEFT_5";
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_THUMB:
                    return "ROLL_RIGHT_1";
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_INDEX:
                    return "ROLL_RIGHT_2";
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_MIDDLE:
                    return "ROLL_RIGHT_3";
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_RING:
                    return "ROLL_RIGHT_4";
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_RIGHT_LITTLE:
                    return "ROLL_RIGHT_5";
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_THUMB:
                    return "ROLL_LEFT_1";
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_INDEX:
                    return "ROLL_LEFT_2";
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_MIDDLE:
                    return "ROLL_LEFT_3";
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_RING:
                    return "ROLL_LEFT_4";
                case GBMSAPI_NET_ScannableObjects.GBMSAPI_NET_SO_ROLL_LEFT_LITTLE:
                    return "ROLL_LEFT_5";
            }
            return "";
        }
        private int gtzw(string pname)
        {
            switch (tempLab.Name)
            {
                case "PLANE_RIGHT_1":
                    return 1;
                case "PLANE_RIGHT_2":
                    return 2;
                case "PLANE_RIGHT_3":
                    return 3;
                case "PLANE_RIGHT_4":
                    ;
                    return 4;
                case "PLANE_RIGHT_5":
                    return 5;

                case "PLANE_LEFT_1":
                    return 6;
                case "PLANE_LEFT_2":
                    return 7;
                case "PLANE_LEFT_3":
                    return 8;
                case "PLANE_LEFT_4":
                    return 9;
                case "PLANE_LEFT_5":
                    return 10;
                case "ROLL_RIGHT_1":
                    return 1;
                case "ROLL_RIGHT_2":
                    return 2;
                case "ROLL_RIGHT_3":
                    return 3;
                case "ROLL_RIGHT_4":
                    return 4;
                case "ROLL_RIGHT_5":
                    return 5;

                case "ROLL_LEFT_1":
                    return 6;
                case "ROLL_LEFT_2":
                    return 7;
                case "ROLL_LEFT_3":
                    return 8;
                case "ROLL_LEFT_4":
                    return 9;
                case "ROLL_LEFT_5":
                    return 10;
            }

            return 1;



        }
        //旋转图片
        public byte[] xztp(byte[] bitmap1)
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
        //image转换为byte[]
        private byte[] imagetobyte(string path)
        {

            Bitmap bp = new Bitmap(path);

            MemoryStream ms1 = new MemoryStream();

            bp.Save(ms1, System.Drawing.Imaging.ImageFormat.Bmp);
            //MessageBox.Show(ms1.Length.ToString());
            byte[] lenArr = new byte[ms1.Length];
            ms1.Position = 0;
            ms1.Read(lenArr, 0, (int)ms1.Length);
            ms1.Close();
            bp.Dispose();
            return lenArr;

        }
        //采集过程中不能切换
        private void tabControl1_Deselecting(object sender, TabControlCancelEventArgs e)
        {
            if (0 == zwsfcjz)
                e.Cancel = true;
            else
                e.Cancel = false;
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
                Log.WriteInfoLog(ex.ToString());
                MessageBox.Show("压缩失败:" + ex.ToString());
            }
            return encoded;
        }
        //格林比特指纹压缩,并保存
        public void GBMSByteToWsq(byte[] RawBuf, string Path, int nWidth, int nHeight)
        {
            byte[] encoded = null;
            try
            {
                WSQPACK_NET_WRAPPER.NW_WSQPACK_Compress WSQC = new WSQPACK_NET_WRAPPER.NW_WSQPACK_Compress(RawBuf, nWidth, nHeight);
                WSQC.CompressionRate = 0.75f;
                encoded = WSQC.Encoded;
                System.IO.File.WriteAllBytes(Path, encoded);
                //this.Close();
            }
            catch (System.Exception ex)
            {
                Log.WriteInfoLog(ex.ToString());
                MessageBox.Show("压缩失败:" + ex.ToString());
            }

        }
        //格林比特指纹解压缩,并保存
        public byte[] GBMSWsqToByte(string wsqpath)
        {

            byte[] wsqBuffer = System.IO.File.ReadAllBytes(wsqpath);
            WSQPACK_NET_WRAPPER.NW_WSQPACK_Uncompress WSQU = new WSQPACK_NET_WRAPPER.NW_WSQPACK_Uncompress(wsqBuffer);
            byte[] decoded = WSQU.ImageDecoded.RawImage;
            return decoded;


        }
        public void strat(string srxml)
        {
            xrxml = srxml;

        }
        public string getbackdata()
        {

            //sdyuantuFPT5 ftpx = new sdyuantuFPT5(xrxml);
            //return ftpx.packFPT();

            return "";
        }

        /// <summary>
        /// 读取INI文件
        /// </summary>
        /// <param name="section">段落</param>
        /// <param name="key">键</param>
        /// <returns>返回的键值</returns>
        public string IniReadValue(string section, string key, string filePath)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(section, key, "", temp, 255, filePath);
            return temp.ToString();
        }


        delegate void SetTextDelegate(Control Ctrl, string Text);
        /// <summary>
        /// 跨线程设置控件Text
        /// </summary>
        /// <param name="Ctrl">待设置的控件</param>
        /// <param name="Text">Text</param>
        public static void SetText(Control Ctrl, string Text)
        {
            if (Ctrl.InvokeRequired == true)
            {
                Ctrl.Invoke(new SetTextDelegate(SetText), Ctrl, Text);
                if ("" != Text)
                {
                    if (int.Parse(Text) > 60)
                    {
                        Ctrl.ForeColor = Color.Green;
                    }
                    else
                    {
                        Ctrl.ForeColor = Color.Red;
                    }
                }

            }
            else
            {
                Ctrl.Text = Text;
            }
        }


        //******************************接口***********************************//

        /// <summary>
        /// 初始化设备
        /// </summary>
        /// <returns>结果xml</returns>
        public string initFingerPlamOCX(string fingerPlamOCXInformation)
        {
            try
            {
                string fingerPlamOCXVersions = "";
                string flag = "ERROR";
                string message = "初始化失败";
                //getInformation(fingerPlamOCXInformation);
                if (1 == PLAM_GLBT.Open() && 1 == getInformation(fingerPlamOCXInformation))
                {
                    flag = "SUCCESS";
                    message = "成功";
                }
                fingerPlamOCXVersions += "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
                fingerPlamOCXVersions += "<root>";
                fingerPlamOCXVersions += "<head>";
                fingerPlamOCXVersions += "<flag>" + flag + "</flag>";
                fingerPlamOCXVersions += "<message>" + message + "</message>";
                fingerPlamOCXVersions += "</head>";
                fingerPlamOCXVersions += "<DATA>";
                fingerPlamOCXVersions += "<MANUFACTURERCODE>" + 0 + "</MANUFACTURERCODE>";
                fingerPlamOCXVersions += "<OCXVERSIONS>" + 0 + "</OCXVERSIONS>";
                fingerPlamOCXVersions += "</DATA>";
                fingerPlamOCXVersions += "</root>";

                //保存到本地
                string jcxx_path = getdburl1() + "//jcxx.xml";
                System.IO.File.WriteAllText(jcxx_path, fingerPlamOCXInformation, Encoding.UTF8);
                return fingerPlamOCXVersions;
            }
            catch (Exception ex)
            {
                Log.WriteInfoLog(ex.ToString());
                throw;
            }

        }
        /// <summary>
        /// 获取打包FPT需要信息
        /// </summary>
        private int getInformation(string fingerPlamOCXInformation)
        {
            try
            {
                int i = 0;
                if ("" == fingerPlamOCXInformation)
                {
                    return 0;
                }
                else
                {
                    inserttext(fingerPlamOCXInformation, this.GetPath() + "\\FPT\\" + InformationData.ysxt_asjxgrybh + ".xml");
                    XmlDocument xx = new XmlDocument();
                    xx.LoadXml(fingerPlamOCXInformation);
                    XmlNode FirstNode = xx.SelectSingleNode("root");
                    XmlNode Node1 = FirstNode.FirstChild;
                    XmlElement xe = null;
                    foreach (XmlNode xxNode in Node1)
                    {
                        xe = (XmlElement)xxNode;
                        switch (xe.Name)
                        {
                            case "fsdw_gajgjgdm":
                                InformationData.fsdw_gajgjgdm = xxNode.InnerText;
                                break;
                            case "fsdw_gajgmc":
                                InformationData.fsdw_gajgmc = xxNode.InnerText;
                                break;
                            case "fsr_xm":
                                InformationData.fsr_xm = xxNode.InnerText;
                                break;
                            case "fsr_gmsfhm":
                                InformationData.fsr_gmsfhm = xxNode.InnerText;
                                break;
                            case "fsr_lxdh":
                                InformationData.fsr_lxdh = xxNode.InnerText;
                                break;
                            case "ysxt_asjxgrybh":
                                InformationData.ysxt_asjxgrybh = xxNode.InnerText;
                                break;
                            case "cjxxyydm":
                                InformationData.cjxxyydm = xxNode.InnerText;
                                break;
                            case "xm":
                                InformationData.xm = xxNode.InnerText;
                                break;
                            case "xbdm":
                                InformationData.xbdm = xxNode.InnerText;
                                break;
                            case "csrq":
                                InformationData.csrq = xxNode.InnerText;
                                //MessageBox.Show(xxNode.InnerText);
                                //MessageBox.Show(InformationData.csrq);
                                break;
                            case "gjdm":
                                InformationData.gjdm = xxNode.InnerText;
                                break;
                            case "mzdm":
                                InformationData.mzdm = xxNode.InnerText;
                                break;
                            case "cyzjdm":
                                InformationData.cyzjdm = xxNode.InnerText;
                                break;
                            case "zjhm":
                                InformationData.zjhm = xxNode.InnerText;
                                break;
                            case "hjdz_xzqhdm":
                                InformationData.hjdz_xzqhdm = xxNode.InnerText;
                                break;
                            case "hjdz_dzmc":
                                InformationData.hjdz_dzmc = xxNode.InnerText;
                                break;
                            case "xzz_xzqhdm":
                                InformationData.xzz_xzqhdm = xxNode.InnerText;
                                break;
                            case "xzz_dzmc":
                                InformationData.xzz_dzmc = xxNode.InnerText;
                                break;
                            case "nydw_gajgjgdm":
                                InformationData.nydw_gajgjgdm = xxNode.InnerText;
                                break;
                            case "nydw_gajgmc":
                                InformationData.nydw_gajgmc = xxNode.InnerText;
                                break;
                            case "nyry_xm":
                                InformationData.nyry_xm = xxNode.InnerText;
                                break;
                            case "nyry_gmsfhm":
                                InformationData.nyry_gmsfhm = xxNode.InnerText;
                                break;
                            case "nyry_lxdh":
                                InformationData.nyry_lxdh = xxNode.InnerText;
                                break;
                            case "faceImage":
                                if (i < 3)
                                {
                                    XmlNode Node3 = xxNode;
                                    //for (int i = 0; i < 3; i++)
                                    {
                                        foreach (XmlNode xxNode1 in Node3)
                                        {
                                            xe = (XmlElement)xxNode1;
                                            switch (xe.Name)
                                            {
                                                case "rxzplxdm":
                                                    InformationData.faceImage[i].rxzplxdm = xxNode1.InnerText;
                                                    break;
                                                case "rx_txsj":
                                                    if (xxNode1.InnerText.Length > 10)
                                                    {
                                                        InformationData.faceImage[i].rx_txsj = Convert.FromBase64String(xxNode1.InnerText);
                                                    }
                                                    break;
                                            }
                                        }
                                        Node3 = Node3.NextSibling;
                                        i++;
                                    }
                                }
                                break;
                        }
                    }
                    return 1;
                }

            }
            catch (Exception ex)
            {

                Log.WriteInfoLog(ex.ToString());
                throw;
            }

        }

        /// <summary>
        /// 测试
        /// </summary>
        public void test()
        {
            try
            {
                using (var writer = File.CreateText(fileName))
                {
                    for (int i = 0; i < 8000000; i++)
                    {
                        writer.WriteLine("Some Junk Data for testing. My Actual Data is created from different sources by Appending to the String Builder.");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteInfoLog(ex.ToString());
                MessageBox.Show("错误信息:\r\n" + ex.ToString());
                throw;
            }
        }
        /// <summary>
        /// 取回图片
        /// </summary>
        /// <returns>保存xml路径</returns>
        public string getFingerPLamList()
        {
            try
            {
                MessageBox.Show(fileName);
                if (System.IO.File.Exists(fileName))
                {
                    File.Delete(fileName);      //删除指定文件
                }
                //inserttext(AllFigFileBytes().ToString());
                //inserttext(test().ToString());
                //test();
                //fileName = "D:\\desktop\\111.xml";
                AllFigFileBytes2();
                if (!System.IO.File.Exists(fileName))
                {

                }
                return fileName;
            }
            catch (Exception ex)
            {
                Log.WriteInfoLog(ex.ToString());
                return "错误信息\r\n" + ex.ToString();
            }
        }
        /// <summary>
        /// 重置控件
        /// </summary>
        /// <returns>fingerPlamStatus 0成功,1失败</returns>
        public string ResetFingerPlamOCX()
        {
            string fingerPlamStatus = "0";
            if (0 != zwzljz)
            {

            }
            return fingerPlamStatus;
        }
        //获取已有指掌纹
        //static byte[] FingerMgs =null;
        static string FingerMgs = "";
        //public string setFingerPlamList(string xml)
        //{
        //    string protraitStatus = "";
        //    try
        //    {
        //        if (1==flag)
        //        {
        //            //FingerMgs = xml;
        //            Thread.Sleep(0);
        //            GetfigXML(xml);
        //            //Thread newthread = new Thread(new ThreadStart(open));
        //            //newthread.Start();
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.ToString());
        //        throw;
        //    }
        //    protraitStatus=flag.ToString();
        //    return protraitStatus;
        //}
        //void open()
        //{
        //    try
        //    {
        //        flag = 0;
        //        GetfigXML(FingerMgs);
        //        flag = 1;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.ToString());
        //    }
        //}
        //
        //public string releaseFingerPlamOCX()
        //{
        //    string fingerPlamStatus = "";
        //    if (0 != PLAM_GLBT.zwzljz)
        //    {

        //    }
        //    //删除本地中间文件
        //    //if (System.IO.File.Exists(fileName))
        //    //{
        //    //    File.Delete(fileName);
        //    //}
        //    return fingerPlamStatus;
        //}
        /// <summary>
        /// 检测指纹是否缺失
        /// </summary>
        /// <returns>采集状态</returns>
        public string gatherFingerPLamFinished()
        {
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    if ("" == plane[i].ZZHWQSQKDM || null == plane[i].ZZHWQSQKDM)
                    {
                        return "0";
                    }
                    if ("" == rolls[i].ZZHWQSQKDM || null == rolls[i].ZZHWQSQKDM)
                    {
                        return "0";
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteInfoLog(ex.ToString());
                return "-1";
            }
            return "1";
        }
        /// <summary>
        /// 将文件转换成byte[] 数组
        /// </summary>
        /// <param name="fileUrl">文件路径文件名称</param>
        /// <returns>byte[]</returns>
        protected byte[] AuthGetFileData(string fileUrl)
        {
            using (FileStream fs = new FileStream(fileUrl, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                byte[] buffur = new byte[fs.Length];
                using (BinaryWriter bw = new BinaryWriter(fs))
                {
                    bw.Write(buffur);
                    bw.Close();
                }
                return buffur;
            }
        }
        /// <summary>
        /// 写byte[]到fileName
        /// </summary>
        /// <param name="pReadByte">byte[]</param>
        /// <param name="fileName">保存至硬盘路径</param>
        /// <returns></returns>
        private bool WriteByteToFile(byte[] pReadByte, string fileName)
        {
            FileStream pFileStream = null;
            try
            {
                pFileStream = new FileStream(fileName, FileMode.OpenOrCreate);
                pFileStream.Write(pReadByte, 0, pReadByte.Length);
            }
            catch (Exception ex)
            {
                Log.WriteInfoLog(ex.ToString());
                return false;
            }
            finally
            {
                if (pFileStream != null)
                    pFileStream.Close();
            }
            return true;
        }

        /// <summary>
        /// 解析指掌纹数据XML
        /// </summary>
        /// <param name="xml">指掌纹数据xml</param>
        /// <returns></returns>
        //public bool GetfigXML(string FileBytesXml)
        //{
        //    try
        //    {
        //        if ("" == FileBytesXml)
        //        {
        //            return false;
        //        }
        //        XmlDocument xx = new XmlDocument();
        //        //string fileName = GetPath() + "\\fingerXmlIn.xml";
        //        //Base64ToOriFile(FileBytesXml, fileName);
        //        ////File.WriteAllBytes(fileName, FileBytesXml);
        //        //if (!System.IO.File.Exists(fileName))
        //        //{
        //        //    MessageBox.Show("文件不存在，请检查");
        //        //    return false;
        //        //}
        //        //xx.Load(fileName);//加载xml
        //        xx.LoadXml(FileBytesXml);
        //        XmlNode FirstNode = xx.SelectSingleNode("root");
        //        XmlNode Node1 = FirstNode.FirstChild;
        //        //XmlNode Node2 = Node1.NextSibling;
        //        XmlElement xe = null;
        //        // XmlNode Node3 = Node2.FirstChild;
        //        XmlNode Node4 = Node1.FirstChild;                            //滚指,平指                                        
        //        XmlNode Node5 = Node1.NextSibling.FirstChild;                //掌纹                               
        //        XmlNode Node6 = Node1.NextSibling.NextSibling.FirstChild;    //四连指                      
        //        XmlNode Node7 = Node1.NextSibling.NextSibling.NextSibling.FirstChild;               //指节纹
        //        XmlNode Node8 = Node1.NextSibling.NextSibling.NextSibling.NextSibling.FirstChild;   //全掌
        //        for (int i = 0; i < 20; i++)//滚指,平指
        //        {
        //            if (null == Node4)
        //                break;
        //            foreach (XmlNode xxNode in Node4)
        //            {
        //                xe = (XmlElement)xxNode;
        //                switch (xe.Name)
        //                {
        //                    case "ZWZWDM":
        //                        if (i < 10)
        //                        {
        //                            rolls[i].ZWZWDM = xxNode.InnerText;
        //                        }
        //                        else
        //                        {
        //                            plane[i - 10].ZWZWDM = xxNode.InnerText;
        //                        }
        //                        break;
        //                    case "ZZHWQSQKDM":
        //                        if (i < 10)
        //                        {
        //                            rolls[i].ZZHWQSQKDM = xxNode.InnerText;
        //                        }
        //                        else
        //                        {
        //                            plane[i - 10].ZZHWQSQKDM = xxNode.InnerText;
        //                        }
        //                        break;
        //                    case "ZW_TXYSFFMS":
        //                        if (i < 10)
        //                        {
        //                            rolls[i].ZW_TXYSFFMS = xxNode.InnerText;
        //                        }
        //                        else
        //                        {
        //                            plane[i - 10].ZW_TXYSFFMS = xxNode.InnerText;
        //                        }
        //                        break;
        //                    case "ZW_TXZL":
        //                        SetText(LableNo[i], xxNode.InnerText);
        //                        if (i < 10)
        //                        {
        //                            rolls[i].ZW_TXZL = xxNode.InnerText;
        //                        }
        //                        else 
        //                        {
        //                            plane[i - 10].ZW_TXZL = xxNode.InnerText;
        //                        }
        //                        break;
        //                    case "ZW_TXSJ":
        //                        if (i < 10)
        //                        {
        //                            rolls[i].ZW_TXSJ = Convert.FromBase64String(xxNode.InnerText);
        //                            if (rolls[i].ZW_TXSJ.Length < 1)
        //                                break;
        //                            rolls[i].ID.Image = Image.FromStream(new MemoryStream(rolls[i].ZW_TXSJ));
        //                            rolls[i].ID.SizeMode = PictureBoxSizeMode.StretchImage;//图片自适应控件大小

        //                        }
        //                        else
        //                        {
        //                            plane[i - 10].ZW_TXSJ = Convert.FromBase64String(xxNode.InnerText);
        //                            if (plane[i - 10].ZW_TXSJ.Length < 1)
        //                                break;
        //                            plane[i - 10].ID.Image = Image.FromStream(new MemoryStream(plane[i - 10].ZW_TXSJ));
        //                            plane[i - 10].ID.SizeMode = PictureBoxSizeMode.StretchImage;//图片自适应控件大小
        //                        }

        //                        break;
        //                }
        //            }
        //            Node4 = Node4.NextSibling;
        //        }
        //        for (int i = 0; i < 4; i++) //掌纹
        //        {
        //            if (null == Node5)
        //                break;
        //            foreach (XmlNode xxNode in Node5)
        //            {
        //                xe = (XmlElement)xxNode;
        //                switch (xe.Name)
        //                {
        //                    case "ZHWZHWDM":
        //                        palms[i].ZHWZHWDM = xxNode.InnerText;
        //                        break;
        //                    case "ZHW_ZZHWQSQKDM":
        //                        palms[i].ZHW_ZZHWQSQKDM = xxNode.InnerText;
        //                        break;
        //                    case "ZHW_TXYSFSMS":
        //                        palms[i].ZHW_TXYSFSMS = xxNode.InnerText;
        //                        break;
        //                    case "ZHW_TXZL":
        //                        palms[i].ZHW_TXZL = xxNode.InnerText;
        //                        break;
        //                    case "ZHW_TXSJ":
        //                        palms[i].ZHW_TXSJ = Convert.FromBase64String(xxNode.InnerText);
        //                        if (palms[i].ZHW_TXSJ.Length < 1)
        //                            break;
        //                        palms[i].ID.Image = Image.FromStream(new MemoryStream(palms[i].ZHW_TXSJ));
        //                        palms[i].ID.SizeMode = PictureBoxSizeMode.StretchImage;//图片自适应控件大小
        //                        break;
        //                }
        //            }
        //            Node5 = Node5.NextSibling;
        //        }
        //        for (int i = 0; i < 3; i++) //四连指
        //        {
        //            if (null == Node6)
        //                break;
        //            foreach (XmlNode xxNode in Node6)
        //            {
        //                xe = (XmlElement)xxNode;
        //                switch (xe.Name)
        //                {
        //                    case "SLZ_ZWZWDM":
        //                        fourfinger[i].SLZ_ZWZWDM = xxNode.InnerText;
        //                        break;
        //                    case "SLZ_ZZHWQSQKDM":
        //                        fourfinger[i].SLZ_ZZHWQSQKDM = xxNode.InnerText;
        //                        break;
        //                    case "SLZ_TXYSFSMS":
        //                        fourfinger[i].SLZ_TXYSFSMS = xxNode.InnerText;
        //                        break;
        //                    case "SLZ_TXZL":
        //                        fourfinger[i].SLZ_TXZL = xxNode.InnerText;
        //                        break;
        //                    case "SLZ_TXSJ":
        //                        fourfinger[i].SLZ_TXSJ = Convert.FromBase64String(xxNode.InnerText);
        //                        if (fourfinger[i].SLZ_TXSJ.Length < 1)
        //                            break;
        //                        fourfinger[i].ID.Image = Image.FromStream(new MemoryStream(fourfinger[i].SLZ_TXSJ));
        //                        fourfinger[i].ID.SizeMode = PictureBoxSizeMode.StretchImage;//图片自适应控件大小
        //                        break;
        //                }
        //            }
        //            Node6 = Node6.NextSibling;
        //        }
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.ToString());
        //        return false;
        //    }
        //}

        //******************************分开打包***********************************//

        /// <summary>
        /// 将xml写入文件
        /// </summary>
        /// <param name="neirong"></param>
        private void inserttext(string neirong, string filepath)
        {
            try
            {
                FileStream fs = new FileStream(filepath, FileMode.Append);
                StreamWriter sw = new StreamWriter(fs);
                sw.WriteLine(neirong);
                sw.Close();
                fs.Close();
            }
            catch (Exception ex)
            {
                Log.WriteInfoLog(ex.ToString());
            }
        }
        /// <summary>
        /// 将本地文件写入2进制数据
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        public byte[] FileToBytes(string path)
        {
            FileStream fs = new FileStream(path, FileMode.Open);
            //获取文件大小
            long size = fs.Length;
            byte[] array = new byte[size];
            //将文件读到byte数组中
            fs.Read(array, 0, array.Length);
            fs.Close();
            return array;
        }
        /// <summary>
        /// 文件转为base64编码
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public string FileToBase64Str(string filePath)
        {
            string base64Str = string.Empty;
            byte[] bt = null;
            try
            {
                using (FileStream filestream = new FileStream(filePath, FileMode.Open))
                {
                    bt = new byte[filestream.Length];
                    //调用read读取方法
                    filestream.Read(bt, 0, bt.Length);
                    //base64Str = Convert.ToBase64String(bt);
                    filestream.Close();
                }
                return System.Text.Encoding.UTF8.GetString(bt); ;
            }
            catch (Exception ex)
            {
                Log.WriteInfoLog(ex.ToString());
                return System.Text.Encoding.UTF8.GetString(bt); ;
            }
        }
        /// <summary>
        /// 文件base64解码
        /// </summary>
        /// <param name="base64Str">文件base64编码</param>
        /// <param name="outPath">生成文件路径</param>
        public void Base64ToOriFile(string base64Str, string outPath)
        {
            //var contents = Convert.FromBase64String(base64Str);
            //using (var fs = new FileStream(outPath, FileMode.Create, FileAccess.Write))
            //{
            //    fs.Write(contents, 0, contents.Length);
            //    fs.Flush();
            //}
            using (StreamWriter sw = new StreamWriter(outPath, false, Encoding.UTF8))
            {
                sw.Write(base64Str);
            }
        }

        /// 写入指纹全数据xml 1.0
        /// </summary>
        private StringBuilder AllFigFileBytes()
        {
            try
            {
                string flag = "SUCCESS";
                string message = "成功";
                StringBuilder fingerPLamXmlStart = new StringBuilder();
                //头
                fingerPLamXmlStart.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                fingerPLamXmlStart.Append("<root>");
                fingerPLamXmlStart.Append("<head>");
                fingerPLamXmlStart.Append("<flag>" + flag + "</flag>");
                fingerPLamXmlStart.Append("<message>" + message + "</message>");
                fingerPLamXmlStart.Append("</head>");
                fingerPLamXmlStart.Append("<DATA>");
                //指纹bmp
                fingerPLamXmlStart.Append("<FINGERBMPLIST>");
                for (int i = 0; i < 10; i++) //滚指
                {
                    fingerPLamXmlStart.Append("<FINGER>");
                    fingerPLamXmlStart.Append("<ZWZWDM>" + (i + 1).ToString() + "</ZWZWDM>");
                    fingerPLamXmlStart.Append("<ZZHWQSQKDM>" + rolls[i].ZZHWQSQKDM + "</ZZHWQSQKDM>");
                    fingerPLamXmlStart.Append("<ZW_TXYSFFMS>" + rolls[i].ZW_TXYSFFMS + "</ZW_TXYSFFMS>");
                    fingerPLamXmlStart.Append("<ZW_TXZL>" + rolls[i].ZW_TXZL + "</ZW_TXZL>");
                    if (null == rolls[i].ZW_TXSJ)
                        fingerPLamXmlStart.Append("<ZW_TXSJ>" + "" + "</ZW_TXSJ>");
                    else
                        fingerPLamXmlStart.Append("<ZW_TXSJ>" + Convert.ToBase64String(rolls[i].ZW_TXSJ) + "</ZW_TXSJ>");
                    fingerPLamXmlStart.Append("</FINGER>");
                }
                for (int i = 0; i < 10; i++) //平面
                {
                    fingerPLamXmlStart.Append("<FINGER>");
                    fingerPLamXmlStart.Append("<ZWZWDM>" + (i + 11).ToString() + "</ZWZWDM>");
                    fingerPLamXmlStart.Append("<ZZHWQSQKDM>" + plane[i].ZZHWQSQKDM + "</ZZHWQSQKDM>");
                    fingerPLamXmlStart.Append("<ZW_TXYSFFMS>" + plane[i].ZW_TXYSFFMS + "</ZW_TXYSFFMS>");
                    fingerPLamXmlStart.Append("<ZW_TXZL>" + plane[i].ZW_TXZL + "</ZW_TXZL>");
                    if (null == plane[i].ZW_TXSJ)
                        fingerPLamXmlStart.Append("<ZW_TXSJ>" + "" + "</ZW_TXSJ>");
                    else
                        fingerPLamXmlStart.Append("<ZW_TXSJ>" + Convert.ToBase64String(plane[i].ZW_TXSJ) + "</ZW_TXSJ>");
                    fingerPLamXmlStart.Append("</FINGER>");
                }
                fingerPLamXmlStart.Append("</FINGERBMPLIST>");
                //掌纹bmp
                fingerPLamXmlStart.Append("<PLAMLBMPIST>");
                for (int i = 0; i < 4; i++)
                {
                    fingerPLamXmlStart.Append("<PLAM>");
                    fingerPLamXmlStart.Append("<ZHWZHWDM>" + (i + 31).ToString() + "</ZHWZHWDM>");
                    fingerPLamXmlStart.Append("<ZHW_ZZHWQSQKDM>" + palms[i].ZHW_ZZHWQSQKDM + "</ZHW_ZZHWQSQKDM>");
                    fingerPLamXmlStart.Append("<ZHW_TXYSFSMS>" + palms[i].ZHW_TXYSFSMS + "</ZHW_TXYSFSMS>");
                    fingerPLamXmlStart.Append("<ZHW_TXZL>" + palms[i].ZHW_TXZL + "</ZHW_TXZL>");
                    if (null == palms[i].ZHW_TXSJ)
                        fingerPLamXmlStart.Append("<ZHW_TXSJ>" + "" + "</ZHW_TXSJ>");
                    else
                        fingerPLamXmlStart.Append("<ZHW_TXSJ>" + Convert.ToBase64String(palms[i].ZHW_TXSJ) + "</ZHW_TXSJ>");
                    fingerPLamXmlStart.Append("</PLAM>");
                }
                fingerPLamXmlStart.Append("</PLAMLBMPIST>");
                //四联指bmp
                fingerPLamXmlStart.Append("<FOURFINGERBMPLIST>");
                for (int i = 0; i < 3; i++)
                {
                    fingerPLamXmlStart.Append("<FOURFINGER>");
                    fingerPLamXmlStart.Append("<SLZ_ZWZWDM>" + (i + 21).ToString() + "</SLZ_ZWZWDM>");
                    fingerPLamXmlStart.Append("<SLZ_ZZHWQSQKDM>" + fourfinger[i].SLZ_ZZHWQSQKDM + "</SLZ_ZZHWQSQKDM>");
                    fingerPLamXmlStart.Append("<SLZ_TXYSFSMS>" + fourfinger[i].SLZ_TXYSFSMS + "</SLZ_TXYSFSMS>");
                    fingerPLamXmlStart.Append("<SLZ_TXZL>" + fourfinger[i].SLZ_TXZL + "</SLZ_TXZL>");
                    if (null == fourfinger[i].SLZ_TXSJ)
                        fingerPLamXmlStart.Append("<SLZ_TXSJ>" + "" + "</SLZ_TXSJ>");
                    else
                        fingerPLamXmlStart.Append("<SLZ_TXSJ>" + Convert.ToBase64String(fourfinger[i].SLZ_TXSJ) + "</SLZ_TXSJ>");
                    fingerPLamXmlStart.Append("</FOURFINGER>");
                }
                fingerPLamXmlStart.Append("</FOURFINGERBMPLIST>");

                fingerPLamXmlStart.Append("</DATA>");
                fingerPLamXmlStart.Append("</root>");
                return fingerPLamXmlStart;
            }
            catch (Exception ex)
            {
                Log.WriteInfoLog(ex.ToString());
                throw;
            }

        }

        /// 写入指纹全数据xml 2.0
        /// </summary>
        private void AllFigFileBytes2()
        {
            try
            {
                string flag = "SUCCESS";
                string message = "成功";
                using (var writer = File.CreateText(fileName))
                {
                    //头
                    writer.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                    writer.WriteLine("<root>");
                    writer.WriteLine("<head>");
                    writer.WriteLine("<flag>" + flag + "</flag>");
                    writer.WriteLine("<message>" + message + "</message>");
                    writer.WriteLine("</head>");
                    writer.WriteLine("<DATA>");
                    //指纹bmp
                    writer.WriteLine("<FINGERBMPLIST>");
                    for (int i = 0; i < 10; i++) //滚指
                    {
                        writer.WriteLine("<FINGER>");
                        writer.WriteLine("<ZWZWDM>" + (i + 1).ToString() + "</ZWZWDM>");
                        writer.WriteLine("<ZZHWQSQKDM>" + rolls[i].ZZHWQSQKDM + "</ZZHWQSQKDM>");
                        writer.WriteLine("<ZW_TXYSFFMS>" + rolls[i].ZW_TXYSFFMS + "</ZW_TXYSFFMS>");
                        writer.WriteLine("<ZW_TXZL>" + rolls[i].ZW_TXZL + "</ZW_TXZL>");
                        if (null == rolls[i].ZW_TXSJ)
                            writer.WriteLine("<ZW_TXSJ>" + "" + "</ZW_TXSJ>");
                        else
                            writer.WriteLine("<ZW_TXSJ>" + Convert.ToBase64String(rolls[i].ZW_TXSJ) + "</ZW_TXSJ>");
                        writer.WriteLine("</FINGER>");
                    }
                    for (int i = 0; i < 10; i++) //平面
                    {
                        writer.WriteLine("<FINGER>");
                        writer.WriteLine("<ZWZWDM>" + (i + 11).ToString() + "</ZWZWDM>");
                        writer.WriteLine("<ZZHWQSQKDM>" + plane[i].ZZHWQSQKDM + "</ZZHWQSQKDM>");
                        writer.WriteLine("<ZW_TXYSFFMS>" + plane[i].ZW_TXYSFFMS + "</ZW_TXYSFFMS>");
                        writer.WriteLine("<ZW_TXZL>" + plane[i].ZW_TXZL + "</ZW_TXZL>");
                        if (null == plane[i].ZW_TXSJ)
                            writer.WriteLine("<ZW_TXSJ>" + "" + "</ZW_TXSJ>");
                        else
                            writer.WriteLine("<ZW_TXSJ>" + Convert.ToBase64String(plane[i].ZW_TXSJ) + "</ZW_TXSJ>");
                        writer.WriteLine("</FINGER>");
                    }
                    writer.WriteLine("</FINGERBMPLIST>");
                    //掌纹bmp
                    writer.WriteLine("<PLAMLBMPIST>");
                    for (int i = 0; i < 4; i++)
                    {
                        writer.WriteLine("<PLAM>");
                        writer.WriteLine("<ZHWZHWDM>" + (i + 31).ToString() + "</ZHWZHWDM>");
                        writer.WriteLine("<ZHW_ZZHWQSQKDM>" + palms[i].ZHW_ZZHWQSQKDM + "</ZHW_ZZHWQSQKDM>");
                        writer.WriteLine("<ZHW_TXYSFSMS>" + palms[i].ZHW_TXYSFSMS + "</ZHW_TXYSFSMS>");
                        writer.WriteLine("<ZHW_TXZL>" + palms[i].ZHW_TXZL + "</ZHW_TXZL>");
                        if (null == palms[i].ZHW_TXSJ)
                            writer.WriteLine("<ZHW_TXSJ>" + "" + "</ZHW_TXSJ>");
                        else
                            writer.WriteLine("<ZHW_TXSJ>" + Convert.ToBase64String(palms[i].ZHW_TXSJ) + "</ZHW_TXSJ>");
                        writer.WriteLine("</PLAM>");
                    }
                    writer.WriteLine("</PLAMLBMPIST>");
                    //四联指bmp
                    writer.WriteLine("<FOURFINGERBMPLIST>");
                    for (int i = 0; i < 3; i++)
                    {
                        writer.WriteLine("<FOURFINGER>");
                        writer.WriteLine("<SLZ_ZWZWDM>" + (i + 21).ToString() + "</SLZ_ZWZWDM>");
                        writer.WriteLine("<SLZ_ZZHWQSQKDM>" + fourfinger[i].SLZ_ZZHWQSQKDM + "</SLZ_ZZHWQSQKDM>");
                        writer.WriteLine("<SLZ_TXYSFSMS>" + fourfinger[i].SLZ_TXYSFSMS + "</SLZ_TXYSFSMS>");
                        writer.WriteLine("<SLZ_TXZL>" + fourfinger[i].SLZ_TXZL + "</SLZ_TXZL>");
                        if (null == fourfinger[i].SLZ_TXSJ)
                            writer.WriteLine("<SLZ_TXSJ>" + "" + "</SLZ_TXSJ>");
                        else
                            writer.WriteLine("<SLZ_TXSJ>" + Convert.ToBase64String(fourfinger[i].SLZ_TXSJ) + "</SLZ_TXSJ>");
                        writer.WriteLine("</FOURFINGER>");
                    }
                    writer.WriteLine("</FOURFINGERBMPLIST>");

                    writer.WriteLine("</DATA>");
                    writer.WriteLine("</root>");
                }
            }
            catch (Exception ex)
            {
                Log.WriteInfoLog(ex.ToString());
                throw;
            }

        }

        /// 上传市库
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void upload_Click(object sender, EventArgs e)
        {
            try
            {
                //打包FPT4.0
                string filepath = "";
                if (Convert.ToInt32(IniReadValue("LT_PT", "ftpgs", getdburl1() + "\\config.ini")) == 4)
                {
                    FPTData3R dp = new FPTData3R();

                    filepath = dp.packFPT();
                    if ("" == filepath)
                    {
                        MessageBox.Show("生成FPT4.0失败！");
                        return;
                    }
                }
                else
                {
                    GeneratedFile();
                    GreateFPT();
                    filepath = this.GetPath() + "\\FPT\\" + InformationData.ysxt_asjxgrybh + ".fptx";
                }
                //上传FPT4.0
                FileInfo fileinfo = new FileInfo(filepath);
                string hostname = IniReadValue("LT_PT", "hostname", getdburl1() + "\\config.ini");
                string targetDir = IniReadValue("LT_PT", "targetDir", getdburl1() + "\\config.ini");
                string username = IniReadValue("LT_PT", "username", getdburl1() + "\\config.ini");
                string password = IniReadValue("LT_PT", "password", getdburl1() + "\\config.ini");
                string ports = IniReadValue("LT_PT", "port", getdburl1() + "\\config.ini");
                ftpcountect FTP1 = new ftpcountect();
                string outmessage;
                if (FTP1.CheckFtp(hostname.Split(':')[0].ToString(), username, password, out outmessage, Convert.ToInt32(ports)))
                {
                    ftpcreat FFFF = new ftpcreat();
                    FFFF.UploadFile(fileinfo, targetDir, hostname, username, password);
                    // MessageBox.Show("zip包生成并上传成功");
                }
                else
                {
                    MessageBox.Show(outmessage);
                }
            }
            catch (Exception ex)
            {
                Log.WriteInfoLog(ex.ToString());
                throw;
            }
        }
        private void GeneratedFile()
        {
            try
            {
                if (File.Exists(this.GetPath() + "\\FPT\\" + InformationData.ysxt_asjxgrybh + ".fptx"))
                {
                    File.Delete(this.GetPath() + "\\FPT\\" + InformationData.ysxt_asjxgrybh + ".fptx");
                }
                using (StreamWriter file = new System.IO.StreamWriter(this.GetPath() + "\\FPT\\" + InformationData.ysxt_asjxgrybh + ".fptx", true))
                {
                    //file.Write("");
                    file.Close();
                }
            }
            catch (Exception ex)
            {
                Log.WriteInfoLog(ex.ToString());
            }
        }
        private void GreateFPT()
        {
            inserttext("<?xml version=\"1.0\" encoding=\"utf-8\" ?><package>", this.GetPath() + "\\FPT\\" + InformationData.ysxt_asjxgrybh + ".fptx");
            gethead();
            getFPackage();
            inserttext("</package>", this.GetPath() + "\\FPT\\" + InformationData.ysxt_asjxgrybh + ".fptx");
        }
        private void gethead()
        {
            string alldata = "";
            alldata = alldata + "<packageHead>";
            alldata = alldata + " <version>FPT0500</version>";
            alldata = alldata + " <createTime>" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + "</createTime>";
            alldata = alldata + " <originSystem>CJ</originSystem>";
            alldata = alldata + " <fsdw_gajgjgdm>" + InformationData.fsdw_gajgjgdm + "</fsdw_gajgjgdm>";
            alldata = alldata + " <fsdw_gajgmc>" + InformationData.fsdw_gajgmc + "</fsdw_gajgmc>";
            alldata = alldata + " <fsdw_xtlx>1900</fsdw_xtlx>";
            alldata = alldata + " <fsr_xm>" + InformationData.fsr_xm + "</fsr_xm>";
            alldata = alldata + " <fsr_gmsfhm>" + InformationData.fsr_gmsfhm + "</fsr_gmsfhm>";
            alldata = alldata + " <fsr_lxdh>" + InformationData.fsr_lxdh + "</fsr_lxdh>";
            alldata = alldata + "</packageHead>";
            inserttext(alldata, this.GetPath() + "\\FPT\\" + InformationData.ysxt_asjxgrybh + ".fptx");
        }
        private void getFPackage()
        {
            inserttext("<fingerprintPackage>", this.GetPath() + "\\FPT\\" + InformationData.ysxt_asjxgrybh + ".fptx");
            getDMsg();
            getCMsg();
            inserttext("<fingers>", this.GetPath() + "\\FPT\\" + InformationData.ysxt_asjxgrybh + ".fptx");
            getfigers();
            inserttext("</fingers>", this.GetPath() + "\\FPT\\" + InformationData.ysxt_asjxgrybh + ".fptx");
            //inserttext("<palms>", this.GetPath() + "\\FPT\\" + InformationData.ysxt_asjxgrybh + ".fptx");
            ////getpalms();
            ////inserttext("</palms>", this.GetPath() + "\\FPT\\" + InformationData.ysxt_asjxgrybh + ".fptx");
            ////inserttext("<fourprints>", this.GetPath() + "\\FPT\\" + InformationData.ysxt_asjxgrybh + ".fptx");
            ////getfourprints();
            //inserttext("</fourprints>", this.GetPath() + "\\FPT\\" + InformationData.ysxt_asjxgrybh + ".fptx");
            inserttext("<faceImages>", this.GetPath() + "\\FPT\\" + InformationData.ysxt_asjxgrybh + ".fptx");
            getpicture();
            inserttext("</faceImages>", this.GetPath() + "\\FPT\\" + InformationData.ysxt_asjxgrybh + ".fptx");
            inserttext("</fingerprintPackage>", this.GetPath() + "\\FPT\\" + InformationData.ysxt_asjxgrybh + ".fptx");

        }
        private void getDMsg()
        {
            string alldata = "";
            alldata = alldata + "<descriptiveMsg>";
            alldata = alldata + " <ysxt_asjxgrybh>" + InformationData.ysxt_asjxgrybh + "</ysxt_asjxgrybh>";
            alldata = alldata + "<jzrybh></jzrybh>";
            alldata = alldata + "<asjxgrybh></asjxgrybh>";
            alldata = alldata + "<zzhwkbh></zzhwkbh>";
            alldata = alldata + "<collectingReasonSet><cjxxyydm>" + InformationData.cjxxyydm + "</cjxxyydm></collectingReasonSet>";
            alldata = alldata + "<xm>" + InformationData.xm + "</xm>";
            alldata = alldata + "<bmch></bmch>";
            alldata = alldata + "<xbdm>" + InformationData.xbdm + "</xbdm>";
            alldata = alldata + "<csrq>" + InformationData.csrq + "</csrq>";
            alldata = alldata + "<gjdm>156</gjdm>";
            alldata = alldata + "<mzdm>" + InformationData.mzdm + "</mzdm>";
            alldata = alldata + "<cyzjdm>111</cyzjdm>";
            alldata = alldata + "<zjhm>" + InformationData.zjhm + "</zjhm>";
            alldata = alldata + "<hjdz_xzqhdm>" + InformationData.hjdz_xzqhdm + "</hjdz_xzqhdm>";
            alldata = alldata + "<hjdz_dzmc>" + InformationData.hjdz_dzmc + "</hjdz_dzmc>";
            alldata = alldata + "<xzz_xzqhdm>" + InformationData.xzz_xzqhdm + "</xzz_xzqhdm>";
            alldata = alldata + "<xzz_dzmc>" + InformationData.xzz_dzmc + "</xzz_dzmc>";
            alldata = alldata + "<bz></bz>";

            alldata = alldata + "</descriptiveMsg>";
            inserttext(alldata, this.GetPath() + "\\FPT\\" + InformationData.ysxt_asjxgrybh + ".fptx");


        }
        private void getCMsg()
        {
            string alldata = "";
            alldata = alldata + "<collectInfoMsg>";
            alldata = alldata + "<zwbdxtlxms>1419</zwbdxtlxms>";
            alldata = alldata + "<nydw_gajgjgdm>" + InformationData.nydw_gajgjgdm + "</nydw_gajgjgdm>";
            alldata = alldata + "<nydw_gajgmc>" + InformationData.nydw_gajgmc + "</nydw_gajgmc>";
            alldata = alldata + "<nyry_xm>" + InformationData.nyry_xm + "</nyry_xm>";
            alldata = alldata + "<nyry_gmsfhm>" + InformationData.nyry_gmsfhm + "</nyry_gmsfhm>";
            alldata = alldata + "<nyry_lxdh>" + InformationData.nyry_lxdh + "</nyry_lxdh>";
            alldata = alldata + "<nysj>" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + "</nysj>";
            alldata = alldata + "</collectInfoMsg>";
            inserttext(alldata, this.GetPath() + "\\FPT\\" + InformationData.ysxt_asjxgrybh + ".fptx");

        }
        private void getfigers()
        {
            string alldata = "";

            for (int i = 0; i < rolls.Length; i++)
            {
                if (rolls[i].ZWZWDM != null)
                {
                    alldata = "<fingerMsg>";
                    alldata = alldata + "<zwzwdm>0" + rolls[i].ZWZWDM.ToString() + "</zwzwdm>";     //01~09指位*
                    alldata = alldata + "<zzhwqsqkdm>" + rolls[i].ZZHWQSQKDM + "</zzhwqsqkdm>";   //指纹其它状态
                    alldata = alldata + "<zw_txspfxcd>640</zw_txspfxcd>";
                    alldata = alldata + "<zw_txczfxcd>640</zw_txczfxcd>";
                    alldata = alldata + "<zw_txfbl>500</zw_txfbl>";
                    alldata = alldata + "<zw_txysffms>" + rolls[i].ZW_TXYSFFMS_WSQ + "</zw_txysffms>";
                    alldata = alldata + "<zw_txzl>" + rolls[i].ZW_TXZL + "</zw_txzl>";
                    if (rolls[i].ZW_TXSJ_WSQ.Length > 0)
                    { alldata = alldata + " <zw_txsj>" + Convert.ToBase64String(rolls[i].ZW_TXSJ_WSQ) + "</zw_txsj>"; }
                    alldata = alldata + "</fingerMsg>";
                    inserttext(alldata, this.GetPath() + "\\FPT\\" + InformationData.ysxt_asjxgrybh + ".fptx");
                }


            }
            for (int i = 0; i < plane.Length; i++)
            {
                if (plane[i].ZWZWDM != null)
                {
                    alldata = "<fingerMsg>";
                    alldata = alldata + "<zwzwdm>0" + plane[i].ZWZWDM.ToString() + "</zwzwdm>";     //01~09指位*
                    alldata = alldata + "<zzhwqsqkdm>" + plane[i].ZZHWQSQKDM + "</zzhwqsqkdm>";   //指纹其它状态
                    alldata = alldata + "<zw_txspfxcd>640</zw_txspfxcd>";
                    alldata = alldata + "<zw_txczfxcd>640</zw_txczfxcd>";
                    alldata = alldata + "<zw_txfbl>500</zw_txfbl>";
                    alldata = alldata + "<zw_txysffms>" + plane[i].ZW_TXYSFFMS_WSQ + "</zw_txysffms>";
                    alldata = alldata + "<zw_txzl>" + plane[i].ZW_TXZL + "</zw_txzl>";
                    if (plane[i].ZW_TXSJ_WSQ.Length > 0)
                    { alldata = alldata + " <zw_txsj>" + Convert.ToBase64String(plane[i].ZW_TXSJ_WSQ) + "</zw_txsj>"; }
                    alldata = alldata + "</fingerMsg>";
                    inserttext(alldata, this.GetPath() + "\\FPT\\" + InformationData.ysxt_asjxgrybh + ".fptx");
                }
            }

        }
        private void getpicture()
        {
            string alldata = "";

            for (int i = 0; i < 3; i++)
            {
                alldata = alldata + "<faceImage>";
                alldata = alldata + "<rxzplxdm>" + InformationData.faceImage[i].rxzplxdm + "</rxzplxdm>";
                alldata = alldata + "<rx_dzwjgs>JPEG</rx_dzwjgs>";
                alldata = alldata + " <rx_txsj>" + InformationData.faceImage[i].rx_txsj + "</rx_txsj>";
                alldata = alldata + "</faceImage>";

            }
            inserttext(alldata, this.GetPath() + "\\FPT\\" + InformationData.ysxt_asjxgrybh + ".fptx");

        }

        //******************************足迹***********************************//




        /// <summary>
        /// 打开足迹设备
        /// </summary>
        /// <returns>true 成功</returns>
        private bool OpenDevice()
        {
            try
            {

                if (form.InitEquipment())
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log.WriteInfoLog(ex.ToString());
            }
            return false;
        }

        /// <summary>
        /// 打包足迹xml
        /// </summary>
        /// <returns>足迹xml</returns>
        public string getFootInfoList()
        {
            string footXml = "";
            switch (InformationData.xbdm)
            {
                case "0":
                    InformationData.xbmc = "未知";
                    break;
                case "1":
                    InformationData.xbmc = "男";
                    break;
                case "2":
                    InformationData.xbmc = "女";
                    break;
                default: break;
            }
            footXml += "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
            footXml += "<suspect>";
            footXml += "<suspectInfo>";
            footXml += "<suspectID>" + InformationData.ysxt_asjxgrybh + "</suspectID>";//人员编号
            footXml += "<bnyrxm>" + InformationData.xm + "</bnyrxm>"; // 嫌疑人姓名
            footXml += "<genderCode>" + InformationData.xbdm + "</genderCode>";//性别id，0-未知,1-男,2-女
            footXml += "<genderName>" + InformationData.xbmc + "</genderName>";
            footXml += "<birthday>" + InformationData.csrq + "</birthday>";
            footXml += "<certificateCardNo>" + InformationData.fsr_gmsfhm + "</certificateCardNo>";
            footXml += "<hjd>" + InformationData.hjdz_dzmc + "</hjd>";
            footXml += "<hjddm>" + InformationData.hjdz_xzqhdm + "</hjddm>";
            footXml += "<xzz>" + InformationData.xzz_dzmc + "</xzz>";
            footXml += "<description>" + "" + "</description>";
            footXml += "<nydwdm>" + InformationData.nydw_gajgjgdm + "</nydwdm>";
            footXml += "<nydwmc>" + InformationData.nydw_gajgmc + "</nydwmc>";
            footXml += "<nyrxm>" + InformationData.nyry_xm + "</nyrxm>";
            footXml += "<nyrq>" + System.DateTime.Now.ToString("yyyy-MM-dd") + "</nyrq>";
            footXml += "<suspectHeight>" + "" + "</suspectHeight>";//身高毫米
            footXml += caseTypeList();  //案件类别
            footXml += suspectFootList();//足迹数据
            footXml += "</suspectInfo>";
            footXml += "</suspect>";
            return footXml;
        }
        /// <summary>
        /// 案件类别
        /// </summary>
        /// <returns>案件类别结果</returns>
        private string caseTypeList()
        {
            string ajlb = "";
            ajlb += "<caseTypeList>";
            ajlb += "<caseType>";
            ajlb += "<caseTypeCode>" + 6 + "</caseTypeCode>";//案件类别id
            ajlb += "<caseTypeName>" + 28 + "</caseTypeName>";//案件类别名称
            ajlb += "</caseType>";
            ajlb += "</caseTypeList>";
            return ajlb;
        }

        /// <summary>
        /// 足迹数据
        /// </summary>
        /// <returns></returns>
        private string suspectFootList()
        {
            string suspectFoot = "";
            suspectFoot += "<suspectFootList>";
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                //打印第i行第j列数据
                //Console.WriteLine(Convert.ToString(dataGridView1[j, i].Value));
                // 注意dataGridView1[j,i]代表的是第i行第j列
                suspectFoot += "<suspectFoot>";
                suspectFoot += "<footID>" + InformationData.ysxt_asjxgrybh + "</footID>";
                suspectFoot += "<footType>" + dataGridView1[4, i].Value + "</footType>";//左右脚标识，0:左脚,1:右脚，10：赤足左脚，11：赤足右键
                suspectFoot += "<footImgBase64>" + ChangeImageToString((Image)dataGridView1[0, i].Value) + "</footImgBase64>";
                suspectFoot += "</suspectFoot>";
            }
            suspectFoot += "</suspectFootList>";
            return suspectFoot;
        }
        /// <summary>
        /// 保存到本地
        /// </summary>
        /// <param name="footimage"></param>
        /// <param name="path"></param>
        private void savefootimage(Image img_footimage, string zj_path, Image img_footxmimage, string xm_path)
        {
            string FilePath = getdburl1() + "foot//";
            if (!Directory.Exists(FilePath))
            {
                Directory.CreateDirectory(FilePath);
            }
            string FilePathzj = FilePath + zj_path;//足迹路径
            string FilePathxm = FilePath + xm_path; //足迹鞋面
            if ("" != FilePathzj)
            {
                if (System.IO.File.Exists(FilePathzj))//检测是否已经存在足迹图片
                {
                    File.Delete(FilePathzj);
                }
                if (img_footimage != null)
                    img_footimage.Save(FilePathzj, ImageFormat.Jpeg);

            }
            if ("" != FilePathxm)
            {
                if (System.IO.File.Exists(FilePathxm))//检测是否已经存在鞋面图片
                {
                    File.Delete(FilePathxm);
                }
                if (img_footxmimage != null)
                    img_footxmimage.Save(FilePathxm, ImageFormat.Jpeg);

            }
        }
        /// <summary>
        /// 拍照
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Photograph_Click(object sender, EventArgs e)
        {
            try
            {
                string footType = "";
                string footimagename = "";
                string footimagexmname = "";
                string footimage = "";              //足迹图片
                string footxmimage = "";            //鞋面图片
                Image img_footimage = null;
                Image img_footxmimage = null;
                if (!form.PickFootImage())
                {
                    MessageBox.Show("采集失败，请重新采集");
                    return;
                }
                if (foot_Position.SelectedIndex == -1)
                {
                    MessageBox.Show("足迹部位不能为空!请选择");
                    Photograph.Enabled = true;
                    return;
                }

                form.footdata(ref footimage, ref footxmimage);
                //***************模拟***************//
                //InformationData.ysxt_asjxgrybh = "R1111111111111111";
                //footimage = ChangeImageToString(ReturnPhoto(getdburl1() + "foot1.jpg"));
                //footxmimage = ChangeImageToString(ReturnPhoto(getdburl1() + "foot2.jpg"));
                //***************模拟***************//

                if (footimage != "")
                {
                    img_footimage = ChangeStringToImage(footimage);
                }
                if (footxmimage != "")
                {
                    img_footxmimage = ChangeStringToImage(footxmimage);
                }
                switch (foot_Position.Text)
                {
                    case "左脚":
                        footType = "0";
                        footimagename = InformationData.ysxt_asjxgrybh + "_FT_L.jpg";
                        footimagexmname = InformationData.ysxt_asjxgrybh + "_FT_L_XM.jpg";
                        break;
                    case "右脚":
                        footType = "1";
                        footimagename = InformationData.ysxt_asjxgrybh + "_FT_R.jpg";
                        footimagexmname = InformationData.ysxt_asjxgrybh + "_FT_R_XM.jpg";
                        break;
                    case "赤足左脚":
                        footType = "10";
                        footimagename = InformationData.ysxt_asjxgrybh + "_CFT_L.jpg";
                        footimagexmname = InformationData.ysxt_asjxgrybh + "_CFT_L_XM.jpg";
                        break;
                    case "赤足右脚":
                        footType = "11";
                        footimagename = InformationData.ysxt_asjxgrybh + "_CFT_R.jpg";
                        footimagexmname = InformationData.ysxt_asjxgrybh + "_CFT_R_XM.jpg";
                        break;
                    default: break;
                }
                if (img_footimage != null && img_footxmimage != null)
                {
                    DataGridViewRow dr = new DataGridViewRow();
                    dr.CreateCells(dataGridView1);
                    dr.Cells[0].Value = img_footimage;
                    dr.Cells[1].Value = foot_Position.Text;
                    dr.Cells[2].Value = footimagename;
                    dr.Cells[4].Value = footType;
                    this.dataGridView1.Rows.Add(dr);

                    DataGridViewRow dr1 = new DataGridViewRow();
                    dr1.CreateCells(dataGridView1);
                    dr1.Cells[0].Value = img_footxmimage;
                    dr1.Cells[1].Value = foot_Position.Text;
                    dr1.Cells[2].Value = footimagexmname;
                    dr1.Cells[4].Value = footType;
                    this.dataGridView1.Rows.Add(dr1);
                }
                else if (img_footimage != null)
                {
                    DataGridViewRow dr = new DataGridViewRow();
                    dr.CreateCells(dataGridView1);
                    dr.Cells[0].Value = img_footimage;
                    dr.Cells[1].Value = foot_Position.Text;
                    dr.Cells[2].Value = footimagename;
                    dr.Cells[4].Value = footType;
                    this.dataGridView1.Rows.Add(dr);
                }
                else if (img_footxmimage != null)
                {
                    DataGridViewRow dr1 = new DataGridViewRow();
                    dr1.CreateCells(dataGridView1);
                    dr1.Cells[0].Value = img_footxmimage;
                    dr1.Cells[1].Value = foot_Position.Text;
                    dr1.Cells[2].Value = img_footxmimage;
                    dr1.Cells[4].Value = footType;
                    this.dataGridView1.Rows.Add(dr1);
                }
                else
                {
                    MessageBox.Show("采集失败，请重新采集");
                    return;
                }
                savefootimage(img_footimage, footimagename, img_footxmimage, footimagexmname);
            }
            catch (Exception ex)
            {
                Log.WriteInfoLog(ex.ToString());
                throw;
            }

        }
        Foot form = new Foot();
        //切换采集项
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)//也可以判断tabControl1.SelectedTab.Text的值
            {
                //执行相应的操作
                //MessageBox.Show("1");
            }
            else if (tabControl1.SelectedIndex == 1)
            {
                this.panelfoot.Controls.Clear();
                //将该子窗体设置成非顶级控件
                form.TopLevel = false;
                //将该子窗体的边框去掉
                form.FormBorderStyle = FormBorderStyle.None;
                //设置子窗体随容器大小自动调整
                form.Dock = DockStyle.Fill;
                //设置mdi父容器为当前窗口
                form.Parent = this.panelfoot;
                //子窗体显示
                form.Show();
                //panelfoot.BringToFront();
                //form.ShowDialog();

                //执行相应的操作
                if (!OpenDevice())
                {
                    MessageBox.Show("足迹设备初始化失败");
                }
                
            }
        }
        /// <summary>
        /// 查看图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                //for (int i = 0; i < dataGridView1.RowCount; i++)
                //{
                //    for (int j = 0; j < dataGridView1.ColumnCount; j++)
                //    {
                //        //打印第i行第j列数据
                //        Console.WriteLine(Convert.ToString(dataGridView1[j, i].Value));
                //        //MessageBox.Show("选中行"+(e.RowIndex+1));
                //        ShowFoot.Image = (Image)dataGridView1[0, i].Value;
                //    }
                //}
            }
            catch (Exception ex)
            {
                Log.WriteInfoLog(ex.ToString());
                throw;
            }
        }
        /// <summary>
        /// 操作图片
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                //查看图片
                if (this.dataGridView1.Columns[e.ColumnIndex].Name == "Column1")
                {
                    for (int i = this.dataGridView1.SelectedRows.Count; i > 0; i--)
                    {
                        ShowFoot.Image = (Image)dataGridView1[0, dataGridView1.SelectedRows[i - 1].Index].Value;
                    }
                }
                //删除图片
                if (this.dataGridView1.Columns[e.ColumnIndex].Name == "Column3")
                {

                    for (int i = this.dataGridView1.SelectedRows.Count; i > 0; i--)
                    {
                        string FilePath = getdburl1() + "foot//" + this.dataGridView1.SelectedRows[i - 1].Cells[2].Value;
                        if (System.IO.File.Exists(FilePath))//检测是否已经存在足迹图片
                        {
                            File.Delete(FilePath);
                        }
                        this.dataGridView1.Rows.RemoveAt(dataGridView1.SelectedRows[i - 1].Index);
                    }

                }
            }
            catch (Exception ex)
            {
                Log.WriteInfoLog(ex.ToString());
                throw;
            }
        }

        /// <summary>
        /// 足迹上传
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void foot_Upload_Click(object sender, EventArgs e)
        {
            try
            {
                //10.64.253.98:18888
                string url = IniReadValue("FOOT", "url", getdburl1() + "\\config.ini");
                string footxml = getFootInfoList();
                //保存到本地
                string jcxx_path = getdburl1() + "//zjxx.xml";
                System.IO.File.WriteAllText(jcxx_path, footxml, Encoding.UTF8);

                footUpload upload = new footUpload();
                string resultmsg = "";
                upload.sendMessage(footxml, url, ref resultmsg);
                string msg = "";
                string zjNum = "";
                if (resultmsg != "")
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    //xmlDoc.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?><result><errorCode>0</errorCode><errorMsg>入库成功</errorMsg><susID>111</susID></result>");
                    xmlDoc.LoadXml(resultmsg);
                    XmlNode rootNode = xmlDoc.SelectSingleNode("result");
                    foreach (XmlNode xxNode in rootNode.ChildNodes)
                    {
                        switch (xxNode.Name)
                        {
                            case "errorCode":
                                msg = xxNode.InnerText;
                                break;
                            case "errorMsg":
                                Log.WriteInfoLog("足迹入库反馈:"+ xxNode.InnerText);
                                break;
                            case "susID":
                                zjNum= xxNode.InnerText;
                                break;
                        }
                    }
                }
                else 
                {
                    MessageBox.Show("上传失败，请联系管理员");
                }
                if (msg=="0") { MessageBox.Show("上传成功,足迹编号为:"+ zjNum); }
            }
            catch (Exception ex)
            {
                Log.WriteInfoLog(ex.ToString());
                throw;
            }
        }
        //二进制转图片
        public Image ReturnPhoto(string imagepath)
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
        ///解码base64
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
    }
}
