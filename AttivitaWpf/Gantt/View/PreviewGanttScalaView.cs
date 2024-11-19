using Commons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttivitaWpf.View
{
    public class PreviewGanttScalaView : NotificationBase
    {
        public bool Stored { get; set; }
        private bool _IsZoomRadioButtonChecked;
        public bool IsZoomRadioButtonChecked
        {
            get
            {

                return _IsZoomRadioButtonChecked;
            }
            set
            {
                SetProperty(ref _IsZoomRadioButtonChecked, value);
                if (value)
                {
                    IsAdattaRadioButtonChecked = false;
                    IsAdattaRadioButtonEnable = false;
                    IsZoomRadioButtonEnable = true;
                }
                    
            }
        }
        private bool _IsAdattaRadioButtonChecked;
        public bool IsAdattaRadioButtonChecked
        {
            get
            {

                return _IsAdattaRadioButtonChecked;
            }
            set
            {
                SetProperty(ref _IsAdattaRadioButtonChecked, value);
                if (value)
                {
                    IsZoomRadioButtonChecked = false;
                    IsZoomRadioButtonEnable = false;
                    IsAdattaRadioButtonEnable = true;
                }
            }
        }

        private bool _IsZoomRadioButtonEnable;
        public bool IsZoomRadioButtonEnable
        {
            get
            {

                return _IsZoomRadioButtonEnable;
            }
            set
            {
                SetProperty(ref _IsZoomRadioButtonEnable, value);
            }
        }
        private bool _IsAdattaRadioButtonEnable;
        public bool IsAdattaRadioButtonEnable
        {
            get
            {

                return _IsAdattaRadioButtonEnable;
            }
            set
            {
                SetProperty(ref _IsAdattaRadioButtonEnable, value);
            }
        }

        private ObservableCollection<int> _ListaAddattaAPagine;
        public ObservableCollection<int> ListaAddattaAPagine
        {
            get
            {

                return _ListaAddattaAPagine;
            }
            set
            {
                SetProperty(ref _ListaAddattaAPagine, value);
            }
        }

        private object _SelectedItemAdattaAPagine;
        public object SelectedItemAdattaAPagine
        {
            get
            {

                return _SelectedItemAdattaAPagine;
            }
            set
            {
                SetProperty(ref _SelectedItemAdattaAPagine, value);
            }
        }

        private ObservableCollection<double> _ListaFattoriZoom;
        public ObservableCollection<double> ListaFattoriZoom
        {
            get
            {

                return _ListaFattoriZoom;
            }
            set
            {
                SetProperty(ref _ListaFattoriZoom, value);
            }
        }

        private object _SelectedItemFattoreZoom;
        public object SelectedItemFattoreZoom
        {
            get
            {

                return _SelectedItemFattoreZoom;
            }
            set
            {
                SetProperty(ref _SelectedItemFattoreZoom, value);
            }
        }

        public PreviewGanttScalaView()
        {
            ListaAddattaAPagine = new ObservableCollection<int>();
            ListaAddattaAPagine.Add(1);
            ListaAddattaAPagine.Add(2);
            ListaAddattaAPagine.Add(3);
            ListaAddattaAPagine.Add(4);
            ListaAddattaAPagine.Add(5);
            ListaAddattaAPagine.Add(6);
            ListaAddattaAPagine.Add(7);
            ListaAddattaAPagine.Add(8);
            ListaAddattaAPagine.Add(9);
            ListaAddattaAPagine.Add(10);
            ListaFattoriZoom = new ObservableCollection<double>();
            ListaFattoriZoom.Add(0.1);
            ListaFattoriZoom.Add(0.25);
            ListaFattoriZoom.Add(0.50);
            ListaFattoriZoom.Add(1);
            ListaFattoriZoom.Add(2);
            ListaFattoriZoom.Add(3);
            ListaFattoriZoom.Add(5);
            ListaFattoriZoom.Add(7);
            ListaFattoriZoom.Add(10);
            IsZoomRadioButtonChecked = true;
            IsZoomRadioButtonEnable = true;
            SelectedItemFattoreZoom = ListaFattoriZoom.ElementAt(3);
            SelectedItemAdattaAPagine = ListaAddattaAPagine.ElementAt(0);
        }

        public void Init(double ZoomFactor, int AdjustToPage)
        {
            Stored = false;
            if (AdjustToPage == 0)
            {
                SelectedItemFattoreZoom = ListaFattoriZoom.Where(r => r == ZoomFactor).FirstOrDefault();
                IsZoomRadioButtonChecked = true;
                IsZoomRadioButtonEnable = true;
                IsAdattaRadioButtonChecked = false;
                IsAdattaRadioButtonEnable = false;
            }

            if (ZoomFactor == 0)
            {
                SelectedItemAdattaAPagine = ListaAddattaAPagine.Where(r => r == AdjustToPage).FirstOrDefault();
                IsAdattaRadioButtonChecked = true;
                IsAdattaRadioButtonEnable = true;
                IsZoomRadioButtonChecked = false;
                IsZoomRadioButtonEnable = false;
            }
        }
        public void AcceptButton()
        {
            Stored = true;
        }
    }
}
