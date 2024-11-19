using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ModelData.Model
{
    public static class BuiltInCodes
    {
        public static class EntityType
        {

            public static string Divisione { get => "DivisioneItem"; }
            public static string DivisioneParent { get => "DivisioneItemParent"; }
            public static string Contatti { get => "ContattiItem"; }
            public static string InfoProgetto { get => "InfoProgettoItem"; }
            public static string Prezzario { get => "PrezzarioItem"; }
            public static string PrezzarioParent { get => "PrezzarioItemParent"; }
            public static string Elementi { get => "ElementiItem"; }
            public static string Computo { get => "ComputoItem"; }
            public static string Capitoli { get => "CapitoliItem"; }
            public static string CapitoliParent { get => "CapitoliItemParent"; }
            public static string Documenti { get => "DocumentiItem"; }
            public static string DocumentiParent { get => "DocumentiItemParent"; }
            public static string Report { get => "ReportItem"; }
            public static string Stili { get => "StiliItem"; }
            public static string ElencoAttivita { get => "ElencoAttivitaItem"; }
            public static string WBS { get => "WBSItem"; }
            public static string WBSParent { get => "WBSItemParent"; }
            public static string Calendari { get => "CalendariItem"; }
            public static string Variabili { get => "VariabiliItem"; }
            public static string Allegati { get => "AllegatiItem"; }
            public static string Tag { get => "TagItem"; }
        }

        public static class DefinizioneAttributo
        {
            public static string Guid { get => "Guid"; }
            public static string Testo { get => "Testo"; }
            public static string TestoRTF { get => "TestoRTF"; }
            public static string Reale { get => "Reale"; }
            public static string Contabilita { get => "Contabilita"; }
            public static string Riferimento { get => "Riferimento"; }
            public static string Data { get => "Data"; }
            public static string TestoCollection { get => "TestoCollection"; }
            public static string GuidCollection { get => "GuidCollection"; }
            public static string Elenco { get => "Elenco"; }
            public static string Booleano { get => "Booleano"; }
            public static string Colore { get => "Colore"; }
            public static string FormatoNumero { get => "FormatoNumero"; }
            public static string Variabile { get => "Variabile"; }
        }


        /// <summary>
        /// Codici degli attibuti riconosciuti dal programma (Codici BuiltIn)
        /// oss. L'attributo con codice un BuiltIn non è necessariamente BuiltIn (ad esempio GlobalId e IfcFileName sono codici BuiltIn ma non attributi BuiltIn)
        /// </summary>
        public static class Attributo
        {
            public static string Id { get; } = "Guid";
            public static string Nome { get; } = "Nome";
            public static string GlobalId { get; } = "GlobalId";
            public static string IfcFileName { get; } = "IfcFileName";
            public static string Model3dRule { get; } = "Model3dRule";
            public static string PrezzarioItem_Guid { get; } = "PrezzarioItem_Guid";
            public static string ElementiItem_Guid { get; } = "ElementiItem_Guid";
            public static string Model3dRuleId { get; } = "Model3dRuleId";
            public static string Model3dRuleElementiItemId { get; } = "Model3dRuleElementId";
            public static string TemporaryFilterByIds { get; } = "TemporaryFilterByIds";
            public static string Quantita { get; } = "Quantita";
            public static string QuantitaTotale { get; } = "QuantitaTotale";
            public static string PrezzarioItem_UnitaMisura { get; } = "PrezzarioItem_UnitaMisura";
            public static string PrezzarioItem_FormatoQuantita { get; } = "PrezzarioItem_FormatoQuantita";
            public static string UnitaMisura { get; } = "UnitaMisura";
            public static string FormatoQuantita { get; } = "FormatoQuantita";
            public static string Codice { get; } = "Codice";
            public static string DescrizioneRTF { get; } = "DescrizioneRTF";
            public static string Prezzo { get; } = "Prezzo";
            public static string CapitoliItem_Guid { get; } = "CapitoliItem_Guid";
            public static string PU { get; } = "PU";
            public static string Data { get; } = "Data";
            public static string Nota { get; } = "Nota";
            public static string Importo { get; } = "Importo";
            public static string ProjectGlobalId { get; } = "ProjectGlobalId";
            public static string Cognome { get; } = "Cognome";
            public static string CF { get; } = "CF";
            public static string Origine { get; } = "Origine";
            public static string PrezzoSicurezza { get; } = "PrezzoSicurezza";
            public static string IncSicurezza { get; } = "IncSicurezza";
            public static string PrezzoManodopera { get; } = "PrezzoManodopera";
            public static string IncManodopera { get; } = "IncManodopera";
            public static string PrezzoMateriali { get; } = "PrezzoMateriali";
            public static string IncMateriali { get; } = "IncMateriali";
            public static string PrezzoNoli { get; } = "PrezzoNoli";
            public static string IncNoli { get; } = "IncNoli";
            public static string PrezzoTrasporti { get; } = "PrezzoTrasporti";
            public static string IncTrasporti { get; } = "IncTrasporti";
            public static string ImportoSicurezza { get; } = "ImportoSicurezza";
            public static string PrezzarioItem_PrezzoSicurezza { get; } = "PrezzarioItem_PrezzoSicurezza";
            public static string ImportoManodopera { get; } = "ImportoManodopera";
            public static string PrezzarioItem_PrezzoManodopera { get; } = "PrezzarioItem_PrezzoManodopera";
            public static string Sezione { get; } = "Sezione";
            public static string DescrizioneReport { get; } = "DescrizioneReport";
            public static string NumeroColonne { get; } = "NumeroColonne";
            public static string Intestazione { get; } = "Intestazione";
            public static string Corpo { get; } = "Corpo";
            public static string Firme { get; } = "Firme";
            public static string PiePagina { get; } = "PiePagina";
            public static string Orientamento { get; } = "Orientamento";
            public static string Report { get; } = "Report";
            public static string ReportWizardSetting { get; } = "ReportSetting";
            public static string Carattere { get; } = "Carattere";
            public static string DimensioneCarattere { get; } = "DimensioneCarattere";
            public static string Grassetto { get; } = "Grassetto";
            public static string IsDigicorpOwner { get; } = "IsDigicorpOwner";
            public static string Italic { get; } = "Italic";
            public static string Stile { get; } = "Stile";
            public static string Barrato { get; } = "Barrato";
            public static string Sottolineato { get; } = "Sottolineato";
            public static string ColoreCarattere { get; } = "ColoreCarattere";
            public static string Allineamento { get; } = "Allineamento";
            public static string ColoreSfondo { get; } = "ColoreSfondo";
            public static string ReportItem_Guid { get; } = "ReportItem_Guid";
            public static string DimensioniPagina { get; } = "DimensioniPagina";
            public static string OrientamentoPagina { get; } = "OrientamentoPagina";
            public static string MargineSuperiore { get; } = "MargineSuperiore";
            public static string MargineInferiore { get; } = "MargineInferiore";
            public static string MargineSinistro { get; } = "MargineSinistro";
            public static string MargineDestro { get; } = "MargineDestro";
            public static string IntestazioneSinistra { get; } = "IntestazioneSinistra";
            public static string IntestazioneCentrale { get; } = "IntestazioneCentrale";
            public static string IntestazioneDestra { get; } = "IntestazioneDestra";
            public static string PiePaginaSinistra { get; } = "PiePaginaSinistra";
            public static string PiePaginaCentrale { get; } = "PiePaginaCentrale";
            public static string PiePaginaDestra { get; } = "PiePaginaDestra";
            public static string StampaNuovaPagina { get; } = "StampaNuovaPagina";
            public static string TabellaBordata { get; } = "TabellaBordata";
            public static string UsaRftAttributi { get; } = "UsaRftAttributi";
            public static string Compilato { get; } = "Compilato";
            public static string StampaVoci { get; } = "StampaVoci";
            public static string StileNumerazione { get; } = "StileNumerazione";
            public static string IniziaDaPag { get; } = "IniziaDaPag";
            public static string StileIntestazioneId { get; } = "StileIntestazioneId";
            public static string StileIntestazione { get; } = "StileIntestazione";
            public static string StilePiepaginaId { get; } = "StilePiepaginaId";
            public static string StilePiepagina { get; } = "StilePiepagina";
            public static string AttributoFilter { get; } = "AttributoFilter";
            public static string AttributoFilterText { get; } = "AttributoFilterText";
            public static string ComputoItemIds { get; } = "ComputoItemIds";
            public static string ElementiItemIds { get; } = "ElementiItemIds";
            public static string Attivita { get; } = "Attivita";
            //public static string OrarioLavoro { get; } = "OrarioLavoro";
            //public static string GiorniLavorativi { get; } = "GiorniLavorativi";
            public static string Lavoro { get; } = "Lavoro";
            public static string Durata { get; } = "Durata";
            public static string DurataCalendario { get; } = "DurataCalendario";
            public static string DataInizio { get; } = "DataInizio";
            public static string DataFine { get; } = "DataFine";
            public static string OggettoLavori { get; } = "OggettoLavori";
            public static string WeekHours { get; } = "WeekHours";
            public static string WeekHoursText { get; } = "WeekHoursText";
            public static string CustomDays { get; } = "CustomDays";
            public static string CustomDaysText { get; } = "CustomDaysText";
            public static string DataInizioGantt { get; } = "DataInizioGantt";
            public static string DataFineGantt { get; } = "DataFineGantt";
            public static string Predecessor { get; } = "Predecessor";
            public static string PredecessorText { get; } = "PredecessorText";
            public static string TaskNote { get; } = "TaskNota";
            public static string TaskProgress { get; } = "TaskProgress";
            public static string Link { get; } = "Link";
            public static string Descrizione { get; } = "Descrizione";
            public static string Tag { get; } = "Tag";
            public static string Categorie1 { get => "Categorie1"; }
            public static string Categorie2 { get => "Categorie2"; }
            public static string Categorie3 { get => "Categorie3"; }
            public static string Capitoli1 { get => "Capitoli1"; }
            public static string Capitoli2 { get => "Capitoli2"; }
            public static string Capitoli3 { get => "Capitoli3"; }
            public static string DesSintetica { get => "DesSintetica"; }
            public static string DesEstesa { get => "DesEstesa"; }
            public static string CodFase { get => "CodFase"; }
            public static string Codice2 { get => "Codice2"; }
            public static string DesRidotta { get => "DesRidotta"; }
            public static string DesBreve { get => "DesBreve"; }
            public static string DescrizioneQta { get => "DescrizioneQta"; }
            public static string Prezzo2 { get => "Prezzo2"; }
            public static string Prezzo3 { get => "Prezzo3"; }
            public static string Prezzo4 { get => "Prezzo4"; }
            public static string Prezzo5 { get => "Prezzo5"; }
            public static string Categoria1 { get => "Categoria1"; }
            public static string Categoria2 { get => "Categoria2"; }
            public static string Categoria3 { get => "Categoria3"; }
            public static string Capitolo1 { get => "Capitolo1"; }
            public static string Capitolo2 { get => "Capitolo2"; }
            public static string Capitolo3 { get => "Capitolo3"; }
            public static string Lunghezza { get => "Lunghezza"; }
            public static string Larghezza { get => "Larghezza"; }
            public static string Altezza { get => "Altezza"; }
            public static string TagBIM { get => "TagBIM"; }
            public static string VediVoce { get => "VediVoce"; }
            public static string VediVoceQtaTot { get => "VediVoceQtaTot"; }
            public static string CodiceWBS { get => "CodiceWBS"; }
            public static string IfcClass { get => "IfcClass"; }
            public static string IfcGroup { get => "IfcGroup"; }
            public static string WBS1 { get => "WBS1"; }
        }

        public static class SectionItemsId
        {
            public static int Contatti { get => 1; }
            public static int InfoProgetto { get => 2; }
            public static int Stili { get => 3; }
            public static int Prezzario { get => 4; }
            public static int Capitoli { get => 5; }
            public static int Elementi { get => 6; }
            public static int Computo { get => 7; }
            public static int ElencoAttivita { get => 8; }
            public static int WBS { get => 9; }
            public static int Gantt { get => 10; }
            public static int Calendari { get => 11; }
            public static int FogliDiCalcolo { get => 12; }
            public static int Variabili { get => 13; }
            //public static int Documenti { get => 7; }
            //public static int Report { get => 8; }       
        }
    }

}
