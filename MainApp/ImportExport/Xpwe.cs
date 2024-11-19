using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MainApp
{
    /* 
	Licensed under the Apache License, Version 2.0
    
	http://www.apache.org/licenses/LICENSE-2.0
	*/


    [XmlRoot(ElementName = "DGSuperCapitoliItem")]
    public class DGSuperCapitoliItem
    {
        [XmlAttribute("ID")]
        public string ID { get; set; }

        [XmlElement(ElementName = "DesSintetica")]
        public string DesSintetica { get; set; }

        [XmlElement(ElementName = "DesEstesa")]
        public string DesEstesa { get; set; }

        [XmlElement(ElementName = "DataInit")]
        public string DataInit { get; set; }

        [XmlElement(ElementName = "Durata")]
        public string Durata { get; set; }

        [XmlElement(ElementName = "CodFase")]
        public string CodFase { get; set; }

        [XmlElement(ElementName = "Percentuale")]
        public string Percentuale { get; set; }

        [XmlElement(ElementName = "Codice")]
        public string Codice { get; set; }

    }

    [XmlRoot(ElementName = "DGCapitoliItem")]
    public class DGCapitoliItem
    {
        [XmlAttribute("ID")]
        public string ID { get; set; }

        [XmlElement(ElementName = "DesSintetica")]
        public string DesSintetica { get; set; }

        [XmlElement(ElementName = "DesEstesa")]
        public string DesEstesa { get; set; }

        [XmlElement(ElementName = "DataInit")]
        public string DataInit { get; set; }

        [XmlElement(ElementName = "Durata")]
        public string Durata { get; set; }

        [XmlElement(ElementName = "CodFase")]
        public string CodFase { get; set; }

        [XmlElement(ElementName = "Percentuale")]
        public string Percentuale { get; set; }

        [XmlElement(ElementName = "Codice")]
        public string Codice { get; set; }
    }

    [XmlRoot(ElementName = "DGSubCapitoliItem")]
    public class DGSubCapitoliItem
    {
        [XmlAttribute("ID")]
        public string ID { get; set; }

        [XmlElement(ElementName = "DesSintetica")]
        public string DesSintetica { get; set; }

        [XmlElement(ElementName = "DesEstesa")]
        public string DesEstesa { get; set; }

        [XmlElement(ElementName = "DataInit")]
        public string DataInit { get; set; }

        [XmlElement(ElementName = "Durata")]
        public string Durata { get; set; }

        [XmlElement(ElementName = "CodFase")]
        public string CodFase { get; set; }

        [XmlElement(ElementName = "Percentuale")]
        public string Percentuale { get; set; }

        [XmlElement(ElementName = "Codice")]
        public string Codice { get; set; }
    }

    [XmlRoot(ElementName = "DGSuperCategorieItem")]
    public class DGSuperCategorieItem
    {
        [XmlAttribute("ID")]
        public string ID { get; set; }

        [XmlElement(ElementName = "DesSintetica")]
        public string DesSintetica { get; set; }

        [XmlElement(ElementName = "DesEstesa")]
        public string DesEstesa { get; set; }

        [XmlElement(ElementName = "DataInit")]
        public string DataInit { get; set; }

        [XmlElement(ElementName = "Durata")]
        public string Durata { get; set; }

        [XmlElement(ElementName = "CodFase")]
        public string CodFase { get; set; }

        [XmlElement(ElementName = "Percentuale")]
        public string Percentuale { get; set; }

        [XmlElement(ElementName = "Codice")]
        public string Codice { get; set; }
    }

    [XmlRoot(ElementName = "DGCategorieItem")]
    public class DGCategorieItem
    {
        [XmlAttribute("ID")]
        public string ID { get; set; }

        [XmlElement(ElementName = "DesSintetica")]
        public string DesSintetica { get; set; }

        [XmlElement(ElementName = "DesEstesa")]
        public string DesEstesa { get; set; }

        [XmlElement(ElementName = "DataInit")]
        public string DataInit { get; set; }

        [XmlElement(ElementName = "Durata")]
        public string Durata { get; set; }

        [XmlElement(ElementName = "CodFase")]
        public string CodFase { get; set; }

        [XmlElement(ElementName = "Percentuale")]
        public string Percentuale { get; set; }

        [XmlElement(ElementName = "Codice")]
        public string Codice { get; set; }
    }

    [XmlRoot(ElementName = "DGSubCategorieItem")]
    public class DGSubCategorieItem
    {
        [XmlAttribute("ID")]
        public string ID { get; set; }

        [XmlElement(ElementName = "DesSintetica")]
        public string DesSintetica { get; set; }

        [XmlElement(ElementName = "DesEstesa")]
        public string DesEstesa { get; set; }

        [XmlElement(ElementName = "DataInit")]
        public string DataInit { get; set; }

        [XmlElement(ElementName = "Durata")]
        public string Durata { get; set; }

        [XmlElement(ElementName = "CodFase")]
        public string CodFase { get; set; }

        [XmlElement(ElementName = "Percentuale")]
        public string Percentuale { get; set; }

        [XmlElement(ElementName = "Codice")]
        public string Codice { get; set; }
    }

    [XmlRoot(ElementName = "PweDGSuperCapitoli")]
    public class PweDGSuperCapitoli
    {
        [XmlElement(ElementName = "DGSuperCapitoliItem")]
        public List<DGSuperCapitoliItem> DGSuperCapitoliItem { get; set; }
    }

    [XmlRoot(ElementName = "PweDGCapitoli")]
    public class PweDGCapitoli
    {
        [XmlElement(ElementName = "DGCapitoliItem")]
        public List<DGCapitoliItem> DGCapitoliItem { get; set; }
    }

    [XmlRoot(ElementName = "PweDGSubCapitoli")]
    public class PweDGSubCapitoli
    {
        [XmlElement(ElementName = "DGSubCapitoliItem")]
        public List<DGSubCapitoliItem> DGSubCapitoliItem { get; set; }
    }

    [XmlRoot(ElementName = "PweDGSuperCategorie")]
    public class PweDGSuperCategorie
    {
        [XmlElement(ElementName = "DGSuperCategorieItem")]
        public List<DGSuperCategorieItem> DGSuperCategorieItem { get; set; }
    }

    [XmlRoot(ElementName = "PweDGCategorie")]
    public class PweDGCategorie
    {
        [XmlElement(ElementName = "DGCategorieItem")]
        public List<DGCategorieItem> DGCategorieItem { get; set; }
    }

    [XmlRoot(ElementName = "PweDGSubCategorie")]
    public class PweDGSubCategorie
    {
        [XmlElement(ElementName = "DGSubCategorieItem")]
        public List<DGSubCategorieItem> DGSubCategorieItem { get; set; }
    }

    [XmlRoot(ElementName = "PweDGCapitoliCategorie")]
    public class PweDGCapitoliCategorie
    {

        [XmlElement(ElementName = "PweDGSuperCapitoli")]
        public PweDGSuperCapitoli DGSuperCapitoli { get; set; }

        [XmlElement(ElementName = "PweDGCapitoli")]
        public PweDGCapitoli DGCapitoli { get; set; }

        [XmlElement(ElementName = "PweDGSubCapitoli")]
        public PweDGSubCapitoli DGSubCapitoli { get; set; }

        [XmlElement(ElementName = "PweDGSuperCategorie")]
        public PweDGSuperCategorie DGSuperCategorie { get; set; }

        [XmlElement(ElementName = "PweDGCategorie")]
        public PweDGCategorie DGCategorie { get; set; }

        [XmlElement(ElementName = "PweDGSubCategorie")]
        public PweDGSubCategorie DGSubCategorie { get; set; }
    }


    [XmlRoot(ElementName = "DGWBSItem")]
    public class DGWBSItem
    {
        [XmlAttribute("ID")]
        public string ID { get; set; }

        [XmlElement(ElementName = "CU")]
        public string CU { get; set; }

        [XmlElement(ElementName = "CUParent")]
        public string CUParent { get; set; }

        [XmlElement(ElementName = "TITOLO")]
        public string TITOLO { get; set; }

        [XmlElement(ElementName = "CODICE")]
        public string CODICE { get; set; }

        [XmlElement(ElementName = "CodiceExt")]
        public string CodiceExt { get; set; }

    }

    [XmlRoot(ElementName = "PweDGWBS")]
    public class PweDGWBS
    {
        [XmlElement(ElementName = "DGWBSAttiva")]
        public string DGWBSAttiva { get; set; }

        [XmlElement(ElementName = "DGWBSItem")]
        public List<DGWBSItem> DGWBSItem { get; set; }

    }

    [XmlRoot(ElementName = "PweDGConfigNumeri")]
    public class PweDGConfigNumeri
    {
        [XmlElement(ElementName = "PartiUguali")]
        public string PartiUguali { get; set; }

        [XmlElement(ElementName = "Lunghezza")]
        public string Lunghezza { get; set; }

        [XmlElement(ElementName = "Larghezza")]
        public string Larghezza { get; set; }

        [XmlElement(ElementName = "HPeso")]
        public string HPeso { get; set; }

        [XmlElement(ElementName = "Quantita")]
        public string Quantita { get; set; }

        [XmlElement(ElementName = "Prezzi")]
        public string Prezzi { get; set; }

        [XmlElement(ElementName = "PrezziTotale")]
        public string PrezziTotale { get; set; }

        [XmlElement(ElementName = "IncidenzaPercentuale")]
        public string IncidenzaPercentuale { get; set; }


    }

    [XmlRoot(ElementName = "PweDGConfigurazione")]
    public class PweDGConfigurazione
    {
        [XmlElement(ElementName = "PweDGConfigNumeri")]
        public PweDGConfigNumeri DGConfigNumeri { get; set; }

    }


    [XmlRoot(ElementName = "PweDatiGenerali")]
    public class PweDatiGenerali
    {
        [XmlElement(ElementName = "PweDGCapitoliCategorie")]
        public PweDGCapitoliCategorie DGCapitoliCategorie { get; set; }

        [XmlElement(ElementName = "PweDGWBS")]
        public PweDGWBS DGWBS { get; set; }

        [XmlElement(ElementName = "PweDGConfigurazione")]
        public PweDGConfigurazione DGConfigurazione { get; set; }
    }

    [XmlRoot(ElementName = "EPItem")]
    public class EPItem
    {
        [XmlAttribute("ID")]
        public string ID { get; set; }

        [XmlElement(ElementName = "TipoEP")]
        public string TipoEP { get; set; }

        [XmlElement(ElementName = "Tariffa")]
        public string Tariffa { get; set; }

        [XmlElement(ElementName = "Articolo")]
        public string Articolo { get; set; }

        [XmlElement(ElementName = "DesRidotta")]
        public string DesRidotta { get; set; }

        [XmlElement(ElementName = "DesEstesa")]
        public string DesEstesa { get; set; }

        [XmlElement(ElementName = "DesBreve")]
        public string DesBreve { get; set; }

        [XmlElement(ElementName = "UnMisura")]
        public string UnMisura { get; set; }

        [XmlElement(ElementName = "Prezzo1")]
        public string Prezzo1 { get; set; }

        [XmlElement(ElementName = "Prezzo2")]
        public string Prezzo2 { get; set; }

        [XmlElement(ElementName = "Prezzo3")]
        public string Prezzo3 { get; set; }

        [XmlElement(ElementName = "Prezzo4")]
        public string Prezzo4 { get; set; }

        [XmlElement(ElementName = "Prezzo5")]
        public string Prezzo5 { get; set; }

        [XmlElement(ElementName = "IDSpCap")]
        public string IDSpCap { get; set; }

        [XmlElement(ElementName = "IDCap")]
        public string IDCap { get; set; }

        [XmlElement(ElementName = "IDSbCap")]
        public string IDSbCap { get; set; }

        [XmlElement(ElementName = "Data")]
        public string Data { get; set; }

        [XmlElement(ElementName = "IncMAT")]
        public string IncMAT { get; set; }

        [XmlElement(ElementName = "IncATTR")]
        public string IncATTR { get; set; }

        [XmlElement(ElementName = "IncSIC")]
        public string IncSIC { get; set; }

        [XmlElement(ElementName = "IncMDO")]
        public string IncMDO { get; set; }

        [XmlElement(ElementName = "TagBIM")]
        public string TagBIM { get; set; }

    }

    [XmlRoot(ElementName = "PweElencoPrezzi")]
    public class PweElencoPrezzi
    {
        [XmlElement(ElementName = "EPItem")]
        public List<EPItem> EPItem { get; set; }
    }

    [XmlRoot(ElementName = "RGItem")]
    public class RGItem
    {
        [XmlAttribute("ID")]
        public string ID { get; set; }

        [XmlElement(ElementName = "IDVV")]
        public string IDVV { get; set; }

        [XmlElement(ElementName = "Descrizione")]
        public string Descrizione { get; set; }

        [XmlElement(ElementName = "PartiUguali")]
        public string PartiUguali { get; set; }

        [XmlElement(ElementName = "Lunghezza")]
        public string Lunghezza { get; set; }

        [XmlElement(ElementName = "Larghezza")]
        public string Larghezza { get; set; }

        [XmlElement(ElementName = "HPeso")]
        public string HPeso { get; set; }

        [XmlElement(ElementName = "Quantita")]
        public string Quantita { get; set; }

        [XmlElement(ElementName = "Flags")]
        public string Flags { get; set; }

    }


    [XmlRoot(ElementName = "PweVCMisure")]
	public class PweVCMisure
    {
        [XmlElement(ElementName = "RGItem")]
        public List<RGItem> RGItem { get; set; }

    }


    [XmlRoot(ElementName = "VCItem")]
    public class VCItem
    {
        [XmlAttribute("ID")]
        public string ID { get; set; }

        [XmlElement(ElementName = "IDEP")]
        public string IDEP { get; set; }

        [XmlElement(ElementName = "Quantita")]
        public string Quantita { get; set; }

        [XmlElement(ElementName = "DataMis")]
        public string DataMis { get; set; }

        [XmlElement(ElementName = "IDSpCat")]
        public string IDSpCat { get; set; }

        [XmlElement(ElementName = "IDCat")]
        public string IDCat { get; set; }

        [XmlElement(ElementName = "IDSbCat")]
        public string IDSbCat { get; set; }

        [XmlElement(ElementName = "CodiceWBS")]
        public string CodiceWBS { get; set; }

        [XmlElement(ElementName = "PweVCMisure")]
        public PweVCMisure VCMisure { get; set; }

    }

    [XmlRoot(ElementName = "PweVociComputo")]
    public class PweVociComputo
    {
        [XmlElement(ElementName = "VCItem")]
        public List<VCItem> VCItem { get; set; }
    }

    [XmlRoot(ElementName = "PweMisurazioni")]
    public class PweMisurazioni
    {
        [XmlElement(ElementName = "PweElencoPrezzi")]
        public PweElencoPrezzi ElencoPrezzi { get; set; }

        [XmlElement(ElementName = "PweVociComputo")]
        public PweVociComputo VociComputo { get; set; }
    }


    [XmlRoot(ElementName = "PweDocumento")]
    public class PweDocumento
    {
        [XmlElement(ElementName = "PweDatiGenerali")]
        public PweDatiGenerali DatiGenerali { get; set; }

        [XmlElement(ElementName = "PweMisurazioni")]
        public PweMisurazioni Misurazioni { get; set; }
    }



}
