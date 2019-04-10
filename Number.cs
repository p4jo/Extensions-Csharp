using System;
using System.Collections.Generic;

namespace Extensions {
	[Serializable]
	public struct Number : IComparable { 
		/// <summary>
		/// Ziffern
		/// </summary>
		uint[] digits;
		/// <summary>
		/// powers of 10 (base)
		/// </summary>
		int p;
		sbyte Sign;
		static uint basis = int.MaxValue;
		public static int divisionAccuracy = 20, maximumDigits = int.MaxValue / 32;
		public static Number zero = new Number
		{
			Sign = 0,
			p = 0,
			digits = new uint[] { 0 }
		};

		public static Number MaxValue (int Digits = 100) {
			var r = new Number(Digits, int.MaxValue, 1);
			for (int i = 0; i < Digits; i++)
			{
				r.digits[i] = basis - 1;
			}
			return r;
		}

		Number(int count, int pow10 = 0, sbyte Positive = 0)
		{
			digits = new uint[count];
			p = pow10;
			Sign = Positive;
		}

		public uint this[int pow] => getdigit(pow);

		uint getdigit(int pow)
		{
			int stelle = pow - p;
			if (stelle >= 0 && stelle < digits.Length)
				return digits[stelle];
			return 0;
		}

		Number Simplify() {
			if (Sign == 0)
			{
				p = 0;
				digits = new uint[] { 0 };
				return this;
			}

			bool isZero = true;
			foreach (var digit in digits)
				if (digit != 0)
				{
					isZero = false;
					break;
				}
			if (isZero)
			{
				Sign = 0;
				p = 0;
				digits = new uint[] { 0 };
				return this;
			}
			uint firstNonZeroDigit = 0;
			long lastNonZeroDigit = digits.LongLength - 1;
			while (digits[firstNonZeroDigit] == 0) { firstNonZeroDigit++; } //es sind ja nicht alle 0
			while (digits[lastNonZeroDigit] == 0) { if (p == int.MaxValue) break;
				lastNonZeroDigit--;  p++; }
			digits = digits.getRange((int)firstNonZeroDigit, (int)lastNonZeroDigit);
			return this;
		}

		bool isOne()
		{
			return Sign == 1 && p == 0 && digits.Length == 1 && digits[0] == 1;
		}

		bool AbsIsOne()
		{
			return p == 0 && digits.Length == 1 && digits[0] == 1;
		}

		#region double-Konversion
		/// <summary>
		/// Initializes a new  <see cref="Number"/> with the value of d.
		/// </summary>
		/// <param name="d">The input double from which this Number is created</param>
		public Number(double d)
		{
			Sign = (sbyte)Math.Sign(d);
			d = Math.Abs(d);
			decimal dec = (decimal)d;
			if (dec % 1 == 0)
			{
				p = 0;
				decimal currentPotenz = basis;
				while (true)
				{
					if (dec % currentPotenz != 0)
						break;
					//temp = (int) temp / basis;
					p++;
					currentPotenz *= basis;
				}
			}
			else
			{
				p = -1;
				decimal currentPotenz = 1m / basis;
				while (true)
				{
					dec %= currentPotenz;
					if (dec == 0)
						break;
					p--;
					currentPotenz /= basis;
				}

			}

			List<uint> ziffern = new List<uint>();
			for (ulong D = (ulong)(d.toThePowerOf(-p));
				D != 0;
				D /= basis)   //darf nur ein oder zwei Mal ausgeführt werden, da ulong.MaxValue = uint.MaxValue^2, wird es hier aber auch, da
							// double auch 64-bit ist
			{
				ziffern.Insert(0, (uint)(D % basis));
			}
			digits = ziffern.ToArray();
			Simplify();
		}
		public Number(decimal d)
		{
			Sign = (sbyte)Math.Sign(d);
			d = Math.Abs(d);
			decimal dec = (decimal)d;
			if (dec % 1 == 0)
			{
				p = 0;
				decimal currentPotenz = basis;
				while (true)
				{
					if (dec % currentPotenz != 0)
						break;
					//temp = (int) temp / basis;
					p++;
					currentPotenz *= basis;
				}
			}
			else
			{
				p = -1;
				decimal currentPotenz = 1m / basis;
				while (true)
				{
					dec %= currentPotenz;
					if (dec == 0)
						break;
					p--;
					currentPotenz /= basis;
				}
				
			}

			List<uint> ziffern = new List<uint>();
			checked
			{
				for (ulong D = (ulong)(d.toThePowerOf(-p));
				   D != 0;
				   D /= basis)//darf nur ein oder zwei Mal ausgeführt werden, da ulong.MaxValue = uint.MaxValue^2
							  // Das ist hier wahrscheinlich ein Problem
				{
					ziffern.Insert(0, (uint)(D % basis));
				}
			}
			digits = ziffern.ToArray();
			Simplify();
		}

