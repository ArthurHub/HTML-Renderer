namespace HtmlRenderer.Demo.WinForms
{
    partial class GenerateImageForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GenerateImageForm));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this._useGdiPlusTSB = new System.Windows.Forms.ToolStripButton();
            this._generateImageTSB = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this._backgroundColorTSB = new System.Windows.Forms.ToolStripComboBox();
            this._pictureBox = new System.Windows.Forms.PictureBox();
            this._toolStripLabel = new System.Windows.Forms.ToolStripLabel();
            this._textRenderingHintTSCB = new System.Windows.Forms.ToolStripComboBox();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._pictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._useGdiPlusTSB,
            this._generateImageTSB,
            this.toolStripSeparator1,
            this._toolStripLabel,
            this._backgroundColorTSB,
            this._textRenderingHintTSCB});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(610, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // _useGdiPlusTSB
            // 
            this._useGdiPlusTSB.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._useGdiPlusTSB.Image = ((System.Drawing.Image)(resources.GetObject("_useGdiPlusTSB.Image")));
            this._useGdiPlusTSB.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._useGdiPlusTSB.Name = "_useGdiPlusTSB";
            this._useGdiPlusTSB.Size = new System.Drawing.Size(142, 22);
            this._useGdiPlusTSB.Text = "Use GDI+ Text Rendering";
            this._useGdiPlusTSB.Click += new System.EventHandler(this.OnUseGdiPlus_Click);
            // 
            // _generateImageTSB
            // 
            this._generateImageTSB.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this._generateImageTSB.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this._generateImageTSB.Image = ((System.Drawing.Image)(resources.GetObject("_generateImageTSB.Image")));
            this._generateImageTSB.ImageTransparentColor = System.Drawing.Color.Magenta;
            this._generateImageTSB.Name = "_generateImageTSB";
            this._generateImageTSB.Size = new System.Drawing.Size(76, 22);
            this._generateImageTSB.Text = "Re-Generate";
            this._generateImageTSB.Click += new System.EventHandler(this.OnGenerateImage_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // _backgroundColorTSB
            // 
            this._backgroundColorTSB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._backgroundColorTSB.Name = "_backgroundColorTSB";
            this._backgroundColorTSB.Size = new System.Drawing.Size(121, 25);
            this._backgroundColorTSB.SelectedIndexChanged += new System.EventHandler(this.OnBackgroundColor_SelectedIndexChanged);
            // 
            // _pictureBox
            // 
            this._pictureBox.BackColor = System.Drawing.Color.Transparent;
            this._pictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._pictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pictureBox.Location = new System.Drawing.Point(0, 25);
            this._pictureBox.Name = "_pictureBox";
            this._pictureBox.Size = new System.Drawing.Size(610, 472);
            this._pictureBox.TabIndex = 1;
            this._pictureBox.TabStop = false;
            // 
            // _toolStripLabel
            // 
            this._toolStripLabel.Name = "_toolStripLabel";
            this._toolStripLabel.Size = new System.Drawing.Size(77, 22);
            this._toolStripLabel.Text = "Background: ";
            // 
            // _textRenderingHintTSCB
            // 
            this._textRenderingHintTSCB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._textRenderingHintTSCB.Name = "_textRenderingHintTSCB";
            this._textRenderingHintTSCB.Size = new System.Drawing.Size(121, 25);
            this._textRenderingHintTSCB.Visible = false;
            this._textRenderingHintTSCB.SelectedIndexChanged += new System.EventHandler(this._textRenderingHintTSCB_SelectedIndexChanged);
            // 
            // GenerateImageForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(610, 497);
            this.Controls.Add(this._pictureBox);
            this.Controls.Add(this.toolStrip1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GenerateImageForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Generate Image";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this._pictureBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.PictureBox _pictureBox;
        private System.Windows.Forms.ToolStripButton _useGdiPlusTSB;
        private System.Windows.Forms.ToolStripButton _generateImageTSB;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripComboBox _backgroundColorTSB;
        private System.Windows.Forms.ToolStripLabel _toolStripLabel;
        private System.Windows.Forms.ToolStripComboBox _textRenderingHintTSCB;
    }
}