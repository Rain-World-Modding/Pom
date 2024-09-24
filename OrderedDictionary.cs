using System.Collections;
#pragma warning disable 1591
namespace PomCore
{
	public class ReadOnlyOrderedDictionary<TK, TV> : IReadOnlyDictionary<TK, TV>, IReadOnlyList<TK>
	{
		public ReadOnlyOrderedDictionary(IDictionary<TK, TV> dictionary)
		{
			foreach ((var key, var val) in dictionary)
			{
				dict[key] = val;
				list.Add(key);
			}
		}

		protected readonly Dictionary<TK, TV> dict = new();
		protected readonly List<TK> list = new();

		public TV this[TK key] => dict[key];

		public TK this[int index] => list[index];

		public IEnumerable<TK> Keys => list;

		public IEnumerable<TV> Values
		{
			get { foreach (var key in list) yield return dict[key]; }
		}

		public int Count => list.Count;

		public bool ContainsKey(TK key) => dict.ContainsKey(key);

		public bool TryGetValue(TK key, out TV value) => dict.TryGetValue(key, out value);

		public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator()
		{
			foreach (var key in list) yield return new(key, dict[key]);
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		IEnumerator<TK> IEnumerable<TK>.GetEnumerator() => list.GetEnumerator();
	}

	public class OrderedDictionary<TK, TV> : ReadOnlyOrderedDictionary<TK, TV>, IDictionary<TK, TV>, IReadOnlyList<TK> where TV : class
	{
		public OrderedDictionary() : base(new Dictionary<TK, TV>())
		{
		}

		TV IDictionary<TK, TV>.this[TK key] { get => base[key]; set => dict[key] = value; }

		public bool IsReadOnly => false;

		ICollection<TK> IDictionary<TK, TV>.Keys => (ICollection<TK>)base.Keys;

		ICollection<TV> IDictionary<TK, TV>.Values => (ICollection<TV>)base.Values;

		public void Add(TK key, TV value)
		{
			dict.Add(key, value);
			list.Add(key);
		}

		public void Add(KeyValuePair<TK, TV> item)
		{
			dict.Add(item.Key, item.Value);
			list.Add(item.Key);
		}

		public void Clear()
		{
			dict.Clear();
			list.Clear();
		}

		public bool Contains(KeyValuePair<TK, TV> item) => dict.TryGetValue(item.Key, out var val) && val == item.Value;

		public void CopyTo(KeyValuePair<TK, TV>[] array, int arrayIndex)
		{
			((IDictionary<TK, TV>)dict).CopyTo(array, arrayIndex);
		}

		public bool Remove(TK key) => dict.Remove(key) && list.Remove(key);

		public bool Remove(KeyValuePair<TK, TV> item) => dict.TryGetValue(item.Key, out var val) && val == item.Value && Remove(item.Key);
	}
}
