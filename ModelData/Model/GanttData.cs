using ProtoBuf;

namespace ModelData.Model
{

    [ProtoContract]
    public class GanttData
    {
        [ProtoMember(1)]
        public DateTime DataInizio { get; set; } = DateTime.Today;//NB: la data fine viene calcolata
        [ProtoMember(2)]
        public bool Offset { get; set; } = false;
        [ProtoMember(3)]
        public bool UseDefaultCalendar { get; set; } = false;
        [ProtoMember(4)]
        public TimeRuler TimeRulerCalendario { get; set; }
        [ProtoMember(5)]
        public TimeRuler TimeRulerAnonimo { get; set; }
        [ProtoMember(6)]
        public TimeRuler TimeRulerFeriale { get; set; }
        [ProtoMember(7)]
        public SettingGrid SettingGrid { get; set; }

        [ProtoMember(8)]
        public SettingChart SettingChart { get; set; }

        [ProtoMember(9)]
        public ProgrammazioneSAL ProgrammazioneSAL { get; set; }

        [ProtoMember(10)]
        public SchedulazioneValori SchedulazioneValori { get; set; }

    }

    [ProtoContract]
    public class TimeRuler
    {
        /// <summary>
        /// LocalizeMenuUnLivello, LocalizeMenuDueLivelli, LocalizeMenuTreLivelli
        /// </summary>
        [ProtoMember(1)]
        public int LayoutLevel { get; set; } = 1;
        [ProtoMember(2)]
        public bool HorizontalSeparator { get; set; } = true;
        [ProtoMember(3)]
        public bool VerticalSeparator { get; set; } = true;
        /// <summary>
        /// LocalizeSinistra,LocalizeCentro,LocalizeDestra
        /// </summary>
        [ProtoMember(4)]
        public int HorizontalAlignement { get; set; } = 1;
        [ProtoMember(5)]
        public List<TimeRulerDetail> TimeRulersDetail { get; set; }
        [ProtoMember(6)]
        public string CodiceStileScala { get; set; }

    }
    [ProtoContract]
    public class TimeRulerDetail
    {
        /// <summary>
        /// LocalizeAnni,LocalizeSemestri,LocalizeTrimestri,LocalizeMesi,LocalizeDecadi,LocalizeSettimane,LocalizeGiorni,LocalizeOre,LocalizeMinuti
        /// </summary>
        [ProtoMember(1)]
        public int UnitTime { get; set; } = 1;
        [ProtoMember(2)]
        public string Format { get; set; }
    }

    [ProtoContract]
    public class SettingGrid
    {
        /// <summary>
        /// LocalizeAnni,LocalizeSemestri,LocalizeTrimestri,LocalizeMesi,LocalizeDecadi,LocalizeSettimane,LocalizeGiorni,LocalizeOre,LocalizeMinuti
        /// </summary>
        [ProtoMember(1)]
        public bool VerticalLines { get; set; }
        [ProtoMember(2)]
        public bool HorizontalLines { get; set; }
        [ProtoMember(3)]
        public string HexadecimalColorVerticalLines { get; set; }
        [ProtoMember(4)]
        public string HexadecimalColorHorizontalLines { get; set; }
    }

    [ProtoContract]
    public class SettingChart
    {
        /// <summary>
        /// LocalizeAnni,LocalizeSemestri,LocalizeTrimestri,LocalizeMesi,LocalizeDecadi,LocalizeSettimane,LocalizeGiorni,LocalizeOre,LocalizeMinuti
        /// </summary>
        [ProtoMember(1)]
        public string HexadecimalTaskNode { get; set; }
        [ProtoMember(2)]
        public string HexadecimalHeaderTaskNode { get; set; }
        [ProtoMember(3)]
        public string HexadecimalConnectorStroke { get; set; }
        [ProtoMember(4)]
        public string HexadecimalNonWorkingHours { get; set; }
        [ProtoMember(5)]
        public string CodiceStileNote { get; set; }
        [ProtoMember(6)]
        public string HexadecimalCriticalPath { get; set; }
    }

    [ProtoContract]
    public class GuidToOpenOrClose
    {
        [ProtoMember(1)]
        public List<Guid> IndexWBSToExpande { get; set; }
        [ProtoMember(2)]
        public List<Guid> IndexWBSToCollapse { get; set; }
    }

    [ProtoContract]
    public class ProgrammazioneSAL
    {
        [ProtoMember(1)]
        public List<PuntoNotevolePerData> PuntiNotevoliPerData { get; set; }
        [ProtoMember(2)]
        public List<AttributoFoglioDiCalcolo> AttributiStandard { get; set; } = new List<AttributoFoglioDiCalcolo>();
    }

    [ProtoContract]
    public class PuntoNotevolePerData
    {
        [ProtoMember(1)]
        public DateTime Data { get; set; }
        [ProtoMember(2)]
        public bool IsSAL { get; set; }
        //[ProtoMember(2)]
        //public bool Offset { get; set; } = false;
        //[ProtoMember(3)]
        //public bool UseDefaultCalendar { get; set; } = false;
        //[ProtoMember(4)]
        //public TimeRuler TimeRulerCalendario { get; set; }
        //[ProtoMember(5)]
        //public TimeRuler TimeRulerAnonimo { get; set; }
    }


    [ProtoContract]
    public class SchedulazioneValori
    {
        [ProtoMember(1)]
        public int Periodo { get; set; }
        [ProtoMember(2)]
        public List<AttributoFoglioDiCalcolo> Attributi { get; set; } = new List<AttributoFoglioDiCalcolo>();

        [ProtoMember(3)]
        public DateTime DateFrom { get; set; }
    }
}
