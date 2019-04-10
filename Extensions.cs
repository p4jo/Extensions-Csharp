using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Forms;

namespace Extensions
{
	public static class Extension
	{
		#region Shuffle
		//aus'm Internet: Der Fisher-Yates algorithm. Er ist gut, aber System.Random ist nicht gut (Zeitbasiert, selbst bessere
		//pseudorandom Generatoren machen nicht alle Möglichkeiten), aber reicht schon
		public static void Shuffle<T>(this Random rng, List<T> list)
		{
			int n = list.Count;
			while (n > 1)
			{
				int k = rng.Next(n--);
				T temp = list[n];
				list[n] = list[k];
				list[k] = temp;
			}
		}

		public static void Shuffle<T>(this Random rng, IEnumerable<T> array)
		{
			var list = new List<T>(array);
			int n = list.Count;
			while (n > 1)
			{
				int k = rng.Next(n--);
				T temp = list[n];
				list[n] = list[k];
				list[k] = temp;
			}
		}
		public static void Shuffle<T>(this List<T> list, Random rng = null)
		{
			if (rng == null) rng = new Random();
			int n = list.Count;
			while (n > 1)
			{
				int k = rng.Next(n--);
				T temp = list[n];
				list[n] = list[k];
				list[k] = temp;
			}
		}
		
		public static void Shuffle<T>(this IEnumerable<T> array, Random rng = null)
		{
			if (rng == null) rng = new Random();
			var list = new List<T>(array);
			int n = list.Count;
			while (n > 1)
			{
				int k = rng.Next(n--);
				T temp = list[n];
				list[n] = list[k];
				list[k] = temp;
			}
		}
		#endregion

		#region other List operations
		public static T removeAt<T>(this List<T> list, int index = 0)
		{
			T c = list[index];
			list.RemoveAt(index);
			return c;
		}

		public static T[] Add<T>(this T[] array, T element)
		{
			var a = new T[array.Length + 1];
			array.CopyTo(a, 0);
			a[array.Length] = element;
			//array = a; //geht wahrscheinlich gar net -> jep
			return a;
		}

		public static T[] AddRange<T>(this T[] array, T[] range)
		{
			var a = new T[array.Length + range.Length];
			array.CopyTo(a, 0);
			range.CopyTo(a, array.Length);
			//array = a; //geht wahrscheinlich gar net -> jep
			return a;
		}
		/* //Geht leider auch nicht. this ref Parameter müssen direkt Werttyp oder generischer Typ sein
		public static ElementsType[] AddReal<ArrayType,ElementsType>(this ref ArrayType array, ElementsType element) where ArrayType: IEnumerable<ElementsType>
		{
			var a = new ElementsType[array.Count() + 1];
			array.ToArray().CopyTo(a, 0);
			array = (ArrayType) (IEnumerable<ElementsType>) a; 
			return a;
		}*/

		public static int IndexOf<T>(this IEnumerable<T> array, T element)
		{
			return new List<T>(array).IndexOf(element);
		}

		public static T ElementAtOrFirstOrDefault<T>(this IEnumerable<T> array, int index, T @default = default(T))
		{
			if (index < array.Count())
				return array.ElementAt(index);
			else if (array.Count() > 0)
				return array.First();
			else
				return @default;
		}

		public static T ElementAtOrDefault<T>(this IEnumerable<T> array, int index, T @default)
		{
			if (index < array.Count())
				return array.ElementAt(index);
			else
				return @default;
		}

		public static T[] getRange<T>(this IEnumerable<T> array, int startIndex)
		{
			List<T> list = new List<T>(array);
			if (startIndex >= list.Count)
				return new T[0];
			else if (startIndex <= 0)
				return array.ToArray();
			return list.GetRange(startIndex, list.Count - startIndex).ToArray();
		}

		public static T[] getRange<T>(this IEnumerable<T> array, int startIndex, int endIndex)
		{
			List<T> list = new List<T>(array);
			if (startIndex >= endIndex)
				return new T[0];
			if (startIndex < 0) 
				startIndex = 0;
			if (endIndex > list.Count)
				endIndex = list.Count;
			return list.GetRange(startIndex, endIndex - startIndex + 1).ToArray();
		}

