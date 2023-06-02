using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using System.Xml;

namespace test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                foreach (var control in panel1.Controls.OfType<UserControl>())
                {
                    control.Dispose();
                }
                panel1.Controls.Clear();

                var type = Type.GetTypeFromCLSID(Guid.Parse("53583C8A-1A04-4C28-BE1D-5EDD04839195"));
                if (type == null)
                {
                    MessageBox.Show("未能找到 CLSID 为 53583C8A-1A04-4C28-BE1D-5EDD04839195 的控件！");
                    return;
                }
                var obj = Activator.CreateInstance(type);
                panel1.Controls.Add((UserControl)obj);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        SC_PLAM_GLBT_DLL.PLAM_GLBT a = new SC_PLAM_GLBT_DLL.PLAM_GLBT();
        private void button1_Click(object sender, EventArgs e)
        {
            string imgPath = "E:\\VS2019\\sc\\四川成都（插件源码）\\指纹插件\\SC_PLAM_GLBT_DLL\\test\\bin\\Debug\\cjz.bmp";
            if(!System.IO.File.Exists(imgPath))
            {
                MessageBox.Show("获取图像失败");
                return;
            }
            Bitmap bit = new Bitmap(imgPath);

            byte[] imgBytes = BitmapByte(bit);
            string aa=Convert.ToBase64String(imgBytes);
            string b = "<root><DATA><fsdw_gajgjgdm>1</fsdw_gajgjgdm><fsdw_gajgmc>2</fsdw_gajgmc><fsr_xm>3</fsr_xm><fsr_gmsfhm>4</fsr_gmsfhm><fsr_lxdh>5</fsr_lxdh><ysxt_asjxgrybh>6</ysxt_asjxgrybh><cjxxyydm>7</cjxxyydm><xm>8</xm><xbdm>9</xbdm><csrq>1994-06-30</csrq><gjdm>11</gjdm><mzdm>12</mzdm><cyzjdm>13</cyzjdm><zjhm>14</zjhm><hjdz_xzqhdm >15</hjdz_xzqhdm><hjdz_dzmc>16</hjdz_dzmc><xzz_xzqhdm>17</xzz_xzqhdm><xzz_dzmc>18</xzz_dzmc><nydw_gajgjgdm>19</nydw_gajgjgdm ><nydw_gajgmc>20</nydw_gajgmc><nyry_xm>21</nyry_xm><nyry_gmsfhm>22</nyry_gmsfhm><nyry_lxdh>23</nyry_lxdh><faceImage><rxzplxdm>1</rxzplxdm><rx_txsj>" + aa + "</rx_txsj></faceImage><faceImage><rxzplxdm>2</rxzplxdm><rx_txsj>" + aa + "</rx_txsj></faceImage><faceImage><rxzplxdm>3</rxzplxdm><rx_txsj>" + aa + "</rx_txsj></faceImage></DATA></root>";
            //string b = "E:\\VS2019\\sc\\四川成都（插件源码）\\指纹插件\\SC_PLAM_GLBT_DLL\\test\\bin\\Debug\\intxt.xml";
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(b);
            string strXml = xdoc.InnerXml;
            string str=a.initFingerPlamOCX(strXml);
            MessageBox.Show(str);
        }
        public static byte[] BitmapByte(Bitmap bit) 
        {
            using(MemoryStream stream=new MemoryStream())
            {
                bit.Save(stream, ImageFormat.Bmp);
                byte[] data = new byte[stream.Length];
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(data, 0, Convert.ToInt32(stream.Length));
                return data; 
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                a.getFingerPLamList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("错误信息\r\n" + ex);
                throw;
            }
        }

       
    }
}
