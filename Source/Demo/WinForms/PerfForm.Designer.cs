using TheArtOfDev.HtmlRenderer.WinForms;

namespace TheArtOfDev.HtmlRenderer.Demo.WinForms
{
    partial class PerfForm
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
            this._splitContainer1 = new System.Windows.Forms.SplitContainer();
            this._clearLink = new System.Windows.Forms.LinkLabel();
            this._iterations = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this._runTestButton = new System.Windows.Forms.Button();
            this._samplesTreeView = new System.Windows.Forms.TreeView();
            this._htmlPanel = new HtmlPanel();
            ((System.ComponentModel.ISupportInitialize)(this._splitContainer1)).BeginInit();
            this._splitContainer1.Panel1.SuspendLayout();
            this._splitContainer1.Panel2.SuspendLayout();
            this._splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this._iterations)).BeginInit();
            this.SuspendLayout();
            // 
            // _splitContainer1
            // 
            this._splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this._splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this._splitContainer1.Location = new System.Drawing.Point(4, 4);
            this._splitContainer1.Name = "_splitContainer1";
            // 
            // _splitContainer1.Panel1
            // 
            this._splitContainer1.Panel1.Controls.Add(this._clearLink);
            this._splitContainer1.Panel1.Controls.Add(this._iterations);
            this._splitContainer1.Panel1.Controls.Add(this.label1);
            this._splitContainer1.Panel1.Controls.Add(this._runTestButton);
            this._splitContainer1.Panel1.Controls.Add(this._samplesTreeView);
            // 
            // _splitContainer1.Panel2
            // 
            this._splitContainer1.Panel2.Controls.Add(this._htmlPanel);
            this._splitContainer1.Size = new System.Drawing.Size(667, 439);
            this._splitContainer1.SplitterDistance = 146;
            this._splitContainer1.TabIndex = 0;
            this._splitContainer1.TabStop = false;
            // 
            // _clearLink
            // 
            this._clearLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._clearLink.AutoSize = true;
            this._clearLink.BackColor = System.Drawing.Color.White;
            this._clearLink.Location = new System.Drawing.Point(111, 365);
            this._clearLink.Name = "_clearLink";
            this._clearLink.Size = new System.Drawing.Size(31, 13);
            this._clearLink.TabIndex = 18;
            this._clearLink.TabStop = true;
            this._clearLink.Text = "Clear";
            this._clearLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnClearLinkClicked);
            // 
            // _iterations
            // 
            this._iterations.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._iterations.Location = new System.Drawing.Point(65, 386);
            this._iterations.Name = "_iterations";
            this._iterations.Size = new System.Drawing.Size(77, 20);
            this._iterations.TabIndex = 16;
            this._iterations.Value = new decimal(new int[] {
            12,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(6, 389);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 13);
            this.label1.TabIndex = 17;
            this.label1.Text = "Iterations:";
            // 
            // _runTestButton
            // 
            this._runTestButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._runTestButton.Location = new System.Drawing.Point(4, 411);
            this._runTestButton.Name = "_runTestButton";
            this._runTestButton.Size = new System.Drawing.Size(138, 23);
            this._runTestButton.TabIndex = 15;
            this._runTestButton.TabStop = false;
            this._runTestButton.Text = "Run Test";
            this._runTestButton.UseVisualStyleBackColor = true;
            this._runTestButton.Click += new System.EventHandler(this.OnRunTestButtonClick);
            // 
            // _samplesTreeView
            // 
            this._samplesTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._samplesTreeView.HideSelection = false;
            this._samplesTreeView.Location = new System.Drawing.Point(0, 0);
            this._samplesTreeView.Name = "_samplesTreeView";
            this._samplesTreeView.Size = new System.Drawing.Size(146, 439);
            this._samplesTreeView.TabIndex = 14;
            this._samplesTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.OnSamplesTreeViewAfterSelect);
            // 
            // _htmlPanel
            // 
            this._htmlPanel.AutoScroll = true;
            this._htmlPanel.AvoidGeometryAntialias = false;
            this._htmlPanel.AvoidImagesLateLoading = false;
            this._htmlPanel.BackColor = System.Drawing.SystemColors.Window;
            this._htmlPanel.BaseStylesheet = null;
            this._htmlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._htmlPanel.Location = new System.Drawing.Point(0, 0);
            this._htmlPanel.Name = "_htmlPanel";
            this._htmlPanel.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this._htmlPanel.Size = new System.Drawing.Size(517, 439);
            this._htmlPanel.TabIndex = 9;
            // 
            // PerfForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(191)))), ((int)(((byte)(219)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(675, 447);
            this.Controls.Add(this._splitContainer1);
            this.KeyPreview = true;
            this.Name = "PerfForm";
            this.Padding = new System.Windows.Forms.Padding(4);
            this.Text = "HTML Renderer Demo";
            this._splitContainer1.Panel1.ResumeLayout(false);
            this._splitContainer1.Panel1.PerformLayout();
            this._splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._splitContainer1)).EndInit();
            this._splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this._iterations)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer _splitContainer1;
        private System.Windows.Forms.Button _runTestButton;
        private System.Windows.Forms.TreeView _samplesTreeView;
        private HtmlPanel _htmlPanel;
        private System.Windows.Forms.NumericUpDown _iterations;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel _clearLink;

    }
}

