using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace SC_PLAM_GLBT_DLL
{
    /********************************************************新指纹评分工具*****************************************************************/
    class gfsqualitycheck_New
    {
        [DllImport("SSImageQuality.dll", CallingConvention = CallingConvention.Cdecl, EntryPoint = "S_FingerQualityCheck")]
        public static extern int S_FingerQualityCheck(int ZWZWDM, int ZW_TXYSFFMS, byte[] ZW_TXSJ, int ZW_TXSJ_LEN, int ZW_TX_WIDTH, int ZW_TX_HEIGHT, ref int ZW_TXZL, ref int ZW_TZZL);
    }
    //    #define	_ERR_PARAMETER	    -1	//参数错误。给定函数的参数有错误。
    //#define	_ERR_MEMORY		    -2	//内存分配失败。没有分配到足够的内存。
    //#define	_ERR_FUNCTION		-3	//功能未实现。调用函数的功能没有实现。
    //#define	_ERR_RESERVE1		-4	//保留
    //#define	_ERR_RESERVE2		-5	//保留
    //#define	_ERR_ERRNUMBER	    -6	//非法的错误号。
    //#define	_ERR_UNAUTHOR		-7	//没有授权
    //#define	_ERR_NONEINIT		-8	//拼接未初始化。
    //#define _ERR_TOOQUICK		-9  //滚动速度太快
    //#define _ERR_ROLLBACK		-10 //大幅度回滚
    //#define _ERR_DISLOCATION	-11 //由于回滚或者捺印变形导致纹线错位
    //#define _ERR_IMGlONGERROR   -12 //图像过大

    //#define _ERR_BADQLEV		-20			//指纹捺印质量差
    //#define _ERR_COMPLETE		-21			//指纹捺印不完整
    //#define _ERR_SMALLAREA	    -22			//指纹捺印面积太小（捺印高度或宽度不够）
    //#define _ERR_LEFT			-23			//指纹捺印太靠左边
    //#define _ERR_RIGHT		    -24			//指纹捺印太靠右边
    //#define _ERR_ANGLE		    -25			//指纹捺印倾斜角太大

    //#define	KERROR_UNKNOWN			-1		//	未知错误
    //#define KERROR_PARAMETER		-2		//	参数错误
    //#define	KERROR_DATA				-3		//	数据错误
    //#define	KERROR_MEMORY			-4		//	内存申请错误
    //#define	KERROR_LICENSE			-5		//	使用权限错误


    //int S_FingerQualityCheck(
    //    int ZWZWDM, //IN.指位（滚指1-10,平面11-20），0表示不确定指位,最好能给出指位，有助于采集面积的判断
    //    int ZW_TXYSFFMS, //IN.0BMP,1WSQ(是否压缩图)
    //    unsigned char * ZW_TXSJ, //IN.BMP文件格式数据
    //    int ZW_TXSJ_LEN, // IN.指纹图像大小
    //    int ZW_TX_WIDTH, // IN.宽
    //    int ZW_TX_HEIGHT, //IN.高
    //    char *KEY, //IN.秘钥，提供给使用者
    //    int * ZW_TXZL, //OUT 指纹图像质量，0－100（接口返回）.
    //    int * ZW_TZZL  //OUT 指纹特征质量 0－100（接口返回）
    //    );
      
    
}
