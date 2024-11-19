using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace PrezzariWpf
{
	/* 
	Licensed under the Apache License, Version 2.0
    
	http://www.apache.org/licenses/LICENSE-2.0
	*/

	[XmlRoot(ElementName = "Lingua")]
	public class Lingua
	{
		[XmlElement(ElementName = "Descrizione")]
		public string Descrizione { get; set; }
		[XmlElement(ElementName = "DescrizioneBreve")]
		public string DescrizioneBreve { get; set; }
		[XmlElement(ElementName = "Binario")]
		public string Binario { get; set; }
		[XmlAttribute(AttributeName = "Lingua")]
		public string _Lingua { get; set; }
	}

	[XmlRoot(ElementName = "Descrizione")]
	public class Descrizione
	{
		[XmlElement(ElementName = "Lingua")]
		public Lingua Lingua { get; set; }
		[XmlElement(ElementName = "Testo")]
		public string Testo { get; set; }
	}

	[XmlRoot(ElementName = "Capitolo")]
	public class Capitolo
	{
		[XmlElement(ElementName = "IdCapitolo")]
		public string IdCapitolo { get; set; }
		[XmlElement(ElementName = "Codice")]
		public string Codice { get; set; }
		[XmlElement(ElementName = "Descrizione")]
		public Descrizione Descrizione { get; set; }
		[XmlElement(ElementName = "Capitolo")]
		public List<Capitolo> Capitoli { get; set; }
	}

	[XmlRoot(ElementName = "Capitoli")]
	public class Capitoli
	{
		[XmlElement(ElementName = "Capitolo")]
		public List<Capitolo> Capitolo { get; set; }
	}

	[XmlRoot(ElementName = "Arrotondamento")]
	public class Arrotondamento
	{
		[XmlAttribute(AttributeName = "Da")]
		public string Da { get; set; }
		[XmlAttribute(AttributeName = "A")]
		public string A { get; set; }
		[XmlAttribute(AttributeName = "Arrotondamento")]
		public string _Arrotondamento { get; set; }
	}

	[XmlRoot(ElementName = "Arrotondamenti")]
	public class Arrotondamenti
	{
		[XmlElement(ElementName = "Arrotondamento")]
		public List<Arrotondamento> Arrotondamento { get; set; }
	}

	[XmlRoot(ElementName = "Tipologia")]
	public class Tipologia
	{
		[XmlElement(ElementName = "IdTipologia")]
		public string IdTipologia { get; set; }
		[XmlElement(ElementName = "Nome")]
		public string Nome { get; set; }
		[XmlElement(ElementName = "Descrizione")]
		public Descrizione Descrizione { get; set; }
		[XmlElement(ElementName = "Formato")]
		public string Formato { get; set; }
		[XmlElement(ElementName = "Arrotondamenti")]
		public Arrotondamenti Arrotondamenti { get; set; }
	}

	[XmlRoot(ElementName = "Tipologie")]
	public class Tipologie
	{
		[XmlElement(ElementName = "Tipologia")]
		public Tipologia Tipologia { get; set; }
		[XmlElement(ElementName = "IdTipologiaCorrente")]
		public string IdTipologiaCorrente { get; set; }
	}

	[XmlRoot(ElementName = "Prezzo")]
	public class Prezzo
	{
		[XmlElement(ElementName = "IdTipologia")]
		public string IdTipologia { get; set; }
		[XmlElement(ElementName = "PrezzoAnalisi")]
		public string PrezzoAnalisi { get; set; }
		[XmlElement(ElementName = "PrezzoApplicazione")]
		public string PrezzoApplicazione { get; set; }
		[XmlElement(ElementName = "Formato")]
		public string Formato { get; set; }
		[XmlElement(ElementName = "OneriSicurezza")]
		public string OneriSicurezza { get; set; }
		[XmlElement(ElementName = "IsSicurezzaPercentuale")]
		public string IsSicurezzaPercentuale { get; set; }
		[XmlElement(ElementName = "Manodopera")]
		public string Manodopera { get; set; }
		[XmlElement(ElementName = "IsManodoperaPercentuale")]
		public string IsManodoperaPercentuale { get; set; }
		[XmlElement(ElementName = "Materiali")]
		public string Materiali { get; set; }
		[XmlElement(ElementName = "IsMaterialiPercentuale")]
		public string IsMaterialiPercentuale { get; set; }
		[XmlElement(ElementName = "Attrezzature")]
		public string Attrezzature { get; set; }
		[XmlElement(ElementName = "IsAttrezzaturePercentuale")]
		public string IsAttrezzaturePercentuale { get; set; }
		[XmlElement(ElementName = "Trasporti")]
		public string Trasporti { get; set; }
		[XmlElement(ElementName = "IsTrasportiPercentuale")]
		public string IsTrasportiPercentuale { get; set; }

	}

	[XmlRoot(ElementName = "Prezzi")]
	public class Prezzi
	{
		[XmlElement(ElementName = "Prezzo")]
		public Prezzo Prezzo { get; set; }
	}

	[XmlRoot(ElementName = "Articolo")]
	public class Articolo
	{
		[XmlElement(ElementName = "IdArticolo")]
		public string IdArticolo { get; set; }
		[XmlElement(ElementName = "Codice")]
		public string Codice { get; set; }
		[XmlElement(ElementName = "Descrizione")]
		public Descrizione Descrizione { get; set; }
		[XmlElement(ElementName = "UnitaMisura")]
		public string UnitaMisura { get; set; }
		[XmlElement(ElementName = "HasAnalisi")]
		public string HasAnalisi { get; set; }
		[XmlElement(ElementName = "IdCapitolo")]
		public string IdCapitolo { get; set; }
		[XmlElement(ElementName = "Prezzi")]
		public Prezzi Prezzi { get; set; }
		[XmlElement(ElementName = "IsForfait")]
		public string IsForfait { get; set; }
		[XmlElement(ElementName = "Numero")]
		public string Numero { get; set; }
		[XmlElement(ElementName = "Articolo")]
		public List<Articolo> Articoli { get; set; }
	}

	[XmlRoot(ElementName = "Articoli")]
	public class Articoli
	{
		[XmlElement(ElementName = "Articolo")]
		public List<Articolo> Articolo { get; set; }
	}

	[XmlRoot(ElementName = "Prezzario")]
	public class Prezzario1
	{
		[XmlElement(ElementName = "IdArchivio")]
		public string IdArchivio { get; set; }
		[XmlElement(ElementName = "Nome")]
		public string Nome { get; set; }
		[XmlElement(ElementName = "Tipo")]
		public string Tipo { get; set; }
		[XmlElement(ElementName = "Percorso")]
		public string Percorso { get; set; }
		[XmlElement(ElementName = "Note")]
		public string Note { get; set; }
		[XmlElement(ElementName = "FormatoIncManodopera")]
		public string FormatoIncManodopera { get; set; }
		[XmlElement(ElementName = "FormatoOneriSicurezza")]
		public string FormatoOneriSicurezza { get; set; }
		[XmlElement(ElementName = "Capitoli")]
		public Capitoli Capitoli { get; set; }
		[XmlElement(ElementName = "Tipologie")]
		public Tipologie Tipologie { get; set; }
		[XmlElement(ElementName = "Articoli")]
		public Articoli Articoli { get; set; }
	}

	[XmlRoot(ElementName = "XMLMosaico")]
	public class XMLMosaico
	{
		[XmlElement(ElementName = "Prezzario")]
		public Prezzario1 Prezzario { get; set; }
	}

	//[XmlRoot(ElementName = "Prescrizione")]
	//public class Prescrizione
	//{
	//	[XmlElement(ElementName = "Testo")]
	//	public string Testo { get; set; }
	//}

	//[XmlRoot(ElementName = "Rtf")]
	//public class Rtf
	//{
	//	[XmlElement(ElementName = "Testo")]
	//	public string Testo { get; set; }
	//}

	//[XmlRoot(ElementName = "DatoGenerale")]
	//public class DatoGenerale1
	//{
	//	[XmlElement(ElementName = "Nome")]
	//	public string Nome { get; set; }
	//	[XmlElement(ElementName = "Prescrizione")]
	//	public Prescrizione Prescrizione { get; set; }
	//	[XmlElement(ElementName = "Tipo")]
	//	public string Tipo { get; set; }
	//	[XmlElement(ElementName = "Rtf")]
	//	public Rtf Rtf { get; set; }
	//	[XmlElement(ElementName = "Data")]
	//	public Data Data { get; set; }
	//	[XmlElement(ElementName = "Testo")]
	//	public string Testo { get; set; }
	//	[XmlElement(ElementName = "DatoGenerale")]
	//	public List<DatoGenerale1> DatoGenerale { get; set; }
	//}

	//[XmlRoot(ElementName = "Data")]
	//public class Data
	//{
	//	[XmlElement(ElementName = "Data")]
	//	public string Data1 { get; set; }
	//	[XmlElement(ElementName = "Formato")]
	//	public string Formato { get; set; }
	//}

	//[XmlRoot(ElementName = "DatiGenerali")]
	//public class DatiGenerali
	//{
	//	[XmlElement(ElementName = "DatoGenerale")]
	//	public List<DatoGenerale1> DatoGenerale { get; set; }
	//}

	//[XmlRoot(ElementName = "Lingua")]
	//public class Lingua
	//{
	//	[XmlElement(ElementName = "Descrizione")]
	//	public string Descrizione { get; set; }
	//	[XmlElement(ElementName = "DescrizioneBreve")]
	//	public string DescrizioneBreve { get; set; }
	//	[XmlAttribute(AttributeName = "Lingua")]
	//	public string _Lingua { get; set; }
	//}

	//[XmlRoot(ElementName = "Descrizione")]
	//public class Descrizione
	//{
	//	[XmlElement(ElementName = "Lingua")]
	//	public Lingua Lingua { get; set; }
	//}

	//[XmlRoot(ElementName = "Capitolo")]
	//public class Capitolo
	//{
	//	[XmlElement(ElementName = "IdCapitolo")]
	//	public string IdCapitolo { get; set; }
	//	[XmlElement(ElementName = "Codice")]
	//	public string Codice { get; set; }
	//	[XmlElement(ElementName = "Descrizione")]
	//	public Descrizione Descrizione { get; set; }
	//	[XmlElement(ElementName = "Capitolo")]
	//	public List<Capitolo> Capitoli { get; set; }
	//}

	//[XmlRoot(ElementName = "Capitoli")]
	//public class Capitoli
	//{
	//	[XmlElement(ElementName = "Capitolo")]
	//	public List<Capitolo> Capitolo { get; set; }
	//}

	//[XmlRoot(ElementName = "Articolo")]
	//public class Articolo
	//{
	//	[XmlElement(ElementName = "IdArticolo")]
	//	public string IdArticolo { get; set; }
	//	[XmlElement(ElementName = "Codice")]
	//	public string Codice { get; set; }
	//	[XmlElement(ElementName = "Descrizione")]
	//	public Descrizione Descrizione { get; set; }
	//	[XmlElement(ElementName = "UnitaMisura")]
	//	public string UnitaMisura { get; set; }
	//	[XmlElement(ElementName = "HasAnalisi")]
	//	public string HasAnalisi { get; set; }
	//	[XmlElement(ElementName = "IdCapitolo")]
	//	public string IdCapitolo { get; set; }
	//	[XmlElement(ElementName = "Numero")]
	//	public string Numero { get; set; }
	//	[XmlElement(ElementName = "IsForfait")]
	//	public string IsForfait { get; set; }
	//	[XmlElement(ElementName = "Articolo")]
	//	public List<Articolo> Articoli { get; set; }
	//}

	//[XmlRoot(ElementName = "Articoli")]
	//public class Articoli
	//{
	//	[XmlElement(ElementName = "Articolo")]
	//	public List<Articolo> Articolo { get; set; }
	//}

	//[XmlRoot(ElementName = "Prezzario")]
	//public class Prezzario1
	//{
	//	[XmlElement(ElementName = "IdArchivio")]
	//	public string IdArchivio { get; set; }
	//	[XmlElement(ElementName = "Nome")]
	//	public string Nome { get; set; }
	//	[XmlElement(ElementName = "Tipo")]
	//	public string Tipo { get; set; }
	//	[XmlElement(ElementName = "Percorso")]
	//	public string Percorso { get; set; }
	//	[XmlElement(ElementName = "Note")]
	//	public string Note { get; set; }
	//	[XmlElement(ElementName = "FormatoIncManodopera")]
	//	public string FormatoIncManodopera { get; set; }
	//	[XmlElement(ElementName = "FormatoOneriSicurezza")]
	//	public string FormatoOneriSicurezza { get; set; }
	//	[XmlElement(ElementName = "DatiGenerali")]
	//	public DatiGenerali DatiGenerali { get; set; }
	//	[XmlElement(ElementName = "Capitoli")]
	//	public Capitoli Capitoli { get; set; }
	//	[XmlElement(ElementName = "Articoli")]
	//	public Articoli Articoli { get; set; }
	//}

	//[XmlRoot(ElementName = "XMLMosaico")]
	//public class XMLMosaico
	//{
	//	[XmlElement(ElementName = "Prezzario")]
	//	public Prezzario1 Prezzario { get; set; }
	//}

}