		/// <summary>
		/// Tries to convert the Number to a double.
		/// </summary>
		/// <returns>The double.</returns>
		public double ToDouble() {
			if (Sign == 0) return 0;
			double r = 0;
			for (uint i =0 ; i < digits.LongLength; i++)
				r += digits[i] * Math.Pow(basis, p + digits.LongLength -i- 1);
			return Sign * r; 
		}

		/// <summary>
		/// Tries to convert the Number to a double.
		/// </summary>
		/// <returns>The double.</returns>
		public static double ToDouble(Number n) {
			return n.ToDouble();
		}

		public static implicit operator double(Number n)
		{
			return n.ToDouble();
		}

		/// <summary>
		/// Returns a Number with the value of d.
		/// </summary>
		/// <returns>The Number.</returns>
		public static Number ToNumber(double d) {
			return new Number(d);
		}

		public static implicit operator Number(Double d) {
			return new Number(d);
		}
		#endregion

		#region unäre Operatoren
		public static Number operator -(Number a) {
			a.Sign *= -1;
			return a;
		}
		public static Number operator + (Number a) { //Betrag
			if (a.Sign == -1)
				a.Sign = 1;
			return a;
		}

		public static Number operator ++(Number a) {
			if (a.Sign == 0)
			{
				a.digits = new uint[] {1};
				a.p = 0;
				a.Sign = 1;
			}
			if (a.p > 0) {
				var dig = new uint[a.digits.Length + a.p];
				a.digits.CopyTo(dig, a.p);
				if (a.Sign == 1)
				{
					dig[0] = 1;
					a.digits = dig;
					a.p = 0;
				}
				else {
					return a + new Number
					{
						digits = new uint[] { 1 },
						p = 0,
						Sign = 1
					};
				}
			}

			if (a.p + a.digits.Length - 1 < 0)
			{
				var dig = new uint[a.p + 1];
				a.digits.CopyTo(dig, 0);
				if (a.Sign > 0)
					dig[dig.Length] = 1;
				else {
					if ((-a).isOne())
						return zero;
					for (int i = a.p; i < dig.Length - 1; i++)
					{
						a.digits[i] = basis - a.digits[i];
					}
					a.Sign = 1;
				}
				a.digits = dig;
			}



			return a;

			
		}
		#endregion

		#region Vergleich
		public int CompareTo(object other) {
			try
			{
				return CompareTo((Number) other);
			}
			catch { return CompareTo((double)other); }
		}

		public int CompareTo(Number n)
		{
			if (n.Sign == 0)
				return Sign;
			if (Sign == 0)
				return -n.Sign;
			if (Sign - n.Sign == -2)
				return -1;
			if (Sign - n.Sign == 2)
				return 1;
			int max = Math.Max(digits.Length + p, n.digits.Length + n.p);
			int min = Math.Min(p, n.p);
			for (int i = max; i >= min; i--)
			{
				var c = getdigit(i).CompareTo(n.getdigit(i));
				if (c != 0)
					return Sign * c; //Positive = n.Positive
			}
			return 0;
		}

