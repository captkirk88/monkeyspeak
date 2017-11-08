using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;

namespace Monkeyspeak.Logging
{
    internal class ConcurrentList<T> : IList<T>
    {
        public event Action<T, int> Removed;

        public event Action<T, int> Added;

        private readonly List<T> underlyingList;
        private readonly object syncRoot = new object();
        private readonly ConcurrentQueue<T> underlyingQueue;
        private bool requiresSync;
        private bool isDirty;

        public ConcurrentList()
        {
            underlyingQueue = new ConcurrentQueue<T>();
            underlyingList = new List<T>();
        }

        public ConcurrentList(IEnumerable<T> items)
        {
            underlyingQueue = new ConcurrentQueue<T>();
            underlyingList = new List<T>(items);
        }

        private void UpdateLists()
        {
            if (!isDirty)
                return;
            lock (syncRoot)
            {
                requiresSync = true;
                while (underlyingQueue.TryDequeue(out T temp))
                {
                    underlyingList.Add(temp);
                }
                requiresSync = false;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            UpdateLists();
            lock (syncRoot)
            {
                var copy = new T[underlyingList.Count];
                underlyingList.CopyTo(copy);
                return copy.Cast<T>().GetEnumerator();
            }
        }

        public void Add(T item)
        {
            if (requiresSync)
                lock (syncRoot)
                    underlyingQueue.Enqueue(item);
            else
                underlyingQueue.Enqueue(item);
            isDirty = true;
        }

        public void AddAll(params T[] items)
        {
            if (requiresSync)
                lock (syncRoot)
                {
                    for (int i = 0; i <= items.Length - 1; i++)
                        underlyingQueue.Enqueue(items[i]);
                }
            else
            {
                if (items == null || items.Length == 0)
                    for (int i = 0; i <= items.Length - 1; i++)
                        underlyingQueue.Enqueue(items[i]);
            }
            isDirty = !(items == null || items.Length == 0);
        }

        public bool TryGetValue(int index, out T value)
        {
            UpdateLists();
            lock (syncRoot)
            {
                int i = 0;
                foreach (var item in underlyingList)
                {
                    if (i == index)
                    {
                        value = item;
                        return true;
                    }
                    i++;
                }
                value = default(T);
                return false;
            }
        }

        public T this[int index]
        {
            get
            {
                UpdateLists();
                lock (syncRoot)
                {
                    int i = 0;
                    foreach (var item in underlyingList)
                    {
                        if (i == index)
                            return item;
                        i++;
                    }
                    return default(T);
                }
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool IsFixedSize
        {
            get { return false; }
        }

        public void Clear()
        {
            UpdateLists();
            lock (syncRoot)
            {
                underlyingList.Clear();
            }
        }

        public bool Contains(T item)
        {
            UpdateLists();
            lock (syncRoot)
            {
                return underlyingList.Contains(item);
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            UpdateLists();
            lock (syncRoot)
            {
                underlyingList.CopyTo(array, arrayIndex);
            }
        }

        public ReadOnlyCollection<T> Clone()
        {
            return this.AsReadOnly();
        }

        public bool Remove(T item)
        {
            UpdateLists();
            lock (syncRoot)
            {
                return underlyingList.Remove(item);
            }
        }

        public void CopyTo(Array array, int index)
        {
            UpdateLists();
            lock (syncRoot)
            {
                underlyingList.CopyTo((T[])array, index);
            }
        }

        public int Count
        {
            get
            {
                UpdateLists();
                lock (syncRoot)
                {
                    return underlyingList.Count;
                }
            }
        }

        public object SyncRoot
        {
            get { return syncRoot; }
        }

        public bool IsSynchronized
        {
            get { return true; }
        }

        public int IndexOf(T item)
        {
            UpdateLists();
            lock (syncRoot)
            {
                int i = 0;
                foreach (var value in underlyingList)
                {
                    if (value.Equals(item))
                        return i;
                    i++;
                }
                return -1;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public T Find(Predicate<T> func)
        {
            UpdateLists();
            lock (syncRoot)
            {
                foreach (var item in underlyingList.Where(item => func(item)))
                {
                    return item;
                }
                return default(T);
            }
        }

        public void Insert(int index, T item)
        {
            UpdateLists();
        }

        public void RemoveAt(int index)
        {
            UpdateLists();
            lock (syncRoot)
            {
                for (int i = 0; i <= underlyingList.Count - 1; i++)
                {
                    if (i == index)
                    {
                        underlyingList.Remove(underlyingList.ElementAt(i));
                    }
                }
            }
        }

        public void InsertRange(int index, params T[] actions)
        {
            UpdateLists();
            if (requiresSync)
                lock (syncRoot)
                {
                    underlyingList.InsertRange(index, actions);
                }
            else
                underlyingList.InsertRange(index, actions);
            isDirty = true;
        }

        public ReadOnlyCollection<T> AsReadOnly()
        {
            if (underlyingList.Count == 0)
                return null;
            return new ReadOnlyCollection<T>(underlyingList.ToArray());
        }

        public void Sort()
        {
            UpdateLists();
            lock (syncRoot)
                underlyingList.Sort();
        }

        public void Sort(Comparer<T> comparer)
        {
            UpdateLists();
            lock (syncRoot)
                underlyingList.Sort(comparer);
        }

        public void Sort(IComparer<T> comparer)
        {
            UpdateLists();
            lock (syncRoot)
                underlyingList.Sort(comparer);
        }

        public void Sort(int index, int count, IComparer<T> comparer)
        {
            UpdateLists();
            lock (syncRoot)
                underlyingList.Sort(index, count, comparer);
        }
    }
}