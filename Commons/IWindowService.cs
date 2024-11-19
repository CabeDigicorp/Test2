using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
    public interface IWindowService
    {
        bool CodiceAttributoWindow(ref string codiceAttributo);
        bool SelectNumberFormatsWnd(ref List<string> formats, bool isSingleSelection);
    }
}
