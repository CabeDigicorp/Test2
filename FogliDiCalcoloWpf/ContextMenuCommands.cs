using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FogliDiCalcoloWpf
{
    public static class ContextMenuCommands
    {

        #region Paste
        static BaseCommand paste;
        public static BaseCommand Paste
        {
            get
            {
                if (paste == null)
                    paste = new BaseCommand(OnPasteClicked, CanPaste);

                return paste;
            }
        }

        private static bool CanPaste(object obj)
        {
            if (Clipboard.GetDataObject() != null && (Clipboard.GetDataObject() as DataObject).ContainsText())
                return true;
            return false;
        }

        private static void OnPasteClicked(object obj)
        {
            if (obj is GridRecordContextMenuInfo)
            {
                var grid = (obj as GridRecordContextMenuInfo).DataGrid;
                grid.GridCopyPaste.Paste();
            }
        }


        #endregion
    }
}
