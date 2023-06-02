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
namespace PLAM_GLBT_dll
{
     [Guid("529364AE-6E88-47E1-BE7C-F85E9EA1C6F0")]
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
        //指节纹
        public struct m_FINGERPRINT
        {
            public PictureBox ID;
            public string Flag;
            public bool temp;
            public string ZJW_ZWZWDM;               //指节纹_指纹指位代码
            public string ZJW_ZZHWQSQKDM;           //指节纹_指掌纹缺失情况代码
            public string ZJW_TXYSFSMS;             //指节纹_图像压缩方法描述
            public string ZJW_TXZL;                 //指节纹_图像质量
            public byte[] ZJW_TXSJ;                 //指节纹_图像数据
            public byte[] ZJW_TXSJ_WSQ;             //指节纹_图像数据WSQ
            public string ZJW_TXYSFSMS_WSQ;         //指节纹_图像压缩方法描述WSQ
        }
        public static m_FINGERPRINT[] fingerprint = new m_FINGERPRINT[10];
        //全掌纹
        public struct m_WHOLEPLAM
        {
            public PictureBox ID;
            public string Flag;
            public bool temp;
            public string QZ_ZHWZHWDM;               //全掌纹_指纹指位代码
            public string QZ_ZZHWQSQKDM;             //全掌纹_指掌纹缺失情况代码
            public string QZ_TXYSFSMS;               //全掌纹_图像压缩方法描述
            public string QZ_TXZL;                   //全掌纹_图像质量
            public byte[] QZ_TXSJ;                   //全掌纹_图像数据
            public byte[] QZ_TXSJ_WSQ;               //全掌纹_图像数据WSQ
            public string QZ_TXYSFSMS_WSQ;           //全掌纹_图像压缩方法描述WSQ
        }
        public static m_WHOLEPLAM[] wholeplam = new m_WHOLEPLAM[2];
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
            palms[0].ID = RIGHT_PALM; palms[1].ID = LEFT_PALM;palms[2].ID = RIGHT_PALMAR; palms[3].ID = LEFT_PALMAR;
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
                 string name1 ="";
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
        private void PLAM_GLBT_Load(object sender, EventArgs e)
        {

            try
            {
                fileName = this.GetPath() + "\\fingerXmlOut.xml";//输出打包文件目录
                string cname = IniReadValue("FINGERPRINT", "pffs", this.GetPath() + "//config.ini");
                if ("0" == cname)
                {
                    zwzljz = NFIQ2DLL.Load();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                throw;
            }
        }
        public static int Open() 
        {
            if (0 != zwzljz)
            {
                return zwzljz;
            }else
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
                        tempLab.Image = Image.FromFile(getdburl());
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
                MessageBox.Show(ex.ToString()); 
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
                catch (System.ObjectDisposedException)
                {
                    MessageBox.Show("No scanners found or some error occurred in AcquisitionForm");
                }
            }
            catch (Exception ex)
            {
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
                Console.WriteLine(ex);
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
            catch (Exception)
            {
                //MessageBox.Show(ex.Message + " in CopyRawImageIntoBitmap", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }
        public void AcquisitionEnded()
        {
            try
            {
                /******************指纹采集状体提示***********************/
                string cname = IniReadValue("FINGERPRINT", "Tips", System.IO.Directory.GetCurrentDirectory() + "//config.ini");
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
                Bitmap LastAcqImage = CaptureGlobals.FullResolutionImage;
                NW_GBFINIMG_InputData ID = new NW_GBFINIMG_InputData(
                    readPixelsFromBmp(LastAcqImage), (uint)LastAcqImage.Width, (uint)LastAcqImage.Height,
                    NW_GBFINIMG_IMAGE_TYPES.NW_GBFINIMG_INPUT_IMAGE_TYPE_LEFT_HAND_4,
                    null, NW_GBFINIMG_SEGMENTATION_OPTIONS.NW_GBFINIMG_HALO_LATENT_ELIMINATION
, (uint)LastAcqImage.Width, (uint)LastAcqImage.Height);

                NW_GBFINIMG_ProcessImage Segmentator = new NW_GBFINIMG_ProcessImage
                {
                    InputToSet = ID
                };

                NW_GBFINIMG_OutputData SegmentationOutput = Segmentator.OutputAfterProcess;
                byte[] FramePtr = SegmentationOutput.OutputFrame;
                CopyRawImageIntoBitmap(FramePtr, ref LastAcqImage);


                MemoryStream ms = new MemoryStream();
                LastAcqImage.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                pictureBox1.Image = LastAcqImage;
                byte[] lenArr = ms.ToArray(); //bitmap转为byte数组,传入评分数据参数需要
                byte[] fileBytes1 = new byte[409600];
                Array.Copy(lenArr, 1078, fileBytes1, 0, 409600);
                int mm = 0;
                //tempLab.Image = Image.FromStream(new MemoryStream(lenArr));
                //tempLab.SizeMode = PictureBoxSizeMode.StretchImage;//图片自适应控件大小
                intosql(fileBytes1, lenArr);
                zwsfcjz = 1;
                //chengborderred(tempLab, Color.Black);//黑框
                // GetFingerNumber(this.ROLL_RIGHT_2, new MouseEventArgs(MouseButtons.Left, 1, this.ROLL_RIGHT_2.Location.X, this.ROLL_RIGHT_2.Location.Y, 0));

                //Acquisition.CaptureObject(CaptureGlobals.ObjectName);//重新采集
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally 
            {
               
            }
        }
        //保存指纹
        private void intosql(byte[] fileBytes1, byte[] lenArr)
        {
            int zwzl=0;//指纹质量
            string cname = IniReadValue("FINGERPRINT", "pffs", System.IO.Directory.GetCurrentDirectory() + "//config.ini");
            //byte[] yuantu = lenArr;
            if (zwzw <= 20)
            {
                switch (cname)
                {
                    case "0":
                        /*********************NFIQ**************************/
                        zwzl = NFIQ_Quality(fileBytes1, 640, 640);
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
                    //string path = this.getdburl1() + "//" + zwzw.ToString() + ".bmp";
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
                    //byte[] yuantu = imagetobyte(path);
                    //string Pathwsq = this.getdburl1() + "//" + zwzw.ToString() + ".wsq";
                    //byte[] xzh = xztp(wtyuantu);
                    //GBMSByteToWsq(xzh, Pathwsq, 640, 640);

                    //将指纹数据存入结构体
                    getfigbytes(zwzw, zwzl, lenArr, "0", "0000");
                    //将指纹wsq数据存入结构体
                    getfigbytes(zwzw, zwzl, GBMSByteToWsq(lenArr,640,640), "0", "1419");
                }
                else
                {
                    zwsfcjz = 1;
                    GetFingerNumber(tempLab, new MouseEventArgs(MouseButtons.Left, 1, this.ROLL_RIGHT_2.Location.X, this.ROLL_RIGHT_2.Location.Y, 0));
                }
            }
            else
            {
                /*********************格林比特**********************/
                //zwzl = Greenbit_Quality(lenArr, 2304, 2304);
                /*********************NFIQ**************************/
                if ("0" == cname)
                {
                    zwzl = NFIQ_Quality(fileBytes1, 2304, 2304);
                    //  MessageBox.Show(zwzl.ToString());
                    if (zwzl == 0 || zwzl == -1 || zwzl == 255)
                    {
                        MessageBox.Show("没有采集到指纹数据");
                        return;
                    }
                }
                //string path = this.getdburl1() + "//" + zwzw.ToString() + ".bmp";
                tempLab.Image = Image.FromStream(new MemoryStream(lenArr));
                tempLab.SizeMode = PictureBoxSizeMode.StretchImage;//图片自适应控件大小
                //tempLab.Image.Save(path);
                //byte[] yuantu = imagetobyte(path);
                //string Pathwsq = this.getdburl1() + "//" + zwzw.ToString() + ".wsq";
                //GBMSByteToWsq(yuantu, Pathwsq, 2304, 2304);
                //将指纹数据存入结构体
                getfigbytes(zwzw, zwzl, lenArr, "0", "0000");
                //将指纹wsq数据存入结构体
                getfigbytes(zwzw, zwzl, GBMSByteToWsq(lenArr,2304,2304), "0", "1419");
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
            if (qu < 60)
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
                case "PLANE_RIGHT_4": ;
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
            if (bmp.PixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed)
            {
                // Lock the bitmap's bits.
                System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height);
                System.Drawing.Imaging.BitmapData bmpData =
                    bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite,
                    bmp.PixelFormat);

                // Get the address of the first line.
                IntPtr ptr = bmpData.Scan0;

                // Declare an array to hold the bytes of the bitmap.
                int bytes = bmpData.Stride * bmp.Height;
                byte[] rgbValues = new byte[bytes];

                // Copy the RGB values into the array.
                System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

                // Unlock the bits.
                bmp.UnlockBits(bmpData);
                return rgbValues;
            }
            return null;
        }
        public string getdburl()
        {
            string[] gtdb = Assembly.GetExecutingAssembly().Location.Split('\\');
            string getdb = "";

            for (int i = 0; i < gtdb.Length - 1; i++)
            {
                getdb = getdb + gtdb[i].ToString() + "\\";

            }
            getdb = getdb + "cjz.bmp";
            return getdb;
        }
        //获取当前根目录
        public string getdburl1()
        {
            string[] gtdb = Assembly.GetExecutingAssembly().Location.Split('\\');
            string getdb = "";

            for (int i = 0; i < gtdb.Length - 1; i++)
            {
                getdb = getdb + gtdb[i].ToString() + "\\";

            }

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
                case "PLANE_RIGHT_4": ;
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
                case "PLANE_RIGHT_4": ;
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
        public string initFingerPlamOCX()
        {
            string fingerPlamOCXVersions = "";
            string flag = "ERROR";
            string message = "初始化失败";
            if (1 == PLAM_GLBT.Open())
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
            return fingerPlamOCXVersions;
        }
        /// <summary>
       ///取回图片
       /// </summary>
       /// <returns>保存xml路径</returns>
        public string getFingerPLamList()
        {
            try 
            {
                //if (System.IO.File.Exists(fileName))
                //{
                //    File.Delete(fileName);      //删除指定文件
                //}
                //inserttext("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                //inserttext("<root>");
                //gethead();
                //inserttext("<DATA>");
                //getfingers();
                //getpalms();
                //getfourprints();
                //getfingerprint();
                //getwholeplam();
                //inserttext("</DATA>");
                //inserttext("</root>");
                //return FileToBase64Str(fileName);
                return AllFigFileBytes().ToString();
            }
            catch (Exception ex) 
            {
                MessageBox.Show("\r\n错误信息" + ex.ToString());
                return null;
            }
        }
        //获取已有指掌纹
        //static byte[] FingerMgs =null;
        static string FingerMgs = "";
        public string setFingerPlamList(string xml)
        {
            string protraitStatus = "";
            try
            {
                if (1==flag)
                {
                    //FingerMgs = xml;
                    Thread.Sleep(0);
                    GetfigXML(xml);
                    //Thread newthread = new Thread(new ThreadStart(open));
                    //newthread.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                throw;
            }
            protraitStatus=flag.ToString();
            return protraitStatus;
        }
        void open()
        {
            try
            {
                flag = 0;
                GetfigXML(FingerMgs);
                flag = 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        //
        public string releaseFingerPlamOCX()
        {
            string fingerPlamStatus = "";
            if (0 != PLAM_GLBT.zwzljz)
            {
               
            }
            //删除本地中间文件
            //if (System.IO.File.Exists(fileName))
            //{
            //    File.Delete(fileName);
            //}
            return fingerPlamStatus;
        }
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
            catch
            {
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
        public bool GetfigXML(string FileBytesXml)
        {
            try
            {
                if ("" == FileBytesXml)
                {
                    return false;
                }
                XmlDocument xx = new XmlDocument();
                //string fileName = GetPath() + "\\fingerXmlIn.xml";
                //Base64ToOriFile(FileBytesXml, fileName);
                ////File.WriteAllBytes(fileName, FileBytesXml);
                //if (!System.IO.File.Exists(fileName))
                //{
                //    MessageBox.Show("文件不存在，请检查");
                //    return false;
                //}
                //xx.Load(fileName);//加载xml
                xx.LoadXml(FileBytesXml);
                XmlNode FirstNode = xx.SelectSingleNode("root");
                XmlNode Node1 = FirstNode.FirstChild;
                //XmlNode Node2 = Node1.NextSibling;
                XmlElement xe = null;
                // XmlNode Node3 = Node2.FirstChild;
                XmlNode Node4 = Node1.FirstChild;                            //滚指,平指                                        
                XmlNode Node5 = Node1.NextSibling.FirstChild;                //掌纹                               
                XmlNode Node6 = Node1.NextSibling.NextSibling.FirstChild;    //四连指                      
                XmlNode Node7 = Node1.NextSibling.NextSibling.NextSibling.FirstChild;               //指节纹
                XmlNode Node8 = Node1.NextSibling.NextSibling.NextSibling.NextSibling.FirstChild;   //全掌
                for (int i = 0; i < 20; i++)//滚指,平指
                {
                    if (null == Node4)
                        break;
                    foreach (XmlNode xxNode in Node4)
                    {
                        xe = (XmlElement)xxNode;
                        switch (xe.Name)
                        {
                            case "ZWZWDM":
                                if (i < 10)
                                {
                                    rolls[i].ZWZWDM = xxNode.InnerText;
                                }
                                else
                                {
                                    plane[i - 10].ZWZWDM = xxNode.InnerText;
                                }
                                break;
                            case "ZZHWQSQKDM":
                                if (i < 10)
                                {
                                    rolls[i].ZZHWQSQKDM = xxNode.InnerText;
                                }
                                else
                                {
                                    plane[i - 10].ZZHWQSQKDM = xxNode.InnerText;
                                }
                                break;
                            case "ZW_TXYSFFMS":
                                if (i < 10)
                                {
                                    rolls[i].ZW_TXYSFFMS = xxNode.InnerText;
                                }
                                else
                                {
                                    plane[i - 10].ZW_TXYSFFMS = xxNode.InnerText;
                                }
                                break;
                            case "ZW_TXZL":
                                SetText(LableNo[i], xxNode.InnerText);
                                if (i < 10)
                                {
                                    rolls[i].ZW_TXZL = xxNode.InnerText;
                                }
                                else 
                                {
                                    plane[i - 10].ZW_TXZL = xxNode.InnerText;
                                }
                                break;
                            case "ZW_TXSJ":
                                if (i < 10)
                                {
                                    rolls[i].ZW_TXSJ = Convert.FromBase64String(xxNode.InnerText);
                                    if (rolls[i].ZW_TXSJ.Length < 1)
                                        break;
                                    rolls[i].ID.Image = Image.FromStream(new MemoryStream(rolls[i].ZW_TXSJ));
                                    rolls[i].ID.SizeMode = PictureBoxSizeMode.StretchImage;//图片自适应控件大小

                                }
                                else
                                {
                                    plane[i - 10].ZW_TXSJ = Convert.FromBase64String(xxNode.InnerText);
                                    if (plane[i - 10].ZW_TXSJ.Length < 1)
                                        break;
                                    plane[i - 10].ID.Image = Image.FromStream(new MemoryStream(plane[i - 10].ZW_TXSJ));
                                    plane[i - 10].ID.SizeMode = PictureBoxSizeMode.StretchImage;//图片自适应控件大小
                                }

                                break;
                        }
                    }
                    Node4 = Node4.NextSibling;
                }
                for (int i = 0; i < 4; i++) //掌纹
                {
                    if (null == Node5)
                        break;
                    foreach (XmlNode xxNode in Node5)
                    {
                        xe = (XmlElement)xxNode;
                        switch (xe.Name)
                        {
                            case "ZHWZHWDM":
                                palms[i].ZHWZHWDM = xxNode.InnerText;
                                break;
                            case "ZHW_ZZHWQSQKDM":
                                palms[i].ZHW_ZZHWQSQKDM = xxNode.InnerText;
                                break;
                            case "ZHW_TXYSFSMS":
                                palms[i].ZHW_TXYSFSMS = xxNode.InnerText;
                                break;
                            case "ZHW_TXZL":
                                palms[i].ZHW_TXZL = xxNode.InnerText;
                                break;
                            case "ZHW_TXSJ":
                                palms[i].ZHW_TXSJ = Convert.FromBase64String(xxNode.InnerText);
                                if (palms[i].ZHW_TXSJ.Length < 1)
                                    break;
                                palms[i].ID.Image = Image.FromStream(new MemoryStream(palms[i].ZHW_TXSJ));
                                palms[i].ID.SizeMode = PictureBoxSizeMode.StretchImage;//图片自适应控件大小
                                break;
                        }
                    }
                    Node5 = Node5.NextSibling;
                }
                for (int i = 0; i < 3; i++) //四连指
                {
                    if (null == Node6)
                        break;
                    foreach (XmlNode xxNode in Node6)
                    {
                        xe = (XmlElement)xxNode;
                        switch (xe.Name)
                        {
                            case "SLZ_ZWZWDM":
                                fourfinger[i].SLZ_ZWZWDM = xxNode.InnerText;
                                break;
                            case "SLZ_ZZHWQSQKDM":
                                fourfinger[i].SLZ_ZZHWQSQKDM = xxNode.InnerText;
                                break;
                            case "SLZ_TXYSFSMS":
                                fourfinger[i].SLZ_TXYSFSMS = xxNode.InnerText;
                                break;
                            case "SLZ_TXZL":
                                fourfinger[i].SLZ_TXZL = xxNode.InnerText;
                                break;
                            case "SLZ_TXSJ":
                                fourfinger[i].SLZ_TXSJ = Convert.FromBase64String(xxNode.InnerText);
                                if (fourfinger[i].SLZ_TXSJ.Length < 1)
                                    break;
                                fourfinger[i].ID.Image = Image.FromStream(new MemoryStream(fourfinger[i].SLZ_TXSJ));
                                fourfinger[i].ID.SizeMode = PictureBoxSizeMode.StretchImage;//图片自适应控件大小
                                break;
                        }
                    }
                    Node6 = Node6.NextSibling;
                }
                for (int i = 0; i < 10; i++) //指节纹
                {
                    if (null == Node7)
                        break;
                    foreach (XmlNode xxNode in Node7)
                    {
                        xe = (XmlElement)xxNode;
                        switch (xe.Name)
                        {
                            case "ZJW_ZWZWDM":
                                fingerprint[i].ZJW_ZWZWDM = xxNode.InnerText;
                                break;
                            case "ZJW_ZZHWQSQKDM":
                                fingerprint[i].ZJW_ZZHWQSQKDM = xxNode.InnerText;
                                break;
                            case "ZJW_TXYSFSMS":
                                fingerprint[i].ZJW_TXYSFSMS = xxNode.InnerText;
                                break;
                            case "ZJW_TXZL":
                                fingerprint[i].ZJW_TXZL = xxNode.InnerText;
                                break;
                            case "ZJW_TXSJ":
                                fingerprint[i].ZJW_TXSJ = Convert.FromBase64String(xxNode.InnerText);
                                break;
                        }
                    }
                    Node7 = Node7.NextSibling;
                }
                for (int i = 0; i < 2; i++) //全掌纹
                {
                    if (null == Node8)
                        break;
                    foreach (XmlNode xxNode in Node8)
                    {
                        xe = (XmlElement)xxNode;
                        switch (xe.Name)
                        {
                            case "QZ_ZHWZHWDM":
                                wholeplam[i].QZ_ZHWZHWDM = xxNode.InnerText;
                                break;
                            case "QZ_ZZHWQSQKDM":
                                wholeplam[i].QZ_ZZHWQSQKDM = xxNode.InnerText;
                                break;
                            case "QZ_TXYSFSMS":
                                wholeplam[i].QZ_TXYSFSMS = xxNode.InnerText;
                                break;
                            case "QZ_TXZL":
                                wholeplam[i].QZ_TXZL = xxNode.InnerText;
                                break;
                            case "QZ_TXSJ":
                                wholeplam[i].QZ_TXSJ = Convert.FromBase64String(xxNode.InnerText);
                                break;
                        }
                    }
                    Node8 = Node8.NextSibling;
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return false;
            }
        }
      
        //******************************分开打包***********************************//
        private void gethead() 
        {
            string head = "";
            string flag = "SUCCESS";
            string message = "成功";
            head += "<head>";
            head += "<flag>" + flag + "</flag>";
            head += "<message>" + message + "</message>";
            head += "</head>";
            inserttext(head);
        }
        /// <summary>
        /// 指纹
        /// </summary>
        /// <returns>指纹XML</returns>
        private void getfingers() 
        {
            string fingerPLamXmlStart = "";
            //指纹
            inserttext("<FINGERLIST>");
            for (int i = 0; i < 10; i++)//滚指
            {
                fingerPLamXmlStart = "";
                fingerPLamXmlStart += "<FINGER>";
                fingerPLamXmlStart += "<ZWZWDM>" + (i + 1).ToString() + "</ZWZWDM>";
                fingerPLamXmlStart += "<ZZHWQSQKDM>" + rolls[i].ZZHWQSQKDM + "</ZZHWQSQKDM>";
                fingerPLamXmlStart += "<ZW_TXYSFFMS>" + rolls[i].ZW_TXYSFFMS + "</ZW_TXYSFFMS>";
                fingerPLamXmlStart += "<ZW_TXZL>" + rolls[i].ZW_TXZL + "</ZW_TXZL>";
                if (null == rolls[i].ZW_TXSJ)
                    fingerPLamXmlStart += "<ZW_TXSJ>" + "" + "</ZW_TXSJ>";
                else
                    fingerPLamXmlStart += "<ZW_TXSJ>" + Convert.ToBase64String(rolls[i].ZW_TXSJ) + "</ZW_TXSJ>";
                fingerPLamXmlStart += "</FINGER>";
                inserttext(fingerPLamXmlStart);
            }
            for (int i = 0; i < 10; i++)//平面
            {
                fingerPLamXmlStart = "";
                fingerPLamXmlStart += "<FINGER>";
                fingerPLamXmlStart += "<ZWZWDM>" + (i + 11).ToString() + "</ZWZWDM>";
                fingerPLamXmlStart += "<ZZHWQSQKDM>" + plane[i].ZZHWQSQKDM + "</ZZHWQSQKDM>";
                fingerPLamXmlStart += "<ZW_TXYSFFMS>" + plane[i].ZW_TXYSFFMS + "</ZW_TXYSFFMS>";
                fingerPLamXmlStart += "<ZW_TXZL>" + plane[i].ZW_TXZL + "</ZW_TXZL>";
                if (null == plane[i].ZW_TXSJ)
                    fingerPLamXmlStart += "<ZW_TXSJ>" + "" + "</ZW_TXSJ>";
                else
                    fingerPLamXmlStart += "<ZW_TXSJ>" + Convert.ToBase64String(plane[i].ZW_TXSJ) + "</ZW_TXSJ>";
                fingerPLamXmlStart += "</FINGER>";
                inserttext(fingerPLamXmlStart);
            }
            inserttext("</FINGERLIST>");
        }
        /// <summary>
        /// 掌纹
        /// </summary>
        /// <returns>掌纹XML</returns>
        private void getpalms() 
        {
            string fingerPLamXmlStart = "";
            inserttext("<PLAMLIST>");
            for (int i = 0; i < 4; i++)
            {
                fingerPLamXmlStart = "";
                fingerPLamXmlStart += "<PLAM>";
                fingerPLamXmlStart += "<ZHWZHWDM>" + (i + 31).ToString() + "</ZHWZHWDM>";
                fingerPLamXmlStart += "<ZHW_ZZHWQSQKDM>" + palms[i].ZHW_ZZHWQSQKDM + "</ZHW_ZZHWQSQKDM>";
                fingerPLamXmlStart += "<ZHW_TXYSFSMS>" + palms[i].ZHW_TXYSFSMS + "</ZHW_TXYSFSMS>";
                fingerPLamXmlStart += "<ZHW_TXZL>" + palms[i].ZHW_TXZL + "</ZHW_TXZL>";
                if (null == palms[i].ZHW_TXSJ)
                    fingerPLamXmlStart += "<ZHW_TXSJ>" + "" + "</ZHW_TXSJ>";
                else
                    fingerPLamXmlStart += "<ZHW_TXSJ>" + Convert.ToBase64String(palms[i].ZHW_TXSJ) + "</ZHW_TXSJ>";
                fingerPLamXmlStart += "</PLAM>";
                inserttext(fingerPLamXmlStart);
            }
            inserttext("</PLAMLIST>");
        } 
        /// <summary>
        /// 四连指
        /// </summary>
        /// <returns>四连指XML</returns>
        private void getfourprints() 
        {
            string fingerPLamXmlStart = "";
           inserttext("<FOURFINGERLIST>");
            for (int i = 0; i < 3; i++)
            {
                fingerPLamXmlStart = "";
                fingerPLamXmlStart += "<FOURFINGER>";
                fingerPLamXmlStart += "<SLZ_ZWZWDM>" + (i + 21).ToString() + "</SLZ_ZWZWDM>";
                fingerPLamXmlStart += "<SLZ_ZZHWQSQKDM>" + fourfinger[i].SLZ_ZZHWQSQKDM + "</SLZ_ZZHWQSQKDM>";
                fingerPLamXmlStart += "<SLZ_TXYSFSMS>" + fourfinger[i].SLZ_TXYSFSMS + "</SLZ_TXYSFSMS>";
                fingerPLamXmlStart += "<SLZ_TXZL>" + fourfinger[i].SLZ_TXZL + "</SLZ_TXZL>";
                if (null == fourfinger[i].SLZ_TXSJ)
                    fingerPLamXmlStart += "<SLZ_TXSJ>" + "" + "</SLZ_TXSJ>";
                else
                    fingerPLamXmlStart += "<SLZ_TXSJ>" + Convert.ToBase64String(fourfinger[i].SLZ_TXSJ) + "</SLZ_TXSJ>";
                fingerPLamXmlStart += "</FOURFINGER>";
                inserttext(fingerPLamXmlStart);
            }
           inserttext("</FOURFINGERLIST>");
        }
        /// <summary>
        /// 指节纹
        /// </summary>
        /// <returns>指节纹XML</returns> 
        private void getfingerprint() 
        { 
            string fingerPLamXmlStart = "";
            inserttext("<PHALANGELIST>");
            for (int i = 0; i < 10; i++)
            {
                fingerPLamXmlStart = "";
                fingerPLamXmlStart+="<PHALANGE>";
                fingerPLamXmlStart+="<ZJW_ZWZWDM>" + fingerprint[i].ZJW_ZWZWDM + "</ZJW_ZWZWDM>";
                fingerPLamXmlStart+="<ZJW_ZZHWQSQKDM>" + fingerprint[i].ZJW_ZZHWQSQKDM + "</ZJW_ZZHWQSQKDM>";
                fingerPLamXmlStart+="<ZJW_TXYSFSMS>" + fingerprint[i].ZJW_TXYSFSMS + "</ZJW_TXYSFSMS>";
                fingerPLamXmlStart+="<ZJW_TXZL>" + fingerprint[i].ZJW_TXZL + "</ZJW_TXZL>";
                if (null == fingerprint[i].ZJW_TXSJ)
                    fingerPLamXmlStart+="<ZJW_TXSJ>" + "" + "</ZJW_TXSJ>";
                else
                    fingerPLamXmlStart+="<ZJW_TXSJ>" + Convert.ToBase64String(fingerprint[i].ZJW_TXSJ) + "</ZJW_TXSJ>";
                fingerPLamXmlStart+="</PHALANGE>";
                inserttext(fingerPLamXmlStart);
            }
            inserttext("</PHALANGELIST>");
        }
        /// <summary>
        /// 全掌纹
        /// </summary>
        /// <returns>全掌纹XML</returns>
        private void getwholeplam() 
        {
            string fingerPLamXmlStart = "";
            inserttext("<FULLPLAMLIST>");
            for (int i = 0; i < 2; i++)
            {
                fingerPLamXmlStart = "";
                fingerPLamXmlStart+="<FULLPLAM>";
                fingerPLamXmlStart += "<QZ_ZHWZHWDM>" + (i + 35).ToString() + "</QZ_ZHWZHWDM>";
                fingerPLamXmlStart+="<QZ_ZZHWQSQKDM>" + wholeplam[i].QZ_ZZHWQSQKDM + "</QZ_ZZHWQSQKDM>";
                fingerPLamXmlStart+="<QZ_TXYSFSMS>" + wholeplam[i].QZ_TXYSFSMS + "</QZ_TXYSFSMS>";
                fingerPLamXmlStart+="<QZ_TXZL>" + wholeplam[i].QZ_TXZL + "</QZ_TXZL>";
                if (null == wholeplam[i].QZ_TXSJ)
                    fingerPLamXmlStart+="<QZ_TXSJ>" + "" + "</QZ_TXSJ>";
                else
                    fingerPLamXmlStart+="<QZ_TXSJ>" + Convert.ToBase64String(wholeplam[i].QZ_TXSJ) + "</QZ_TXSJ>";
                fingerPLamXmlStart+="</FULLPLAM>";
                inserttext(fingerPLamXmlStart);
            }
            inserttext("</FULLPLAMLIST>");
        }
        /// <summary>
        /// 将xml写入文件
        /// </summary>
        /// <param name="neirong"></param>
        private void inserttext(string neirong)
        {
            try 
            {
                FileStream fs = new FileStream(fileName, FileMode.Append);
                StreamWriter sw = new StreamWriter(fs);
                sw.WriteLine(neirong);
                sw.Close();
                fs.Close();
            }
            catch (Exception ex) 
            {
                MessageBox.Show(ex.ToString());
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

        /// 写入指纹全数据xml
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
                //指纹wsq
                fingerPLamXmlStart.Append("<FINGERWSQLIST>");
                for (int i = 0; i < 10; i++) //滚指
                {
                    fingerPLamXmlStart.Append("<FINGER>");
                    fingerPLamXmlStart.Append("<ZWZWDM>" + (i + 1).ToString() + "</ZWZWDM>");
                    fingerPLamXmlStart.Append("<ZZHWQSQKDM>" + rolls[i].ZZHWQSQKDM + "</ZZHWQSQKDM>");
                    fingerPLamXmlStart.Append("<ZW_TXYSFFMS>" + rolls[i].ZW_TXYSFFMS_WSQ + "</ZW_TXYSFFMS>");
                    fingerPLamXmlStart.Append("<ZW_TXZL>" + rolls[i].ZW_TXZL + "</ZW_TXZL>");
                    if (null == rolls[i].ZW_TXSJ_WSQ)
                        fingerPLamXmlStart.Append("<ZW_TXSJ>" + "" + "</ZW_TXSJ>");
                    else
                        fingerPLamXmlStart.Append("<ZW_TXSJ>" + Convert.ToBase64String(rolls[i].ZW_TXSJ_WSQ) + "</ZW_TXSJ>");
                    fingerPLamXmlStart.Append("</FINGER>");
                }
                for (int i = 0; i < 10; i++) //平面
                {
                    fingerPLamXmlStart.Append("<FINGER>");
                    fingerPLamXmlStart.Append("<ZWZWDM>" + (i + 11).ToString() + "</ZWZWDM>");
                    fingerPLamXmlStart.Append("<ZZHWQSQKDM>" + plane[i].ZZHWQSQKDM + "</ZZHWQSQKDM>");
                    fingerPLamXmlStart.Append("<ZW_TXYSFFMS>" + plane[i].ZW_TXYSFFMS_WSQ + "</ZW_TXYSFFMS>");
                    fingerPLamXmlStart.Append("<ZW_TXZL>" + plane[i].ZW_TXZL + "</ZW_TXZL>");
                    if (null == plane[i].ZW_TXSJ_WSQ)
                        fingerPLamXmlStart.Append("<ZW_TXSJ>" + "" + "</ZW_TXSJ>");
                    else
                        fingerPLamXmlStart.Append("<ZW_TXSJ>" + Convert.ToBase64String(plane[i].ZW_TXSJ_WSQ) + "</ZW_TXSJ>");
                    fingerPLamXmlStart.Append("</FINGER>");
                }
                fingerPLamXmlStart.Append("</FINGERWSQLIST>");
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
                //掌纹wsq
                fingerPLamXmlStart.Append("<PLAMLWSQIST>");
                for (int i = 0; i < 4; i++)
                {
                    fingerPLamXmlStart.Append("<PLAM>");
                    fingerPLamXmlStart.Append("<ZHWZHWDM>" + (i + 31).ToString() + "</ZHWZHWDM>");
                    fingerPLamXmlStart.Append("<ZHW_ZZHWQSQKDM>" + palms[i].ZHW_ZZHWQSQKDM + "</ZHW_ZZHWQSQKDM>");
                    fingerPLamXmlStart.Append("<ZHW_TXYSFSMS>" + palms[i].ZHW_TXYSFSMS_WSQ + "</ZHW_TXYSFSMS>");
                    fingerPLamXmlStart.Append("<ZHW_TXZL>" + palms[i].ZHW_TXZL + "</ZHW_TXZL>");
                    if (null == palms[i].ZHW_TXSJ_WSQ)
                        fingerPLamXmlStart.Append("<ZHW_TXSJ>" + "" + "</ZHW_TXSJ>");
                    else
                        fingerPLamXmlStart.Append("<ZHW_TXSJ>" + Convert.ToBase64String(palms[i].ZHW_TXSJ_WSQ) + "</ZHW_TXSJ>");
                    fingerPLamXmlStart.Append("</PLAM>");
                }
                fingerPLamXmlStart.Append("</PLAMLWSQIST>");
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
                //四联指wsq
                fingerPLamXmlStart.Append("<FOURFINGERWSQLIST>");
                for (int i = 0; i < 3; i++)
                {
                    fingerPLamXmlStart.Append("<FOURFINGER>");
                    fingerPLamXmlStart.Append("<SLZ_ZWZWDM>" + (i + 21).ToString() + "</SLZ_ZWZWDM>");
                    fingerPLamXmlStart.Append("<SLZ_ZZHWQSQKDM>" + fourfinger[i].SLZ_ZZHWQSQKDM + "</SLZ_ZZHWQSQKDM>");
                    fingerPLamXmlStart.Append("<SLZ_TXYSFSMS>" + fourfinger[i].SLZ_TXYSFSMS_WSQ + "</SLZ_TXYSFSMS>");
                    fingerPLamXmlStart.Append("<SLZ_TXZL>" + fourfinger[i].SLZ_TXZL + "</SLZ_TXZL>");
                    if (null == fourfinger[i].SLZ_TXSJ_WSQ)
                        fingerPLamXmlStart.Append("<SLZ_TXSJ>" + "" + "</SLZ_TXSJ>");
                    else
                        fingerPLamXmlStart.Append("<SLZ_TXSJ>" + Convert.ToBase64String(fourfinger[i].SLZ_TXSJ_WSQ) + "</SLZ_TXSJ>");
                    fingerPLamXmlStart.Append("</FOURFINGER>");
                }
                fingerPLamXmlStart.Append("</FOURFINGERWSQLIST>");
                //指节纹bmp
                fingerPLamXmlStart.Append("<PHALANGEBMPLIST>");
                for (int i = 0; i < 10; i++)
                {
                    fingerPLamXmlStart.Append("<PHALANGE>");
                    fingerPLamXmlStart.Append("<ZJW_ZWZWDM>" + fingerprint[i].ZJW_ZWZWDM + "</ZJW_ZWZWDM>");
                    fingerPLamXmlStart.Append("<ZJW_ZZHWQSQKDM>" + fingerprint[i].ZJW_ZZHWQSQKDM + "</ZJW_ZZHWQSQKDM>");
                    fingerPLamXmlStart.Append("<ZJW_TXYSFSMS>" + fingerprint[i].ZJW_TXYSFSMS + "</ZJW_TXYSFSMS>");
                    fingerPLamXmlStart.Append("<ZJW_TXZL>" + fingerprint[i].ZJW_TXZL + "</ZJW_TXZL>");
                    if (null == fingerprint[i].ZJW_TXSJ)
                        fingerPLamXmlStart.Append("<ZJW_TXSJ>" + "" + "</ZJW_TXSJ>");
                    else
                        fingerPLamXmlStart.Append("<ZJW_TXSJ>" + Convert.ToBase64String(fingerprint[i].ZJW_TXSJ) + "</ZJW_TXSJ>");
                    fingerPLamXmlStart.Append("</PHALANGE>");
                }
                fingerPLamXmlStart.Append("</PHALANGEBMPLIST>");
                //指节纹wsq
                fingerPLamXmlStart.Append("<PHALANGEWSQLIST>");
                for (int i = 0; i < 10; i++)
                {
                    fingerPLamXmlStart.Append("<PHALANGE>");
                    fingerPLamXmlStart.Append("<ZJW_ZWZWDM>" + fingerprint[i].ZJW_ZWZWDM + "</ZJW_ZWZWDM>");
                    fingerPLamXmlStart.Append("<ZJW_ZZHWQSQKDM>" + fingerprint[i].ZJW_ZZHWQSQKDM + "</ZJW_ZZHWQSQKDM>");
                    fingerPLamXmlStart.Append("<ZJW_TXYSFSMS>" + fingerprint[i].ZJW_TXYSFSMS_WSQ + "</ZJW_TXYSFSMS>");
                    fingerPLamXmlStart.Append("<ZJW_TXZL>" + fingerprint[i].ZJW_TXZL + "</ZJW_TXZL>");
                    if (null == fingerprint[i].ZJW_TXSJ_WSQ)
                        fingerPLamXmlStart.Append("<ZJW_TXSJ>" + "" + "</ZJW_TXSJ>");
                    else
                        fingerPLamXmlStart.Append("<ZJW_TXSJ>" + Convert.ToBase64String(fingerprint[i].ZJW_TXSJ_WSQ) + "</ZJW_TXSJ>");
                    fingerPLamXmlStart.Append("</PHALANGE>");
                }
                fingerPLamXmlStart.Append("</PHALANGEWSQLIST>");
                //全掌纹bmp
                fingerPLamXmlStart.Append("<FULLPLAMBMPLIST>");
                for (int i = 0; i < 2; i++)
                {
                    fingerPLamXmlStart.Append("<FULLPLAM>");
                    fingerPLamXmlStart.Append("<QZ_ZHWZHWDM>" + (i + 35).ToString() + "</QZ_ZHWZHWDM>");
                    fingerPLamXmlStart.Append("<QZ_ZZHWQSQKDM>" + wholeplam[i].QZ_ZZHWQSQKDM + "</QZ_ZZHWQSQKDM>");
                    fingerPLamXmlStart.Append("<QZ_TXYSFSMS>" + wholeplam[i].QZ_TXYSFSMS + "</QZ_TXYSFSMS>");
                    fingerPLamXmlStart.Append("<QZ_TXZL>" + wholeplam[i].QZ_TXZL + "</QZ_TXZL>");
                    if (null == wholeplam[i].QZ_TXSJ)
                        fingerPLamXmlStart.Append("<QZ_TXSJ>" + "" + "</QZ_TXSJ>");
                    else
                        fingerPLamXmlStart.Append("<QZ_TXSJ>" + Convert.ToBase64String(wholeplam[i].QZ_TXSJ) + "</QZ_TXSJ>");
                    fingerPLamXmlStart.Append("</FULLPLAM>");
                }
                fingerPLamXmlStart.Append("</FULLPLAMBMPLIST>");
                //全掌纹wsq
                fingerPLamXmlStart.Append("<FULLPLAMWSQLIST>");
                for (int i = 0; i < 2; i++)
                {
                    fingerPLamXmlStart.Append("<FULLPLAM>");
                    fingerPLamXmlStart.Append("<QZ_ZHWZHWDM>" + (i + 35).ToString() + "</QZ_ZHWZHWDM>");
                    fingerPLamXmlStart.Append("<QZ_ZZHWQSQKDM>" + wholeplam[i].QZ_ZZHWQSQKDM + "</QZ_ZZHWQSQKDM>");
                    fingerPLamXmlStart.Append("<QZ_TXYSFSMS>" + wholeplam[i].QZ_TXYSFSMS_WSQ + "</QZ_TXYSFSMS>");
                    fingerPLamXmlStart.Append("<QZ_TXZL>" + wholeplam[i].QZ_TXZL + "</QZ_TXZL>");
                    if (null == wholeplam[i].QZ_TXSJ_WSQ)
                        fingerPLamXmlStart.Append("<QZ_TXSJ>" + "" + "</QZ_TXSJ>");
                    else
                        fingerPLamXmlStart.Append("<QZ_TXSJ>" + Convert.ToBase64String(wholeplam[i].QZ_TXSJ_WSQ) + "</QZ_TXSJ>");
                    fingerPLamXmlStart.Append("</FULLPLAM>");
                }
                fingerPLamXmlStart.Append("</FULLPLAMWSQLIST>");
                fingerPLamXmlStart.Append("</DATA>");
                fingerPLamXmlStart.Append("</root>");
                return fingerPLamXmlStart;
            }
            catch (Exception)
            {

                throw;
            }

        }
    }
}
