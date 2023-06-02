using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SC_PLAM_GLBT_DLL
{
    public partial class FigState : Form
    {

        public FigState()
        {
            InitializeComponent();
        }
        public string OutValue;
        private bool col = false;
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            if (radioButton1.Checked)
            {
                this.OutValue = "1";
            }
            if (radioButton2.Checked)
            {
                this.OutValue = "2";
            }
            if (radioButton3.Checked)
            {
                this.OutValue = "3";
            }
            if (radioButton4.Checked)
            {
                this.OutValue = "9";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (radioButton4.Checked && "" == textBox1.Text)
            {
                MessageBox.Show("未填写缺失情况，请填写后再次确认。");
                col = true;
            }
            else
            {
                col = false;
                this.DialogResult = DialogResult.OK;
                this.Close();
               
            }
           
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked)
            {
                this.textBox1.Visible = true;
            }
           
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            this.textBox1.Visible = false;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            this.textBox1.Visible = false;
        }

        private void FigState_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = col;
        }

    }
}
