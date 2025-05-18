using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Lab4
{
    public record DSAVerificationResult(BigInteger Hash, BigInteger W, BigInteger U1, BigInteger U2, BigInteger V, bool Result, bool IsSignatureInBounds);
}
