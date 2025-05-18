using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Lab4
{
    public record DSAGenerateSignatureResult(BigInteger R, BigInteger S, BigInteger Hash, BigInteger G, BigInteger Y);
}