		public override bool Equals(object other) {
			try
			{
				return this == (Number)other;
			}
			catch { return this == (double)other; }
		}

		public bool Equals(Number other)
		{
			return this == other;
		}

		public static bool operator <(Number a, Number b)
		{
			if (b.Sign == 0)
				return a.Sign < 0;
			if (a.Sign == 0)
				return b.Sign > 0;
			if (a.Sign - b.Sign == -2)
				return true;
			if (a.Sign - b.Sign == 2)
				return false;
			int max = Math.Max(a.digits.Length + a.p, b.digits.Length + b.p);
			int min = Math.Min(a.p, b.p);
			for (int i = max; i >= min; i--)
			{
				if (a.getdigit(i) > b.getdigit(i))
					return a.Sign == -1;
				if (a.getdigit(i) < b.getdigit(i))
					return a.Sign == 1;
			}
			return false;
		}

		public static bool operator >(Number a, Number b)
		{
			if (b.Sign == 0)
				return a.Sign > 0;
			if (a.Sign == 0)
				return b.Sign < 0;
			if (a.Sign - b.Sign == -2)
				return false;
			if (a.Sign - b.Sign == 2)
				return true;
			int max = Math.Max(a.digits.Length + a.p, b.digits.Length + b.p);
			int min = Math.Min(a.p, b.p);
			for (int i = max - 1; i >= min; i--)
			{
				if (a.getdigit(i) > b.getdigit(i))
					return a.Sign == 1;
				if (a.getdigit(i) < b.getdigit(i))
					return a.Sign == -1;
			}
			return false;
		}

		public static bool operator >=(Number a, Number b)
		{
			if (b.Sign == 0)
				return a.Sign >= 0;
			if (a.Sign == 0)
				return b.Sign <= 0;
			if (a.Sign - b.Sign == -2)
				return false;
			if (a.Sign - b.Sign == 2)
				return true;
			int max = Math.Max(a.digits.Length + a.p, b.digits.Length + b.p);
			int min = Math.Min(a.p, b.p);
			for (int i = max; i >= min; i--)
			{
				if (a.getdigit(i) < b.getdigit(i))
					return a.Sign == -1;
				if (a.getdigit(i) > b.getdigit(i))
					return a.Sign == 1;
			}
			return true;
		}

		public static bool operator <=(Number a, Number b)
		{
			if (b.Sign == 0)
				return a.Sign <= 0;
			if (a.Sign == 0)
				return b.Sign >= 0;
			if (a.Sign - b.Sign == -2)
				return true;
			if (a.Sign - b.Sign == 2)
				return false;
			int max = Math.Max(a.digits.Length + a.p, b.digits.Length + b.p);
			int min = Math.Min(a.p, b.p);
			for (int i = max; i >= min; i--)
			{
				if (a.getdigit(i) < b.getdigit(i))
					return a.Sign == 1;
				if (a.getdigit(i) > b.getdigit(i))
					return a.Sign == -1;
			}
			return true;
		}

		public static bool operator ==(Number a, Number b)
		{
			if (b.Sign == 0)
				return a.Sign == 0;
			if (a.Sign == 0)
				return true; //da b.Positive != 0
			if (a.Sign == -b.Sign)
				return false;
			int max = Math.Max(a.digits.Length + a.p, b.digits.Length + b.p);
			int min = Math.Min(a.p, b.p);
			for (int i = max; i >= min; i--)
			{
				if (a.getdigit(i) != b.getdigit(i))
					return false;
			}
			return true;
		}

		public static bool operator !=(Number a, Number b)
		{
			if (b.Sign == 0)
				return a.Sign != 0;
			if (a.Sign == 0)
				return true; //da b.Positive != 0
			if (a.Sign == -b.Sign)
				return true;
			int max = Math.Max(a.digits.Length + a.p, b.digits.Length + b.p);
			int min = Math.Min(a.p, b.p);
			for (int i = max; i >= min; i--)
			{
				if (a.getdigit(i) != b.getdigit(i))
					return true;
			}
			return false;
		}
		
