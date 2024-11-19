using Commons;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace StampeWpf.Wizard
{
    public class GroupSettingView : NotificationBase
    {
        private bool _IsCheckedRiepilogo;
        public bool IsCheckedRiepilogo
        {
            get
            {
                return _IsCheckedRiepilogo;
            }
            set
            {
                if (SetProperty(ref _IsCheckedRiepilogo, value))
                {
                    _IsCheckedRiepilogo = value;
                }
            }
        }

        private bool _IsCheckedTotale;
        public bool IsCheckedTotale
        {
            get
            {
                return _IsCheckedTotale;
            }
            set
            {
                if (SetProperty(ref _IsCheckedTotale, value))
                {
                    _IsCheckedTotale = value;
                }
            }
        }

        private bool _IsCheckedNuovaPagina;
        public bool IsCheckedNuovaPagina
        {
            get
            {
                return _IsCheckedNuovaPagina;
            }
            set
            {
                if (SetProperty(ref _IsCheckedNuovaPagina, value))
                {
                    _IsCheckedNuovaPagina = value;
                }
            }
        }

        private bool _IsCheckedDescrizioneBreve;
        public bool IsCheckedDescrizioneBreve
        {
            get
            {
                return _IsCheckedDescrizioneBreve;
            }
            set
            {
                if (SetProperty(ref _IsCheckedDescrizioneBreve, value))
                {
                    _IsCheckedDescrizioneBreve = value;
                }
            }
        }
        public GroupSettingView(){ }
        public void Init(OpzioniDiStampa opzioniDiStampa)
        {
            IsCheckedRiepilogo = opzioniDiStampa.IsCheckedRiepilogo;
            IsCheckedNuovaPagina = opzioniDiStampa.IsCheckedNuovaPagina;
            IsCheckedDescrizioneBreve = opzioniDiStampa.IsCheckedDescrizioneBreve;
        }
    }
}
