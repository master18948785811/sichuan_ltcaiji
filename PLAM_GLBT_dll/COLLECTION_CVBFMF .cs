using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Xml;

namespace PLAM_GLBT_dll
{
    [Guid("529364AE-6E88-47E1-BE7C-F85E9EA1C6F0")]
    public partial class COLLECTION_CVBFMF : IObjectSafety
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
            if(1== PLAM_GLBT.Open())
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
        //取回图片
        public string getFingerPLamList()
        {
            //string fingerPLamXmlStart = "";
            StringBuilder fingerPLamXmlStart = new StringBuilder();
            string flag = "SUCCESS";
            string message = "成功";
            fingerPLamXmlStart.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            fingerPLamXmlStart.Append("<root>");
             fingerPLamXmlStart.Append("<head>");
             fingerPLamXmlStart.Append("<flag>" + flag + "</flag>");
             fingerPLamXmlStart.Append("<message>" + message + "</message>");
             fingerPLamXmlStart.Append("</head>");
             fingerPLamXmlStart.Append("<DATA>");
            //指纹
             fingerPLamXmlStart.Append("<FINGERLIST>");
            for (int i = 0; i < 10;i++ )//滚指
            {
                 fingerPLamXmlStart.Append("<FINGER>");
                 fingerPLamXmlStart.Append("<ZWZWDM>" + (i+1).ToString() + "</ZWZWDM>");
                 fingerPLamXmlStart.Append("<ZZHWQSQKDM>" + PLAM_GLBT.rolls[i].ZZHWQSQKDM + "</ZZHWQSQKDM>");
                 fingerPLamXmlStart.Append("<ZW_TXYSFFMS>" + PLAM_GLBT.rolls[i].ZW_TXYSFFMS + "</ZW_TXYSFFMS>");
                 fingerPLamXmlStart.Append("<ZW_TXZL>" + PLAM_GLBT.rolls[i].ZW_TXZL + "</ZW_TXZL>");
                if (null == PLAM_GLBT.rolls[i].ZW_TXSJ)
                     fingerPLamXmlStart.Append("<ZW_TXSJ>" + "" + "</ZW_TXSJ>");
                else
                     fingerPLamXmlStart.Append("<ZW_TXSJ>" + Convert.ToBase64String(PLAM_GLBT.rolls[i].ZW_TXSJ) + "</ZW_TXSJ>");
                 fingerPLamXmlStart.Append("</FINGER>");
            }
            for (int i = 0; i < 10;i++ )//平面
            {
                 fingerPLamXmlStart.Append("<FINGER>");
                 fingerPLamXmlStart.Append("<ZWZWDM>" + (i + 11).ToString() + "</ZWZWDM>");
                 fingerPLamXmlStart.Append("<ZZHWQSQKDM>" + PLAM_GLBT.plane[i].ZZHWQSQKDM + "</ZZHWQSQKDM>");
                 fingerPLamXmlStart.Append("<ZW_TXYSFFMS>" + PLAM_GLBT.plane[i].ZW_TXYSFFMS + "</ZW_TXYSFFMS>");
                 fingerPLamXmlStart.Append("<ZW_TXZL>" + PLAM_GLBT.plane[i].ZW_TXZL + "</ZW_TXZL>");
                if (null == PLAM_GLBT.plane[i].ZW_TXSJ)
                     fingerPLamXmlStart.Append("<ZW_TXSJ>" + "" + "</ZW_TXSJ>");
                else
                     fingerPLamXmlStart.Append("<ZW_TXSJ>" + Convert.ToBase64String(PLAM_GLBT.plane[i].ZW_TXSJ) + "</ZW_TXSJ>");
                 fingerPLamXmlStart.Append("</FINGER>");
            }
             fingerPLamXmlStart.Append("</FINGERLIST>");
            //掌纹
            //StringBuilder fingerPLamXml1 = new StringBuilder();
             fingerPLamXmlStart.Append("<PLAMLIST>");
            for (int i = 0; i < 4;i++ )
            {
                fingerPLamXmlStart.Append("<PLAM>");
                fingerPLamXmlStart.Append("<ZHWZHWDM>" + (i + 31).ToString() + "</ZHWZHWDM>");
                fingerPLamXmlStart.Append("<ZHW_ZZHWQSQKDM>" + PLAM_GLBT.palms[i].ZHW_ZZHWQSQKDM + "</ZHW_ZZHWQSQKDM>");
                fingerPLamXmlStart.Append("<ZHW_TXYSFSMS>" + PLAM_GLBT.palms[i].ZHW_TXYSFSMS + "</ZHW_TXYSFSMS>");
                fingerPLamXmlStart.Append("<ZHW_TXZL>" + PLAM_GLBT.palms[i].ZHW_TXZL + "</ZHW_TXZL>");
                if (null == PLAM_GLBT.palms[i].ZHW_TXSJ)
                    fingerPLamXmlStart.Append("<ZHW_TXSJ>" + "" + "</ZHW_TXSJ>");
                else
                    fingerPLamXmlStart.Append("<ZHW_TXSJ>" + Convert.ToBase64String(PLAM_GLBT.palms[i].ZHW_TXSJ) + "</ZHW_TXSJ>");
                fingerPLamXmlStart.Append("</PLAM>");
            }
            fingerPLamXmlStart.Append("</PLAMLIST>");
            //四连指
            fingerPLamXmlStart.Append("<FOURFINGERLIST>");
            for (int i = 0; i < 4; i++)
            {
                fingerPLamXmlStart.Append("<FOURFINGER>");
                fingerPLamXmlStart.Append("<SLZ_ZWZWDM>" + (i + 21).ToString() + "</SLZ_ZWZWDM>");
                fingerPLamXmlStart.Append("<SLZ_ZZHWQSQKDM>" + PLAM_GLBT.fourfinger[i].SLZ_ZZHWQSQKDM + "</SLZ_ZZHWQSQKDM>");
                fingerPLamXmlStart.Append("<SLZ_TXYSFSMS>" + PLAM_GLBT.fourfinger[i].SLZ_TXYSFSMS + "</SLZ_TXYSFSMS>");
                fingerPLamXmlStart.Append("<SLZ_TXZL>" + PLAM_GLBT.fourfinger[i].SLZ_TXZL + "</SLZ_TXZL>");
                if (null == PLAM_GLBT.fourfinger[i].SLZ_TXSJ)
                    fingerPLamXmlStart.Append("<SLZ_TXSJ>" + "" + "</SLZ_TXSJ>");
                else
                    fingerPLamXmlStart.Append("<SLZ_TXSJ>" + Convert.ToBase64String(PLAM_GLBT.fourfinger[i].SLZ_TXSJ) + "</SLZ_TXSJ>");
                fingerPLamXmlStart.Append("</FOURFINGER>");
            }
           fingerPLamXmlStart.Append("</FOURFINGERLIST>");
            //指节纹
            fingerPLamXmlStart.Append("<PHALANGELIST>");
            for (int i = 0; i < 10; i++)
            {
                fingerPLamXmlStart.Append("<PHALANGE>");
                fingerPLamXmlStart.Append("<ZJW_ZWZWDM>" + PLAM_GLBT.fingerprint[i].ZJW_ZWZWDM + "</ZJW_ZWZWDM>");
                fingerPLamXmlStart.Append("<ZJW_ZZHWQSQKDM>" + PLAM_GLBT.fingerprint[i].ZJW_ZZHWQSQKDM + "</ZJW_ZZHWQSQKDM>");
                fingerPLamXmlStart.Append("<ZJW_TXYSFSMS>" + PLAM_GLBT.fingerprint[i].ZJW_TXYSFSMS + "</ZJW_TXYSFSMS>");
                fingerPLamXmlStart.Append("<ZJW_TXZL>" + PLAM_GLBT.fingerprint[i].ZJW_TXZL + "</ZJW_TXZL>");
                if (null == PLAM_GLBT.fingerprint[i].ZJW_TXSJ)
                    fingerPLamXmlStart.Append("<ZJW_TXSJ>" + "" + "</ZJW_TXSJ>");
                else
                    fingerPLamXmlStart.Append("<ZJW_TXSJ>" + Convert.ToBase64String(PLAM_GLBT.fingerprint[i].ZJW_TXSJ) + "</ZJW_TXSJ>");
                fingerPLamXmlStart.Append("</PHALANGE>");
            }
            fingerPLamXmlStart.Append("</PHALANGELIST>");
            //全掌纹
            fingerPLamXmlStart.Append("<FULLPLAMLIST>");
            for (int i = 0; i <2; i++)
            {
                fingerPLamXmlStart.Append("<FULLPLAM>");
                fingerPLamXmlStart.Append("<QZ_ZHWZHWDM>" + PLAM_GLBT.wholeplam[i].QZ_ZHWZHWDM + "</QZ_ZHWZHWDM>");
                fingerPLamXmlStart.Append("<QZ_ZZHWQSQKDM>" + PLAM_GLBT.wholeplam[i].QZ_ZZHWQSQKDM + "</QZ_ZZHWQSQKDM>");
                fingerPLamXmlStart.Append("<QZ_TXYSFSMS>" + PLAM_GLBT.wholeplam[i].QZ_TXYSFSMS + "</QZ_TXYSFSMS>");
                fingerPLamXmlStart.Append("<QZ_TXZL>" + PLAM_GLBT.wholeplam[i].QZ_TXZL + "</QZ_TXZL>");
                if (null == PLAM_GLBT.wholeplam[i].QZ_TXSJ)
                    fingerPLamXmlStart.Append("<QZ_TXSJ>" + "" + "</QZ_TXSJ>");
                else
                    fingerPLamXmlStart.Append("<QZ_TXSJ>" + Convert.ToBase64String(PLAM_GLBT.wholeplam[i].QZ_TXSJ) + "</QZ_TXSJ>");
                fingerPLamXmlStart.Append("</FULLPLAM>");
            }
            fingerPLamXmlStart.Append("</FULLPLAMLIST>");
            fingerPLamXmlStart.Append("</DATA>");
            fingerPLamXmlStart.Append("</root>");

            return fingerPLamXmlStart.ToString();
        }
        //获取已有指掌纹
        public string setFingerPlamList(string xml)
        {
            string protraitStatus = "";
            PLAM_GLBT.GetfigXML(xml);

            return protraitStatus;
        }
        //
        public string releaseFingerPlamOCX()
        {
            string fingerPlamStatus = "";
            if (0 != PLAM_GLBT.zwzljz) 
            {

            }
            return fingerPlamStatus;
        }
    }
}
