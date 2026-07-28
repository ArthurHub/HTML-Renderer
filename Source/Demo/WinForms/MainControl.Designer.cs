namespace TheArtOfDev.HtmlRenderer.Demo.WinForms
{
    partial class MainControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this._splitContainer1 = new System.Windows.Forms.SplitContainer();
            this._samplesTreeView = new System.Windows.Forms.TreeView();
            this._splitContainer2 = new System.Windows.Forms.SplitContainer();
            this._splitContainer3 = new System.Windows.Forms.SplitContainer();
            this._htmlPanel = new HtmlRenderer.WinForms.HtmlPanel();
            this._splitter = new System.Windows.Forms.Splitter();
            this._webBrowser = new System.Windows.Forms.WebBrowser();
            this._inputHtmlLabel = new System.Windows.Forms.Label();
            this._reloadColorsLink = new System.Windows.Forms.LinkLabel();
            this._htmlEditor = new System.Windows.Forms.RichTextBox();
            this._fixedHtmlLabel = new System.Windows.Forms.Label();
            this._fixedHtmlEditor = new System.Windows.Forms.RichTextBox();
            this._htmlToolTip = new HtmlRenderer.WinForms.HtmlToolTip();
            this._splitContainer1.Panel1.SuspendLayout();
            this._splitContainer1.Panel2.SuspendLayout();
            this._splitContainer1.SuspendLayout();
            this._splitContainer2.Panel1.SuspendLayout();
            this._splitContainer2.Panel2.SuspendLayout();
            this._splitContainer2.SuspendLayout();
            this._splitContainer3.Panel1.SuspendLayout();
            this._splitContainer3.Panel2.SuspendLayout();
            this._splitContainer3.SuspendLayout();
            this.SuspendLayout();
            // 
            // _splitContainer1
            // 
            this._splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this._splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this._splitContainer1.Location = new System.Drawing.Point(0, 0);
            this._splitContainer1.Name = "_splitContainer1";
            // 
            // _splitContainer1.Panel1
            // 
            this._splitContainer1.Panel1.Controls.Add(this._samplesTreeView);
            // 
            // _splitContainer1.Panel2
            // 
            this._splitContainer1.Panel2.Controls.Add(this._splitContainer2);
            this._splitContainer1.Size = new System.Drawing.Size(879, 593);
            this._splitContainer1.SplitterDistance = 146;
            this._splitContainer1.TabIndex = 1;
            this._splitContainer1.TabStop = false;
            // 
            // _samplesTreeView
            // 
            this._samplesTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this._samplesTreeView.HideSelection = false;
            this._samplesTreeView.Location = new System.Drawing.Point(0, 0);
            this._samplesTreeView.Name = "_samplesTreeView";
            this._samplesTreeView.Size = new System.Drawing.Size(146, 593);
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
            this._splitContainer2.Panel2.Controls.Add(this._splitContainer3);
            this._splitContainer2.Size = new System.Drawing.Size(729, 593);
            this._splitContainer2.SplitterDistance = 476;
            this._splitContainer2.TabIndex = 13;
            this._splitContainer2.TabStop = false;
            // 
            // _splitContainer3
            // 
            this._splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this._splitContainer3.Location = new System.Drawing.Point(0, 0);
            this._splitContainer3.Name = "_splitContainer3";
            // 
            // _splitContainer3.Panel1
            // 
            this._splitContainer3.Panel1.Controls.Add(this._htmlEditor);
            this._splitContainer3.Panel1.Controls.Add(this._reloadColorsLink);
            this._splitContainer3.Panel1.Controls.Add(this._inputHtmlLabel);
            // 
            // _splitContainer3.Panel2
            // 
            this._splitContainer3.Panel2.Controls.Add(this._fixedHtmlEditor);
            this._splitContainer3.Panel2.Controls.Add(this._fixedHtmlLabel);
            this._splitContainer3.Size = new System.Drawing.Size(729, 113);
            this._splitContainer3.Panel1MinSize = 150;
            this._splitContainer3.Panel2MinSize = 150;
            this._splitContainer3.SplitterDistance = 364;
            this._splitContainer3.TabIndex = 9;
            this._splitContainer3.TabStop = false;
            // 
            // _htmlPanel
            // 
            this._htmlPanel.AutoScroll = true;
            this._htmlPanel.BackColor = System.Drawing.SystemColors.Window;
            this._htmlPanel.BaseStylesheet = null;
            this._htmlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this._htmlPanel.Location = new System.Drawing.Point(0, 0);
            this._htmlPanel.Name = "_htmlPanel";
            this._htmlPanel.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this._htmlPanel.Size = new System.Drawing.Size(485, 476);
            this._htmlPanel.TabIndex = 8;
            this._htmlPanel.Text = null;
            this._htmlPanel.UseSystemCursors = true;
            // 
            // _splitter
            // 
            this._splitter.Dock = System.Windows.Forms.DockStyle.Right;
            this._splitter.Location = new System.Drawing.Point(485, 0);
            this._splitter.Name = "_splitter";
            this._splitter.Size = new System.Drawing.Size(3, 476);
            this._splitter.TabIndex = 9;
            this._splitter.TabStop = false;
            this._splitter.Visible = false;
            // 
            // _webBrowser
            // 
            this._webBrowser.Dock = System.Windows.Forms.DockStyle.Right;
            this._webBrowser.Location = new System.Drawing.Point(488, 0);
            this._webBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this._webBrowser.Name = "_webBrowser";
            this._webBrowser.Size = new System.Drawing.Size(241, 476);
            this._webBrowser.TabIndex = 7;
            this._webBrowser.Visible = false;
            // 
            // _inputHtmlLabel
            // 
            this._inputHtmlLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this._inputHtmlLabel.Location = new System.Drawing.Point(0, 0);
            this._inputHtmlLabel.Name = "_inputHtmlLabel";
            this._inputHtmlLabel.Size = new System.Drawing.Size(364, 24);
            this._inputHtmlLabel.TabIndex = 9;
            this._inputHtmlLabel.Text = "Input HTML";
            this._inputHtmlLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _reloadColorsLink
            // 
            this._reloadColorsLink.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this._reloadColorsLink.AutoSize = true;
            this._reloadColorsLink.BackColor = System.Drawing.Color.White;
            this._reloadColorsLink.Location = new System.Drawing.Point(277, 6);
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
            this._htmlEditor.Location = new System.Drawing.Point(0, 24);
            this._htmlEditor.Name = "_htmlEditor";
            this._htmlEditor.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Both;
            this._htmlEditor.Size = new System.Drawing.Size(364, 89);
            this._htmlEditor.TabIndex = 7;
            this._htmlEditor.Text = "";
            this._htmlEditor.WordWrap = false;
            this._htmlEditor.TextChanged += new System.EventHandler(this.OnHtmlEditorTextChanged);
            // 
            // _fixedHtmlLabel
            // 
            this._fixedHtmlLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this._fixedHtmlLabel.Location = new System.Drawing.Point(0, 0);
            this._fixedHtmlLabel.Name = "_fixedHtmlLabel";
            this._fixedHtmlLabel.Size = new System.Drawing.Size(361, 24);
            this._fixedHtmlLabel.TabIndex = 10;
            this._fixedHtmlLabel.Text = "Fixed HTML";
            this._fixedHtmlLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // _fixedHtmlEditor
            // 
            this._fixedHtmlEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this._fixedHtmlEditor.Location = new System.Drawing.Point(0, 24);
            this._fixedHtmlEditor.Name = "_fixedHtmlEditor";
            this._fixedHtmlEditor.ReadOnly = true;
            this._fixedHtmlEditor.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Both;
            this._fixedHtmlEditor.Size = new System.Drawing.Size(361, 89);
            this._fixedHtmlEditor.TabIndex = 11;
            this._fixedHtmlEditor.Text = "";
            this._fixedHtmlEditor.WordWrap = false;
            // 
            // _htmlToolTip
            // 
            this._htmlToolTip.AutoPopDelay = 15000;
            this._htmlToolTip.BaseStylesheet = null;
            this._htmlToolTip.InitialDelay = 500;
            this._htmlToolTip.MaximumSize = new System.Drawing.Size(0, 0);
            this._htmlToolTip.OwnerDraw = true;
            this._htmlToolTip.ReshowDelay = 100;
            this._htmlToolTip.TooltipCssClass = "htmltooltip";
            // 
            // MainControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this._splitContainer1);
            this.Name = "MainControl";
            this.Size = new System.Drawing.Size(879, 593);
            this._splitContainer1.Panel1.ResumeLayout(false);
            this._splitContainer1.Panel2.ResumeLayout(false);
            this._splitContainer1.ResumeLayout(false);
            this._splitContainer2.Panel1.ResumeLayout(false);
            this._splitContainer2.Panel2.ResumeLayout(false);
            this._splitContainer2.ResumeLayout(false);
            this._splitContainer3.Panel1.ResumeLayout(false);
            this._splitContainer3.Panel1.PerformLayout();
            this._splitContainer3.Panel2.ResumeLayout(false);
            this._splitContainer3.Panel2.PerformLayout();
            this._splitContainer3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer _splitContainer1;
        private System.Windows.Forms.TreeView _samplesTreeView;
        private System.Windows.Forms.SplitContainer _splitContainer2;
        private System.Windows.Forms.SplitContainer _splitContainer3;
        private HtmlRenderer.WinForms.HtmlPanel _htmlPanel;
        private System.Windows.Forms.Splitter _splitter;
        private System.Windows.Forms.WebBrowser _webBrowser;
        private System.Windows.Forms.Label _inputHtmlLabel;
        private System.Windows.Forms.LinkLabel _reloadColorsLink;
        private System.Windows.Forms.RichTextBox _htmlEditor;
        private System.Windows.Forms.Label _fixedHtmlLabel;
        private System.Windows.Forms.RichTextBox _fixedHtmlEditor;
        private HtmlRenderer.WinForms.HtmlToolTip _htmlToolTip;
    }
}
