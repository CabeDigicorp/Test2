using MasterDetailModel;
using MasterDetailView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatiGeneraliWpf.View
{
    public class AllegatiAttributiSettingsView : AttributiSettingsView
    {
        protected override void LoadDefinizioneAttributi()
        {
            //combo definizione attributo
            DefinizioniAttributo = DataService.GetDefinizioniAttributo();
            _definizioniAttributoCodice.Clear();
            DefinizioniAttributoLoc.Clear();

            _definizioniAttributoCodice = DefinizioniAttributo.Values.Where(item => item.AllowAttributoCustom).Select(item => item.Codice).ToList();

            //if (_definizioniAttributoCodice.Contains(BuiltInCodes.DefinizioneAttributo.Riferimento))
            //    _definizioniAttributoCodice.Remove(BuiltInCodes.DefinizioneAttributo.Riferimento);

            //if (_definizioniAttributoCodice.Contains(BuiltInCodes.DefinizioneAttributo.Guid))
            //    _definizioniAttributoCodice.Remove(BuiltInCodes.DefinizioneAttributo.Guid);

            //if (_definizioniAttributoCodice.Contains(BuiltInCodes.DefinizioneAttributo.GuidCollection))
            //    _definizioniAttributoCodice.Remove(BuiltInCodes.DefinizioneAttributo.GuidCollection);

            _definizioniAttributoCodice.RemoveAll(item => item != BuiltInCodes.DefinizioneAttributo.Testo);

            foreach (string codice in _definizioniAttributoCodice)
            {
                string comboItem = AttributiSettingsView.GetDefinizioneAttributoLocalizedName(codice);//LocalizationProvider.GetString(codice);

                if (comboItem != null)
                    DefinizioniAttributoLoc.Add(comboItem);
            }
        }
    }
}
