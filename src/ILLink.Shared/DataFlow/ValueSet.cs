// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ILLink.Shared.DataFlow
{
	public readonly struct ValueSet<TValue> : IEquatable<ValueSet<TValue>>, IEnumerable<TValue>
		where TValue : notnull
	{
		// Since we're going to do lot of type checks for this class a lot, it is much more efficient
		// if the class is sealed (as then the runtime can do a simple method table pointer comparison)
		sealed class EnumerableValues : HashSet<TValue>
		{
			public EnumerableValues (IEnumerable<TValue> values) : base (values) { }

			public override int GetHashCode ()
			{
				int hashCode = 0;
				foreach (var item in this)
					hashCode = HashUtils.Combine (hashCode, item);
				return hashCode;
			}
		}

		public struct Enumerator : IEnumerator<TValue>, IDisposable, IEnumerator
		{
			readonly object? _value;
			int _state;  // 0 before begining, 1 at item, 2 after end
			readonly IEnumerator<TValue>? _enumerator;

			internal Enumerator (object? values)
			{
				_state = 0;
				if (values is EnumerableValues valuesSet) {
					_enumerator = valuesSet.GetEnumerator ();
					_value = null;
				} else {
					_enumerator = null;
					_value = values;
				}
			}

			public TValue Current => _enumerator is not null
				? _enumerator.Current
				: (_state == 1 ? (TValue) _value! : default!);

			object? IEnumerator.Current => Current;

			public void Dispose ()
			{
			}

			public bool MoveNext ()
			{
				if (_enumerator is not null)
					return _enumerator.MoveNext ();

				if (_value is null)
					return false;

				if (_state > 1)
					return false;

				_state++;
				return _state == 1;
			}

			public void Reset ()
			{
				if (_enumerator is not null)
					_enumerator.Reset ();
				else
					_state = 0;
			}
		}

		// This stores the values. By far the most common case will be either no values, or a single value.
		// Cases where there are multiple values stored are relatively very rare.
		//   null - no values (empty set)
		//   TValue - single value itself
		//   EnumerableValues typed object - multiple values, stored in the hashset
		readonly object? _values;

		public ValueSet (TValue value) => _values = value;

		public ValueSet (IEnumerable<TValue> values) => _values = new EnumerableValues (values);

		ValueSet (EnumerableValues values) => _values = values;

		public static implicit operator ValueSet<TValue> (TValue value) => new (value);

		public override bool Equals (object? obj) => obj is ValueSet<TValue> other && Equals (other);

		public bool Equals (ValueSet<TValue> other)
		{
			if (_values == null)
				return other._values == null;
			if (other._values == null)
				return false;

			if (_values is EnumerableValues enumerableValues) {
				Debug.Assert (enumerableValues.Count > 1);
				if (other._values is EnumerableValues otherValuesSet) {
					Debug.Assert (otherValuesSet.Count > 1);
					return enumerableValues.SetEquals (otherValuesSet);
				} else
					return false;
			} else {
				if (other._values is EnumerableValues otherEnumerableValues) {
					Debug.Assert (otherEnumerableValues.Count > 1);
					return false;
				}

				return EqualityComparer<TValue>.Default.Equals ((TValue) _values, (TValue) other._values);
			}
		}

		public override int GetHashCode ()
		{
			if (_values == null)
				return typeof (ValueSet<TValue>).GetHashCode ();

			if (_values is EnumerableValues enumerableValues)
				return enumerableValues.GetHashCode ();

			return _values.GetHashCode ();
		}

		public Enumerator GetEnumerator () => new (_values);

		IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator () => GetEnumerator ();

		IEnumerator IEnumerable.GetEnumerator () => GetEnumerator ();

		public bool Contains (TValue value) => _values is null
			? false
			: _values is EnumerableValues valuesSet
				? valuesSet.Contains (value)
				: EqualityComparer<TValue>.Default.Equals (value, (TValue) _values);

		internal static ValueSet<TValue> Meet (ValueSet<TValue> left, ValueSet<TValue> right)
		{
			if (left._values == null)
				return right;
			if (right._values == null)
				return left;

			if (left._values is not EnumerableValues && right.Contains ((TValue) left._values))
				return right;

			if (right._values is not EnumerableValues && left.Contains ((TValue) right._values))
				return left;

			var values = new EnumerableValues (left);
			values.UnionWith (right);
			return new ValueSet<TValue> (values);
		}

		public bool IsEmpty () => _values == null;

		public override string ToString ()
		{
			StringBuilder sb = new ();
			sb.Append ("{");
			sb.Append (string.Join (",", this.Select (v => v.ToString ())));
			sb.Append ("}");
			return sb.ToString ();
		}
	}
}