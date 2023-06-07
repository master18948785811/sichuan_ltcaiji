using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Xml;

namespace SC_PLUGIN_DLL
{
    [Guid("DEC1538D-6127-4C6C-9F23-CA9F5F6500CD")]
    public partial class SC_PLUGIN_DLL : UserControl,IObjectSafety
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

        private struct CLASSID 
        {
            public Type classid_type;    //获取classid句柄
            public object classid_obj;   //返回值
            public Panel pan;            //当前加载界面
        }
        CLASSID[] cs = null;
        string classid = "53583C8A-1A04-4C28-BE1D-5EDD04839195";
        public SC_PLUGIN_DLL()
        {
            InitializeComponent();
            cs = new CLASSID[2]
            {
                new CLASSID{classid_obj = null, classid_type = null, pan = plam_panel },
                new CLASSID{classid_obj = null, classid_type = null, pan = foot_panel }
            };
        }

        private void SC_PLUGIN_DLL_Load(object sender, EventArgs e)
        {
            if ("" != ReadClassId("plam"))
                classid = ReadClassId("plam");
            GetClassId(ref cs[0]);

        }
        public string ReadClassId(string colName)
        {
            string device = "";
            switch (colName) 
            {
                case "plam":
                    device = "53583C8A-1A04-4C28-BE1D-5EDD04839195";
                    break;
                case "foot":
                    device = "3F8B1A9D-385F-42EB-8B50-7868AB484825";
                    break;
            }
            return device;
        }
        /// <summary>
        /// 获取classid
        /// </summary>
        private void GetClassId(ref CLASSID cLASSID)
        {
            if (cLASSID.classid_type != null)
                return;
            //根据classId获取ActiveX类
            cLASSID.classid_type = Type.GetTypeFromCLSID(new Guid(classid));
            //创建类的实例，第二个参数是object数组，就是你的构造方法里面的参数，
            //null即为无参构造方法，也可以这么写：
            // object obj = Activator.CreateInstance(type);
            cLASSID.classid_obj = Activator.CreateInstance(cLASSID.classid_type, null);
            //把ActiveX控件添加到窗体;
            Control con = (Control)cLASSID.classid_obj;
            con.Dock = DockStyle.Fill;
            cLASSID.pan.Controls.Add(con);
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                if ("" != ReadClassId("plam"))
                    classid = ReadClassId("plam");
                GetClassId(ref cs[0]);
            }else if(tabControl1.SelectedIndex == 1) 
            {
                if ("" != ReadClassId("foot"))
                    classid = ReadClassId("foot");
                GetClassId(ref cs[1]);
            }
        }
        
    }
}
