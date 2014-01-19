using HtmlRenderer.WinForms;

namespace HtmlRenderer.Demo.WinForms
{
    partial class DemoForm
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
            this._showGeneratedHtmlCB = new System.Windows.Forms.CheckBox();
            this._openExternalViewButton = new System.Windows.Forms.Button();
            this._toggleWebBrowserButton = new System.Windows.Forms.Button();
            this._runTestButton = new System.Windows.Forms.Button();
            this._samplesTreeView = new System.Windows.Forms.TreeView();
            this._splitContainer2 = new System.Windows.Forms.SplitContainer();
            this._htmlPanel = new HtmlPanel();
            this._splitter = new System.Windows.Forms.Splitter();
            this._webBrowser = new System.Windows.Forms.WebBrowser();
            this._reloadColorsLink = new System.Windows.Forms.LinkLabel();
            this._htmlEditor = new System.Windows.Forms.RichTextBox();
            this._htmlToolTip = new HtmlToolTip();
            this._splitContainer1.Panel1.SuspendLayout();
            this._splitContainer1.Panel2.SuspendLayout();
            this._splitContainer1.SuspendLayout();
            this._splitContainer2.Panel1.SuspendLayout();
            this._splitContainer2.Panel2.SuspendLayout();
            this._splitContainer2.SuspendLayout();
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
            this._splitContainer1.Panel1.Controls.Add(this._showGeneratedHtmlCB);
            this._splitContainer1.Panel1.Controls.Add(this._openExternalViewButton);
            this._splitContainer1.Panel1.Controls.Add(this._toggleWebBrowserButton);
            this._splitContainer1.Panel1.Controls.Add(this._runTestButton);
            this._splitContainer1.Panel1.Controls.Add(this._samplesTreeView);
            // 
            // _splitContainer1.Panel2
            // 
            this._splitContainer1.Panel2.Controls.Add(this._splitContainer2);
            this._splitContainer1.Size = new System.Drawing.Size(667, 439);
            this._splitContainer1.SplitterDistance = 146;
            this._splitContainer1.TabIndex = 0;
            this._splitContainer1.TabStop = false;
            // 
            // _showGeneratedHtmlCB
            // 
            this._showGeneratedHtmlCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this._showGeneratedHtmlCB.AutoSize = true;
            this._showGeneratedHtmlCB.BackColor = System.Drawing.Color.White;
            this._showGeneratedHtmlCB.Location = new System.Drawing.Point(6, 330);
            this._showGeneratedHtmlCB.Name = "_showGeneratedHtmlCB";
            this._showGeneratedHtmlCB.Size = new System.Drawing.Size(137, 17);
            this._showGeneratedHtmlCB.TabIndex = 16;
            this._showGeneratedHtmlCB.Text = "Show generated HTML";
            this._showGeneratedHtmlCB.UseVisualStyleBackColor = false;
            this._showGeneratedHtmlCB.CheckedChanged += new System.EventHandler(this.OnShowGeneratedHtmlCheckedChanged);
            // 
            // _openExternalViewButton
            // 
            this._openExternalViewButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._openExternalViewButton.Location = new System.Drawing.Point(4, 382);
            this._openExternalViewButton.Name = "_openExternalViewButton";
            this._openExternalViewButton.Size = new System.Drawing.Size(138, 23);
            this._openExternalViewButton.TabIndex = 13;
            this._openExternalViewButton.TabStop = false;
            this._openExternalViewButton.Text = "Open External View";
            this._openExternalViewButton.UseVisualStyleBackColor = true;
            this._openExternalViewButton.Click += new System.EventHandler(this.OnOpenExternalViewButtonClick);
            // 
            // _toggleWebBrowserButton
            // 
            this._toggleWebBrowserButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._toggleWebBrowserButton.Location = new System.Drawing.Point(4, 353);
            this._toggleWebBrowserButton.Name = "_toggleWebBrowserButton";
            this._toggleWebBrowserButton.Size = new System.Drawing.Size(138, 23);
            this._toggleWebBrowserButton.TabIndex = 13;
            this._toggleWebBrowserButton.TabStop = false;
            this._toggleWebBrowserButton.Text = "Show IE View";
            this._toggleWebBrowserButton.UseVisualStyleBackColor = true;
            this._toggleWebBrowserButton.Click += new System.EventHandler(this.OnToggleWebBrowserButton_Click);
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
            this._runTestButton.Text = "Run Performance Test";
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
            // _splitContainer2
            // 
            this._splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this._splitContainer2.Location = new System.Drawing.Point(0, 0);
            this._splitContainer2.Name = "_splitContainer2";
            this._splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // _splitContainer2.Panel1
            // 
            this._splitContainer2.Panel1.Controls.Add(this._htmlPanel);
            this._splitContainer2.Panel1.Controls.Add(this._splitter);
            this._splitContainer2.Panel1.Controls.Add(this._webBrowser);
            // 
            // _splitContainer2.Panel2
            // 
            this._splitContainer2.Panel2.Controls.Add(this._reloadColorsLink);
            this._splitContainer2.Panel2.Controls.Add(this._htmlEditor);
            this._splitContainer2.Size = new System.Drawing.Size(517, 439);
            this._splitContainer2.SplitterDistance = 354;
            this._splitContainer2.TabIndex = 13;
            this._splitContainer2.TabStop = false;
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
            this._htmlPanel.Size = new System.Drawing.Size(273, 354);
            this._htmlPanel.TabIndex = 8;
            // 
            // _splitter
            // 
            this._splitter.Dock = System.Windows.Forms.DockStyle.Right;
            this._splitter.Location = new System.Drawing.Point(273, 0);
            this._splitter.Name = "_splitter";
            this._splitter.Size = new System.Drawing.Size(3, 354);
            this._splitter.TabIndex = 9;
            this._splitter.TabStop = false;
            this._splitter.Visible = false;
            // 
            // _webBrowser
            // 
            this._webBrowser.Dock = System.Windows.Forms.DockStyle.Right;
            this._webBrowser.Location = new System.Drawing.Point(276, 0);
            this._webBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this._webBrowser.Name = "_webBrowser";
            this._webBrowser.Size = new System.Drawing.Size(241, 354);
            this._webBrowser.TabIndex = 7;
            this._webBrowser.Visible = false;
            // 
            // _reloadColorsLink
            // 
            this._reloadColorsLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this._reloadColorsLink.AutoSize = true;
            this._reloadColorsLink.BackColor = System.Drawing.Color.White;
            this._reloadColorsLink.Location = new System.Drawing.Point(454, 65);
            this._reloadColorsLink.Name = "_reloadColorsLink";
            this._reloadColorsLink.Size = new System.Drawing.Size(44, 13);
            this._reloadColorsLink.TabIndex = 8;
            this._reloadColorsLink.TabStop = true;
            this._reloadColorsLink.Text = "Refresh";
            this._reloadColorsLink.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnReloadColorsLinkClicked);
            // 
            // _htmlEditor
            // 
            this._htmlEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this._htmlEditor.Location = new System.Drawing.Point(0, 0);
            this._htmlEditor.Name = "_htmlEditor";
            this._htmlEditor.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this._htmlEditor.Size = new System.Drawing.Size(517, 81);
            this._htmlEditor.TabIndex = 7;
            this._htmlEditor.Text = "";
            this._htmlEditor.TextChanged += new System.EventHandler(this.OnHtmlEditorTextChanged);
            // 
            // _htmlToolTip
            // 
            this._htmlToolTip.AllowLinksHandling = true;
            this._htmlToolTip.AutoPopDelay = 15000;
            this._htmlToolTip.BaseStylesheet = null;
            this._htmlToolTip.InitialDelay = 500;
            this._htmlToolTip.MaximumSize = new System.Drawing.Size(0, 0);
            this._htmlToolTip.OwnerDraw = true;
            this._htmlToolTip.ReshowDelay = 100;
            // 
            // DemoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(191)))), ((int)(((byte)(219)))), ((int)(((byte)(255)))));
            this.ClientSize = new System.Drawing.Size(675, 447);
            this.Controls.Add(this._splitContainer1);
            this.KeyPreview = true;
            this.Name = "DemoForm";
            this.Padding = new System.Windows.Forms.Padding(4);
            this.Text = "HTML Renderer Demo";
            this._splitContainer1.Panel1.ResumeLayout(false);
            this._splitContainer1.Panel1.PerformLayout();
            this._splitContainer1.Panel2.ResumeLayout(false);
            this._splitContainer1.ResumeLayout(false);
            this._splitContainer2.Panel1.ResumeLayout(false);
            this._splitContainer2.Panel2.ResumeLayout(false);
            this._splitContainer2.Panel2.PerformLayout();
            this._splitContainer2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer _splitContainer1;
        private System.Windows.Forms.Button _toggleWebBrowserButton;
        private System.Windows.Forms.Button _runTestButton;
        private System.Windows.Forms.TreeView _samplesTreeView;
        private System.Windows.Forms.SplitContainer _splitContainer2;
        private HtmlPanel _htmlPanel;
        private System.Windows.Forms.Splitter _splitter;
        private System.Windows.Forms.WebBrowser _webBrowser;
        private System.Windows.Forms.RichTextBox _htmlEditor;
        private HtmlToolTip _htmlToolTip;
        private System.Windows.Forms.Button _openExternalViewButton;
        private System.Windows.Forms.CheckBox _showGeneratedHtmlCB;
        private System.Windows.Forms.LinkLabel _reloadColorsLink;

    }
}

