/***************************************************************************/
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
    /// Validates a type that is a true type but
    /// that is itself a generic type, possibly containing generic parameters.
    /// </summary>
    internal class GenericTypeValidator : TypeValidatorBase
    {
        /// <summary>
        /// Creates a new <see cref="GenericTypeValidator"/>.
        /// </summary>
        /// <param name="genericParameterType">Open generic type of the parameter.</param>
        /// <param name="genericArgumentTypeValidators">Validators for each expected generic argument.</param>
        public GenericTypeValidator(
            Type genericParameterType,
            params TypeValidatorBase[] genericArgumentTypeValidators)
        {
            Contract.Requires(genericParameterType != null);
            Contract.Requires(genericParameterType.IsGenericTypeDefinition);
            Contract.Requires(genericArgumentTypeValidators != null);
            Contract.Requires(genericArgumentTypeValidators.Length > 0);
            Contract.Ensures(this.GenericParameterType != null);
            Contract.Ensures(this.GenericParameterType.IsGenericTypeDefinition);
            Contract.Ensures(this.GenericArgumentTypeValidators != null);
            Contract.Ensures(this.GenericArgumentTypeValidators.Length > 0);

            this.GenericParameterType = genericParameterType;
            this.GenericArgumentTypeValidators = genericArgumentTypeValidators;
        }

        /// <summary>
        /// Gets or sets the generic type definition that is expected.
        /// </summary>
        public Type GenericParameterType { get; }

        /// <summary>
        /// Gets or sets the collection of validators for the generic arguments.
        /// </summary>
        public TypeValidatorBase[] GenericArgumentTypeValidators { get; }

        /// <summary>
        /// Validates the type.
        /// </summary>
        public override void Validate(Type actualType)
        {
            Assert.That(this.GenericParameterType, Is.SameAs(actualType.GetGenericTypeDefinition()));

            var arguments = actualType.GetGenericArguments();
            Assert.That(this.GenericArgumentTypeValidators.Length, Is.EqualTo(arguments.Length));
            Parallel.For(0, this.GenericArgumentTypeValidators.Length, i => this.GenericArgumentTypeValidators[i].Validate(arguments[i]));
        }
    }
}
