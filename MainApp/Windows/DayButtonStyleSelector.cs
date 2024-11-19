using MasterDetailView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Calendar;

namespace MainApp
{
    public class DayButtonStyleSelector : StyleSelector
    {
        public Style GiorniFestivi { get; set; }
        public Style GiorniModificati { get; set; }
        public Style GiorniModificatiFestivi { get; set; }

        public override Style SelectStyle(object item, DependencyObject container)
        {
            CalendarButtonContent content = item as CalendarButtonContent;
            RadCalendar expander = ParentOfTypeExtensions.ParentOfType<RadCalendar>(container);
            SetAttributoCustomDayView DataContextParent = expander.DataContext as SetAttributoCustomDayView;

            if (content != null)
            {
                if (content.Date.DayOfWeek == DayOfWeek.Monday && content.ButtonType == CalendarButtonType.Date)
                    if (SelectStyle(DataContextParent, content.Date, 0) != null)
                        return SelectStyle(DataContextParent, content.Date, 0);
                if (content.Date.DayOfWeek == DayOfWeek.Tuesday && content.ButtonType == CalendarButtonType.Date)
                    if (SelectStyle(DataContextParent, content.Date, 1) != null)
                        return SelectStyle(DataContextParent, content.Date, 1);
                if (content.Date.DayOfWeek == DayOfWeek.Wednesday && content.ButtonType == CalendarButtonType.Date)
                    if (SelectStyle(DataContextParent, content.Date, 2) != null)
                        return SelectStyle(DataContextParent, content.Date, 2);
                if (content.Date.DayOfWeek == DayOfWeek.Thursday && content.ButtonType == CalendarButtonType.Date)
                    if (SelectStyle(DataContextParent, content.Date, 3) != null)
                        return SelectStyle(DataContextParent, content.Date, 3);
                if (content.Date.DayOfWeek == DayOfWeek.Friday && content.ButtonType == CalendarButtonType.Date)
                    if (SelectStyle(DataContextParent, content.Date, 4) != null)
                        return SelectStyle(DataContextParent, content.Date, 4);
                if (content.Date.DayOfWeek == DayOfWeek.Saturday && content.ButtonType == CalendarButtonType.Date)
                    if (SelectStyle(DataContextParent, content.Date, 5) != null)
                        return SelectStyle(DataContextParent, content.Date, 5);
                if (content.Date.DayOfWeek == DayOfWeek.Sunday && content.ButtonType == CalendarButtonType.Date)
                    if (SelectStyle(DataContextParent, content.Date, 6) != null)
                        return SelectStyle(DataContextParent, content.Date, 6);
            }
            return base.SelectStyle(item, container);
        }

        public Style SelectStyle(SetAttributoCustomDayView DataContextParent, DateTime Date, int Index)
        {
            if (!string.IsNullOrEmpty(DataContextParent.GiorniLavorativi.ElementAt(Index).Hours))
                if (DataContextParent.ListaEccezioni != null)
                    if (DataContextParent.ListaEccezioni.Where(d => d.Day.Day == Date.Day && d.Day.Month == Date.Month && d.Day.Year == Date.Year).FirstOrDefault() != null)
                        if (string.IsNullOrEmpty(DataContextParent.ListaEccezioni.Where(d => d.Day.Day == Date.Day && d.Day.Month == Date.Month && d.Day.Year == Date.Year).FirstOrDefault().Hours))
                            return GiorniModificatiFestivi;
            if (DataContextParent.ListaEccezioni != null)
                if (DataContextParent.ListaEccezioni.Where(d => d.Day.Day == Date.Day && d.Day.Month == Date.Month && d.Day.Year == Date.Year).FirstOrDefault() != null)
                    return GiorniModificati;
            if (string.IsNullOrEmpty(DataContextParent.GiorniLavorativi.ElementAt(Index).Hours))
                return GiorniFestivi;

            if (!string.IsNullOrEmpty(DataContextParent.GiorniLavorativi.ElementAt(Index).Hours))
                if (DataContextParent.ListaEccezioniLocale != null)
                    if (DataContextParent.ListaEccezioniLocale.Where(d => d.Day.Day == Date.Day && d.Day.Month == Date.Month && d.Day.Year == Date.Year).FirstOrDefault() != null)
                        if (string.IsNullOrEmpty(DataContextParent.ListaEccezioniLocale.Where(d => d.Day.Day == Date.Day && d.Day.Month == Date.Month && d.Day.Year == Date.Year).FirstOrDefault().Hours))
                            return GiorniModificatiFestivi;
            if (DataContextParent.ListaEccezioniLocale != null)
                if (DataContextParent.ListaEccezioniLocale.Where(d => d.Day.Day == Date.Day && d.Day.Month == Date.Month && d.Day.Year == Date.Year).FirstOrDefault() != null)
                    return GiorniModificati;
            if (string.IsNullOrEmpty(DataContextParent.GiorniLavorativi.ElementAt(Index).Hours))
                return GiorniFestivi;
            return null;
        }
    }
}
