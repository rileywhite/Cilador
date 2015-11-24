/***************************************************************************/
// Copyright 2013-2015 Riley White
// 
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
using System.Diagnostics.Contracts;

namespace Cilador.Graph
{
    /// <summary>
    /// Contracts for <see cref="ICgraph"/>.
    /// </summary>
    [ContractClassFor(typeof(ICgraph))]
    public abstract class CgraphContracts : ICgraph
    {
        public IEnumerable<object> Vertices
        {
            get
            {
                Contract.Ensures(Contract.Result<IEnumerable<object>>() != null);
                throw new NotSupportedException();
            }
        }

        public int VertexCount
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);
                throw new NotSupportedException();
            }
        }

        public IEnumerable<object> Roots
        {
            get
            {
                Contract.Ensures(Contract.Result<IEnumerable<object>>() != null);
                throw new NotSupportedException();
            }
        }

        public IEnumerable<ParentChildCilEdge> ParentChildEdges
        {
            get
            {
                Contract.Ensures(Contract.Result<IEnumerable<ICilEdge>>() != null);
                throw new NotSupportedException();
            }
        }

        public TParent GetParentOf<TParent>(object child) where TParent : class
        {
            Contract.Requires(child != null);
            Contract.Ensures(Contract.Result<TParent>() != null);
            throw new NotSupportedException();
        }

        public bool TryGetParentOf<TParent>(object child, out TParent parent) where TParent : class
        {
            Contract.Requires(child != null);
            Contract.Ensures(!Contract.Result<bool>() || Contract.ValueAtReturn(out parent) != null);
            throw new NotSupportedException();
        }

        public int GetDepth(object item)
        {
            Contract.Requires(item != null);
            Contract.Ensures(Contract.Result<int>() >= 0);
            throw new NotSupportedException();
        }

        public IEnumerable<SiblingCilEdge> SiblingEdges
        {
            get
            {
                Contract.Ensures(Contract.Result<IEnumerable<ICilEdge>>() != null);
                throw new NotSupportedException();
            }
        }

        public TSibling GetPreviousSiblingOf<TSibling>(TSibling sibling) where TSibling : class
        {
            Contract.Requires(sibling != null);
            Contract.Ensures(Contract.Result<TSibling>() != null);
            throw new NotSupportedException();
        }

        public bool TryGetPreviousSiblingOf<TSibling>(TSibling sibling, out TSibling previousSibling) where TSibling : class
        {
            Contract.Requires(sibling != null);
            Contract.Ensures(!Contract.Result<bool>() || Contract.ValueAtReturn(out previousSibling) != null);
            throw new NotSupportedException();
        }

        public IEnumerable<DependencyCilEdge> DependencyEdges
        {
            get
            {
                Contract.Ensures(Contract.Result<IEnumerable<ICilEdge>>() != null);
                throw new NotSupportedException();
            }
        }
    }
}
