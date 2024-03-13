using TheArtOfDev.HtmlRenderer.WinForms;

namespace TheArtOfDev.HtmlRenderer.Demo.WinForms
{
    partial class SampleForm
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
            this._htmlToolTip = new TheArtOfDev.HtmlRenderer.WinForms.HtmlToolTip();
            this._changeTooltipButton = new System.Windows.Forms.Button();
            this._splitContainer = new System.Windows.Forms.SplitContainer();
            this._htmlPanel = new TheArtOfDev.HtmlRenderer.WinForms.HtmlPanel();
            this.label2 = new System.Windows.Forms.Label();
            this._htmlLabelHostingPanel = new System.Windows.Forms.Panel();
            this._htmlLabel = new TheArtOfDev.HtmlRenderer.WinForms.HtmlLabel();
            this.label1 = new System.Windows.Forms.Label();
            this._pGrid = new System.Windows.Forms.PropertyGrid();
            this._splitContainer.Panel1.SuspendLayout();
            this._splitContainer.Panel2.SuspendLayout();
            this._splitContainer.SuspendLayout();
            this._htmlLabelHostingPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // _htmlToolTip
            // 
            this._htmlToolTip.BaseStylesheet = null;
            this._htmlToolTip.MaximumSize = new System.Drawing.Size(0, 0);
            this._htmlToolTip.OwnerDraw = true;
            this._htmlToolTip.TooltipCssClass = "htmltooltip";
            // 
            // _changeTooltipButton
            // 
            this._changeTooltipButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._changeTooltipButton.Location = new System.Drawing.Point(7, 461);
            this._changeTooltipButton.Name = "_changeTooltipButton";
            this._changeTooltipButton.Size = new System.Drawing.Size(407, 23);
            this._changeTooltipButton.TabIndex = 11;
            this._changeTooltipButton.Text = "Click me to change tooltip";
            this._htmlToolTip.SetToolTip(this._changeTooltipButton, "When you click this button, this tooltip will be replaced for the text of the <b>" +
        "HtmlLabel</b>");
            this._changeTooltipButton.UseVisualStyleBackColor = true;
            this._changeTooltipButton.Click += new System.EventHandler(this.OnButtonClick);
            // 
            // _splitContainer
            // 
            this._splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this._splitContainer.Location = new System.Drawing.Point(0, 0);
            this._splitContainer.Name = "_splitContainer";
            // 
            // _splitContainer.Panel1
            // 
            this._splitContainer.Panel1.Controls.Add(this._changeTooltipButton);
            this._splitContainer.Panel1.Controls.Add(this._htmlPanel);
            this._splitContainer.Panel1.Controls.Add(this.label2);
            this._splitContainer.Panel1.Controls.Add(this._htmlLabelHostingPanel);
            this._splitContainer.Panel1.Controls.Add(this.label1);
            // 
            // _splitContainer.Panel2
            // 
            this._splitContainer.Panel2.Controls.Add(this._pGrid);
            this._splitContainer.Size = new System.Drawing.Size(719, 496);
            this._splitContainer.SplitterDistance = 422;
            this._splitContainer.TabIndex = 7;
            // 
            // _htmlPanel
            // 
            this._htmlPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._htmlPanel.AutoScroll = true;
            this._htmlPanel.BackColor = System.Drawing.SystemColors.Window;
            this._htmlPanel.BaseStylesheet = null;
            this._htmlPanel.Cursor = System.Windows.Forms.Cursors.IBeam;
            this._htmlPanel.Location = new System.Drawing.Point(7, 221);
            this._htmlPanel.Name = "_htmlPanel";
            this._htmlPanel.Size = new System.Drawing.Size(407, 225);
            this._htmlPanel.TabIndex = 10;
            this._htmlPanel.Text = null;
            this._htmlPanel.UseSystemCursors = true;
            this._htmlPanel.Click += new System.EventHandler(this.OnHtmlPanelClick);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 205);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "HtmlPanel";
            // 
            // _htmlLabelHostingPanel
            // 
            this._htmlLabelHostingPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._htmlLabelHostingPanel.BackColor = System.Drawing.Color.Transparent;
            this._htmlLabelHostingPanel.Controls.Add(this._htmlLabel);
            this._htmlLabelHostingPanel.Location = new System.Drawing.Point(7, 23);
            this._htmlLabelHostingPanel.Name = "_htmlLabelHostingPanel";
            this._htmlLabelHostingPanel.Size = new System.Drawing.Size(407, 169);
            this._htmlLabelHostingPanel.TabIndex = 8;
            this._htmlLabelHostingPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.OnHtmlLabelHostingPanelPaint);
            // 
            // _htmlLabel
            // 
            this._htmlLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this._htmlLabel.AutoSize = false;
            this._htmlLabel.AutoSizeHeightOnly = true;
            this._htmlLabel.BackColor = System.Drawing.Color.Transparent;
            this._htmlLabel.BaseStylesheet = null;
            this._htmlLabel.Location = new System.Drawing.Point(10, 11);
            this._htmlLabel.Name = "_htmlLabel";
            this._htmlLabel.Size = new System.Drawing.Size(385, 0);
            this._htmlLabel.TabIndex = 0;
            this._htmlLabel.Text = null;
            this._htmlLabel.Click += new System.EventHandler(this.OnHtmlLabelClick);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(54, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "HtmlLabel";
            // 
            // _pGrid
            // 
            this._pGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this._pGrid.Location = new System.Drawing.Point(0, 0);
            this._pGrid.Name = "_pGrid";
            this._pGrid.Size = new System.Drawing.Size(293, 496);
            this._pGrid.TabIndex = 3;
            this._pGrid.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.OnPropertyValueChanged);
            // 
            // SampleForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(719, 496);
            this.Controls.Add(this._splitContainer);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SampleForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Sample Form";
            this._splitContainer.Panel1.ResumeLayout(false);
            this._splitContainer.Panel1.PerformLayout();
            this._splitContainer.Panel2.ResumeLayout(false);
            this._splitContainer.ResumeLayout(false);
            this._htmlLabelHostingPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private HtmlToolTip _htmlToolTip;
        private System.Windows.Forms.SplitContainer _splitContainer;
        private System.Windows.Forms.Button _changeTooltipButton;
        private HtmlPanel _htmlPanel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel _htmlLabelHostingPanel;
        private HtmlLabel _htmlLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PropertyGrid _pGrid;
    }
}