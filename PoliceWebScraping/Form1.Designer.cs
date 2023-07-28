namespace PoliceWebScraping
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            webView21 = new Microsoft.Web.WebView2.WinForms.WebView2();
            btStep = new Button();
            pbCaptcha = new PictureBox();
            txtpbCaptcha = new TextBox();
            splitContainer1 = new SplitContainer();
            txtDescription = new TextBox();
            cbViolationSelector = new ComboBox();
            groupBox1 = new GroupBox();
            btUpload = new Button();
            fbdMediaSelect = new FolderBrowserDialog();
            statusStrip1 = new StatusStrip();
            toolStripDropDownButton1 = new ToolStripDropDownButton();
            重整ToolStripMenuItem = new ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)webView21).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pbCaptcha).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // webView21
            // 
            webView21.AllowExternalDrop = true;
            webView21.CreationProperties = null;
            webView21.DefaultBackgroundColor = Color.White;
            webView21.Dock = DockStyle.Fill;
            webView21.Location = new Point(0, 0);
            webView21.Name = "webView21";
            webView21.Size = new Size(1525, 1208);
            webView21.TabIndex = 0;
            webView21.ZoomFactor = 1D;
            // 
            // btStep
            // 
            btStep.Font = new Font("Microsoft JhengHei UI", 36F, FontStyle.Regular, GraphicsUnit.Point);
            btStep.Location = new Point(25, 1028);
            btStep.Name = "btStep";
            btStep.Size = new Size(281, 147);
            btStep.TabIndex = 1;
            btStep.Text = "填單";
            btStep.UseVisualStyleBackColor = true;
            btStep.Click += btStep_ClickAsync;
            // 
            // pbCaptcha
            // 
            pbCaptcha.Image = (Image)resources.GetObject("pbCaptcha.Image");
            pbCaptcha.Location = new Point(25, 37);
            pbCaptcha.Name = "pbCaptcha";
            pbCaptcha.Size = new Size(281, 87);
            pbCaptcha.SizeMode = PictureBoxSizeMode.Zoom;
            pbCaptcha.TabIndex = 3;
            pbCaptcha.TabStop = false;
            // 
            // txtpbCaptcha
            // 
            txtpbCaptcha.Font = new Font("Microsoft JhengHei UI", 24F, FontStyle.Regular, GraphicsUnit.Point);
            txtpbCaptcha.Location = new Point(25, 158);
            txtpbCaptcha.Name = "txtpbCaptcha";
            txtpbCaptcha.PlaceholderText = "驗證碼";
            txtpbCaptcha.ReadOnly = true;
            txtpbCaptcha.Size = new Size(281, 68);
            txtpbCaptcha.TabIndex = 5;
            txtpbCaptcha.TextAlign = HorizontalAlignment.Center;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(webView21);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(txtDescription);
            splitContainer1.Panel2.Controls.Add(cbViolationSelector);
            splitContainer1.Panel2.Controls.Add(groupBox1);
            splitContainer1.Panel2.Controls.Add(btUpload);
            splitContainer1.Panel2.Controls.Add(btStep);
            splitContainer1.Panel2.Controls.Add(txtpbCaptcha);
            splitContainer1.Panel2.Controls.Add(pbCaptcha);
            splitContainer1.Size = new Size(1857, 1208);
            splitContainer1.SplitterDistance = 1525;
            splitContainer1.TabIndex = 7;
            splitContainer1.SplitterMoved += splitContainer1_SplitterMoved;
            // 
            // txtDescription
            // 
            txtDescription.Font = new Font("Microsoft JhengHei UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            txtDescription.Location = new Point(25, 667);
            txtDescription.Multiline = true;
            txtDescription.Name = "txtDescription";
            txtDescription.PlaceholderText = "輸入描述";
            txtDescription.Size = new Size(281, 344);
            txtDescription.TabIndex = 10;
            // 
            // cbViolationSelector
            // 
            cbViolationSelector.AutoCompleteSource = AutoCompleteSource.ListItems;
            cbViolationSelector.DropDownStyle = ComboBoxStyle.DropDownList;
            cbViolationSelector.FlatStyle = FlatStyle.Popup;
            cbViolationSelector.Font = new Font("Microsoft JhengHei UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
            cbViolationSelector.FormattingEnabled = true;
            cbViolationSelector.Location = new Point(25, 612);
            cbViolationSelector.Name = "cbViolationSelector";
            cbViolationSelector.Size = new Size(281, 36);
            cbViolationSelector.TabIndex = 9;
            cbViolationSelector.DropDown += ComboBox_DropDown;
            // 
            // groupBox1
            // 
            groupBox1.AutoSize = true;
            groupBox1.BackColor = SystemColors.Info;
            groupBox1.Font = new Font("Microsoft JhengHei UI", 8F, FontStyle.Regular, GraphicsUnit.Point);
            groupBox1.Location = new Point(25, 414);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(281, 171);
            groupBox1.TabIndex = 8;
            groupBox1.TabStop = false;
            groupBox1.Text = "Car Number";
            // 
            // btUpload
            // 
            btUpload.Font = new Font("Microsoft JhengHei UI", 28F, FontStyle.Regular, GraphicsUnit.Point);
            btUpload.Location = new Point(25, 241);
            btUpload.Name = "btUpload";
            btUpload.Size = new Size(281, 147);
            btUpload.TabIndex = 7;
            btUpload.Text = "上傳";
            btUpload.UseVisualStyleBackColor = true;
            btUpload.Click += btUpload_Click;
            // 
            // statusStrip1
            // 
            statusStrip1.ImageScalingSize = new Size(24, 24);
            statusStrip1.Items.AddRange(new ToolStripItem[] { toolStripDropDownButton1 });
            statusStrip1.Location = new Point(0, 1178);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(1857, 30);
            statusStrip1.TabIndex = 8;
            statusStrip1.Text = "statusStrip1";
            // 
            // toolStripDropDownButton1
            // 
            toolStripDropDownButton1.DisplayStyle = ToolStripItemDisplayStyle.Text;
            toolStripDropDownButton1.DropDownItems.AddRange(new ToolStripItem[] { 重整ToolStripMenuItem });
            toolStripDropDownButton1.Image = (Image)resources.GetObject("toolStripDropDownButton1.Image");
            toolStripDropDownButton1.ImageTransparentColor = Color.Magenta;
            toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            toolStripDropDownButton1.Size = new Size(64, 27);
            toolStripDropDownButton1.Text = "選項";
            // 
            // 重整ToolStripMenuItem
            // 
            重整ToolStripMenuItem.Name = "重整ToolStripMenuItem";
            重整ToolStripMenuItem.Size = new Size(146, 34);
            重整ToolStripMenuItem.Text = "重整";
            重整ToolStripMenuItem.Click += 重整ToolStripMenuItem_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(11F, 23F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1857, 1208);
            Controls.Add(statusStrip1);
            Controls.Add(splitContainer1);
            Icon = (Icon)resources.GetObject("$this.Icon");
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Whistleblower Management System";
            ((System.ComponentModel.ISupportInitialize)webView21).EndInit();
            ((System.ComponentModel.ISupportInitialize)pbCaptcha).EndInit();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 webView21;
        private Button btStep;
        private PictureBox pbCaptcha;
        private TextBox txtpbCaptcha;
        private SplitContainer splitContainer1;
        private FolderBrowserDialog fbdMediaSelect;
        private Button btUpload;
        private GroupBox groupBox1;
        private StatusStrip statusStrip1;
        private ToolStripDropDownButton toolStripDropDownButton1;
        private ToolStripMenuItem 重整ToolStripMenuItem;
        private ComboBox cbViolationSelector;
        private TextBox txtDescription;
    }
}