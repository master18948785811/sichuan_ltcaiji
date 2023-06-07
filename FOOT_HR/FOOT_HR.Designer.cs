namespace FOOT_HR
{
    partial class FOOT_HR
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
            this.panelfoot = new System.Windows.Forms.Panel();
            this.foot_Upload = new System.Windows.Forms.Button();
            this.Photograph = new System.Windows.Forms.Button();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.Column1 = new System.Windows.Forms.DataGridViewImageColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column3 = new System.Windows.Forms.DataGridViewButtonColumn();
            this.Column6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ShowFoot = new System.Windows.Forms.PictureBox();
            this.foot_Position = new System.Windows.Forms.ComboBox();
            this.label27 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ShowFoot)).BeginInit();
            this.SuspendLayout();
            // 
            // panelfoot
            // 
            this.panelfoot.BackColor = System.Drawing.Color.White;
            this.panelfoot.Location = new System.Drawing.Point(3, 3);
            this.panelfoot.Name = "panelfoot";
            this.panelfoot.Size = new System.Drawing.Size(427, 608);
            this.panelfoot.TabIndex = 361;
            // 
            // foot_Upload
            // 
            this.foot_Upload.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(78)))), ((int)(((byte)(208)))));
            this.foot_Upload.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.foot_Upload.ForeColor = System.Drawing.Color.White;
            this.foot_Upload.Location = new System.Drawing.Point(478, 9);
            this.foot_Upload.Name = "foot_Upload";
            this.foot_Upload.Size = new System.Drawing.Size(113, 23);
            this.foot_Upload.TabIndex = 360;
            this.foot_Upload.Text = "上传市局";
            this.foot_Upload.UseVisualStyleBackColor = false;
            this.foot_Upload.Click += new System.EventHandler(this.foot_Upload_Click);
            // 
            // Photograph
            // 
            this.Photograph.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(78)))), ((int)(((byte)(208)))));
            this.Photograph.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(78)))), ((int)(((byte)(208)))));
            this.Photograph.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Photograph.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Photograph.ForeColor = System.Drawing.Color.White;
            this.Photograph.Location = new System.Drawing.Point(176, 619);
            this.Photograph.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.Photograph.Name = "Photograph";
            this.Photograph.Size = new System.Drawing.Size(80, 28);
            this.Photograph.TabIndex = 359;
            this.Photograph.Text = "拍照";
            this.Photograph.UseVisualStyleBackColor = false;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.BackgroundColor = System.Drawing.Color.White;
            this.dataGridView1.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("宋体", 9F);
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle4;
            this.dataGridView1.ColumnHeadersHeight = 30;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Column1,
            this.Column2,
            this.Column4,
            this.Column3,
            this.Column6});
            dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle6.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle6.Font = new System.Drawing.Font("宋体", 9F);
            dataGridViewCellStyle6.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle6.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle6.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle6.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGridView1.DefaultCellStyle = dataGridViewCellStyle6;
            this.dataGridView1.Location = new System.Drawing.Point(432, 56);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowTemplate.Height = 30;
            this.dataGridView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dataGridView1.Size = new System.Drawing.Size(509, 555);
            this.dataGridView1.TabIndex = 355;
            // 
            // Column1
            // 
            this.Column1.HeaderText = "足迹图片";
            this.Column1.ImageLayout = System.Windows.Forms.DataGridViewImageCellLayout.Zoom;
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.Column1.Width = 155;
            // 
            // Column2
            // 
            this.Column2.HeaderText = "足迹部位";
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            this.Column2.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.Column2.Width = 80;
            // 
            // Column4
            // 
            this.Column4.HeaderText = "图片名称";
            this.Column4.Name = "Column4";
            // 
            // Column3
            // 
            this.Column3.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            dataGridViewCellStyle5.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle5.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(78)))), ((int)(((byte)(208)))));
            dataGridViewCellStyle5.ForeColor = System.Drawing.Color.White;
            this.Column3.DefaultCellStyle = dataGridViewCellStyle5;
            this.Column3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Column3.HeaderText = "删除";
            this.Column3.Name = "Column3";
            this.Column3.Text = "删除";
            this.Column3.UseColumnTextForButtonValue = true;
            // 
            // Column6
            // 
            this.Column6.HeaderText = "足迹部位代码";
            this.Column6.Name = "Column6";
            this.Column6.Visible = false;
            // 
            // ShowFoot
            // 
            this.ShowFoot.BackColor = System.Drawing.Color.White;
            this.ShowFoot.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ShowFoot.Location = new System.Drawing.Point(943, 3);
            this.ShowFoot.Name = "ShowFoot";
            this.ShowFoot.Size = new System.Drawing.Size(326, 608);
            this.ShowFoot.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.ShowFoot.TabIndex = 358;
            this.ShowFoot.TabStop = false;
            // 
            // foot_Position
            // 
            this.foot_Position.FormattingEnabled = true;
            this.foot_Position.Items.AddRange(new object[] {
            "左脚",
            "右脚",
            "赤足左脚",
            "赤足右脚"});
            this.foot_Position.Location = new System.Drawing.Point(674, 10);
            this.foot_Position.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.foot_Position.Name = "foot_Position";
            this.foot_Position.Size = new System.Drawing.Size(219, 20);
            this.foot_Position.TabIndex = 357;
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(603, 14);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(65, 12);
            this.label27.TabIndex = 356;
            this.label27.Text = "足迹部位：";
            // 
            // FOOT_HR
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelfoot);
            this.Controls.Add(this.foot_Upload);
            this.Controls.Add(this.Photograph);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.ShowFoot);
            this.Controls.Add(this.foot_Position);
            this.Controls.Add(this.label27);
            this.Name = "FOOT_HR";
            this.Size = new System.Drawing.Size(1280, 900);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ShowFoot)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelfoot;
        private System.Windows.Forms.Button foot_Upload;
        private System.Windows.Forms.Button Photograph;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.DataGridViewImageColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column4;
        private System.Windows.Forms.DataGridViewButtonColumn Column3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column6;
        private System.Windows.Forms.PictureBox ShowFoot;
        private System.Windows.Forms.ComboBox foot_Position;
        private System.Windows.Forms.Label label27;
    }
}
