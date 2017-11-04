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
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace Cilador.Graph.Factory
{
    internal static class GraphExtensions
    {
        /// <summary>
        /// Gets dependencies for a referenced type.
        /// </summary>
        /// <param name="type">Item to get dependencies from</param>
        /// <returns>Collection of dependencies</returns>
        public static IEnumerable<TypeDefinition> GetDependencies(this TypeReference type)
        {
            if (type == null) { yield break; }

            yield return type.Resolve();
            if (type is TypeDefinition) { yield break; }

            if (!type.IsGenericInstance) { yield break; }

            var genericInstanceTypes = new Stack<GenericInstanceType>();
            genericInstanceTypes.Push((GenericInstanceType)type);

            while (genericInstanceTypes.Count > 0)
            {
                var current = genericInstanceTypes.Pop();
                foreach (var genericArgument in current.GenericArguments)
                {
                    yield return genericArgument.Resolve();
                    if (genericArgument.IsGenericInstance)
                    {
                        genericInstanceTypes.Push((GenericInstanceType)genericArgument);
                    }
                }
            }
        }

        /// <summary>
        /// Gets dependencies for a referenced field.
        /// </summary>
        /// <param name="field">Item to get dependencies from</param>
        /// <returns>Collection of dependencies</returns>
        public static IEnumerable<IMemberDefinition> GetDependencies(this FieldReference field)
        {
            if (field == null) { yield break; }

            yield return field.Resolve();
            if (field is FieldDefinition) { yield break; }

            foreach (var dependency in field.FieldType.GetDependencies())
            {
                yield return dependency;
            }
        }

        /// <summary>
        /// Gets dependencies for a referenced method.
        /// </summary>
        /// <param name="method">Item to get dependencies from</param>
        /// <returns>Collection of dependencies</returns>
        public static IEnumerable<IMemberDefinition> GetDependencies(this MethodReference method)
        {
            if (method == null) { yield break; }

            yield return method.Resolve();
            if (method is MethodDefinition) { yield break; }

            foreach (var dependency in method.ReturnType.GetDependencies())
            {
                yield return dependency;
            }

            if (!method.IsGenericInstance) { yield break; }

            var genericInstanceMethod = (GenericInstanceMethod)method;
            foreach (var dependency in genericInstanceMethod.GenericArguments.SelectMany(genericArgument => genericArgument.GetDependencies())) {
                yield return dependency;
            }
        }
    }
}
