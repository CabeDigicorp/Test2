using DevExpress.Xpf.Core;
using DevExpress.Xpf.Spreadsheet;
using DevExpress.XtraSpreadsheet;
using DevExpress.XtraSpreadsheet.Forms;
using DevExpress.Xpf.Spreadsheet.Forms;
using DevExpress.Xpf.Spreadsheet.Localization;
using DevExpress.Xpf.Spreadsheet.Internal;
using System.Windows.Controls;
using System.Windows.Threading;
using DevExpress.Xpf.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FogliDiCalcoloWpf
{
    public class CustomSpreadSheet : SpreadsheetControl, ISpreadsheetControl
    {
        public CustomSpreadSheet()
        {

        }

        void ISpreadsheetControl.ShowPageSetupForm(PageSetupViewModel viewModel, PageSetupFormInitialTabPage initialTabPage)
        {
            PageSetupControl control = new PageSetupControl(viewModel);
            DialogClosedDelegate onFormClosed = (dialogResult) => {
                if (dialogResult.HasValue && dialogResult.Value)
                {
                    viewModel.ApplyChanges();
                }
            };
            Func<bool> okClick = new Func<bool>(() => { return control.ValidatePageSetupChanges(); });
            ShowModelessDialog(control, onFormClosed, XpfSpreadsheetLocalizer.GetString(SpreadsheetControlStringId.Caption_PageSetupFormTitle), okClick);
        }
        void ShowModelessDialog(UserControl form, DialogClosedDelegate onDialogClosed, string title, Func<bool> onOkClickDelegate = null)
        {
            ModelessDXDialog dialog = new ModelessDXDialog(title, DialogButtons.OkCancel);
            SetupDXDialog(dialog, form, onDialogClosed, onOkClickDelegate);
            dialog.Loaded += Dialog_Loaded;
            ShowModelessDialog(dialog);
        }

        private void Dialog_Loaded(object sender, RoutedEventArgs e)
        {
            DXTabControl tabControl = (DXTabControl)LayoutHelper.FindElementByName((CustomDXDialog)sender, "tabControl");
            if (tabControl != null)
            {
                foreach (DXTabItem Item in tabControl.Items)
                {
                    if (Item.TabIndex != 4)
                        Item.Visibility = Visibility.Collapsed;
                }
                DXTabItem tabItem = (DXTabItem)LayoutHelper.FindElementByName(tabControl, "tabSheet");
                if (tabItem != null)
                {
                    Grid FirstContent = (Grid)tabItem.Content;
                    PageSetupSheetControl pageSetupSheetControl = (PageSetupSheetControl)LayoutHelper.FindElementByType(FirstContent, typeof(PageSetupSheetControl));
                    Grid SecondContent = (Grid)pageSetupSheetControl.Content;
                    PageSetupPrintButtonsControl pageSetupPrintButtonsContro1l = (PageSetupPrintButtonsControl)LayoutHelper.FindElementByType(SecondContent, typeof(PageSetupPrintButtonsControl));
                    pageSetupPrintButtonsContro1l.Visibility = Visibility.Collapsed;
                }
            }

        }

        void SetupDXDialog(CustomDXDialog dialog, UserControl form, DialogClosedDelegate onDialogClosed, Func<bool> onOkClickDelegate = null, bool clearCopiedRanges = true)
        {
            SetupDXWindow(dialog, SizeToContent.WidthAndHeight, WindowStartupLocation.CenterOwner, ResizeMode.NoResize, clearCopiedRanges);
            dialog.Content = form;
            if (onDialogClosed != null)
            {
                dialog.Closed += (s, e) => {
                    onDialogClosed.Invoke(dialog.DialogResult);
                };
            }
            if (onOkClickDelegate != null)
            {
                dialog.OkButtonClick += (s, e) => {
                    e.Cancel = !onOkClickDelegate.Invoke();
                };
            }
        }
        void ShowModelessDialog(ModelessDXDialog dialog)
        {
            dialog.Show();
            Dispatcher.BeginInvoke(new Action(() => dialog.Focus()), DispatcherPriority.Render);
        }
        void SetupDXWindow(DXWindow window, SizeToContent sizeToContent, WindowStartupLocation startupLocation, ResizeMode resizeMode, bool clearCopiedRanges = true)
        {
            window.SizeToContent = sizeToContent;
            window.ResizeMode = resizeMode;
            window.WindowStartupLocation = startupLocation;
            TrySetDialogOwnerWindow(window);
            window.ShowIcon = false;
            ApplyDialogThemeName(window);
            if (clearCopiedRanges)
                DocumentModel.ClearCopiedRange();
        }
        void TrySetDialogOwnerWindow(DXWindow window)
        {
            Window owner = LayoutHelper.FindParentObject<Window>(this);
            if (owner != null)
                window.Owner = owner;
        }
        void ApplyDialogThemeName(DXWindow window)
        {
            string themeName = this.GetValue(ThemeManager.ThemeNameProperty) as string;
            if (!String.IsNullOrEmpty(themeName))
                window.SetValue(ThemeManager.ThemeNameProperty, themeName);
        }
    }
}
