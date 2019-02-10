/***************************************************************************/
// Copyright 2013-2019 Riley White
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

namespace Cilador.Aop.Core
{
    /// <summary>
    /// Interface for an actual weaving operation that can be applied to a target
    /// of type <typeparamref name="TTarget"/>.
    /// </summary>
    /// <typeparam name="TTarget">Type to which the weaving operation may apply.</typeparam>
    public interface IConceptWeaver<in TTarget>
    {
        /// <summary>
        /// Performs a weaving operation on a given target.
        /// </summary>
        /// <param name="target">Target to which the weave will be applied.</param>
        void Weave(TTarget target);
    }
}
