using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FOOT_HR
{
    public partial class Foot : Form
    {
        public Foot()
        {
            InitializeComponent();
        }
        //初始化控件
        public bool InitEquipment() 
        {
            return axCapture1.InitEquipment();
        }

        public bool PickFootImage()
        {
            return axCapture1.PickFootImage();
        }

        public void footdata(ref string FootImage,ref string FootImageCapture) 
        {
            FootImage=axCapture1.FootImage;
            FootImageCapture=axCapture1.FootImageCapture;
        }
        //反初始化控件
        public bool UnInitEquipment() 
        {
            return axCapture1.UnInitEquipment();
        }
    }
}