		public override int GetHashCode() { return 0; }
		#endregion

		#region + und -
		/*
		public static Number operator +(Number a, Number b) {
			Number d = new Number(
				count: Math.Max(a.zi.Length + a.p, b.zi.Length + b.p) + Math.Max(-a.p, -b.p),
				pow10: Math.Min(a.p, b.p));
			uint i = 0;
			uint übertrag = 0;
			if (a.p < b.p)
			{
				for (; i < b.p - a.p; i++)
					d.zi[i] = a.zi[i];
				uint diff = i;
				for (i = 0; i < a.zi.Length - diff && i < b.zi.Length; i++)
				{
					uint currentSum = a.zi[i + diff] + b.zi[i] + übertrag;
					if (currentSum < basis)
					{
						d.zi[i + diff] = currentSum;
						übertrag = 0;
					}
					else
					{
						übertrag = currentSum / basis;
						d.zi[i + diff] = currentSum % basis;
					}
				}
				//i++;
				if (a.zi.Length - diff > b.zi.Length)
				{
					i += diff;	//+= diff, da in d und a sonst überall i + diff stehen würde
					
					uint currentSum = a.zi[i] + übertrag;
					if (currentSum < basis)
					{
						d.zi[i] = currentSum;
					}
					else
					{
						d.zi[i] = currentSum % basis; //int %
						i++;
						d.zi[i] = a.zi[i] + currentSum / basis; //übertrag - kann denk eh nur 1 sein
					}
					for (i++; i < a.zi.Length; i++)
						d.zi[i] = a.zi[i];
				}
				else if (a.zi.Length - diff < b.zi.Length)
				{
					uint currentSum = b.zi[i] + übertrag;
					if (currentSum < basis)
					{
						d.zi[i+diff] = currentSum;
					}
					else
					{
						d.zi[i+diff] = currentSum % basis; //int %
						i++;
						d.zi[i+diff] = b.zi[i] + currentSum / basis; //übertrag - kann denk eh nur 1 sein
					}
					for (i++; i < b.zi.Length; i++)
						d.zi[i+diff] = b.zi[i];
				}
			}
			else if (b.p < a.p)
			{
				for (; i < a.p - b.p; i++)
					d.zi[i] = b.zi[i];
				uint diff = i;
				for (i = 0; i < a.zi.Length && i < b.zi.Length - diff; i++)
				{
					uint currentSum = a.zi[i] + b.zi[i + diff] + übertrag;
					if (currentSum < basis)
					{
						d.zi[i + diff] = currentSum;
						übertrag = 0;
					}
					else
					{
						übertrag = currentSum / basis; //int /
						d.zi[i + diff] = currentSum % basis; //int %
					}
				}
				//i++;
				if (b.zi.Length - diff > a.zi.Length)
				{
					i += diff;
					uint currentSum = b.zi[i] + übertrag;
					if (currentSum < basis)
					{
						d.zi[i] = currentSum;
					}
					else
					{
						d.zi[i] = currentSum % basis; //int %
						i++;
						d.zi[i] = b.zi[i] + currentSum / basis; //übertrag - kann denk eh nur 1 sein
					}
					for (i++; i < b.zi.Length; i++)
						d.zi[i] = b.zi[i];
				}
				else if (b.zi.Length - diff < a.zi.Length)
				{
					uint currentSum = a.zi[i] + übertrag;
					if (currentSum < basis)
					{
						d.zi[i + diff] = currentSum;
					}
					else
					{
						d.zi[i + diff] = currentSum % basis; //int %
						i++;
						d.zi[i + diff] = a.zi[i] + currentSum / basis; //übertrag - kann denk eh nur 1 sein
					}
					for (i++; i < a.zi.Length; i++)
						d.zi[i + diff] = a.zi[i];
				}
			}
			else
			{
				for (i = 0; i < a.zi.Length && i < b.zi.Length; i++)
				{
					uint currentSum = a.zi[i] + b.zi[i] + übertrag;
					if (currentSum < basis)
					{
						d.zi[i] = currentSum;
						übertrag = 0;
					}
					else
					{
						übertrag = currentSum / basis;
						d.zi[i] = currentSum % basis;
					}
				}
				//i++;
				if (b.zi.Length > a.zi.Length)
				{
					uint currentSum = b.zi[i] + übertrag;
					if (currentSum < basis)
					{
						d.zi[i] = currentSum;
					}
					else
					{
						d.zi[i] = currentSum % basis; //int %
						i++;
						d.zi[i] = b.zi[i] + currentSum / basis; //übertrag - kann denk eh nur 1 sein
					}
					for (i++; i < b.zi.Length; i++)
						d.zi[i] = b.zi[i];
				}
				else if (b.zi.Length < a.zi.Length)
				{
					d.zi[i] = a.zi[i] + übertrag;
					for (i++; i < a.zi.Length; i++)
						d.zi[i] = a.zi[i];
					uint currentSum = a.zi[i] + übertrag;
					if (currentSum < basis)
					{
						d.zi[i] = currentSum;
					}
					else
					{
						d.zi[i] = currentSum % basis; //int %
						i++;
						d.zi[i] = a.zi[i] + currentSum / basis; //übertrag - kann denk eh nur 1 sein
					}
					for (i++; i < a.zi.Length; i++)
						d.zi[i] = a.zi[i];
				}
			}

			return d;
		}
		*/
		public static Number operator +(Number a, Number b)
		{
			if (a.Sign == 0)
				return b;
			if (b.Sign == 0)
				return a;
			if (a.Sign == 1 && b.Sign == -1)
				return a - (-b);
			if (a.Sign == -1 && b.Sign == 1)
				return b - (-a);

			int max = Math.Max(a.digits.Length + a.p, b.digits.Length + b.p);
			int min = Math.Min(a.p, b.p);
			int count = max - min;
			Number d = new Number
			{
				digits = new uint[count],
				p = min,
				Sign = a.Sign //a.Positive == b.Positive
			};
			int currentPow = 0;
			uint übertrag = 0;
			for (currentPow = min; currentPow < max; currentPow++)
			{
				uint currentSum = a.getdigit(currentPow) + b.getdigit(currentPow) + übertrag;
				if (currentSum < basis)
				{
					d.digits[count - currentPow] = currentSum;
					übertrag = 0;
				}
				else
				{
					übertrag = currentSum / basis;
					d.digits[count - currentPow] = currentSum % basis;
				}
			}
			if (übertrag > 0)
				d.digits = d.digits.Add(übertrag);
			return d.Simplify();
		}

