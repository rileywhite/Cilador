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

using Mono.Cecil.Cil;
using System;
using Mono.Cecil;

namespace Bix.Mixers.ILCloning
{
    /// <summary>
    /// Defines an interface for cloners with a method body context
    /// that provide access to the <see cref="MethodBody.ThisParameter"/>
    /// of the target method.
    /// </summary>
    internal interface ICloneToMethodBody<out TSource> : ICloner<TSource, MethodBody>
        where TSource : class
    {
        /// <summary>
        /// Gets the <see cref="ParameterDefinition"/>, if any, that
        /// instructions should map to the <see cref="MethodBody.ThisParameter"/>
        /// of the target method body. Generally this will be the ThisParameter
        /// of a source method body.
        /// </summary>
        ParameterDefinition SourceThisParameter { get; }
    }
}
