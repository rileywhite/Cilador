/***************************************************************************/
// Copyright 2013-2017 Riley White
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

using Cilador.Graph.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cilador.Graph.Operations
{
    public static class MergeExtension
    {
        public static ICilGraph Merge(this ICilGraph original, ICilGraph addition, IDictionary<object, object> replacementMap)
        {
            throw new NotImplementedException();
            //var vertices = original.Vertices.Concat(addition);
            // problem here is that the operation makes sense on the Cecil module rather than on the graph
            // you can do the replacement on the graph, but how would you round-trip it back to the module?
            // maybe do it as a modified clone operation on the module, and then generate a new graph...but then why do you need the graph?
        }
    }
}