		public static Number operator -(Number a, Number b)
		{
			if (a.Sign == 0)
				return -b;
			if (b.Sign == 0)
				return a;
			/*if (a.Positive == 1 && b.Positive == -1)
				return a + (-b);
			if (a.Positive == -1 && b.Positive == 1)
				return a + (-b);*/
			if (a.Sign + b.Sign == 0)
				return a + (-b);
			if (b > a)
				return -(b - a); //- rechnet nur mit positiven Zahlen ab jetzt

			int min = Math.Min(a.p, b.p);
			int max = Math.Max(a.digits.Length + a.p, b.digits.Length + b.p);
			Number d = new Number(count: max - min, pow10: min);
			int currentPow = 0;
			bool übertrag = false;
			for (currentPow = min; currentPow < max; currentPow++)
			{
				long currentSum = a.getdigit(currentPow) - b.getdigit(currentPow) - (übertrag ? 1 : 0);
				if (currentSum < 0)
				{
					d.digits[currentPow - min] = basis + (uint)(-currentSum);
					übertrag = true;
				}
				else
				{
					d.digits[currentPow - min] = (uint) currentSum;
					übertrag = false;
				}
			}
			return d.Simplify();
		}

		public static Number operator +(Number a, double b)
		{
			return a + new Number(b);
		}

