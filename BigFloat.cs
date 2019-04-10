using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Extensions
{
	public struct BigFloat: IComparable//, IConvertible
	{
		public BigInteger value;
		public long p;
		///////////////////////////////////
		public const uint basis = 10;
		public static int divisionAccuracy = 20;

		public static void Test() {
			refreshTimer("nö");
			BigFloat a = "7thzvqo34z8vpuhfpqn9384znvqp0w8edufn02398n 09809w8eüam09sdmf mm90qü+#+´1´´3ß0´239´59ß023ß58´9´12´ß´4|36";
			refreshTimer("a");
			BigFloat b = new BigFloat("1759729580907582317965890327821953705097852057923970352973259873920873852222222,25252525", 12);
			refreshTimer("b");
			var I = upshift(a.value, divisionAccuracy);
			refreshTimer("upshift");
			refreshTimer((a | 10) + "=a");
			refreshTimer((a | 12) + "=a");
			refreshTimer((b | 10) + "=b");
			refreshTimer((b | 12) + "=b");
		}

		static long now = DateTime.Now.Ticks;
		static void refreshTimer(object a = null) {
			Debug.Print((DateTime.Now.Ticks - now)+" "+a);
			now = DateTime.Now.Ticks;
		}

		#region standard

		BigFloat Simplify()
		{
			if (value == 0)
			{
				p = 0;
				return this;
			}
			while (value % basis == 0)
			{
				p++;
				value /= basis;
			}
			return this;
		}

		public static BigFloat one = new BigFloat { value = BigInteger.One, p = 0 };
		public static BigFloat zero = new BigFloat { value = BigInteger.Zero, p = 0 };
		public static BigFloat negativeOne = new BigFloat { value = BigInteger.MinusOne, p = 0 };

		#endregion

		#region conversion
		//Without Simplify, have that in mind. Unsimplified BigFloats shouldn't be exposed to the outside since they can produce
		// inconsistent behaivours
		public static BigFloat fromBigInteger(BigInteger i) =>
			new BigFloat { value = i, p = 0 };

		public BigFloat(BigInteger I, long power = 0)
		{
			value = I;
			p = power;
			Simplify();
		}

		public BigFloat(long I, long power = 0)
		{
			value = I;
			p = power;
			Simplify();
		}
		public BigFloat(ulong I, long power = 0)
		{
			value = I;
			p = power;
			Simplify();
		}

		public BigFloat(int I, long power = 0)
		{
			value = I;
			p = power;
			Simplify();
		}
		public BigFloat(uint I, long power = 0)
		{
			value = I;
			p = power;
			Simplify();
		}

		public BigFloat(sbyte I, long power = 0)
		{
			value = I;
			p = power;
			Simplify();
		}
		public BigFloat(byte I, long power = 0)
		{
			value = I;
			p = power;
			Simplify();
		}

		public BigFloat(short I, long power = 0)
		{
			value = I;
			p = power;
			Simplify();
		}
		public BigFloat(ushort I, long power = 0)
		{
			value = I;
			p = power;
			Simplify();
		}

		public BigFloat(float F, long power = 0)
		{
			p = power;
			if (F % 1 == 0)
			{
				while (F % basis == 0)
				{
					F /= basis;
					p++;
				}
			}
			else
			{
				while (F % 1 != 0)
				{
					F *= basis;
					p--;
				}
			}
			value = (BigInteger)F;
		}

		public BigFloat(double F, long power = 0)
		{
			p = power;
			if (F % 1 == 0)
			{
				while (F % basis == 0)
				{
					F /= basis;
					p++;
				}
			}
			else
			{
				while (F % 1 != 0)
				{
					F *= basis;
					p--;
				}
			}
			value = (BigInteger)F;
		}

		public BigFloat (decimal F, long power = 0)
		{
			p = power;
			if (F % 1 == 0)
			{
				while (F % basis == 0)
				{
					F /= basis;
					p++;
				}
			}
			else
			{
				while (F % 1 != 0)
				{
					F *= basis;
					p--;
				}
			}
			value = (BigInteger)F;
		}

		public BigFloat(string S, int inputBase = 10)
		{
			if (inputBase > 50 || inputBase < 1) inputBase = 10;
			value = 0;
			int digitsBeforeDD = 0, numberOfDigits = 0;
			if (inputBase == 10)
			{
				string b = "";
				for (int i = 0; i < S.Length; i++)
				{
					if (char.IsDigit(S[i]))
					{
						b += S[i];
						numberOfDigits++;
					}
					else if (digitsBeforeDD == 0 && (S[i] == '.' || S[i] == ','))
						digitsBeforeDD = numberOfDigits;
				}
				BigInteger.TryParse(b, out value);
			}
			else
				for (int i = 0; i < S.Length; i++)
				{
					int dig = digitValue(S[i]);
					if (dig > 0 && dig < inputBase)
					{
						value *= inputBase;
						value += dig;
						numberOfDigits++;
					}
					else if (digitsBeforeDD == 0 && (S[i] == '.' || S[i] == ','))
						digitsBeforeDD = numberOfDigits;
				}
			double afterDDD = (numberOfDigits - digitsBeforeDD) * Math.Log(inputBase, basis);
			double afterDDDafter = Math.Ceiling(afterDDD);
			double ratio = Math.Pow(basis, afterDDDafter) / Math.Pow(inputBase,afterDDD);
			//value *= ratio;
			value *= (BigInteger)(ratio * ((double)basis).toThePowerOf(divisionAccuracy));
			p = -(long)afterDDDafter - divisionAccuracy;
			Simplify();
		}
		/// <summary>
		/// 0123456789abcdefghijklmnopqrstuvwxyz(=35)[\]^_(=40)`!"#$(=45)?@&'~(=50) --bis zu 51 Stellen
		/// </summary>
		public static int digitValue(char c)
		{
			if (c >= 48 && c <= 57)
				return c - 48;
			if (char.ToUpper(c) < 97 && c >= 65)
				return char.ToUpper(c) - 55;
			if (c >= 33 && c <= 36)
				return c + 9;
			if (c == 63 || c == 64)
				return c - 17;
			if (c == 38 || c == 39)
				return c - 10;
			if (c == '~') return 50;
			return -1;
		}

		//////////////////////////////

		public static implicit operator BigFloat(BigInteger I) => new BigFloat(I);
		public static implicit operator BigFloat(long I) => new BigFloat(I);
		public static implicit operator BigFloat(ulong I) => new BigFloat(I);
		public static implicit operator BigFloat(int I) => new BigFloat(I);
		public static implicit operator BigFloat(uint I) => new BigFloat(I);
		public static implicit operator BigFloat(short I) => new BigFloat(I);
		public static implicit operator BigFloat(ushort I) => new BigFloat(I);
		public static implicit operator BigFloat(sbyte I) => new BigFloat(I);
		public static implicit operator BigFloat(byte I) => new BigFloat(I);

		public static implicit operator BigFloat(float F) => new BigFloat(F);
		public static implicit operator BigFloat(double F) => new BigFloat(F);
		public static implicit operator BigFloat(decimal F) => new BigFloat(F);

		public static implicit operator BigFloat(string S)
		{
			if (!S.Contains('|'))
				return new BigFloat(S);

			string sho = "";
			int di = S.IndexOf('|');
			for (int i = di; i < S.Length; i++)
				if (char.IsDigit(S[i]))
					sho += S[i];
			short.TryParse(sho, out short ba);

			/*
			if (S.Contains('|'))
				//short.TryParse(S.Substring('|').ToNumbers<char, short>().ToString(""));
				short.TryParse((string) S.Substring("|").TakeWhile((c)=>char.IsDigit(c)), out b);*/

			return new BigFloat(S.Substring(0,di),ba);
		}

		public override string ToString() => ToString(10);

		public string ToString(short outputBasis, char separator = '.')
		{
			if (outputBasis <= 1) outputBasis = 10;
			String r = "";
			for (long i = 1; value != 0; i++)
			{
				var mod = (int)(value % outputBasis);
				if (mod < 10)
					r = mod.ToString() + r;
				else
					r = (char)(mod + 55) + r;
				value /= outputBasis;
				if (i == -p)
					r = separator + r;
			}
			return r;
		}
		public static implicit operator string (BigFloat a) => a.ToString(10);
		/// <summary>
		/// Converts to a string in a given base
		/// </summary>
		public static string operator |(BigFloat a, short basis) => a.ToString(basis);
		/*
		public static BigFloat parse<T>(T other) where T : IConvertible {

		}

		public object ToType(Type type, IFormatProvider formatProvider = null) {
			if (type == typeof(BigInteger) || type == typeof(long) || type == typeof(ulong) || type == typeof(int)
					|| type == typeof(uint) || type == typeof(short) || type == typeof(ushort) || type == typeof(sbyte)
					|| type == typeof(byte))
				return 
		}
		*/
		#endregion

		#region comparison

		#endregion

		#region + und -

		public static BigFloat operator +(BigFloat a, BigFloat b)
		{
			var diff = a.p - b.p;
			a.value = upshift(a.value,diff) + upshift(b.value,-diff);
			if (diff < 0) a.p = b.p;
			else if (diff == 0) return a.Simplify();
			//in other cases, Simplify is not required, since a and b should already be simplified
			
			return a; 
		}

		public static BigFloat operator -(BigFloat a, BigFloat b)
		{
			var diff = a.p - b.p;
			a.value = upshift(a.value, diff) - upshift(b.value, -diff); //only max. one of them is changed by upshift
			if (diff < 0) a.p = b.p;
			else if (diff == 0) return a.Simplify();
			//in other cases, Simplify is not required, since a and b should already be simplified

			return a;
		}
		#endregion

		#region * und /
		public static BigFloat operator *(BigFloat a, BigFloat b)
		{
			if (b.value.IsOne)
			{
				a.p += b.p;
				return a;
			}
			else if ((-b.value).IsOne)
			{
				a.p += b.p;
				a.value = -a.value;
				return a;
			}
			else if (a.value.IsOne)
			{
				b.p += a.p;
				return b;
			}
			else if ((-a.value).IsOne)
			{
				b.p += a.p;
				b.value = -b.value;
				return b;
			}
			a.p += b.p;
			a.value *= b.value;
			return a.Simplify();
		}

		public static BigFloat operator / (BigFloat a, BigFloat b)
		{
			if (b.value.IsZero) throw new DivideByZeroException("You divided " + a + " by 0");
			if (b.value.IsOne) {
				a -= b.p;
				return a;
			}
			else if ((-b.value).IsOne) {
				a -= b.p;
				a.value = -a.value;
			}
			else if (a.value.IsOne)
			{
				b -= a.p;
				return b;
			}
			else if ((-a.value).IsOne)
			{
				b -= a.p;
				b.value = -b.value;
			}
			a.p -= b.p + divisionAccuracy;
			a.value = upshift(a.value, divisionAccuracy) / b.value;
			return a.Simplify();
		}
		/// <summary>
		/// * base^<paramref name="p"/>
		/// </summary>
		public static BigFloat operator >>(BigFloat a, int p)
		{
			a.p += p;
			return a;
		}
		/// <summary>
		/// * base^<paramref name="p"/>
		/// </summary>
		public static BigFloat operator <<(BigFloat a, int p)
		{
			a.p -= p;
			return a;
		}

		static BigInteger upshift(BigInteger I, long n) //>> für den value Wert
		{
			for (int i = 1; i <= n; i++)
				I *= basis;
			return I;
		}

		static BigInteger shift(BigInteger I, long n) //>> und << für den value Wert
		{
			for (int i = 1; i <= n; i++)
				I *= basis;
			for (int i = -1; i >= n; i--)
				I /= basis;
			return I;
		}

		#endregion

		#region unäre Operatoren
		public static BigFloat operator -(BigFloat a)
		{
			a.value = -a.value;
			return a;
		}
		public static BigFloat operator +(BigFloat a)
		{ //Betrag
			if (a.value.Sign == -1)
				a.value = -a.value;
			return a;
		}

		public static BigFloat operator ++(BigFloat a) => a + one;

		public static BigFloat operator --(BigFloat a) => a - one;
		#endregion

		#region Vergleich
		public int CompareTo(object other)
		{
			if (other is BigFloat)
				return CompareTo((BigFloat)other);
			return CompareTo((BigFloat)(double)other);
		}
		
		public int CompareTo(BigFloat n)
		{
			if (this == n) return 0;
			if (this < n) return -1;
			return 1;
		}

		public override bool Equals(object other)
		{
			if (other is BigFloat)
				return this == (BigFloat)other;
			return this == (double)other; 
		}

		public bool Equals(BigFloat other)
		{
			return this == other;
		}

		public static bool operator <=(BigFloat a, BigFloat b)
		{
			var diff = a.p - b.p;
			if (diff > 0)
			{
				if (a.value > b.value) return false;
				for (int i = 1; i <= diff; i++)
				{
					a.value *= basis;
					if (a.value > b.value) return false;
				}
				return true;
			}
			if (diff < 0)
			{
				if (a.value <= b.value) return true;
				for (int i = -1; i >= diff; i--)
				{
					b.value *= basis;
					if (a.value <= b.value) return true;
				}
				return false;
			}
			return a.value <= b.value;
		}

		public static bool operator <(BigFloat a, BigFloat b)
		{
			var diff = a.p - b.p;
			if (diff > 0)
			{
				if (a.value >= b.value) return false;
				for (int i = 1; i <= diff; i++)
				{
					a.value *= basis;
					if (a.value >= b.value) return false;
				}
				return true;
			}
			if (diff < 0)
			{
				if (a.value < b.value) return true;
				for (int i = -1; i >= diff; i--)
				{
					b.value *= basis;
					if (a.value < b.value) return true;
				}
				return false;
			}
			return a.value < b.value;
		}

		public static bool operator >=(BigFloat b, BigFloat a) //einfach umgetauscht von <=
		{
			var diff = a.p - b.p;
			if (diff > 0)
			{
				if (a.value > b.value) return false;
				for (int i = 1; i <= diff; i++)
				{
					a.value *= basis;
					if (a.value > b.value) return false;
				}
				return true;
			}
			if (diff < 0)
			{
				if (a.value <= b.value) return true;
				for (int i = -1; i >= diff; i--)
				{
					b.value *= basis;
					if (a.value <= b.value) return true;
				}
				return false;
			}
			return a.value <= b.value;
		}

		public static bool operator >(BigFloat b, BigFloat a) //einfach umgetauscht von <
		{
			var diff = a.p - b.p;
			if (diff > 0)
			{
				if (a.value >= b.value) return false;
				for (int i = 1; i <= diff; i++)
				{
					a.value *= basis;
					if (a.value >= b.value) return false;
				}
				return true;
			}
			if (diff < 0)
			{
				if (a.value < b.value) return true;
				for (int i = -1; i >= diff; i--)
				{
					b.value *= basis;
					if (a.value < b.value) return true;
				}
				return false;
			}
			return a.value < b.value;
		}


		public static bool operator ==(BigFloat a, BigFloat b)
		{
			if (a.p != b.p)
				return false;
			return a.value == b.value;
		}

		public static bool operator !=(BigFloat a, BigFloat b)
		{
			if (a.p != b.p)
				return true;
			return a.value != b.value;
		}
	
		public override int GetHashCode() { return 0; }
		#endregion

		#region Statische Methoden
		public static ref BigFloat Max(ref BigFloat a, ref BigFloat b)
		{
			if (b > a) return ref b;
			return ref a;
		}

		public static BigFloat Max(BigFloat a, BigFloat b)
		{
			if (b > a) return b;
			return a;
		}
		public static BigFloat Max(BigFloat a, params BigFloat[] b)
		{
			foreach (BigFloat c in b)
				if (c > a)
					a = c;
			return a;
		}
		public static BigFloat Min(BigFloat a, BigFloat b)
		{
			if (b < a) return b;
			return a;
		}

		public static BigFloat Min(BigFloat a, params BigFloat[] b)
		{
			foreach (BigFloat c in b)
				if (c < a)
					a = c;
			return a;
		}
		#endregion
	}
}
