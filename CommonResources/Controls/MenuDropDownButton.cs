
using System;
using System.Windows;

namespace CommonResources
{
    /// <summary>
    /// DropDownButton per il menu (si apre e chiude con MouseOver)
    /// </summary>
    public class MenuDropDownButton : Xceed.Wpf.Toolkit.DropDownButton
    //public class MenuDropDownButton : DevExpress.Xpf.Core.DropDownButton
    {
        public MenuDropDownButton()
        {
            MouseMove += MenuDropDownButton_MouseMove;
            
        }

        

        //protected override void OnIsOpenChanged(bool oldValue, bool newValue)
        //{
        //    //Debug.WriteLine(string.Format("Menu Open: {0}", IsFocused));
        //    base.OnIsOpenChanged(oldValue, newValue);
        //}

        private void MenuDropDownButton_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void MenuDropDownButton_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void MenuDropDownButton_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void MenuDropDownButton_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }

        //private void MenuDropDownButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        //{
        //    Xceed.Wpf.Toolkit.DropDownButton dropDownButton = sender as Xceed.Wpf.Toolkit.DropDownButton;
        //    dropDownButton.IsOpen = true;
        //}

        private void MenuDropDownButton_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Xceed.Wpf.Toolkit.DropDownButton dropDownButton = sender as Xceed.Wpf.Toolkit.DropDownButton;
            //DevExpress.Xpf.Core.DropDownButton dropDownButton = sender as DevExpress.Xpf.Core.DropDownButton;

            Point pt = e.GetPosition(dropDownButton);
            bool isOpen = false;

            //Controllo il button principale
            double width = dropDownButton.ActualWidth;
            double height = dropDownButton.ActualHeight + 2;
            isOpen = CheckMousePosition(pt, width, height);

            //Scopo: fare il modo che rimanga aperta quando si clicca sul dropDownButton
            if (dropDownButton.IsOpen && isOpen)
                dropDownButton.IsOpen = false;

            //if (dropDownButton.IsPopupOpen && isOpen)
            //    dropDownButton.IsPopupOpen = false;


            //controllo l'area aperta
            if (dropDownButton.IsOpen && !isOpen)
            //if (dropDownButton.IsPopupOpen && !isOpen)
            {
                FrameworkElement dropDownButtonContent = dropDownButton.DropDownContent as FrameworkElement;
                //FrameworkElement dropDownButtonContent = dropDownButton.PopupContent as FrameworkElement;
                if (dropDownButtonContent != null)
                {
                    pt = e.GetPosition(dropDownButtonContent);

                    width = dropDownButtonContent.ActualWidth;
                    height = dropDownButtonContent.ActualHeight;

                    isOpen = CheckMousePosition(pt, width, height);
                }
            }

            //Debug.WriteLine(string.Format("Menu Open: {0}", isOpen));



            dropDownButton.IsOpen = isOpen;
            //dropDownButton.IsPopupOpen = isOpen;
        }

        private static bool CheckMousePosition(Point pt, double width, double height)
        {
            if ((pt.X < 0.0) || (pt.Y < 0.0) || (pt.X > width) || (pt.Y > height))
                return false;
            else
                return true;
        }
        
    }
}
