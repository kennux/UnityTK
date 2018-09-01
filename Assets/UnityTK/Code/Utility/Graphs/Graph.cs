using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityTK
{
    /// <summary>
    /// Basic generic graph implementation.
    /// Nothing fancy here, just a simple and naive implementation of <see cref="IGraph"/>.
    /// 
    /// Underlying implementation just uses 2 <see cref="Dictionary{TKey, TValue}"/> for nodes.
    /// It uses a dictionary with a list of tuples (index, connection data) for connections.
    /// </summary>
    public class Graph<TIndex, TNode, TConnectionData> : IGraph<TIndex, TNode, TConnectionData>, IEnumerable<TNode>
    {
        /// <summary>
        /// All nodes of this graph.
        /// </summary>
        private Dictionary<TIndex, TNode> nodes = new Dictionary<TIndex, TNode>();

        /// <summary>
        /// Node reverse mappings.
        /// </summary>
        private Dictionary<TNode, TIndex> nodesReverse = new Dictionary<TNode, TIndex>();

        /// <summary>
        /// Connections dictionary
        /// </summary>
        private Dictionary<TIndex, List<ConnectionData>> connections = new Dictionary<TIndex, List<ConnectionData>>();

        private struct ConnectionData
        {
            public TNode node;
            public TConnectionData data;
        }

        public void Add(TIndex index, TNode node)
        {
            this.nodes.Add(index, node);
            this.nodesReverse.Add(node, index);
        }

        public TNode Get(TIndex index)
        {
            return this.nodes[index];
        }

        public void Remove(TIndex index)
        {
            TNode node;

            if (this.nodes.TryGetValue(index, out node))
            {
                this.nodes.Remove(index);
                this.nodesReverse.Remove(node);

                List<ConnectionData> lst;
                if (this.connections.TryGetValue(index, out lst))
                {
                    this.connections.Remove(index);
                    ListPool<ConnectionData>.Return(lst);
                }
            }
        }

        /// <summary>
        /// Finds a connection index in the specified list of connection datas.
        /// </summary>
        /// <returns>Index in connection data, -1 if not found</returns>
        private int FindConnectionIndex(List<ConnectionData> connectionData, TNode node, out ConnectionData data)
        {
            var cmp = EqualityComparer<TNode>.Default;

            for (int i = 0; i < connectionData.Count; i++)
            {
                var cData = connectionData[i];
                if (cmp.Equals(cData.node, node))
                {
                    data = cData;
                    return i;
                }
            }

            data = default(ConnectionData);
            return -1;
        }

        /// <summary>
        /// Returns the connections from node to any other nodes.
        /// </summary>
        private List<ConnectionData> GetConnections(TNode node, bool createIfNotExisting = true)
        {
            // Look up index
            TIndex index;
            if (!this.nodesReverse.TryGetValue(node, out index))
                return null;

            List<ConnectionData> cons;
            if (!this.connections.TryGetValue(index, out cons) && createIfNotExisting)
            {
                cons = ListPool<ConnectionData>.Get();
                this.connections.Add(index, cons);
            }

            return cons;
        }

        public void Connect(TNode from, TNode to, TConnectionData connectionData)
        {
            // Look up
            var cons = GetConnections(from);

            // Check for connection
            ConnectionData cData;
            var conId = FindConnectionIndex(cons, to, out cData);
            
            if (conId == -1)
                cons.Add(new ConnectionData()
                {
                    data = connectionData,
                    node = to
                });
            else
            {
                var con = cons[conId];
                con.data = connectionData;
                cons[conId] = con;
            }
        }

        public void Disconnect(TNode from, TNode to)
        {
            // Look up
            var cons = GetConnections(from);

            // Check for connection
            ConnectionData cData;
            var conId = FindConnectionIndex(cons, to, out cData);

            if (conId != -1)
            {
                cons.RemoveAt(conId);
            }
        }

        public bool IsConnected(TNode from, TNode to)
        {
            TConnectionData data;
            return TryGetConnection(from, to, out data);
        }

        public bool TryGetConnection(TNode from, TNode to, out TConnectionData connectionData)
        {
            // Look up
            List<ConnectionData> cons = GetConnections(from, false);
            if (ReferenceEquals(cons, null))
            {
                connectionData = default(TConnectionData);
                return false;
            }

            // Check for connection
            ConnectionData cData;
            var conId = FindConnectionIndex(cons, to, out cData);

            if (conId == -1)
            {
                connectionData = default(TConnectionData);
                return false;
            }
            else
            {
                connectionData = cons[conId].data;
                return true;
            }
        }

        public Dictionary<TIndex, TNode>.ValueCollection.Enumerator GetEnumerator()
        {
            return this.nodes.Values.GetEnumerator();
        }

        IEnumerator<TNode> IEnumerable<TNode>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}