		public static Number operator +(double a, Number b)
		{
			return b + new Number(a);
		}

		public static Number operator -(Number a, double b)
		{
			return a - new Number(b);
		}

		public static Number operator -(double a, Number b)
		{
			return b - new Number(a);
		}

		#endregion

		#region * und /
		public static Number operator *(Number a, Number b)
		{
			int posit = a.Sign * b.Sign; //a.Positive == b.Positive ? 1 : -1 braucht auch eine Umwandlung nach sbyte *facepalm*
			if (posit == 0) return zero;
			if (a.AbsIsOne()) return (a.Sign == 1 ? b : -b);
			if (b.AbsIsOne()) return (b.Sign == 1 ? a : -a);
			int max = (a.digits.Length - 1 + a.p) * (b.digits.Length - 1 + b.p); //höchste pows
			int min = a.p * b.p; //niedrigste pows
			int count = max - min + 2; //+2, da bei der höchsten Ziffernmultiplikation, der mit der Basis max, noch bis ausschließlich basis² rauskommen kann + übermäßiger Übertrag
			
			var partialSums = new ulong[count];
			for (uint ia = 0; ia < a.digits.LongLength; ia++)
			{
				for (uint ib = 0; ib < b.digits.LongLength; ib++)
				{
					partialSums[(ia + a.p) * (ib + b.p) - min] += a.digits[ia] * b.digits[ib]; //ia + a.p ist die current power in a, selbiges für b
				}
			} 
			Number d = new Number(count, min, (sbyte)posit);
			for (int ip = 0; ip < count; ip++)
			{
				long cpp = ip + min;
				if (partialSums[ip] > basis)
				{
					partialSums[ip + 1] += partialSums[ip] / basis; //sollte beim höchsten ip nicht mehr vorkommen
					d.digits[count - cpp] = (uint)(partialSums[ip] % basis);
				}
				else
				{
					d.digits[count - cpp] = (uint)partialSums[ip];
				}
			}
			return d.Simplify();
		}

		public static Number operator / (Number a, Number b)
		{
			if (b.Sign == 0) throw new DivideByZeroException("You divided "+ a + " by 0");
			return a;
		}
		/// <summary>
		/// * base^p
		/// </summary>
		public static Number operator >> (Number a, int p) 
		{
			a.p += p;
			return a;
		}
		///*base^-p
		public static Number operator << (Number a, int p) 
		{
			a.p -= p;
			return a;
		}

		public static Number operator *(Number a, double b)
		{
			return a * new Number(b);
		}

		public static Number operator *(double a, Number b)
		{
			return b * new Number(a);
		}

		public static Number operator /(Number a, double b)
		{
			return a / new Number(b);
		}

		public static Number operator /(double a, Number b)
		{
			return b / new Number(a);
		}
		#endregion

		#region ToString
		public string ToString(string numberFormat = "D")
		{
			return "Ziffern: _A /nVorzeichen: _B /n Grundpotenz: _C".format(ToString(digits, ",", numberFormat), Sign.ToString(), p.ToString());
		}
		public string ToString(int @base = 10)
		{
			if (Sign == 0)
				return "0";
			string r = Sign == -1 ? "-" : "";
			
			return r; //Sign == -1 ? "-" : "Ziffern: _A /nVorzeichen: _B /n Grundpotenz: _C".format(ToString(digits, @base), Sign.ToString(), p.ToString());
		}

		static string ToString(uint[] list, string separator = "", string numberFormat = "D")
		{
			if (list.Length == 0)
				return "";
			string res = "";
			int i = 0;
			for (; i < list.Length - 1; i++)
			{
				res += list[i].ToString(numberFormat) + separator;
			}
			return res + list[i].ToString(numberFormat);
		}

		#endregion
	}
}