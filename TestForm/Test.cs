using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;

namespace TestForm
{
    public partial class Test : Form
    {
        public Test()
        {
            InitializeComponent();
        }

        private void Test_Load(object sender, EventArgs e)
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
            catch (Exception ex) {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Test_Load(sender, e);
        }

        
    }
}
