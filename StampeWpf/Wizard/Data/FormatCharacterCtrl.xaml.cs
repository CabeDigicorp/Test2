using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace StampeWpf.Wizard
{
    /// <summary>
    /// Interaction logic for FormatCharacter.xaml
    /// </summary>
    public partial class FormatCharacterCtrl : Window
    {
        public FormatCharacterView FormatCharacterView { get => DataContext as FormatCharacterView; }
        public FormatCharacterCtrl()
        {
            InitializeComponent();
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            if (FormatCharacterView.TextBoxItemView != null)
            {
                if (FormatCharacterView.TextBoxItemView.InClosing == false)
                {
                    if (FormatCharacterView.TextBoxItemView.StilePrecedente.ColorBackground != null) { FormatCharacterView.ColorBackground = FormatCharacterView.Colors.Where(c => c.HexValue == FormatCharacterView.TextBoxItemView.StilePrecedente.ColorBackground.HexValue).FirstOrDefault(); }
                    if (FormatCharacterView.TextBoxItemView.StilePrecedente.ColorCharacther != null) { FormatCharacterView.ColorCharacther = FormatCharacterView.Colors.Where(c => c.HexValue == FormatCharacterView.TextBoxItemView.StilePrecedente.ColorCharacther.HexValue).FirstOrDefault(); }
                    FormatCharacterView.FontFamily = FormatCharacterView.ListFontFamily.Where(c => c == FormatCharacterView.TextBoxItemView.StilePrecedente.FontFamily).FirstOrDefault();
                    FormatCharacterView.IsBarrato = FormatCharacterView.TextBoxItemView.StilePrecedente.IsBarrato;
                    FormatCharacterView.IsGrassetto = FormatCharacterView.TextBoxItemView.StilePrecedente.IsGrassetto;
                    FormatCharacterView.IsCorsivo = FormatCharacterView.TextBoxItemView.StilePrecedente.IsCorsivo;
                    FormatCharacterView.IsSottolineato = FormatCharacterView.TextBoxItemView.StilePrecedente.IsSottolineato;
                    FormatCharacterView.Size = FormatCharacterView.ListSize.Where(c => c == FormatCharacterView.TextBoxItemView.StilePrecedente.Size).FirstOrDefault();
                    FormatCharacterView.TextAlignement = FormatCharacterView.ListTextHorizontalAlignement.Where(c => c == FormatCharacterView.TextBoxItemView.StilePrecedente.TextAlignement).FirstOrDefault();
                    FormatCharacterView.TextAlignementCode = FormatCharacterView.TextAlignementCode;
                    FormatCharacterView.TextVerticalAlignement = FormatCharacterView.ListTextVerticalAlignement.Where(c => c == FormatCharacterView.TextBoxItemView.StilePrecedente.TextVerticalAlignement).FirstOrDefault();
                    if (FormatCharacterView.TextBoxItemView.StilePrecedente.StileConPropieta != null)
                    {
                        FormatCharacterView.SettaStileProgettoPerNome(FormatCharacterView.TextBoxItemView.StilePrecedente.StileConPropieta.NomeECodice);
                    }
                    FormatCharacterView.IsModificatoVisible = FormatCharacterView.TextBoxItemView.StilePrecedente.IsModificatoVisible;
                    FormatCharacterView.Nascondi = FormatCharacterView.TextBoxItemView.StilePrecedente.Nascondi;
                    FormatCharacterView.Rtf = FormatCharacterView.TextBoxItemView.StilePrecedente.Rtf;
                    FormatCharacterView.StampaFormula = FormatCharacterView.TextBoxItemView.StilePrecedente.StampaFormula;
                    FormatCharacterView.ConcatenaEtichettaEValore = FormatCharacterView.TextBoxItemView.StilePrecedente.ConcatenaEtichettaEValore;
                    if (FormatCharacterView.TextBoxItemView.StilePrecedente.Nascondi)
                    {
                        FormatCharacterView.TextBoxItemView.HideAttributeColor = System.Windows.Media.Brushes.Gray;
                    }
                    else
                    {
                        FormatCharacterView.TextBoxItemView.HideAttributeColor = System.Windows.Media.Brushes.Black;
                    }
                }
            }
            else
            {
                if (FormatCharacterView.IntestazioneColonnaEntity.StilePrecedente.ColorBackground != null) { FormatCharacterView.ColorBackground = FormatCharacterView.Colors.Where(c => c.HexValue == FormatCharacterView.IntestazioneColonnaEntity.StilePrecedente.ColorBackground.HexValue).FirstOrDefault(); }
                if (FormatCharacterView.IntestazioneColonnaEntity.StilePrecedente.ColorCharacther != null) { FormatCharacterView.ColorCharacther = FormatCharacterView.Colors.Where(c => c.HexValue == FormatCharacterView.IntestazioneColonnaEntity.StilePrecedente.ColorCharacther.HexValue).FirstOrDefault(); }
                FormatCharacterView.FontFamily = FormatCharacterView.ListFontFamily.Where(c => c == FormatCharacterView.IntestazioneColonnaEntity.StilePrecedente.FontFamily).FirstOrDefault();
                FormatCharacterView.IsBarrato = FormatCharacterView.IntestazioneColonnaEntity.StilePrecedente.IsBarrato;
                FormatCharacterView.IsGrassetto = FormatCharacterView.IntestazioneColonnaEntity.StilePrecedente.IsGrassetto;
                FormatCharacterView.IsCorsivo = FormatCharacterView.IntestazioneColonnaEntity.StilePrecedente.IsCorsivo;
                FormatCharacterView.IsSottolineato = FormatCharacterView.IntestazioneColonnaEntity.StilePrecedente.IsSottolineato;
                FormatCharacterView.Size = FormatCharacterView.ListSize.Where(c => c == FormatCharacterView.IntestazioneColonnaEntity.StilePrecedente.Size).FirstOrDefault();
                FormatCharacterView.TextAlignement = FormatCharacterView.ListTextHorizontalAlignement.Where(c => c == FormatCharacterView.IntestazioneColonnaEntity.StilePrecedente.TextAlignement).FirstOrDefault();
                FormatCharacterView.TextAlignementCode = FormatCharacterView.TextAlignementCode;
                FormatCharacterView.TextVerticalAlignement = FormatCharacterView.ListTextVerticalAlignement.Where(c => c == FormatCharacterView.IntestazioneColonnaEntity.StilePrecedente.TextVerticalAlignement).FirstOrDefault();
                if (FormatCharacterView.IntestazioneColonnaEntity.StilePrecedente.StileConPropieta != null)
                {
                    FormatCharacterView.SettaStileProgettoPerNome(FormatCharacterView.IntestazioneColonnaEntity.StilePrecedente.StileConPropieta.NomeECodice);
                }
                FormatCharacterView.IsModificatoVisible = FormatCharacterView.IntestazioneColonnaEntity.StilePrecedente.IsModificatoVisible;
                FormatCharacterView.Nascondi = FormatCharacterView.IntestazioneColonnaEntity.StilePrecedente.Nascondi;
                FormatCharacterView.Rtf = FormatCharacterView.IntestazioneColonnaEntity.StilePrecedente.Rtf;
                FormatCharacterView.StampaFormula = FormatCharacterView.IntestazioneColonnaEntity.StilePrecedente.StampaFormula;
                FormatCharacterView.ConcatenaEtichettaEValore = FormatCharacterView.IntestazioneColonnaEntity.StilePrecedente.ConcatenaEtichettaEValore;
            }
            e.Cancel = true;
            this.Visibility = Visibility.Collapsed;
        }

        private void AcceptButton_Click(object sender, RoutedEventArgs e)
        {
            if (FormatCharacterView.IntestazioneColonnaEntity != null)
            {
                FormatCharacterView.IntestazioneColonnaEntity.ExecuteAccept();
            }
        }
    }
}
