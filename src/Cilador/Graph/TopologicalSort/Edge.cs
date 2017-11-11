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

namespace Cilador.Graph.TopologicalSort
{
    public struct Edge<TVertex> : IEdge<TVertex>
    {
        public TVertex From { get; }
        public TVertex To { get; }

        public Edge(TVertex from, TVertex to) : this()
        {
            this.From = from;
            this.To = to;
        }
    }

    public static class Edge
    {
        public static IEdge<T> Create<T>(T from, T to)
        {
            return new Edge<T>(from, to);
        }
    }
}
