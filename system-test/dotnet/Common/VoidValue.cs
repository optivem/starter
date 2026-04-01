using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Common
{
    public class VoidValue
    {
        private VoidValue() { }

        public static VoidValue Empty { get; } = new VoidValue();
    }
}
