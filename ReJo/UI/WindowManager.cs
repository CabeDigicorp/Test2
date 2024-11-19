using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ReJo.UI
{
    internal class WindowManager
    {
        static FiltersTagWnd _FiltersTagWnd = null;
        static RulesWnd _RulesWnd = null;

        internal static FiltersTagWnd CreateFiltersTagWnd()
        {
            _FiltersTagWnd = new FiltersTagWnd();
            return _FiltersTagWnd;
        }

        internal static RulesWnd CreateRulesWnd()
        {
            _RulesWnd = new RulesWnd();
            return _RulesWnd;
        }

        internal static void CloseModelessWindows()
        {

            if (_RulesWnd != null && _RulesWnd.IsVisible)
                _RulesWnd.Close();

            _FiltersTagWnd = null;
            _RulesWnd = null;

            FiltersPane.This.Show(false);
        }

        internal static void CloseModalWindows()
        {
            if (_FiltersTagWnd != null && _FiltersTagWnd.IsVisible)
            {

                Dispatcher dispatcher = _FiltersTagWnd.Dispatcher;

                dispatcher.Invoke(() =>
                {
                    _FiltersTagWnd.Close();
                });
            }

        }

    }
}