		public static int[] ConvertToInt<T>(this IEnumerable<T> array)
		{
			var list = new List<int>(0);/*
			try
			{
				for (int i = 0; i < array.Count(); i++)
				{
					list.Add(Convert.ToInt32(array.ElementAt(i)));
				}
				return list.ToArray();
			}
			catch { return new int[array.Count()]; }*/
			for (int i = 0; i < array.Count(); i++)
			{
				//list.Add(i.Try(()=>Convert.ToInt32(array.ElementAt(i)), 0));
				try { list.Add(Convert.ToInt32(array.ElementAt(i))); }
				catch { list.Add(0); }
			}
			return list.ToArray();
		}

		public static T MaxNumberOfSame<T>(this IEnumerable<T> list, Func<T, T, bool> comparator = null)
		{
			if (comparator == null) comparator = (t1, t2) => t1.Equals(t2);
			Dictionary<T, ushort> res = new Dictionary<T, ushort>();
			bool done = false;
			foreach (T element in list)
			{
				foreach (T key in res.Keys)
				{
					if (comparator(element, key))
					{
						res[key]++;
						done = true;
						break;
					}
				}
				if (!done)
					res.Add(element, 1);
			}
			return list.ElementAtOrDefault(res.Values.IndexOfMax());
		}

		public static int IndexOfMaxNumberOfSame<T>(this IEnumerable<T> list, Func<T, T, bool> comparator = null)
		{
			if (comparator == null) comparator = (t1, t2) => t1.Equals(t2);
			Dictionary<T, ushort> res = new Dictionary<T, ushort>();
			bool done = false;
			foreach (T element in list)
			{
				foreach (T key in res.Keys)
				{
					if (comparator(element, key))
					{
						res[key]++;
						done = true;
						break;
					}
				}
				if (!done)
					res.Add(element, 1);
			}
			return res.Values.IndexOfMax();
		}

		public static int IndexOfMax<T>(this IEnumerable<T> list, Func<T, T, bool> greaterThanComparer = null)
		{
			if (greaterThanComparer == null)
			{
				if (typeof(T).GetInterface("IComparable") != null)
					greaterThanComparer = (t1, t2) => ((IComparable)t1).CompareTo(t2) > 0;
				else
					return list.IndexOf(list.Max()); //sollte dasselbe Problem wie ich haben, aber...
			}
			T currentMax = list.FirstOrDefault();
			int index = 0;
			for (int i = 1; i < list.Count(); i++)
			{
				if (greaterThanComparer(list.ElementAt(i), currentMax))
				{
					currentMax = list.ElementAt(i);
					index = i;
				}
			}
			return index;
		}

		public static int IndexOfMin<T>(this IEnumerable<T> list, Func<T, T, bool> smallerThanComparer = null)
		{
			if (smallerThanComparer == null)
			{
				//if (T is IComparable) { }
				if (typeof(T).GetInterface("IComparable") != null)
					smallerThanComparer = (t1, t2) => ((IComparable)t1).CompareTo(t2) > 0;
				else
					return -1; 
			}
			T currentMin = list.FirstOrDefault();
			int index = 0;
			for (int i = 1; i < list.Count(); i++)
			{
				if (smallerThanComparer(list.ElementAt(i), currentMin))
				{
					currentMin = list.ElementAt(i);
					index = i;
				}
			}
			return index;
		}

		public static string nthWord<T>(this string input, int n)
		{
			int startIndex = -1;
			bool firstWhiteSpace = false;
			for (int i = 0; i < input.Length; i++)
			{
				if (char.IsWhiteSpace(input[i]))
				{
					if (firstWhiteSpace)
					{
						n--;
						firstWhiteSpace = false;
					}
				}
				else
				{
					firstWhiteSpace = true;
				}
				if (startIndex == -1 && n == 0)
					startIndex = i;
				else if (startIndex >= 0 && n == -1)
					return input.Substring(startIndex, i - startIndex);
			}
			return "";
		}

		public static string ToString<T>(this IEnumerable<T> list, string separator = "")
		{
			if (list.Count() == 0)
				return "";
			string res = "";
			int i = 0;
			for (; i < list.Count() - 1; i++)
			{
				res += list.ElementAt(i).ToString() + separator;
			}
			return res + list.ElementAt(i).ToString();
		}

