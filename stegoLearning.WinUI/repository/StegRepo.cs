using stegoLearning.WinUI.model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stegoLearning.WinUI.repository
{
    public class StegRepo
    {
        private StegObj stegForm;
        public StegRepo()
        {
            stegForm = new StegObj(null) {
                Mensagem = "teste",
                OpcaoBit = "1",
                PalavraPasse = "",
                ImagemOriginal = null,
                ImagemStego = null
            };
        }
        public StegObj returnStegRepo()
        {
            return stegForm;
        }
    }
}
