using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace FOOT_HR
{
    [Guid("3F8B1A9D-385F-42EB-8B50-7868AB484825")]
    public partial class FOOT_HR: UserControl, IObjectSafety
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
        public FOOT_HR()
        {
            InitializeComponent();
        }

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
                suspectFoot += "<footImgBase64>" + Tool.ChangeImageToString((Image)dataGridView1[0, i].Value) + "</footImgBase64>";
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
            string FilePath = Tool.getdburl1() + "foot//";
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
                    img_footimage = Tool.ChangeStringToImage(footimage);
                }
                if (footxmimage != "")
                {
                    img_footxmimage = Tool.ChangeStringToImage(footxmimage);
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
                        string FilePath = Tool.getdburl1() + "foot//" + this.dataGridView1.SelectedRows[i - 1].Cells[2].Value;
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

        private void foot_Upload_Click(object sender, EventArgs e)
        {
            try
            {
                //10.64.253.98:18888
                string url = Tool.IniReadValue("FOOT", "url", Tool.getdburl1() + "\\config.ini");
                string footxml = getFootInfoList();
                //保存到本地
                string jcxx_path = Tool.getdburl1() + "//zjxx.xml";
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
                                Log.WriteInfoLog("足迹入库反馈:" + xxNode.InnerText);
                                break;
                            case "susID":
                                zjNum = xxNode.InnerText;
                                break;
                        }
                    }
                }
                else
                {
                    MessageBox.Show("上传失败，请联系管理员");
                }
                if (msg == "0") { MessageBox.Show("上传成功,足迹编号为:" + zjNum); }
            }
            catch (Exception ex)
            {
                Log.WriteInfoLog(ex.ToString());
                throw;
            }
        }

        
        
    }
}
