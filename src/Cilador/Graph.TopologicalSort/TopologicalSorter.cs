/***************************************************************************/
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
/***************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;

namespace Cilador.Graph.TopologicalSort
{
    public static class TopologicalSorter
    {
        // Tarjan's algorithm: http://en.wikipedia.org/wiki/Tarjan%27s_strongly_connected_components_algorithm
        private class Algorithm<T>
        {
            private ILookup<T, T> Edges { get; }
            private IEqualityComparer<T> Comparer { get; }

            private Dictionary<T, int> Indices { get; set; }
            private Dictionary<T, int> LowLink { get; set; }
            private List<IList<T>> Result { get; set; }
            private Stack<T> Stack { get; set; }
            private int Index { get; set; }

            public Algorithm(ILookup<T, T> edges, IEqualityComparer<T> comparer)
            {
                this.Edges = edges;
                this.Comparer = comparer ?? EqualityComparer<T>.Default;
            }

            public List<IList<T>> MainLoop(IEnumerable<T> vertices)
            {
                this.Result = new List<IList<T>>();
                this.Indices = new Dictionary<T, int>(Comparer);
                this.LowLink = new Dictionary<T, int>(Comparer);
                this.Stack = new Stack<T>();
                this.Index = 0;
                foreach (var v in vertices)
                {
                    if (!Indices.ContainsKey(v))
                    {
                        StrongConnect(v);
                    }
                }
                return Result;
            }

            private void StrongConnect(T v)
            {
                this.Indices[v] = Index;
                this.LowLink[v] = Index;
                this.Index++;
                this.Stack.Push(v);

                foreach (var w in this.Edges[v])
                {
                    if (!this.Indices.ContainsKey(w))
                    {
                        this.StrongConnect(w);
                        this.LowLink[v] = Math.Min(this.LowLink[v], this.LowLink[w]);
                    }
                    else if (this.Stack.Contains(w))
                    {
                        this.LowLink[v] = Math.Min(this.LowLink[v], this.Indices[w]);
                    }
                }

                if (this.LowLink[v] == this.Indices[v])
                {
                    var scc = new List<T>();
                    T w;
                    do
                    {
                        w = this.Stack.Pop();
                        scc.Add(w);
                    } while (!Comparer.Equals(v, w));
                    this.Result.Add(scc);
                }
            }
        }

        /// <summary>
        /// Identifies all strongly connected components (http://en.wikipedia.org/wiki/Strongly_connected_component) in a graph, and returns them in an order such that all SCCs are returned after all SCCs on which they are dependent.
        /// This can be seen as topological sorting that supports cycles in the graph, and all items in a cycle are returned as one entity in the result.
        /// </summary>
        /// <param name="vertices">Vertices in the graph</param>
        /// <param name="edges">Edges in the graph. If there is an edge such that (From, To) == (a, b), then b will appear before a in the result.</param>
        /// <param name="comparer">Comparer used to compare the vertices, or null for the default comparer.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="vertices"/> or <paramref name="edges"/> is null.</exception>
        public static IList<IList<T>> FindAndTopologicallySortStronglyConnectedComponents<T>(IEnumerable<T> vertices, IEnumerable<IEdge<T>> edges, IEqualityComparer<T> comparer = null)
        {
            if (vertices == null) throw new ArgumentNullException("vertices");
            if (edges == null) throw new ArgumentNullException("edges");

            var result = new Algorithm<T>(edges.ToLookup(e => e.From, e => e.To), comparer).MainLoop(vertices);
            return result;
        }

        /// <summary>
        /// Identifies all strongly connected components (http://en.wikipedia.org/wiki/Strongly_connected_component) in a graph, and returns them in an order such that all SCCs are returned after all SCCs on which they are dependent.
        /// This can be seen as topological sorting that supports cycles in the graph, and all items in a cycle are returned as one entity in the result.
        /// </summary>
        /// <param name="source">Objects to sort</param>
        /// <param name="getVertex">Function used to get from one of the objects being sorted to the corresponding vertex. All objects must have unique vertices.</param>
        /// <param name="edges">Edges in the graph. If there is an edge such that (From, To) == (a, b), then b will appear before a in the result.</param>
        /// <param name="comparer">Comparer used to compare the vertices, or null for the default comparer.</param>
        /// <exception cref="ArgumentNullException">If any of <paramref name="source"/>, <paramref name="getVertex"/> or <paramref name="edges"/> is null.</exception>
        public static IList<IList<TSource>> FindAndTopologicallySortStronglyConnectedComponents<TSource, TVertex>(IEnumerable<TSource> source, Func<TSource, TVertex> getVertex, IEnumerable<IEdge<TVertex>> edges, IEqualityComparer<TVertex> comparer = null)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (getVertex == null) throw new ArgumentNullException("source");
            if (edges == null) throw new ArgumentNullException("edges");

            var backref = source.ToDictionary(getVertex, comparer ?? EqualityComparer<TVertex>.Default);
            return FindAndTopologicallySortStronglyConnectedComponents(backref.Keys, edges, comparer).Select(l => (IList<TSource>)l.Select(t => backref[t]).ToList()).ToList();
        }

        /// <summary>
        /// Topologically sorts the specified sequence. Throws if there are any cycles in the graph.
        /// </summary>
        /// <param name="vertices">Vertices in the graph</param>
        /// <param name="edges">Edges in the graph. If there is an edge such that (From, To) == (a, b), then b will appear before a in the result.</param>
        /// <param name="comparer">Comparer used to compare the vertices, or null for the default comparer.</param>
        /// <exception cref="ArgumentException">If there are any cycles in the graph.</exception>
        /// <exception cref="ArgumentNullException">If <paramref name="vertices"/> or <paramref name="edges"/> is null.</exception>
        public static IEnumerable<T> TopologicalSort<T>(IEnumerable<T> vertices, IEnumerable<IEdge<T>> edges, IEqualityComparer<T> comparer = null)
        {
            var result = FindAndTopologicallySortStronglyConnectedComponents(vertices, edges, comparer);
            if (result.Any(x => x.Count > 1))
                throw new ArgumentException("Cycles in graph", "edges");
            return result.Select(x => x[0]);
        }

        /// <summary>
        /// Topologically sorts the specified sequence. Throws if there are any cycles in the graph.
        /// </summary>
        /// <param name="source">Objects to sort</param>
        /// <param name="getVertex">Function used to get from one of the objects being sorted to the corresponding vertex. All objects must have unique vertices.</param>
        /// <param name="edges">Edges in the graph. If there is an edge such that (From, To) == (a, b), then b will appear before a in the result.</param>
        /// <param name="comparer">Comparer used to compare the vertices, or null for the default comparer.</param>
        /// <exception cref="ArgumentException">If there are any cycles in the graph.</exception>
        /// <exception cref="ArgumentNullException">If any of <paramref name="source"/>, <paramref name="getVertex"/> or <paramref name="edges"/> is null.</exception>
        public static IEnumerable<TSource> TopologicalSort<TSource, TVertex>(IEnumerable<TSource> source, Func<TSource, TVertex> getVertex, IEnumerable<IEdge<TVertex>> edges, IEqualityComparer<TVertex> comparer = null)
        {
            var backref = source.ToDictionary(getVertex, comparer ?? EqualityComparer<TVertex>.Default);
            return TopologicalSort(backref.Keys, edges, comparer).Select(t => backref[t]);
        }
    }
}
