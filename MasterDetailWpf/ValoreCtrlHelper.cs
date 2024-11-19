using MasterDetailView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace MasterDetailWpf
{
    public static class ValoreCtrlHelper
    {
        /// <summary>
        /// Scopo: forzare l'update della sorgente prima del cambiamento di selezione se ho il focus su un attributo dei dettagli
        /// </summary>
        /// <param name="frameworkElement"></param>
        public static void ForceUpdateSource(IInputElement inputElement)
        {
            FrameworkElement frameworkElement = inputElement as FrameworkElement;
            if (frameworkElement == null)
                return;

            ValoreView valoreView = frameworkElement.DataContext as ValoreView;
            if (valoreView == null)
                return;
            
            if (frameworkElement is TextBox)
            {
                BindingExpression exp = frameworkElement.GetBindingExpression(TextBox.TextProperty);
                if (exp != null)
                    exp.UpdateSource();
            }

        }
    }
}
