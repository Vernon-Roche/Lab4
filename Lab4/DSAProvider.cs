using System.IO;
using System.Numerics;

namespace Lab4
{
    public class DSAProvider
    {
        private BigInteger p;
        private BigInteger q;
        private BigInteger h;
        private BigInteger x;
        private BigInteger k;
        private BigInteger g;
        private BigInteger y;

        public void SetParameters(BigInteger p, BigInteger q, BigInteger h, BigInteger x, BigInteger k)
        {
            if (!IsPrime(p) || !IsPrime(q))
                throw new ArgumentException("p и q должны быть простыми числами");
            if ((p - 1) % q != 0)
                throw new ArgumentException("q должен делить (p-1)");
            if (h <= 1 || h >= p)
                throw new ArgumentException("h должно быть в диапазоне (1, p-1)");
            if (x <= 0 || x >= q)
                throw new ArgumentException("x должен быть в диапазоне (0, q)");
            if (k <= 0 || k >= q)
                throw new ArgumentException("k должен быть в диапазоне (0, q)");

            this.p = p;
            this.q = q;
            this.h = h;
            this.x = x;
            this.k = k;

            // Вычисление g
            BigInteger exponent = (p - 1) / q;
            g = FastModExp(h, exponent, p);

            y = FastModExp(g, x, p);
        }

        public DSAGenerateSignatureResult GenerateSignature(byte[] fileContent)
        {
            BigInteger hash = ComputeHash(fileContent);
            BigInteger r, s;

            r = FastModExp(g, k, p) % q;
            if (r == 0)
            {
                throw new InvalidOperationException("r = 0, выберите другой k");
            }

            BigInteger kInv = FastModExp(k, q - 2, q);
            s = (kInv * (hash + x * r)) % q;
            if (s == 0)
            {
                throw new InvalidOperationException("s = 0, выберите другой k");
            }

            return new DSAGenerateSignatureResult(r, s, hash, g, y);
        }

        public DSAVerificationResult VerifySignature(byte[] fileContent, BigInteger r, BigInteger s)
        {
            if (r <= 0 || r >= q || s <= 0 || s >= q)
                return new DSAVerificationResult(0, 0, 0, 0, 0, false, false);

            BigInteger hash = ComputeHash(fileContent);
            BigInteger w = FastModExp(s, q - 2, q);
            BigInteger u1 = (hash * w) % q;
            BigInteger u2 = (r * w) % q;
            BigInteger v = (FastModExp(g, u1, p) * FastModExp(y, u2, p)) % p % q;

            return new DSAVerificationResult(hash, w, u1, u2, v, v == r, true);
        }

        private BigInteger ComputeHash(byte[] fileContent)
        {
            BigInteger H = 100;

            foreach (byte b in fileContent)
            {
                H = FastModExp(H + b, 2, q);
            }

            return H;
        }

        private bool IsPrime(BigInteger number)
        {
            if (number <= 1) return false;
            if (number == 2) return true;
            if (number % 2 == 0) return false;

            for (BigInteger i = 3; i * i <= number; i += 2)
                if (number % i == 0)
                    return false;

            return true;
        }

        private BigInteger FastModExp(BigInteger a, BigInteger exponent, BigInteger mod)
        {
            BigInteger result = 1;
            a = a % mod;

            while (exponent > 0)
            {
                if (exponent % 2 == 1)
                    result = (result * a) % mod;

                exponent >>= 1;
                a = (a * a) % mod;
            }

            return result;
        }
    }
}