		public static string ToString<T>(this IEnumerable<T> list, string beforeString = "", string afterString = "")
		{
			string res = "";
			for (int i = 0; i < list.Count(); i++)
			{
				res += beforeString + list.ElementAt(i).ToString() + afterString;
			}
			return res;
		}
		/// <param name="eachString">Enter {0} at the position(s) where the list element should be</param>
		public static string ToStringF<T>(this IEnumerable<T> list, string eachString = "{0}")
		{
			if (!eachString.Contains("{0}"))
				eachString += "{0}";
			string res = "";
			foreach (T el in list)
			{
				res += String.Format(eachString, el.ToString());
			}
			return res;
		}

		public static List<string> ToStrings<T>(this IEnumerable<T> list)
		{
			var res = new List<string>();
			foreach (T s in list) {
				res.Add(s.ToString());
			}
			return res;
		}

		public static List<outputT> ToNumbers<outputT>(this IEnumerable<object> list) {
			var res = new List<outputT>();
			foreach (object s in list)
			{
				res.Add(s.To<outputT>());
			}
			return res;
		}
		/// <summary>
		/// Gets the position of the nth element of the array that is the same as <paramref name="element"/>.
		/// If there are less than n such elements or n is out of bounds, returns -1.
		/// </summary>
		/// <typeparam name="T">Type of the elements</typeparam>
		/// <param name="array">The array or list this method is called from</param>
		/// <param name="element">The element to find the nth instance of</param>
		/// <param name="n">The one-based number of which element is searched</param>
		/// <returns>The zero-based index of the nth occurence of element</returns>
		public static int IndexOfNth<T>(this IEnumerable<T> array, T element, int n) {
			int res = array.IndexOf(element);
			if (res == -1 || n <= 1)
				return res;
			else
				return array.getRange(res).IndexOfNth(element, n - 1);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="products"></param>
		/// <param name="obj"></param>
		/// <param name="comparer">A delegate to check if two <typeparamref name="T"/> are the same. By default calls t1.compare(t2) or t1.Equals(t2)</param>
		/// <param name="refresher">What to do: Insert a Delegate that takes this list <paramref name="products"/>, the index at which to refresh and <paramref name="obj"/>. By default it will call refresh(<paramref name="obj"/>) on the <paramref name="products"/>[index] or just overwrite it</param>
		/// <param name="elseAction">What to do if <paramref name="obj"/> is not contained in <paramref name="products"/>. By defualt adds it to <paramref name="products"/>. To deactivate, set <paramref name="doElseAction"/> to <see langword="false"/></param>
		/// <param name="count">The number of times the refresher-Action can be applied maximum - the number of elements to refresh if several match obj. Set it to a negative for infinity</param>
		/// <returns>-1 if <paramref name="elseAction"/>, else the (first) index in the list that got refreshed</returns>
		public static int Refresh<T>(this List<T> products, T obj, Func<T, T, bool> comparer = null, Action<List<T>, int, T> refresher = null, Action<List<T>, T> elseAction = null, bool doElseAction = true, int count = 1)
		{
			if (comparer == null)
				comparer = (T t, T t2) => {
					try
					{
						return (bool)typeof(T).GetMethod("compare").Invoke(t, new object[] { t2 });
					}
					catch
					{
						return t.Equals(t2);
					}
				};

			if (refresher == null)      //if (typeof(T) == typeof(CsharpDaemon.Product)) 
										//	action = (list, i, t) => ((CsharpDaemon.Product)(object)list[i]).refresh((CsharpDaemon.Product)(object)t);
				refresher = (List<T> list, int i, T t) =>
				{
					try
					{
						typeof(T).GetMethod("refresh").Invoke(list[i], new object[] { t });
					}
					catch
					{
						list[i] = t;
					}
				};
			if (elseAction == null)
				elseAction = (list, t) => list.Add(t);
			//hier geht's los
			int succeed = -1;
			for (int i = 0; i < products.Count && count != 0; i++)
			{
				if (comparer(products[i], obj))
				{
					refresher(products, i, obj);
					if(succeed == -1)
						succeed = i;
					count--;
				}
			}
			if (succeed == -1 && doElseAction)
				elseAction(products, obj);
			return succeed;
		}

		public static List<T2> ForEachN<T,T2> (this IEnumerable<T> lst, Func<T,T2> function)
		{
			List<T2> res = new List<T2>(lst.Count());
			foreach (T a in lst)
				res.Add(function(a));
			return res;
		}
		#endregion

		#region Colors
		/*
        public static Color makeLighter(this Color c, float amount) {
			return Color.FromArgb((byte)(255 * (1 - amount) + amount * c.R),
				(byte)(255 * (1 - amount) + amount * c.G),
				(byte)(255 * (1 - amount) + amount * c.B));
		}*/
		public static Color makeLighter(this Color c, float amount) => Color.FromArgb((byte)(255 * amount + (1 - amount) * c.R),
				(byte)(255 * amount + (1 - amount) * c.G),
				(byte)(255 * amount + (1 - amount) * c.B));

		public static bool resembles(this Color a, Color b, int similiarity = 25)
			=> Math.Abs(a.R - b.R) + Math.Abs(a.B - b.B) + Math.Abs(a.G - b.G) <= similiarity;
		#endregion
		
		#region number conversions
		public static int RoundToInt(this float a)
		{
			//return (int)Math.Round(a);
			return a % 1 < 0.5f ? (int)a : (int)a + 1;
		}

		public static int RoundToInt(this double a)
		{
			//return (int)Math.Round(a);
			return a % 1 < 0.5 ? (int)a : (int)a + 1;
		}

		public static string ToStringWithSign(this float value)
		{
			return value > 0 ? "+" + value.ToString() : value.ToString();
		}
		public static string ToStringWithSign(this int value)
		{
			return value > 0 ? "+" + value.ToString() : value.ToString();
		}

		public static bool decrease(ref int a)
		{
			bool b = a > 0;
			if (b) a--;
			return b;
		}

		public static bool decrease(this int a)
		{
			bool b = a > 0;
			if (b) a--;
			return b;
		}

		public static string ToColumn(this int i) {
			if (i <= 0)
				return "0";
			string res = "";
			while (i > 0) {
				int a = i % 26;
				if (a == 0) a = 26;
				res = (char)(a + 64) + res;
				i /= 26;
			}
			return res;
		}
		#endregion

		#region Other number operations
		public static T toThePowerOf<T>(this T b, int n) where T : IConvertible
		{
			if (n == 0)
				return (T)Convert.ChangeType(1, typeof(T));

			decimal d = b.ToDecimal(null);
			if (n < 0)
			{
				decimal D = 1 / d;
				for (int i = -2; i >= n; i--)
				{
					D /= d;
				}
				return (T)Convert.ChangeType(D, typeof(T));
			}
			else
			{
				decimal D = d;
				for (int i = 2; i <= n; i++)
				{
					d *= D;
				}
				return (T)Convert.ChangeType(D, typeof(T));
			}
		}

		public static Double toThePowerOf(this double d, int n) 
		{
			if (n == 0)
				return 1;
			if (n < 0)
			{
				double D = 1 / d;
				for (int i = -2; i >= n; i--)
				{
					D /= d;
				}
				return D;
			}
			else
			{
				double D = d;
				for (int i = 2; i <= n; i++)
				{
					D *= d;
				}
				return D;
			}
		}
		#endregion

		#region String to numbers
		public static double ToDouble(this string s)
		{
			try { return Convert.ToDouble(s); }
			catch { return 0; }
		}

		public static float ToFloat(this string s)
		{
			try { return Convert.ToSingle(s); }
			catch { return 0; }
		}

		public static int ToInt(this string s,bool removeText = true)
		{
			//return s.Try(() => Convert.ToInt32(s), 0); //wäre neue kompakte Form
			try {
				if (removeText)
				{
					string b = "";
					foreach (char c in s)
						if (char.IsNumber(c))
							b += c;
					return Convert.ToInt32(b);
				}
				else
					return Convert.ToInt32(s);
			}
			catch {
				return 0; }

		}

		public static Int32 ToInt32<T>(this T value) where T : IConvertible => value.ToInt32(); //WIE UNSINNIG IST DAS DENN!!! 
		//Die Basisdatentypen sind alle IConvertible, aber man kann die .ToInt32 , .ToUInt16 etc. nicht aufrufen, außer man konvertiert 
		//in IConvertible wie hier. Man soll halt das über Convert.bla machen, aber das ist doch sau blöd
		public static char[] numberChars = {'1','2','3','4','5','6','7','8','9','0',',','.','-' };
		public static outputType To<outputType>(this object s, bool removeText = true)
			//Convert.ChangeType macht glaube ich dasselbe... 
			//Warum muss es denn alles schon geben??
		{
			try
			{
				string b = "";
				if (removeText)
				{
					foreach (char c in s.ToString())
						if (numberChars.Contains(c))
							b += c;
				}
				else b = s.ToString();
				/*
				A.Expression expression = new A.Expression("Convert.To" + typeof(type).Name + "(" + b + ")");
				return (type)expression.Evaluate();*/
				//Com.Executor.ExecWait("Convert.To" + typeof(type).Name + "(" + b + ")", new Com.TempFileCollection()); //void...

				return CompileAndExecute<outputType>("Convert.To" + typeof(outputType).Name + "(\"" + b + "\")",false,"System");
			}
			catch 
			{ 
				return default(outputType);
			}
		}
		#endregion

		#region other string operations
		/// <summary>
		/// Tries to add value.ToString() + \n to the file at this location. [StreamWriter.WriteLine] 
		/// Returns if it succeeded or not
		/// </summary>
		public static bool writeLine(this string path, object value = null)
		{
			try
			{
				var sw = new StreamWriter(path, append: true);
				sw.WriteLine(value);
				sw.Close();
				return true;
			}
			catch { return false; }
		}
		
		/// <summary>
		/// Tries to add value.ToString() to the file at this location. [StreamWriter.WriteLine] 
		/// Returns if it succeeded or not
		/// </summary>
		public static bool write(this string path, object value = null)
		{
			try
			{
				var sw = new StreamWriter(path, append: true);
				sw.Write(value);
				sw.Close();
				return true;
			}
			catch { return false; }
		}

		public static bool Contains(this string s, string a, int times) {
			string b = s;
			for (int c = 0; c < times; c++) {
				int i = b.IndexOf(a);
				if (i >= 0)
					b = b.Substring(i + a.Length);
				else return false;
			}
			return true;
		}
		public static bool Contains(this string s, string a, Func<char,char,bool> comparer = null)
		{
			if (comparer == null) comparer = (A, B) => A == B;
			int posInA = 0;
			for (int i = 0; i < s.Length; i++)
			{
				if (comparer(s[i], a[posInA]))
				{
					posInA++;
					if (posInA == a.Length)
						return true;
				}
				else
					posInA = 0;
			}
			return false;
		}

		public static bool ContainsUpperLower(this string s, string a) {
			return s.Contains(a, (A,B) => Char.ToLower(A) == Char.ToLower(B));
		}


		public static string Substring(this string a, string beginSeparator)
		{
			return a.Substring(a.IndexOf(beginSeparator) + beginSeparator.Length);
		}
		public static string Substring(this string a, string beginSeparator = null, string endSeparator = "")
		{
			if (String.IsNullOrEmpty(beginSeparator))
				return a.Substring(0, a.IndexOf(endSeparator));
			int startIndex = a.IndexOf(beginSeparator) + beginSeparator.Length;
			return a.Substring(startIndex,  a.IndexOf(endSeparator) - startIndex);
		}

		public static string EnforceAt(this string th, string s, int index = 0)
		{
			if (th.Substring(index, s.Length) != s)
				return th.Insert(index, s);
			return th;
		}

		public static string EnforceAtEnd(this string th, string s)
		{
			if (th.Substring(th.Length - s.Length) != s)
				return th + s;
			return th;
		}

		public static string format(this string s, params object[] parameters) {
			for (int i = 1; i <= parameters.Length; i++)
			{
				s = s.Replace("_" + i.ToColumn(),parameters[i-1].ToString()).Replace("_" + i.ToColumn().ToLower(), parameters[i-1].ToString()).Replace("_" + i,parameters[i-1].ToString());
			}
			return s;
		}

		/// <param name="newFormattedString">The string every occurence of every string in oldStrings will be replaced with. A _ will be replaced with the old char.</param>
		public static string Replace(this string s, string[] oldStrings, string newFormattedString)
		{
			for (int i = 0; i < oldStrings.Length; i++)
			{
				s = s.Replace(oldStrings[i], newFormattedString.Replace("_", oldStrings[i]));
			}
			return s;
		}

		public static string Replace(this string s, string[] oldStrings, string[] newStrings)
		{
			for (int i = 0; i < oldStrings.Length && i < oldStrings.Length; i++)
			{
				s = s.Replace(oldStrings[i], newStrings[i]);
			}
			return s;
		}
		/// <param name="newFormattedString">The string every occurence of every char in oldStrings will be replaced with. A _ will be replaced with the old char.</param>
		public static string Replace(this string s,char[] oldStrings, string newFormattedString)
		{
			for (int i = 0; i < oldStrings.Length; i++)
			{
				s = s.Replace(oldStrings[i].ToString(), newFormattedString.Replace('_', oldStrings[i]));
			}
			return s;
		}

		public static string Replace(this string s,char[] oldStrings, string[] newStrings)
		{ 
			for (int i = 0; i < oldStrings.Length && i < oldStrings.Length; i++)
			{
				s = s.Replace(oldStrings[i].ToString(), newStrings[i]);
			}
			return s;
		}

		//public static string ReplaceFirst (this string s, int number = 1, params string[] oldStrings)

		#endregion

		#region runtime compiling
		/// <summary>
		/// Executes code (which returns a value) represented by a string. For methods with input parameters use CompileMethod.
		/// </summary>
		/// <typeparam name="T">The return type the result gets casted to.</typeparam>
		/// <param name="method">Set to false if code is just an expression that can follow "return ". Set to true if code represents several lines of code (it must then contain "return ")</param>
		public static T CompileAndExecute<T>(string code, bool method = false, params string[] usings)
		{
			try
			{
				return (T)new CSharpCodeProvider().CompileAssemblyFromSource(
						new CompilerParameters
						{
							GenerateExecutable = false,
							GenerateInMemory = true
						}, usings.ToString("using ", "; ") + @"
					public class SomeClass {
						public object someFunction () {" +
								(method ? code : ("return " + code + "; ")) + "}}"
					).CompiledAssembly.GetType("SomeClass").GetMethod("someFunction").Invoke(null, null); /*
			
					).CompiledAssembly.CreateInstance("SomeClass");
				return (T) typeInstance.GetType().GetMethod("someFunction").Invoke(typeInstance, null);*/
			}
			catch {
				return default(T); }
		}
		
		/// <summary>
		 /// Executes code represented by a string. For methods with input parameters use CompileMethod.
		 /// </summary>
		public static void CompileAndExecute(string code, params string[] usings)
		{
			try
			{
				new CSharpCodeProvider().CompileAssemblyFromSource(
					new CompilerParameters
					{
						GenerateExecutable = false,
						GenerateInMemory = true
					}, usings.ToString("using ", "; \n ") + @"
					public class SomeClass {
						public object someFunction () {" +
							code + "; " +
					"}}"
				).CompiledAssembly.GetType("SomeClass").GetMethod("someFunction").Invoke(null, null); /*
				).CompiledAssembly.CreateInstance("SomeClass");
				typeInstance.GetType().GetMethod("someFunction").Invoke(typeInstance, null);*/
			}
			catch { }
		}

		/// <summary>
		/// Compiles a method for multiple uses from a string.
		/// </summary>
		/// <param name="code">The code for the method with the header without modifiers ([type] [name] (...){ [return...] }</param>
		public static MethodInfo CompileMethod (string code)
		{
			try
			{
				return new CSharpCodeProvider().CompileAssemblyFromSource(
					new CompilerParameters
					{
						GenerateExecutable = false,
						GenerateInMemory = true
					}, @" public static class SomeClass { public static " + code + "}"
					).CompiledAssembly.GetType("SomeClass").GetMethod(code.Substring(code.IndexOf(' ')));
			}
			catch { return null; }
		}

		/// <summary>
		/// Compiles classes for multiple uses from this string.
		/// </summary>
		public static Type[] CompileClasses(string code)
		{
			try
			{
				return new CSharpCodeProvider().CompileAssemblyFromSource(
					new CompilerParameters
					{
						GenerateExecutable = false,
						GenerateInMemory = true
					},
					code
				).CompiledAssembly.GetTypes();
			}
			catch { return null; }
		}

		#endregion

		#region Try
		public static T Try<T>(this object o, Func<T> t, T cv, params Func<T>[] c) //Mit Ersatzwert
		{
			try
			{
				return t.Invoke();
			}
			catch
			{
				return Try(0, cv, c);
			}
		}

		public static T Try<T>(this object o, Func<T> t, params Func<T>[] c)
		{
			try
			{
				return t.Invoke();
			}
			catch
			{
				return Try(0, default(T), c);
			}
		}

		private static T Try<T>(int n, T cv, Func<T>[] c) { //intern
			try
			{
				if (c != null && n < c.Length) //Abbruchbedingung
					return c[n].Invoke();
				return cv;
			}
			catch
			{
				return Try(n + 1, cv, c); //mit einer Schleife wäre es evtl. schneller, aber egaaal :)
			}
		}
		/// <summary>
		/// returns -1 if it fails, else the index of the delegate that succeeded
		/// </summary>
		public static int Try(this object o, Action t, params Action[] c)
		{
			try
			{
				t.Invoke();
				return 0;
			}
			catch
			{
				for (int i = 0; i < c.Length; i++)
				{
					try
					{
						c[i].Invoke();
						return i + 1;
					}
					catch
					{
					}
				}
				return -1;
			}
		}
		/// <summary>
		/// returns -1 if it fails, else the number of tries (1..n) it took to succeed
		/// </summary>
		public static int Try(this object o, Action t, int n)
		{
			for (int i = 0; i < n; i++)
			{
				try
				{
					t.Invoke();
					return i + 1;
				}
				catch
				{
				}
			}
			return -1;
		}
		/// <summary>
		/// Try <paramref name="t"/> <paramref name="n"/> times
		/// </summary>
		/// <typeparam name="T">Input parameter type</typeparam>
		/// <param name="t">Delegate of what to try (use lamda notation f.ex.)</param>
		/// <param name="n">Try it <paramref name="n"/> times</param>
		/// <param name="cv">Default value</param>
		/// <returns>Returns what your Delegate returns</returns>
		public static T Try<T>(this object o, Func<T> t, int n, T cv = default(T))
		{
			for (int i = 0; i < n; i++)
			{
				try
				{
					return t.Invoke();
				}
				catch
				{
				}
			}
			return cv;
		}
		#endregion

		#region HtmlAgilityPack.HtmlNode Extension for websites not using names, but classes to identify nodes
		public static IEnumerable<HtmlAgilityPack.HtmlNode> DescendantsC(this HtmlAgilityPack.HtmlNode node, string clas)
		{
			return node.Descendants().Where((HtmlAgilityPack.HtmlNode h) => h.HasClass(clas));
		}

		public static HtmlAgilityPack.HtmlNode ElementC(this HtmlAgilityPack.HtmlNode node, string clas, int layerdepth = 1)
		{
			return node.Descendants(layerdepth).First((HtmlAgilityPack.HtmlNode h) => h.HasClass(clas));
		}

		public static HtmlAgilityPack.HtmlNode FirstChildC(this HtmlAgilityPack.HtmlNode node, string name)
		{
			return node.ChildNodes.First((HtmlAgilityPack.HtmlNode h) => h.Name == name);
		}

		public static List<HtmlAgilityPack.HtmlNode> ChildNodesC(this HtmlAgilityPack.HtmlNode node, string name)
		{
			return node.ChildNodes.Where((HtmlAgilityPack.HtmlNode h) => h.Name == name).ToList();
		}

		public static HtmlAgilityPack.HtmlNode FirstChildC(this HtmlAgilityPack.HtmlNode node)
		{
			return node.ChildNodes.First((HtmlAgilityPack.HtmlNode h) => h.Name != "#text");
		}

		public static List<HtmlAgilityPack.HtmlNode> ChildNodesC(this HtmlAgilityPack.HtmlNode node)
		{
			return node.ChildNodes.Where((HtmlAgilityPack.HtmlNode h) => h.Name != "#text").ToList();
		}
		#endregion

		#region object operations
		public static void CopyIntoThis<TypeOfThis>(this TypeOfThis @this, TypeOfThis other) {
			Type T = typeof(TypeOfThis);
			foreach (FieldInfo variable in T.GetFields()) { 
				//alle Variablen (hier: Felder, im Unterricht: Attribute (die hier was anderes sind)) 
				variable.SetValue(@this, variable.GetValue(other));
			}
			foreach (PropertyInfo variable in T.GetProperties())
			{ //alle "Variablen", die get und set-Accessor haben (hier: Eigenschaften (properties)) 
				variable.SetValue(@this, variable.GetValue(other));
			}
		}

		public static string ToStringS(this object o, string separator = ", ")
		{
			if (o is IEnumerable<object>)
				return ToString(o as IEnumerable<object>, separator);
			return o.ToString();
		}

		//public static void ToVoid(this object o) { }
		#endregion

		#region Serialization
		public static void Save(string path, object data)
        {
			for (int i = 0; i < 50; i++)
				try
				{
					FileStream serializationStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
					new BinaryFormatter().Serialize(serializationStream, data);
					serializationStream.Close();
					return;
				}
				catch {
					Thread.Sleep(20);
				}
			MessageBox.Show("Write access on " + path + " timed out (> 1 s)");
        }

        public static object Load(string path)
        {
			for (int i = 0; i < 50; i++)
				try
				{
					FileStream serializationStream = new FileStream(path, FileMode.Open, FileAccess.Read);
					var a = new BinaryFormatter().Deserialize(serializationStream);
					serializationStream.Close();
					return a;
				}
				catch
				{
					Thread.Sleep(20);
				}
			MessageBox.Show("Read access on " + path + " timed out (> 1 s)");
			return null;
		}
		#endregion

		#region MathStuff


		public static double Clamp(this double variableToClamp, double min = -1, double max = 1)
		{
			if (variableToClamp <= min) { return min; }
			if (variableToClamp >= max) { return max; }
			return variableToClamp;
		}

		public static Tout BigOperator<T1, T2, Tout>(this IEnumerable<T1> lst, Func<T1,T2> element, Func<Tout, T2, Tout> iteration, Tout start = default(Tout))
		{
			if (element == null)
			{
				/* if (typeof(Tout) == typeof(Tin))
					return BigOperator<Tout, Tout>(lst, a => a, iteration);
				else	war gut, aber geht nicht*/
			return default(Tout);
			}
			if (iteration == null)
				// iteration = (a, b) => a + b; geht auch nicht
				return default(Tout);

			foreach(var lstelem in lst)
			{
				start = iteration(start, element(lstelem));
			}

			return start;
		}

		//examples
		/// <summary>
		/// Sum of elements, + has to be defined
		/// </summary>
		public static double Σ(this IEnumerable<double> lst, Func<double, double> element = null) 
		{
			if (element == null)
			{
				return BigOperator<double, double, double>(lst, a => a, (S, b) => S + b);
			}
			return BigOperator<double, double, double>(lst, element, (S, b) => S + b);
		} //geht auch nicht ...*/
		public static int Σ (this IEnumerable<int> lst, Func<int, int> element = null) 
		{
			if (element == null)
			{
				return BigOperator<int, int, int>(lst, a => a, (S, b) => S + b);
			}
			return BigOperator<int, int, int>(lst, element, (S, b) => S + b);
		} //geht auch nicht ...*/

		public static T Min<T, T2>(this IEnumerable<T> lst, Func<T, T2> element) where T2 : IComparable
		{
			T start = lst.FirstOrDefault();
			foreach (var lstelem in lst)
			{
				start = element(start).CompareTo(element(lstelem)) < 0 ? start : lstelem;
			}
			return start;
		}

		public static T Min<T>(this IEnumerable<T> lst) where T : IComparable
		{
			T start = lst.FirstOrDefault();
			foreach (var lstelem in lst)
			{
				start = start.CompareTo(lstelem) < 0 ? start : lstelem;
			}
			return start;
		}


		#endregion
	}

	public static class Alphabets
	{
		public static char[] Major => "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray(); 
		public static char[] Minor => "abcdefghijklmnopqrstuvwxyz".ToCharArray(); 
		public static char[] MinorAndMajor => "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray(); 
		public static char[] MajorAndNumbers => "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray(); 
		public static char[] MinorAndNumbers => "abcdefghijklmnopqrstuvwxyz0123456789".ToCharArray(); 
		public static char[] MinorAndMajorAndNumbers => "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
	}
}
