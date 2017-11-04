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

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cilador.Fody.Tests.Common
{
    /// <summary>
    /// Validates a generic parameter type
    /// </summary>
    internal class GenericParameterTypeValidator : TypeValidatorBase
    {
        /// <summary>
        /// Creates a new <see cref="GenericParameterTypeValidator"/>
        /// that validates an expected generic parameter with a given name.
        /// </summary>
        /// <param name="parameterName">Name of expected generic parameter.</param>
        /// <returns></returns>
        public static GenericParameterTypeValidator Named(string parameterName)
        {
            Contract.Requires(parameterName != null);
            Contract.Ensures(Contract.Result<GenericParameterTypeValidator>() != null);
            return new GenericParameterTypeValidator(parameterName);
        }

        /// <summary>
        /// Creates a new <see cref="GenericParameterTypeValidator"/>.
        /// </summary>
        /// <param name="parameterName">Name of the expected generic parameter.</param>
        private GenericParameterTypeValidator(string parameterName)
        {
            Contract.Requires(parameterName != null);
            Contract.Ensures(this.ParameterName != null);
            this.ParameterName = parameterName;
        }

        /// <summary>
        /// Gets or sets the name of the expected generic parameter
        /// </summary>
        public string ParameterName { get; private set; }

        /// <summary>
        /// Validates the type.
        /// </summary>
        public override void Validate(Type actualType)
        {
            Assert.That(actualType.IsGenericParameter);
            Assert.That(this.ParameterName, Is.EqualTo(actualType.Name));
        }
    }
}
