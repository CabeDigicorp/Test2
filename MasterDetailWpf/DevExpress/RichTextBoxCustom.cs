using CommonResources;
using DevExpress.Xpf.RichEdit;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Windows;
using DevExpress.XtraRichEdit.API.Native;

namespace MasterDetailWpf
{
    public class RichTextBoxCustom_devExpress : RichEditControl
    {
        public RichTextBoxCustom_devExpress()
        {
            Loaded += RichTextBoxCustom_devExpress_Loaded;
            
        }

        private void RichTextBoxCustom_devExpress_Loaded(object sender, RoutedEventArgs e)
        {
            Views.SimpleView.Padding = new DevExpress.Portable.PortablePadding(0);
            Views.DraftView.Padding = new DevExpress.Portable.PortablePadding(0);
        }
    }
}
