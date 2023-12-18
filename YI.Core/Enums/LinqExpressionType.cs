using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YI.Core.Enums
{
    public enum LinqExpressionType
    {
        Equal = 0,//=
        NotEqual = 1,//!=
        GreaterThan,//>
        LessThan,//<
        ThanOrEqual,//>=
        LessThanOrEqual,//<=
        In,
        Contains,//Contains
        NotContains//NotContains
    }
}
