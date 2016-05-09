/***************************************************************************/
// Copyright 2013-2016 Riley White
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
using System.Diagnostics.Contracts;

namespace Cilador.Clone
{
    /// <summary>
    /// Contracts for <see cref="ClonerBase{TSource, TTarget}"/>.
    /// </summary>
    [ContractClassFor(typeof(ClonerBase<,>))]
    internal abstract class ClonerBaseContract<TSource, TTarget> : ClonerBase<TSource, TTarget>
        where TSource : class
        where TTarget : class
    {
        /// <summary>
        /// Unused constructor for <see cref="ClonerBaseContract{TSource, TTarget}"/>.
        /// </summary>
        protected ClonerBaseContract() : base(null, null) { }

        /// <summary>
        /// Contracts for <see cref="ClonerBase{TSource,TTarget}.GetTarget"/>
        /// </summary>
        /// <returns></returns>
        protected override TTarget GetTarget()
        {
            Contract.Ensures(Contract.Result<TTarget>() != null);
            throw new NotSupportedException();
        }
    }
}
