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
    /// Validates a non-generic parameter type
    /// </summary>
    internal class NonGenericTypeValidator : TypeValidatorBase
    {
        /// <summary>
        /// Gets a validator for an expected type.
        /// </summary>
        /// <typeparam name="T">Expected parameter type.</typeparam>
        /// <returns>New type validator for a non-generic type.</returns>
        public static NonGenericTypeValidator ForType<T>()
        {
            Contract.Ensures(Contract.Result<NonGenericTypeValidator>() != null);
            return new NonGenericTypeValidator(typeof(T));
        }

        /// <summary>
        /// Gets a validator for an expected type.
        /// </summary>
        /// <returns>New type validator for a void type.</returns>
        public static NonGenericTypeValidator ForVoidType()
        {
            Contract.Ensures(Contract.Result<NonGenericTypeValidator>() != null);
            return new NonGenericTypeValidator(typeof(void));
        }

        /// <summary>
        /// Creates a new <see cref="NonGenericTypeValidator"/>.
        /// </summary>
        /// <param name="parameterType">Expected parameter type.</param>
        private NonGenericTypeValidator(Type parameterType)
        {
            Contract.Requires(parameterType != null);
            Contract.Ensures(this.ParameterType != null);
            this.ParameterType = parameterType;
        }

        /// <summary>
        /// Gets or sets the expected parameter type.
        /// </summary>
        public Type ParameterType { get; }

        /// <summary>
        /// Validates the type.
        /// </summary>
        public override void Validate(Type actualType)
        {
            Assert.That(!actualType.IsGenericParameter);
            Assert.That(this.ParameterType, Is.SameAs(actualType));
        }
    }
}
