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

using Mono.Cecil;
using Mono.Cecil.Cil;
using System;

namespace Cilador.Dispatch
{
    /// <summary>
    /// Provides a way to ensure that any cecil references are resolved
    /// as well as providing information about whether then needed to
    /// be resolved.
    /// </summary>
    internal static class DynamicResolver
    {
        /// <summary>
        /// Attempts to resolve a given object of an unknown type.
        /// </summary>
        /// <remarks>
        /// If <paramref name="item"/> is a resolvable cecil type, then it will be resolved. If it is already resolved (i.e. a member definition)
        /// or if it is not a resolvable type, then it will not be resolved.
        /// </remarks>
        /// <param name="item">Item to possibly resolve.</param>
        /// <param name="resolvedItem">If the resolve was necesary, then this will be populated with the resolved item, otherwise it will be given the value of <paramref name="item"/>.</param>
        /// <returns><c>true</c> if the item was resolved, else <c>false</c>.</returns>
        public static bool DynamicTryResolve(dynamic item, out object resolvedItem)
        {
            if (item != null) { return (bool)TryResolve(item, out resolvedItem); }

            resolvedItem = null;
            return false;
        }

        /// <summary>
        /// Any non-resolvable types will be directed to this method.
        /// </summary>
        /// <param name="item">Non-cecil type instance.</param>
        /// <param name="resolvedItem">Output parameter that will be populated with <paramref name="item"/>.</param>
        /// <returns><c>false</c></returns>
        private static bool TryResolve(object item, out object resolvedItem)
        {
            resolvedItem = item;
            return false;
        }

        /// <summary>
        /// A cecil type reference will be directed to this method.
        /// </summary>
        /// <param name="item">Cecil reference.</param>
        /// <param name="resolvedItem">Output parameter that will be populated with the resolved cecil definition.</param>
        /// <returns><c>true</c></returns>
        private static bool TryResolve(TypeReference item, out object resolvedItem)
        {
            resolvedItem = item.Resolve();
            return true;
        }

        /// <summary>
        /// A cecil type definition will be directed to this method.
        /// </summary>
        /// <param name="item">Cecil definition.</param>
        /// <param name="resolvedItem">Output parameter that will be populated with the value of <paramref name="item"/>.</param>
        /// <returns><c>false</c></returns>
        private static bool TryResolve(TypeDefinition item, out object resolvedItem)
        {
            resolvedItem = item;
            return false;
        }

        /// <summary>
        /// A cecil field reference will be directed to this method.
        /// </summary>
        /// <param name="item">Cecil reference.</param>
        /// <param name="resolvedItem">Output parameter that will be populated with the resolved cecil definition.</param>
        /// <returns><c>true</c></returns>
        private static bool TryResolve(FieldReference item, out object resolvedItem)
        {
            resolvedItem = item.Resolve();
            return true;
        }

        /// <summary>
        /// A cecil field definition will be directed to this method.
        /// </summary>
        /// <param name="item">Cecil definition.</param>
        /// <param name="resolvedItem">Output parameter that will be populated with the value of <paramref name="item"/>.</param>
        /// <returns><c>false</c></returns>
        private static bool TryResolve(FieldDefinition item, out object resolvedItem)
        {
            resolvedItem = item;
            return false;
        }

        /// <summary>
        /// A cecil property reference will be directed to this method.
        /// </summary>
        /// <param name="item">Cecil reference.</param>
        /// <param name="resolvedItem">Output parameter that will be populated with the resolved cecil definition.</param>
        /// <returns><c>true</c></returns>
        private static bool TryResolve(PropertyReference item, out object resolvedItem)
        {
            resolvedItem = item.Resolve();
            return true;
        }

        /// <summary>
        /// A cecil property definition will be directed to this method.
        /// </summary>
        /// <param name="item">Cecil definition.</param>
        /// <param name="resolvedItem">Output parameter that will be populated with the value of <paramref name="item"/>.</param>
        /// <returns><c>false</c></returns>
        private static bool TryResolve(PropertyDefinition item, out object resolvedItem)
        {
            resolvedItem = item;
            return false;
        }

        /// <summary>
        /// A cecil method reference will be directed to this method.
        /// </summary>
        /// <param name="item">Cecil reference.</param>
        /// <param name="resolvedItem">Output parameter that will be populated with the resolved cecil definition.</param>
        /// <returns><c>true</c></returns>
        private static bool TryResolve(MethodReference item, out object resolvedItem)
        {
            resolvedItem = item.Resolve();
            return true;
        }

        /// <summary>
        /// A cecil method definition will be directed to this method.
        /// </summary>
        /// <param name="item">Cecil definition.</param>
        /// <param name="resolvedItem">Output parameter that will be populated with the value of <paramref name="item"/>.</param>
        /// <returns><c>false</c></returns>
        private static bool TryResolve(MethodDefinition item, out object resolvedItem)
        {
            resolvedItem = item;
            return false;
        }

        /// <summary>
        /// A cecil event reference will be directed to this method.
        /// </summary>
        /// <param name="item">Cecil reference.</param>
        /// <param name="resolvedItem">Output parameter that will be populated with the resolved cecil definition.</param>
        /// <returns><c>true</c></returns>
        private static bool TryResolve(EventReference item, out object resolvedItem)
        {
            resolvedItem = item.Resolve();
            return true;
        }

        /// <summary>
        /// A cecil event definition will be directed to this method.
        /// </summary>
        /// <param name="item">Cecil definition.</param>
        /// <param name="resolvedItem">Output parameter that will be populated with the value of <paramref name="item"/>.</param>
        /// <returns><c>false</c></returns>
        private static bool TryResolve(EventDefinition item, out object resolvedItem)
        {
            resolvedItem = item;
            return false;
        }

        /// <summary>
        /// A cecil parameter reference will be directed to this method.
        /// </summary>
        /// <param name="item">Cecil reference.</param>
        /// <param name="resolvedItem">Output parameter that will be populated with the resolved cecil definition.</param>
        /// <returns><c>true</c></returns>
        private static bool TryResolve(ParameterReference item, out object resolvedItem)
        {
            resolvedItem = item.Resolve();
            return true;
        }

        /// <summary>
        /// A cecil parameter definition will be directed to this method.
        /// </summary>
        /// <param name="item">Cecil definition.</param>
        /// <param name="resolvedItem">Output parameter that will be populated with the value of <paramref name="item"/>.</param>
        /// <returns><c>false</c></returns>
        private static bool TryResolve(ParameterDefinition item, out object resolvedItem)
        {
            resolvedItem = item;
            return false;
        }

        /// <summary>
        /// A cecil variable reference will be directed to this method.
        /// </summary>
        /// <param name="item">Cecil reference.</param>
        /// <param name="resolvedItem">Output parameter that will be populated with the resolved cecil definition.</param>
        /// <returns><c>true</c></returns>
        private static bool TryResolve(VariableReference item, out object resolvedItem)
        {
            resolvedItem = item.Resolve();
            return true;
        }

        /// <summary>
        /// A cecil variable definition will be directed to this method.
        /// </summary>
        /// <param name="item">Cecil definition.</param>
        /// <param name="resolvedItem">Output parameter that will be populated with the value of <paramref name="item"/>.</param>
        /// <returns><c>false</c></returns>
        private static bool TryResolve(VariableDefinition item, out object resolvedItem)
        {
            resolvedItem = item;
            return false;
        }
    }
}
