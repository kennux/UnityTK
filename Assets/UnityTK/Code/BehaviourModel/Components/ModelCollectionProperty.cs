using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK.BehaviourModel
{
	/// <summary>
	/// Similar to ModelProperty but provides the ability to concat collections returned from the getters.
    /// This is useful in situations where you have for example an inventory mechanic property that lists all items but the items are held by multiple logic components(for example multiple bags).
    /// Setters can consume objects from the set call in order to claim them being set on themselves.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ModelCollectionProperty<T> : IEnumerable<T>
	{
        /// <summary>
        /// Internal enumerator used for enumerating over a set of objects.
        /// </summary>
        private struct ConcatEnumerator : IEnumerator<T>
        {
            private int indexBounds;
            private int index;
            private List<ICollection<T>> collections;

            public void Init()
            {
                this.indexBounds = 0;
                this.index = -1;
                this.collections = ListPool<ICollection<T>>.Get();
            }

            public T Current
            {
                get
                {
                    int id = this.index;
                    for (int i = 0; i < collections.Count; i++)
                    {
                        var collection = collections[i];
                        foreach (var element in collection)
                        {
                            if (id == 0)
                                return element;
                            id--;
                        }
                    }

                    return default(T);
                }
            }

            object IEnumerator.Current
            {
                get { return this.Current; }
            }

            public void Dispose()
            {
                ListPool<ICollection<T>>.Return(this.collections);
                this.collections = null;
            }

            public bool MoveNext()
            {
                this.index++;
                return this.index < this.indexBounds;
            }

            public void AddCollection(ICollection<T> collection)
            {
                this.collections.Add(collection);
                this.indexBounds += collection.Count;
            }

            public void Reset()
            {
                this.index = 0;
                this.indexBounds = 0;
                this.collections.Clear();
            }
        }
        
        /// <summary>
        /// The getters bound to this property.
        /// </summary>
        private List<System.Func<ICollection<T>>> getters = new List<System.Func<ICollection<T>>>();

        /// <summary>
        /// The setters bound to this property.
        /// The parameter of this event is a queue that contains all elements that were set using <see cref="Set(ICollection{T})"/>.
        /// 
        /// Handlers of this event can dequeue elements from the queue to avoid next calls taking the same elements.
        /// </summary>
        public event System.Action<Queue<T>> onSetValue;

		public void RegisterGetter(System.Func<ICollection<T>> getter)
		{
            this.getters.Add(getter);
		}

		public IEnumerator<T> Get()
		{
			if (this.getters == null)
			{
				Debug.LogWarning("Tried getting value event with no getter!");
				return default(IEnumerator<T>);
			}

            // Setup enumerator
            ConcatEnumerator enumerator = new ConcatEnumerator();
            enumerator.Init();
            for (int i = 0; i < this.getters.Count; i++)
                enumerator.AddCollection(this.getters[i]());

            return enumerator;
		}

        /// <summary>
        /// Sets the specified collection to this property.
        /// This will invoke the <see cref="onSetValue"/> event.
        /// </summary>
        /// <param name="col">The collection to set</param>
		public void Set(ICollection<T> col)
		{
			if (this.onSetValue == null)
				Debug.LogWarning("Tried setting value event with no setter!");
			else
            {
                // Put col into a queue for implementor consumation
                Queue<T> queue = QueuePool<T>.Get();

                foreach (var obj in col)
                    queue.Enqueue(obj);

                // Pass queue to listeners
                this.onSetValue(queue);

                // Return queue
                QueuePool<T>.Return(queue);
            }
		}

        public IEnumerator<T> GetEnumerator()
        {
            return Get();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Get();
        }
    }
}