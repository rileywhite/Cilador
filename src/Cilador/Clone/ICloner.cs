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
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Cilador.Clone
{
    /// <summary>
    /// Interface implemented by all item cloners.
    /// </summary>
    /// <typeparam name="TSource">Type of the cloning source item.</typeparam>
    /// <typeparam name="TTarget">Type of item to be cloned.</typeparam>
    [ContractClass(typeof(ClonerContract<,>))]
    public interface ICloner<out TSource, out TTarget>
        where TSource : class
        where TTarget : class
    {
        /// <summary>
        /// Gets the context for cloning.
        /// </summary>
        ICloningContext CloningContext { get; }

        /// <summary>
        /// Gets or the transform list to apply to the target after creating and before final cloning.
        /// </summary>
        /// <remarks>
        /// In the cloning operation, transforms are executed in the order of item creation. A parent item
        /// (say a <see cref="MethodSignatureCloner"/>) transform will execute before the child items have
        /// been created. In this way, the parent can be modified in a way that may affect the parameters
        /// used to create child items.
        /// </remarks>
        IList<Action<ICloner<object, object>>> TargetTransforms { get; }

        /// <summary>
        /// Gets whether the item has been cloned.
        /// </summary>
        bool IsCloned { get; }

        /// <summary>
        /// Clones the item.
        /// </summary>
        void Clone();

        /// <summary>
        /// Gets the cloning source.
        /// </summary>
        TSource Source { get; }

        /// <summary>
        /// Gets the cloning target.
        /// </summary>
        TTarget Target { get; }
    }

    /// <summary>
    /// Simplified interface for a symmetric cloner, where the source and target are the same type.
    /// </summary>
    /// <typeparam name="TCloned">Type of both the cloning source and target.</typeparam>
    public interface ICloner<out TCloned> : ICloner<TCloned, TCloned> where TCloned : class { }

}
