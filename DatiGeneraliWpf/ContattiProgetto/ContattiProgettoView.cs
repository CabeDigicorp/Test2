using Commons;
using ContattiWpf.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatiGeneraliWpf.View
{
    public class ContattiProgettoView : ContattiView, SectionItemTemplateView
    {
        public int Code => (int) DatiGeneraliSectionItemsId.ContattiProgetto;

    }
}
