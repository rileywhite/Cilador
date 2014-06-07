/***************************************************************************/
// Copyright 2013-2014 Riley White
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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.Tests.Common
{
    /// <summary>
    /// Base type for validating a parameter type
    /// </summary>
    [ContractClass(typeof(ParameterTypeValidatorBaseContract))]
    internal abstract class TypeValidatorBase
    {
        /// <summary>
        /// Validates a method against expected return and parameter types.
        /// </summary>
        /// <param name="actualMethod">Method to validate.</param>
        /// <param name="returnTypeValidator">Expected return type.</param>
        /// <param name="parameterTypeValidators">Expected parameter types.</param>
        public static void ValidateParameters(
            MethodInfo actualMethod,
            TypeValidatorBase returnTypeValidator,
            params TypeValidatorBase[] parameterTypeValidators)
        {
            Contract.Requires(actualMethod != null);
            Contract.Requires(returnTypeValidator != null);
            Contract.Requires(parameterTypeValidators != null);

            returnTypeValidator.Validate(actualMethod.ReturnType);

            var parameters = actualMethod.GetParameters();
            Assert.That(parameterTypeValidators.Length, Is.EqualTo(parameters.Length));
            Parallel.For(0, parameterTypeValidators.Length, i => parameterTypeValidators[i].Validate(parameters[i].ParameterType));
        }

        /// <summary>
        /// Validates a type against expected generic argument types.
        /// </summary>
        /// <param name="actualType">Type to validate.</param>
        /// <param name="typeValidator">Expected types.</param>
        public static void ValidateType(
            Type actualType,
            TypeValidatorBase typeValidator)
        {
            Contract.Requires(actualType != null);
            Contract.Requires(typeValidator != null);

            typeValidator.Validate(actualType);
        }

        /// <summary>
        /// Validates the type.
        /// </summary>
        public abstract void Validate(Type actualType);
    }
}
