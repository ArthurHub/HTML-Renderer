using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace HtmlRenderer.Demo
{
    class Panel2 : Panel
    {
        /// <param name="levent">A <see cref="T:System.Windows.Forms.LayoutEventArgs"/> that contains the event data. </param>
        protected override void OnLayout(LayoutEventArgs levent)
        {
            //base.OnLayout(levent);
            if( Controls.Count > 0 )
            {
                Controls[0].Width = Width;
                Controls[0].PerformLayout();
                Controls[0].Refresh();
            }
        }
    }
}
