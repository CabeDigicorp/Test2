using Commons;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    [ProtoContract]
    public class FogliDiCalcoloData
    {
        [ProtoMember(1)]
        public List<FoglioDiCalcolo> FoglioDiCalcolo { get; set; }

        [ProtoMember(2)]
        public byte[] SerializedData { get; set; } = null;

        [ProtoMember(3)]
        public List<string> SheetNameToPrint { get; set; }

        [ProtoMember(4)]
        public int FitToPageKey { get; set; } = -1;
    }
    [ProtoContract]
    public class FoglioDiCalcolo
    {
        [ProtoMember(1)]
        public string SezioneKey { get; set; }
        [ProtoMember(2)]
        public string Foglio { get; set; }
        [ProtoMember(3)]
        public string Tabella { get; set; }
        [ProtoMember(4)]
        public List<AttributoFoglioDiCalcolo> AttributiFormuleFoglioDiCalcolo { get; set; } = new List<AttributoFoglioDiCalcolo>();
        [ProtoMember(5)]
        public List<AttributoFoglioDiCalcolo> AttributiStandardFoglioDiCalcolo { get; set; } = new List<AttributoFoglioDiCalcolo>();
    }
    [ProtoContract]
    public class AttributoFoglioDiCalcolo
    {
        [ProtoMember(1)]
        public string CodiceOrigine { get; set; }
        public string DefinizioneAttributo { get; set; }
        [ProtoMember(2)]
        public string Etichetta { get; set; }
        [ProtoMember(3)]
        public bool IsChecked { get; set; }
        [ProtoMember(4)]
        public string Formula { get; set; }
        [ProtoMember(5)]
        public string Note { get; set; }
        [ProtoMember(6)]
        public string SezioneRiferita { get; set; }
        [ProtoMember(7)]
        public bool Amount { get; set; }
        [ProtoMember(8)]
        public bool ProgressiveAmount { get; set; }
        [ProtoMember(9)]
        public bool ProductivityPerHour { get; set; }
    }
}
