using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
namespace SC_PLAM_GLBT_DLL
{
    class NFIQ2DLL
    {
        
        [DllImport("NFIQ2DLL.dll", EntryPoint = "getNFIQ2QualityScoreEx2")]
        public static extern int getNFIQ2QualityScoreEx2(byte[] img, int imgsize, int weight, int height);

        [DllImport("NFIQ2DLL.dll", EntryPoint = "Load")]
        public static extern int Load();

        
    }
}
