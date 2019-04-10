using System;
using System.Linq;
using System.Collections.Generic;

namespace Extensions
{
	public struct Chainable<T> : IComparable where T : IComparable
	{
		////////////
		public List<T> Values;
		bool Correct;
		////////////
		public T Value => Values.FirstOrDefault();
		////////////
		
		public Chainable(T value, params T[] others)
		{
			if (others != null && others.Length > 0)
			{
				Values = others.ToList();
				Values.Insert(0, value);
			}
			else
				Values = new List<T> { value };
			Correct = true;
		}

		public static implicit operator bool(Chainable<T> me) => me.Correct;

		public static Chainable<T> Fail => new Chainable<T> { Correct = false, Values = new List<T>() };

		public override bool Equals(object obj) => Value.Equals(obj);
		public override int GetHashCode() => Value.GetHashCode();
		public int CompareTo(object other) {
			T o = (T)other;
			return
				this > o ? 1 :
				this < o ? -1 :
				0;
		}

		public static implicit operator Chainable<T>(T[] array)
		{
			return new Chainable<T> { Correct = true, Values = array.ToList() };
		}

		public static implicit operator Chainable<T>(List<T> array)
		{
			return new Chainable<T> { Correct = true, Values = array };
		}

		#region <
		public static Chainable<T> operator <(Chainable<T> me, T other)
		{
			if (!me.Correct)
				return me;

			return me.Value.CompareTo(other) < 0
					   ? new Chainable<T>(other)
					   : Fail;
		}

		public static Chainable<T> operator < (T other, Chainable<T> me)
		{
			//if (!me.Correct)
				//return Fail;

			return other.CompareTo(me.Value) < 0
					   ? me
					   : Fail;
		}

		public static Chainable<T> operator <(Chainable<T> me, Chainable<T> other)
		{
			foreach (T s in me.Values)
			{
				foreach (T t in other.Values)
				{
					if (!me.Correct)
						return me;
					me.Correct = s.CompareTo(t) < 0;
				}
			}
			return me;
		}
		#endregion
		#region >
		public static Chainable<T> operator >(Chainable<T> me, T other)
		{
			if (!me.Correct)
				return me;

			return me.Value.CompareTo(other) > 0
					   ? new Chainable<T>(other)
					   : Fail;
		}

		public static Chainable<T> operator >(T other, Chainable<T> me)
		{
			//if (!me.Correct)
				//return me;

			return other.CompareTo(me.Value) > 0
					   ? me
					   : Fail;
		}

		public static Chainable<T> operator >(Chainable<T> me, Chainable<T> other)
		{
			foreach (T s in me.Values)
			{
				foreach (T t in other.Values)
				{
					if (!me.Correct)
						return me;
					me.Correct = s.CompareTo(t) > 0;
				}
			}
			return me;
		}
		#endregion
		#region ==
		public static Chainable<T> operator ==(Chainable<T> me, T other)
		{
			if (!me.Correct)
				return me;

			return me.Value.CompareTo(other) == 0
					   ? new Chainable<T>(other)
					   : Fail;
		}

		public static Chainable<T> operator ==(T other, Chainable<T> me)
		{
			//if (!me.Correct)
				//return me; unnötig, denn, wenn me nicht correct, ist es unten egal, was rauskommt

			return other.CompareTo(me.Value) == 0
					   ? me
					   : Fail;
		}

		public static Chainable<T> operator ==(Chainable<T> me, Chainable<T> other)
		{
			foreach (T s in me.Values)
			{
				foreach (T t in other.Values)
				{
					if (!me.Correct)
						return me;
					me.Correct = s.CompareTo(t) == 0;
				}
			}
			return me;
		}
		#endregion
		#region !=
		public static Chainable<T> operator !=(Chainable<T> me, T other)
		{
			if (!me.Correct)
				return me;

			return me.Value.CompareTo(other) != 0
					   ? new Chainable<T>(other)
					   : Fail;
		}

		public static Chainable<T> operator !=(T other, Chainable<T> me)
		{
			//if (!me.Correct)
				//return me;

			return me.Value.CompareTo(other) != 0
					   ? me
					   : Fail;
		}

		public static Chainable<T> operator !=(Chainable<T> me, Chainable<T> other)
		{
			foreach (T s in me.Values)
			{
				foreach (T t in other.Values)
				{
					if (!me.Correct)
						return me;
					me.Correct = s.CompareTo(t) != 0;
				}
			}
			return me;
		}
		#endregion
		#region & - Verknüpfung
		public static Chainable<T> operator &(Chainable<T> me, Chainable<T> other)
		{
			me.Values.AddRange(other.Values);
			return me;
		}

		public static Chainable<T> operator &(Chainable<T> me, T other)
		{
			me.Values.Add(other);
			return me;
		}

		public static Chainable<T> operator &(T other, Chainable<T> me)
		{
			me.Values.Insert(0, other);
			return me;
		}
		#endregion
	}

	public static class ChainExt
	{
		public static Chainable<T> c<T>(this T value, params T[] others) where T : IComparable
		{
			return new Chainable<T>(value, others);
		}

		public static Chainable<T> c<T>(this IEnumerable<T> value, params T[] others) where T : IComparable
		{
			return new Chainable<T> { Values = value.Concat(others).ToList() };
		}
	}
}
