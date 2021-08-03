using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EAKompensator
{
    interface IValue<T>
    {
        T Value { get; set; }
    }
}
