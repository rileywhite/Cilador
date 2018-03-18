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

using Cilador.Clone;
using Cilador.Graph.Core;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;

namespace Cilador.Tests.CloneTests
{
    /// <summary>
    /// Fake cloning context.
    /// </summary>
    internal class FakeCloningConext : ICloningContext
    {
        public FakeCloningConext()
        {
            this.RootImportObjectDelegate = item => item;
        }

        public ICilGraph CilGraph { get; set; }
        public TypeDefinition RootSource { get; set; }
        public TypeDefinition RootTarget { get; set; }

        public TItem DynamicRootImport<TItem>(TItem item)
        {
            return (TItem)this.RootImport((dynamic)item);
        }

        public Func<object, object> RootImportObjectDelegate { get; set; }
        private object RootImport(object item)
        {
            var handler = this.RootImportObjectDelegate;
            return handler != null ? handler(item) : item;
        }

        public Func<TypeReference, TypeReference> RootImportTypeDelegate { get; set; }
        public TypeReference RootImport(TypeReference type)
        {
            var handler = this.RootImportTypeDelegate;
            if (handler != null) { return handler(type); }
            return type;
        }

        public Func<MethodReference, MethodReference> RootImportMethodDelegate { get; set; }
        public MethodReference RootImport(MethodReference method)
        {
            var handler = this.RootImportMethodDelegate;
            if (handler != null) { return handler(method); }
            return method;
        }

        public Func<FieldReference, FieldReference> RootImportFieldDelegate { get; set; }
        public FieldReference RootImport(FieldReference field)
        {
            var handler = this.RootImportFieldDelegate;
            if (handler != null) { return handler(field); }
            return field;
        }


        public Func<ParameterDefinition, ParameterDefinition> RootImportParameterDelegate { get; set; }
        public ParameterDefinition RootImport(ParameterDefinition parameter)
        {
            var handler = this.RootImportParameterDelegate;
            if (handler != null) { return handler(parameter); }
            return parameter;
        }

        public Func<VariableDefinition, VariableDefinition> RootImportVariableDelegate { get; set; }
        public VariableDefinition RootImport(VariableDefinition variable)
        {
            var handler = this.RootImportVariableDelegate;
            if (handler != null) { return handler(variable); }
            return variable;
        }

        public Func<Instruction, Instruction> RootImportInstructionDelegate { get; set; }
        public Instruction RootImport(Instruction instruction)
        {
            var handler = this.RootImportInstructionDelegate;
            if (handler != null) { return handler(instruction); }
            return instruction;
        }
    }
}
