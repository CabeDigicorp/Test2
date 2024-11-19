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
    public class NumericFormat
    {
        [ProtoMember(1)]
        public string Format { get; set; } = NumericFormatHelper.DefaultFormat;

    }
}
