using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SC_PLAM_GLBT_DLL
{
    class InformationData
    {
        public static string fsdw_gajgjgdm = "";               //发送单位代码
        public static string fsdw_gajgmc = "";                 //发送单位名称
        public static string fsr_xm = "";                      //发送人姓名
        public static string fsr_gmsfhm = "";                  //发送人身份证号
        public static string fsr_lxdh = "";                    //发送人联系电话
        public static string ysxt_asjxgrybh = "";              //原始系统_案事件相关人员编号
        public static string cjxxyydm = "";                    //采集信息原因代码
        public static string xm = "";                          //姓名
        public static string xbdm = "";                        //性别代码
        public static string xbmc = "";                        //性别名称
        public static string csrq = "";                        //出生日期
        public static string gjdm = "";                        //国籍代码
        public static string mzdm = "";                        //民族代码
        public static string cyzjdm = "";                      //常用证件代码
        public static string zjhm = "";                        //证件号码
        public static string hjdz_xzqhdm = "";                 //户籍地址_行政区划代码
        public static string hjdz_dzmc = "";                   //户籍地址_地址名称
        public static string xzz_xzqhdm = "";                  //现住址_行政区划代码
        public static string xzz_dzmc = "";                    //现住址_地址名称	
        public static string nydw_gajgjgdm = "";               //捺印单位_公安机关机构代码
        public static string nydw_gajgmc = "";                 //捺印单位_公安机关名称
        public static string nyry_xm = "";                     //捺印人员_姓名
        public static string nyry_gmsfhm = "";                 //捺印人员_公民身份号码
        public static string nyry_lxdh = "";                   //捺印人员_联系电话

        public struct FACEIMAGE 
        {
            public string rxzplxdm ;                    //人像照片类型代码
            public byte[] rx_txsj;                      //人像_图像数据
        }
        public static FACEIMAGE[] faceImage = new FACEIMAGE[3];

        public struct FOOTIMAGE 
        {
            public string footType;                     //左右脚标识，0:左脚,1:右脚，10：赤足左脚，11：赤足右键
            public byte[] footImgBase64;                //足迹样本base64图片
        }
        //10.64.253.98:18888
    }
}

