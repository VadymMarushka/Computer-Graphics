using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lab_03_CG.MVVM.Model
{
    public abstract class AddFractalDTO
    {
        public byte[] Image { get; set; }
        public string SuggestedTitle { get; set; } = string.Empty;
    }
}
