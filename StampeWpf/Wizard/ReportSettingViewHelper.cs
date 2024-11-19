using CommonResources;
using MasterDetailModel;
using Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace StampeWpf.Wizard
{
    public class ReportSettingViewHelper
    {
        private ObservableCollection<TreeviewItem> ListAttributiPerEstrazioneAlberi;
        public static ObservableCollection<TreeviewItem> ListaAttributiWizard { get; set; }
        public ClientDataService DataService { get; set; }
        private string SezioneKey;
        public static string AttributoCodicePathSeparator { get; } = "__";

        public ReportSettingViewHelper(ClientDataService dataService, string sezioneKey)
        {
            DataService = dataService;
            ListAttributiPerEstrazioneAlberi = new ObservableCollection<TreeviewItem>();
            ListaAttributiWizard = new ObservableCollection<TreeviewItem>();
            SezioneKey = sezioneKey;
        }
        public void EstraiAlberoDaAttributoSelezionato(List<AttributiUtilizzati> AttributiUtilizzatiGiaInseriti)
        {
            if (AttributiUtilizzatiGiaInseriti == null)
                return;

            List<AttributiUtilizzati> MasterListaAttributiAlbero = new List<AttributiUtilizzati>();
            List<AttributiUtilizzati> ListaAttributiAlbero = new List<AttributiUtilizzati>();
            bool FoundEndElement = false;

            foreach (var AttributiUtilizzatiGiaInserito in AttributiUtilizzatiGiaInseriti)
            {

                foreach (var AttributiUtilizzato in AttributiUtilizzatiGiaInserito.AttributiPerEntityType)
                {
                    foreach (var PrimoLivello in ListAttributiPerEstrazioneAlberi)
                    {
                        ListaAttributiAlbero = new List<AttributiUtilizzati>();

                        if (!FoundEndElement)
                        {
                            if (AttributiUtilizzatiGiaInserito.EntityType == PrimoLivello.EntityType && AttributiUtilizzato == PrimoLivello.AttrbutoCodice)
                            {
                                FoundEndElement = true;
                            }
                            else
                            {
                                if (PrimoLivello.PropertyType == BuiltInCodes.DefinizioneAttributo.Guid)
                                {
                                    if (ListaAttributiAlbero.Where(f => f.EntityType == PrimoLivello.EntityType).FirstOrDefault() == null)
                                    {
                                        AttributiUtilizzati attributoutilizzato1 = new AttributiUtilizzati() { EntityType = PrimoLivello.EntityType, PropertyType = new List<string>() { PrimoLivello.PropertyType } };
                                        attributoutilizzato1.AttributiPerEntityType = new List<string>();
                                        attributoutilizzato1.AttributiPerEntityType.Add(PrimoLivello.AttrbutoCodice);
                                        ListaAttributiAlbero.Add(attributoutilizzato1);
                                        FoundEndElement = true;
                                    }
                                }
                            }
                        }

                        foreach (var SecondoLivello in PrimoLivello.Items)
                        {
                            if (!FoundEndElement)
                            {
                                if (AttributiUtilizzatiGiaInserito.EntityType == SecondoLivello.EntityType && AttributiUtilizzato == SecondoLivello.AttrbutoCodice)
                                {
                                    FoundEndElement = true;

                                }
                                else
                                {
                                    if (PrimoLivello.PropertyType == BuiltInCodes.DefinizioneAttributo.Guid)
                                    {
                                        if (ListaAttributiAlbero.Where(f => f.EntityType == SecondoLivello.EntityType).FirstOrDefault() == null)
                                        {
                                            AttributiUtilizzati attributoutilizzato2 = new AttributiUtilizzati() { EntityType = SecondoLivello.EntityType, PropertyType = new List<string>() { PrimoLivello.PropertyType } };
                                            attributoutilizzato2.AttributiPerEntityType = new List<string>();
                                            attributoutilizzato2.AttributiPerEntityType.Add(PrimoLivello.AttrbutoCodice);
                                            ListaAttributiAlbero.Add(attributoutilizzato2);
                                            FoundEndElement = true;
                                        }
                                    }
                                }
                            }
                        }
                        if (FoundEndElement) { MasterListaAttributiAlbero.AddRange(ListaAttributiAlbero); }
                        FoundEndElement = false;
                    }
                }
            }

            AggiungiAgliAttributiGiàInseriti(AttributiUtilizzatiGiaInseriti, MasterListaAttributiAlbero);
        }

        private void AggiungiAgliAttributiGiàInseriti(List<AttributiUtilizzati> AttributiUtilizzatiGiaInseriti, List<AttributiUtilizzati> AttributiUtilizzatiDaInserire)
        {
            foreach (var AttributiUtilizzatoDaInserire in AttributiUtilizzatiDaInserire)
            {
                foreach (var AttributoUtilizzatoDaInserire in AttributiUtilizzatoDaInserire.AttributiPerEntityType)
                {
                    if (AttributiUtilizzatiGiaInseriti.Where(a => a.EntityType == AttributiUtilizzatoDaInserire.EntityType).FirstOrDefault() == null)
                    {
                        AttributiUtilizzati attributoutilizzato1 = new AttributiUtilizzati() { EntityType = AttributiUtilizzatoDaInserire.EntityType };
                        attributoutilizzato1.AttributiPerEntityType = new List<string>();
                        attributoutilizzato1.AttributiPerEntityType.Add(AttributoUtilizzatoDaInserire);
                        AttributiUtilizzatiGiaInseriti.Add(attributoutilizzato1);
                    }
                    else
                    {
                        if (AttributiUtilizzatiGiaInseriti.Where(a => a.EntityType == AttributiUtilizzatoDaInserire.EntityType).FirstOrDefault().AttributiPerEntityType.Where(d => d == AttributoUtilizzatoDaInserire).FirstOrDefault() == null)
                        {
                            AttributiUtilizzatiGiaInseriti.Where(a => a.EntityType == AttributiUtilizzatoDaInserire.EntityType).FirstOrDefault().AttributiPerEntityType.Add(AttributoUtilizzatoDaInserire);
                            if (AttributiUtilizzatiGiaInseriti.Where(a => a.EntityType == AttributiUtilizzatoDaInserire.EntityType).FirstOrDefault().PropertyType == null)
                            {
                                AttributiUtilizzatiGiaInseriti.Where(a => a.EntityType == AttributiUtilizzatoDaInserire.EntityType).FirstOrDefault().PropertyType = new List<string>();
                            }
                            AttributiUtilizzatiGiaInseriti.Where(a => a.EntityType == AttributiUtilizzatoDaInserire.EntityType).FirstOrDefault().PropertyType.Add(AttributiUtilizzatoDaInserire.PropertyType.ElementAt(0));
                        }
                    }
                }
            }
        }
    }
}
