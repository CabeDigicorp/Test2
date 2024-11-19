using CommonResources;
using Commons;
using MasterDetailModel;
using MasterDetailView;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FogliDiCalcoloWpf
{
    public class FoglioDiCalcoloGanttDataView : FoglioDiCalcoloBaseDataView
    {
        public FoglioDiCalcoloGanttDataView() : base()
        {
        }

        public void RimuoviAttributiDiversiDaRealiContabilita()
        {
            var ListaOrigine = ListaFiltrati.Where(r => r.DefinizioneAttributo == BuiltInCodes.DefinizioneAttributo.Reale || r.DefinizioneAttributo == BuiltInCodes.DefinizioneAttributo.Contabilita).ToList();
            ListaAttributiNonFiltrati.Clear();
            ListaFiltrati.Clear();
            foreach (var item in ListaOrigine)
            {
                ListaAttributiNonFiltrati.Add(item);
                ListaFiltrati.Add(item);
            }
        }

        public override bool Accept()
        {
            bool ExistSezione = false;

            if (fogliDiCalcoloData.FoglioDiCalcolo == null)
            {
                fogliDiCalcoloData.FoglioDiCalcolo = new List<Model.FoglioDiCalcolo>();
            }

            foreach (var foglioDiCalcolo in fogliDiCalcoloData.FoglioDiCalcolo)
            {
                if (foglioDiCalcolo.SezioneKey == sezionekey && foglioDiCalcolo.Foglio == DataText)
                {
                    ExistSezione = true;

                    foglioDiCalcolo.AttributiStandardFoglioDiCalcolo.Clear();

                    foreach (var Filtrato in ListaAttributiNonFiltrati)
                    {
                        if (Filtrato.Amount || Filtrato.ProgressiveAmount || Filtrato.ProductivityPerHour)
                        {
                            AttributoFoglioDiCalcolo attributo = new AttributoFoglioDiCalcolo();
                            attributo.CodiceOrigine = Filtrato.CodiceOrigine;
                            attributo.DefinizioneAttributo = Filtrato.DefinizioneAttributo;
                            attributo.Etichetta = Filtrato.Etichetta;
                            attributo.Formula = Filtrato.Formula;
                            attributo.Note = Filtrato.Note;
                            attributo.Amount = Filtrato.Amount;
                            attributo.ProgressiveAmount = Filtrato.ProgressiveAmount;
                            attributo.ProductivityPerHour = Filtrato.ProductivityPerHour;
                            foglioDiCalcolo.AttributiStandardFoglioDiCalcolo.Add(attributo);
                        }
                    }
                }
            }

            if (!ExistSezione)
            {
                fogliDiCalcoloData.FoglioDiCalcolo.Add(new Model.FoglioDiCalcolo());
                fogliDiCalcoloData.FoglioDiCalcolo.LastOrDefault().Foglio = DataText;
                fogliDiCalcoloData.FoglioDiCalcolo.LastOrDefault().Tabella = TabellaText;
                fogliDiCalcoloData.FoglioDiCalcolo.LastOrDefault().SezioneKey = sezionekey;
                fogliDiCalcoloData.FoglioDiCalcolo.LastOrDefault().AttributiStandardFoglioDiCalcolo = new List<AttributoFoglioDiCalcolo>();

                foreach (var Filtrato in ListaAttributiNonFiltrati)
                {
                    if (Filtrato.Amount || Filtrato.ProgressiveAmount || Filtrato.ProductivityPerHour)
                    {
                        AttributoFoglioDiCalcolo attributo = new AttributoFoglioDiCalcolo();
                        attributo.CodiceOrigine = Filtrato.CodiceOrigine;
                        attributo.DefinizioneAttributo = Filtrato.DefinizioneAttributo;
                        attributo.Etichetta = Filtrato.Etichetta;
                        attributo.Formula = Filtrato.Formula;
                        attributo.Note = Filtrato.Note;
                        attributo.Amount = Filtrato.Amount;
                        attributo.ProgressiveAmount = Filtrato.ProgressiveAmount;
                        attributo.ProductivityPerHour = Filtrato.ProductivityPerHour;
                        fogliDiCalcoloData.FoglioDiCalcolo.LastOrDefault().AttributiStandardFoglioDiCalcolo.Add(attributo);
                    }
                }
            }



            DataService.SetFogliDiCalcoloData(fogliDiCalcoloData);
            return true;
        }
    }
}
