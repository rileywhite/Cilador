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

using Bix.Mixers.Fody.Core;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace Bix.Mixers.Fody.ILCloning
{
    /// <summary>
    /// Clones <see cref="MethodDefinition"/> contents from a source to a target.
    /// </summary>
    internal class MethodSignatureCloner : ClonerBase<MethodDefinition>
    {
        /// <summary>
        /// Creates a new <see cref="MethodSignatureCloner"/>
        /// </summary>
        /// <param name="ilCloningContext">IL cloning context.</param>
        /// <param name="target">Cloning target.</param>
        /// <param name="source">Cloning source.</param>
        public MethodSignatureCloner(ILCloningContext ilCloningContext, MethodDefinition target, MethodDefinition source)
            : base(ilCloningContext, target, source)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(target != null);
            Contract.Requires(target.Parameters != null);
            Contract.Requires(source != null);
            Contract.Requires(source.Parameters != null);
            Contract.Ensures(this.ParameterCloners != null);

            this.PopulateParameterCloners();
        }

        /// <summary>
        /// Populates <see cref="ParameterCloners"/>
        /// </summary>
        private void PopulateParameterCloners()
        {
            Contract.Ensures(this.ParameterCloners != null);

            this.ParameterCloners = new List<ParameterCloner>();

            var voidTypeReference = this.ILCloningContext.RootTarget.Module.Import(typeof(void));
            foreach (var sourceParameter in this.Source.Parameters)
            {
                var targetParameter = new ParameterDefinition(
                    sourceParameter.Name,
                    sourceParameter.Attributes,
                    voidTypeReference);
                this.Target.Parameters.Add(targetParameter);
                this.ParameterCloners.Add(new ParameterCloner(this, targetParameter, sourceParameter));
            }
        }

        /// <summary>
        /// Gets or sets the collection of parameter cloners for contained parameters
        /// </summary>
        public List<ParameterCloner> ParameterCloners { get; private set; }

        /// <summary>
        /// Clones the method with the exception of the method body
        /// </summary>
        public override void Clone()
        {
            Contract.Assert(this.Target.DeclaringType != null);
            Contract.Assert(this.Target.Name == this.Source.Name);
            Contract.Assert(this.Target.Parameters.Count == this.Source.Parameters.Count);

            this.Target.Attributes = this.Source.Attributes;
            this.Target.CallingConvention = this.Source.CallingConvention;
            this.Target.ExplicitThis = this.Source.ExplicitThis;
            this.Target.HasThis = this.Source.HasThis;
            this.Target.ImplAttributes = this.Source.ImplAttributes;
            this.Target.IsAddOn = this.Source.IsAddOn;
            this.Target.IsCheckAccessOnOverride = this.Source.IsCheckAccessOnOverride;
            this.Target.IsFire = this.Source.IsFire;
            this.Target.IsForwardRef = this.Source.IsForwardRef;
            this.Target.IsGetter = this.Source.IsGetter;
            this.Target.IsIL = this.Source.IsIL;
            this.Target.IsInternalCall = this.Source.IsInternalCall;
            this.Target.IsManaged = this.Source.IsManaged;
            this.Target.IsNative = this.Source.IsNative;
            this.Target.IsOther = this.Source.IsOther;
            this.Target.IsPreserveSig = this.Source.IsPreserveSig;
            this.Target.IsRemoveOn = this.Source.IsRemoveOn;
            this.Target.IsRuntime = this.Source.IsRuntime;
            this.Target.IsSetter = this.Source.IsSetter;
            this.Target.IsSynchronized = this.Source.IsSynchronized;
            this.Target.IsUnmanaged = this.Source.IsUnmanaged;
            this.Target.NoInlining = this.Source.NoInlining;
            this.Target.NoOptimization = this.Source.NoOptimization;
            this.Target.SemanticsAttributes = this.Source.SemanticsAttributes;

            // TODO research correct usage of method MetadataToken
            //this.Target.MetadataToken = new MetadataToken(this.Source.MetadataToken.TokenType, this.Source.MetadataToken.RID);

            if(this.Source.IsPInvokeImpl)
            {
                throw new WeavingException(string.Format(
                    "Configured mixin implementation may not contain extern methods: [{0}]",
                    this.ILCloningContext.RootSource.FullName));
            }
            Contract.Assert(this.Source.PInvokeInfo == null);

            if (this.Source.HasOverrides)
            {
                foreach (var sourceOverride in this.Source.Overrides)
                {
                    this.Target.Overrides.Add(this.ILCloningContext.RootImport(sourceOverride));
                }
            }

            // I get a similar issue here as with the duplication in the FieldCloner...adding a clear line to work around
            this.Target.CustomAttributes.Clear();
            this.Target.CloneAllCustomAttributes(this.Source, this.ILCloningContext);

            if (this.Source.HasSecurityDeclarations)
            {
                // TODO method security declarations
                throw new WeavingException(string.Format(
                    "Configured mixin implementation may not contain methods annotated with security attributes: [{0}]",
                    this.ILCloningContext.RootSource.FullName));
            }

            var sourceMethodReturnType = this.Source.MethodReturnType;
            Contract.Assert(sourceMethodReturnType != null);
            this.Target.MethodReturnType = new MethodReturnType(this.Target);
            this.Target.MethodReturnType.ReturnType = this.ILCloningContext.RootImport(sourceMethodReturnType.ReturnType);
            this.Target.MethodReturnType.Attributes = sourceMethodReturnType.Attributes;
            this.Target.MethodReturnType.Constant = sourceMethodReturnType.Constant;
            this.Target.MethodReturnType.HasConstant = sourceMethodReturnType.HasConstant;

            // TODO research correct usage of MethodReturnType.MarshalInfo
            if (sourceMethodReturnType.MarshalInfo != null)
            {
                this.Target.MethodReturnType.MarshalInfo = new MarshalInfo(sourceMethodReturnType.MarshalInfo.NativeType);
            }

            // TODO research correct usage of MethodReturnType.MetadataToken
            //this.Target.MethodReturnType.MetadataToken =
            //    new MetadataToken(sourceMethodReturnType.MetadataToken.TokenType, sourceMethodReturnType.MetadataToken.RID);

            this.Target.MethodReturnType.CloneAllCustomAttributes(sourceMethodReturnType, this.ILCloningContext);

            this.IsCloned = true;
        }
    }
}
