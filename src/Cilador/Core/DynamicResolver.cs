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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cilador.Core
{
    internal static class DynamicResolver
    {
        public static bool DynamicTryResolve(dynamic item, out object resolvedItem)
        {
            if (item == null)
            {
                resolvedItem = null;
                return false;
            }
            return (bool)TryResolve(item, out resolvedItem);
        }

        private static bool TryResolve(object item, out object resolvedItem)
        {
            resolvedItem = item;
            return false;
        }

        private static bool TryResolve(TypeReference item, out object resolvedItem)
        {
            resolvedItem = item.Resolve();
            return true;
        }

        private static bool TryResolve(FieldReference item, out object resolvedItem)
        {
            resolvedItem = item.Resolve();
            return true;
        }

        private static bool TryResolve(PropertyReference item, out object resolvedItem)
        {
            resolvedItem = item.Resolve();
            return true;
        }

        private static bool TryResolve(MethodReference item, out object resolvedItem)
        {
            resolvedItem = item.Resolve();
            return true;
        }

        private static bool TryResolve(EventReference item, out object resolvedItem)
        {
            resolvedItem = item.Resolve();
            return true;
        }

        private static bool TryResolve(ParameterReference item, out object resolvedItem)
        {
            resolvedItem = item.Resolve();
            return true;
        }

        private static bool TryResolve(VariableReference item, out object resolvedItem)
        {
            resolvedItem = item.Resolve();
            return true;
        }
    }
}
