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
        /// Getter delegate that is used to retrieve collections of registered getters <see cref="RegisterGetter(Getter)"/>
        /// </summary>
        public delegate ICollection<T> Getter();

        /// <summary>
        /// Set handler for <see cref="onSetValue"/>.
        /// </summary>
        /// <param name="queue">A queue of inserted elements, can dequeues consome the object for subsequent propagation calls</param>
        public delegate void Setter(Queue<T> queue);

        /// <summary>
        /// Insertion handler that can be used to handle object insertions.
        /// </summary>
        /// <param name="obj">The object to insert</param>
        /// <returns>Whether or not the object could be inserted</returns>
        public delegate bool Inserter(T obj);

        /// <summary>
        /// The getters bound to this property.
        /// </summary>
        private List<Getter> getters = new List<Getter>();

        /// <summary>
        /// List of insertion handlers.
        /// </summary>
        private List<Inserter> inserters = new List<Inserter>();

        /// <summary>
        /// The setters bound to this property.
        /// The parameter of this event is a queue that contains all elements that were set using <see cref="Set(ICollection{T})"/>.
        /// 
        /// Handlers of this event can dequeue elements from the queue to avoid next calls taking the same elements.
        /// </summary>
        public event Setter onSetValue;

        /// <summary>
        /// Registers a collection getter that is being used in <see cref="Get"/>.
        /// </summary>
		public void RegisterGetter(Getter getter)
		{
            this.getters.Add(getter);
		}

        /// <summary>
        /// Registers an insertion handler.
        /// <see cref="Inserter"/>
        /// </summary>
        public void RegisterInsertHandler(Inserter inserter)
        {
            this.inserters.Add(inserter);
        }

        /// <summary>
        /// Returns the content of the collection as <see cref="ConcatEnumerator"/>.
        /// </summary>
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
				Debug.LogWarning("Tried setting collection property with no setters bound!");
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

        /// <summary>
        /// Inserts the specified object into this colletion.
        /// This operation is only supported if <see cref="RegisterInsertHandler(Inserter)"/> inserters are registered.
        /// </summary>
        /// <returns>Whether or not the item could be inserted.</returns>
        public bool Insert(T obj)
        {
            if (this.inserters.Count == 0)
                Debug.LogWarning("Tried inserting value into model collection property without inserters bound!");
            else
            {
                foreach (var inserter in this.inserters)
                    if (inserter.Invoke(obj))
                        return true; // insertion successfull!
            }

            // No insertion possible!
            return false;
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