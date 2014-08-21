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

using Mono.Cecil;
using Mono.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.ILCloning
{
    /// <summary>
    /// Extension methods used for IL cloning
    /// </summary>
    internal static class ILCloningExtensions
    {
        /// <summary>
        /// Invokes clone on each item
        /// </summary>
        /// <param name="cloners"></param>
        public static void Clone(this IEnumerable<ICloner> cloners)
        {
            Contract.Requires(cloners != null);
            Contract.Requires(!cloners.Any(cloner => cloner.IsCloned));
            Contract.Ensures(cloners.All(cloner => cloner.IsCloned));

            // TODO test this parallel version
            //Parallel.ForEach(
            //    cloners,
            //    new Action<ICloner>(cloner => cloner.Clone()));
            foreach (var cloner in cloners) { cloner.Clone(); }
        }

        /// <summary>
        /// Determines whether a type is nested within another type, even deeply.
        /// </summary>
        /// <param name="type">Type that may be nested within <paramref name="possibleAncestorType"/></param>
        /// <param name="possibleAncestorType">Type that may contain <paramref name="type"/>.</param>
        /// <returns><c>true</c> if <paramref name="type"/> is nested within <paramref name="possibleAncestorType"/>, else <c>false</c>.</returns>
        [Pure]
        public static bool IsNestedWithin(this TypeReference type, TypeDefinition possibleAncestorType)
        {
            if (type == null || type.DeclaringType == null) { return false; }
            else if (type.DeclaringType.Resolve().FullName == possibleAncestorType.FullName) { return true; }
            else { return type.DeclaringType.IsNestedWithin(possibleAncestorType); }
        }

        /// <summary>
        /// Determines whether a member that
        /// (1) is a generic instance or is contained within a generic instance that
        /// (2) has any generic argument that
        /// (3) is a nested type within another given type.
        /// </summary>
        /// <param name="member">Member to check.</param>
        /// <param name="argumentsSearchType">Type to look for generic arguments that are nested within it.</param>
        /// <returns><c>true</c> if <paramref name="member"/> is requires the use of a type within <paramref name="argumentsSearchType"/> for a generic argument, else <c>false</c>.</returns>
        [Pure]
        public static bool IsAnyTypeAncestorAGenericInstanceWithArgumentsIn(this MemberReference member, TypeDefinition argumentsSearchType)
        {
            if (member == null || member.DeclaringType == null)
            {
                return false;
            }
            else if (member.DeclaringType.IsGenericInstance &&
                     ((GenericInstanceType)member.DeclaringType).GenericArguments.Any(
                        genericArgument => genericArgument.IsNestedWithin(argumentsSearchType)))
            {
                return true;
            }
            else
            {
                return
                    member.DeclaringType != null &&
                    member.DeclaringType.IsAnyTypeAncestorAGenericInstanceWithArgumentsIn(argumentsSearchType);
            }
        }

        /// <summary>
        /// Determines whether a target and source method reference have equivalent signatures within the context
        /// of IL cloning.
        /// </summary>
        /// <param name="target">Target method..</param>
        /// <param name="source">Source method</param>
        /// <param name="ilCloningContext">IL cloning context.</param>
        /// <returns><c>true</c> if the signatures are equivalent within the root target and source, else <c>false</c></returns>
        [Pure]
        public static bool SignatureEquals(this MethodReference target, MethodReference source, ILCloningContext ilCloningContext)
        {
            Contract.Requires(ilCloningContext != null);

            if (target == null || source == null) { return target == null && source == null; }

            return source.FullName.Replace(ilCloningContext.RootSource.FullName, ilCloningContext.RootTarget.FullName) == target.FullName;
        }

        /// <summary>
        /// Clones all custom attributes from a source to a target.
        /// </summary>
        /// <param name="target">Cloning target.</param>
        /// <param name="source">Cloning source.</param>
        /// <param name="ilCloningContext">Context for IL cloning.</param>
        public static void CloneAllCustomAttributes(
            this ICustomAttributeProvider target,
            ICustomAttributeProvider source,
            ILCloningContext ilCloningContext)
        {
            Contract.Requires(target != null);
            Contract.Requires(target.CustomAttributes != null);
            Contract.Requires(target.CustomAttributes.Count == 0 || target == ilCloningContext.RootTarget);
            Contract.Requires(source != null);
            Contract.Requires(source.CustomAttributes != null);
            Contract.Requires(target != source);
            Contract.Requires(ilCloningContext != null);
            Contract.Ensures(
                target.CustomAttributes.Count == source.CustomAttributes.Count ||
                (target == ilCloningContext.RootTarget && target.CustomAttributes.Count > source.CustomAttributes.Count));

            foreach (var sourceAttribute in source.CustomAttributes)
            {
                // TODO what is the blob argument for custom attributes?
                var targetAttribute = new CustomAttribute(ilCloningContext.RootImport(sourceAttribute.Constructor));
                if (sourceAttribute.HasConstructorArguments)
                {
                    foreach (var sourceArgument in sourceAttribute.ConstructorArguments)
                    {
                        targetAttribute.ConstructorArguments.Add(
                            new CustomAttributeArgument(
                                ilCloningContext.RootImport(sourceArgument.Type),
                                ilCloningContext.DynamicRootImport(sourceArgument.Value)));
                    }
                }

                if (sourceAttribute.HasProperties)
                {
                    foreach (var sourceProperty in sourceAttribute.Properties)
                    {
                        targetAttribute.Properties.Add(
                            new CustomAttributeNamedArgument(
                                sourceProperty.Name,
                                new CustomAttributeArgument(
                                    ilCloningContext.RootImport(sourceProperty.Argument.Type),
                                    ilCloningContext.DynamicRootImport(sourceProperty.Argument.Value))));
                    }
                }
                target.CustomAttributes.Add(targetAttribute);
            }
        }
    }
}
