using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stegoLearning.WinUI.model
{
    enum TiposFicheiroOrigem
    {
        png,
        bmp,
        jpeg,
        jpg,
        ico
    }

    enum TiposFicheiroDestino
    {
        png,
        bmp
    }
    //public class TiposFicheiro
    //{

    //    public IList<string> ListarTiposFicheiroOrigem()
    //    {
    //        IList<string> lista = (IList<string>)Enum.GetNames(typeof(TiposFicheiroOrigem));
    //        lista = lista.Select(x => "*." + x).ToList();

    //        return lista;
    //    }
    //}
}
