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
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.ILCloning
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
            IILCloningContext ilCloningContext)
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

        /// <summary>
        /// Determines whether in instruction opcode stores a variable.
        /// </summary>
        /// <param name="code">OpCode's CIL code</param>
        /// <returns><c>true</c> if the code is a variable store code, else <c>false</c>.</returns>
        internal static bool IsStoreVariableOpCode(this Code code)
        {
            return
                code == Code.Stloc ||
                code == Code.Stloc_S ||
                code == Code.Stloc_0 ||
                code == Code.Stloc_1 ||
                code == Code.Stloc_2 ||
                code == Code.Stloc_3;
        }

        /// <summary>
        /// Determines whether in instruction opcode loads a variable.
        /// </summary>
        /// <param name="code">OpCode's CIL code</param>
        /// <returns><c>true</c> if the code is a variable load code, else <c>false</c>.</returns>
        internal static bool IsLoadVariableOpCode(this Code code)
        {
            return
                code == Code.Ldloc ||
                code == Code.Ldloc_S ||
                code == Code.Ldloc_0 ||
                code == Code.Ldloc_1 ||
                code == Code.Ldloc_2 ||
                code == Code.Ldloc_3;
        }

        /// <summary>
        /// Tries to find the index of a variable referenced by an instruction, if any.
        /// </summary>
        /// <param name="instruction">Instruction to examine.</param>
        /// <param name="variableIndex">Index to populate, if found.</param>
        /// <returns><c>true</c> if a referenced variable was found, otherwise <c>false</c>.</returns>
        internal static bool TryGetVariableIndex(this Instruction instruction, out int? variableIndex)
        {
            Contract.Requires(instruction != null);
            Contract.Ensures(Contract.ValueAtReturn<int?>(out variableIndex).HasValue || !Contract.Result<bool>());

            switch (instruction.OpCode.Code)
            {
                case Code.Ldloc_0:
                case Code.Stloc_0:
                    variableIndex = 0;
                    return true;

                case Code.Ldloc_1:
                case Code.Stloc_1:
                    variableIndex = 1;
                    return true;

                case Code.Ldloc_2:
                case Code.Stloc_2:
                    variableIndex = 2;
                    return true;

                case Code.Ldloc_3:
                case Code.Stloc_3:
                    variableIndex = 3;
                    return true;

                case Code.Ldloc:
                case Code.Ldloc_S:
                case Code.Ldloca:
                case Code.Ldloca_S:
                case Code.Stloc:
                case Code.Stloc_S:
                    variableIndex = (int)instruction.Operand;
                    return true;

                default:
                    variableIndex = default(int?);
                    return false;
            }
        }

        /// <summary>
        /// Examines the instruction and, if it accesses a local variable by index, provides a new version
        /// that translates the access by the given count.
        /// </summary>
        internal static Instruction ApplyLocalVariableTranslation(this Instruction instruction, int translate)
        {
            Contract.Requires(instruction != null);

            int? initialVariableIndex;
            if (translate == 0 || !instruction.TryGetVariableIndex(out initialVariableIndex)) { return instruction; }
            Contract.Assert(initialVariableIndex.HasValue);

            var newIndex = initialVariableIndex.Value + translate;
            if (newIndex < 0)
            {
                throw new InvalidOperationException("A variable index less than 0 cannot be used.");
            }
            if (newIndex > ushort.MaxValue)
            {
                throw new InvalidOperationException(string.Format("A variable index greater than {0} cannot be used.", ushort.MaxValue));
            }

            if (instruction.OpCode.Code.IsStoreVariableOpCode())
            {
                switch(newIndex)
                {
                    case 0:
                        return Instruction.Create(OpCodes.Stloc_0);

                    case 1:
                        return Instruction.Create(OpCodes.Stloc_1);

                    case 2:
                        return Instruction.Create(OpCodes.Stloc_2);

                    case 3:
                        return Instruction.Create(OpCodes.Stloc_3);

                    default:
                        if (newIndex <= byte.MaxValue)
                        {
                            return Instruction.Create(OpCodes.Stloc_S, Convert.ToByte(newIndex));
                        }
                        else if(newIndex <= ushort.MaxValue)
                        {
                            return Instruction.Create(OpCodes.Stloc_S, Convert.ToUInt16(newIndex));
                        }
                        else
                        {
                            throw new InvalidOperationException("Variable index for store instruction is not in range of byte or ushort.");
                        }
                }
            }
            else if (instruction.OpCode.Code.IsLoadVariableOpCode())
            {
                switch (newIndex)
                {
                    case 0:
                        return Instruction.Create(OpCodes.Ldloc_0);

                    case 1:
                        return Instruction.Create(OpCodes.Ldloc_1);

                    case 2:
                        return Instruction.Create(OpCodes.Ldloc_2);

                    case 3:
                        return Instruction.Create(OpCodes.Ldloc_3);

                    default:
                        if (newIndex <= byte.MaxValue)
                        {
                            return Instruction.Create(OpCodes.Ldloc_S, Convert.ToByte(newIndex));
                        }
                        else if (newIndex <= ushort.MaxValue)
                        {
                            return Instruction.Create(OpCodes.Ldloc_S, Convert.ToUInt16(newIndex));
                        }
                        else
                        {
                            throw new InvalidOperationException("Variable index for load instruction is not in range of byte or ushort.");
                        }
                }
            }
            else
            {
                throw new InvalidOperationException("Expected a variable store or load code.");
            }
        }
    }
}
