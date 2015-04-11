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

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Bix.Mixers.ILCloning
{
    /// <summary>
    /// Clones <see cref="MethodDefinition"/> contents from a source to a target.
    /// </summary>
    internal class MethodSignatureCloner : ClonerBase<MethodDefinition>
    {
        /// <summary>
        /// Creates a new <see cref="MethodSignatureCloner"/>
        /// </summary>
        /// <param name="parent">Cloner for the type that contains the method to be cloned.</param>
        /// <param name="source">Cloning source.</param>
        public MethodSignatureCloner(ICloner<TypeDefinition> parent, MethodDefinition source)
            : base(parent.ILCloningContext, source)
        {
            Contract.Requires(parent != null);
            Contract.Requires(parent.ILCloningContext != null);
            Contract.Ensures(this.Parent != null);
            Contract.Ensures(this.ParameterCloners != null);

            this.Parent = parent;
            this.ParameterCloners = new List<ParameterCloner>();
        }

        /// <summary>
        /// Gets or sets the cloner for the type that contains the method to be cloned.
        /// </summary>
        private ICloner<TypeDefinition> Parent { get; set; }

        /// <summary>
        /// Gets or sets whether this cloning operation is redirecting a static constructor.
        /// </summary>
        /// <remarks>
        /// This will be <c>true</c> when a root source and root target both have static constructors.
        /// To deal with the case, the source static constructor is cloned into a different target static
        /// method, and an operation to call that new method is inserted at the beginning of the
        /// target's existing static constructor.
        /// </remarks>
        private bool IsRedirectedStaticConstructor { get; set; }

        /// <summary>
        /// Creates the target method.
        /// </summary>
        /// <returns>Created target.</returns>
        protected override MethodDefinition CreateTarget()
        {
            MethodDefinition targetMethod = null;
            var voidReference = this.ILCloningContext.RootTarget.Module.Import(typeof(void));  // TODO get rid of void ref

            if (this.Source.IsConstructor &&
                this.Source.IsStatic &&
                this.Source.DeclaringType == this.ILCloningContext.RootSource)
            {
                var targetStaticConstructor = this.ILCloningContext.RootTarget.Methods.SingleOrDefault(method => method.IsConstructor && method.IsStatic);

                // if there is no static constructor in the target, then treat it like any other method
                // otherwise we need to merge the methods
                if (targetStaticConstructor != null)
                {
                    // if there is already a target static constructor, then redirect the clone to a different method
                    // and then add a call to the new method into the existing static constructor
                    targetMethod = new MethodDefinition(
                        string.Format("cctor_{0:N}", Guid.NewGuid()),
                        MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig,
                        voidReference);
                    this.Parent.Target.Methods.Add(targetMethod);
                    this.IsRedirectedStaticConstructor = true;
                }
            }

            if (targetMethod == null)
            {
                targetMethod = new MethodDefinition(this.Source.Name, this.Source.Attributes, voidReference);
                this.Parent.Target.Methods.Add(targetMethod);
            }

            return targetMethod;
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

            if (this.IsRedirectedStaticConstructor)
            {
                this.AddCallToRedirectedStaticConstructor();
            }

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

            if(this.Source.IsPInvokeImpl)
            {
                throw new InvalidOperationException(string.Format(
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

            if (this.Source.HasSecurityDeclarations)
            {
                // TODO method security declarations
                throw new InvalidOperationException(string.Format(
                    "Configured mixin implementation may not contain methods annotated with security attributes: [{0}]",
                    this.ILCloningContext.RootSource.FullName));
            }

            this.IsCloned = true;
        }

        /// <summary>
        /// Adds an instruction into the beginning of the target's existing static constructor
        /// to call this cloning target method.
        /// </summary>
        private void AddCallToRedirectedStaticConstructor()
        {
            Contract.Requires(this.IsRedirectedStaticConstructor);

            var targetStaticConstructor =
                this.Parent.Target.Methods.Single(method => method.IsConstructor && method.IsStatic);
            Contract.Assert(targetStaticConstructor.IsConstructor);
            Contract.Assert(targetStaticConstructor.IsStatic);
            Contract.Assert(targetStaticConstructor.HasBody);
            Contract.Assert(targetStaticConstructor.DeclaringType == this.ILCloningContext.RootTarget);

            var firstInstruction = targetStaticConstructor.Body.Instructions[0];
            var targetILProcessor = targetStaticConstructor.Body.GetILProcessor();

            targetILProcessor.InsertBefore(firstInstruction, targetILProcessor.Create(OpCodes.Call, this.Target));
        }
    }
}
