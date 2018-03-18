﻿/***************************************************************************/
// Copyright 2013-2018 Riley White
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cilador.Fody.Tests.Common
{
    /// <summary>
    /// Contracts for <see cref="TypeValidatorBase"/>.
    /// </summary>
    [ContractClassFor(typeof(TypeValidatorBase))]
    internal abstract class ParameterTypeValidatorBaseContract : TypeValidatorBase
    {
        /// <summary>
        /// Contracts for <see cref="Validate(Type)"/>.
        /// </summary>
        /// <param name="actualType">Actual parameter type.</param>
        public override void Validate(Type actualType)
        {
            Contract.Requires(actualType != null);
        }
    }
}
