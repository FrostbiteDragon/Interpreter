using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript
{
    public class SoftCopyDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly IDictionary<TKey, TValue> original;
        private readonly IDictionary<TKey, TValue> current;

        public SoftCopyDictionary()
        {
            original = new Dictionary<TKey, TValue>();
            current = new Dictionary<TKey, TValue>();
        }

        public SoftCopyDictionary(IDictionary<TKey, TValue> original)
        {
            this.original = original;
            current = new Dictionary<TKey, TValue>();
        }


        public TValue this[TKey key]
        {
            get => original.ContainsKey(key) ? original[key] : current[key];
            set
            {
                if (original.ContainsKey(key)) original[key] = value;
                else current[key] = value;
            }
        }

        public ICollection<TKey> Keys => original.Keys.Union(current.Keys).ToList();

        public ICollection<TValue> Values => original.Values.Union(current.Values).ToList();

        public int Count => original.Count + current.Count;

        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value)
        {
            this[key] = value;
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            this[item.Key] = item.Value;
        }

        public void Clear()
        {
            original.Clear();
            current.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) => original.Contains(item) || current.Contains(item);

        public bool ContainsKey(TKey key) => original.ContainsKey(key) || current.ContainsKey(key);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => throw new NotImplementedException();

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => original.AsEnumerable().Union(current.AsEnumerable()).GetEnumerator();

        public bool Remove(TKey key) => original.Remove(key) || current.Remove(key);

        public bool Remove(KeyValuePair<TKey, TValue> item) => original.Remove(item) || current.Remove(item);

        public bool TryGetValue(TKey key, out TValue value) => original.TryGetValue(key, out value) || current.TryGetValue(key, out value);

        IEnumerator IEnumerable.GetEnumerator() => original.AsEnumerable().Union(current.AsEnumerable()).GetEnumerator();
    }
}
