using CommonResources;
using MasterDetailModel;
using MasterDetailView;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace DatiGeneraliWpf.View
{
    public class VariabiliAttributiSettingsView : AttributiSettingsView
    {


        protected override void LoadDefinizioneAttributi()
        {
            //combo definizione attributo
            DefinizioniAttributo = DataService.GetDefinizioniAttributo();
            _definizioniAttributoCodice.Clear();
            DefinizioniAttributoLoc.Clear();

            List<string> definizioniAttributoCodice = new List<string>();
            definizioniAttributoCodice = DefinizioniAttributo.Values.Where(item => item.AllowAttributoCustom).Select(item => item.Codice).ToList();

            foreach (string def in definizioniAttributoCodice)
            {
                if (def == BuiltInCodes.DefinizioneAttributo.Testo ||
                    def == BuiltInCodes.DefinizioneAttributo.Reale ||
                    def == BuiltInCodes.DefinizioneAttributo.Contabilita )
                    _definizioniAttributoCodice.Add(def);
            }

            foreach (string codice in _definizioniAttributoCodice)
            {
                string comboItem = AttributiSettingsView.GetDefinizioneAttributoLocalizedName(codice);

                if (comboItem != null)
                    DefinizioniAttributoLoc.Add(comboItem);
            }
        }
    }
}
