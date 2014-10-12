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
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.ILCloning
{
    /// <summary>
    /// A visitor that traverses a cloner target hierarchy and creates
    /// cloners for each item that should be cloned.
    /// </summary>
    /// <remarks>
    /// This "visits" each item in the hierarchy, similar to the visitor pattern,
    /// but the classic visitor pattern doesn't quite make sense here. This
    /// is a variation.
    /// </remarks>
    internal class ClonerGatheringVisitor
    {
        /// <summary>
        /// Create a new <see cref="ClonerGatheringVisitor"/>
        /// </summary>
        public ClonerGatheringVisitor(ILCloningContext ilCloningContext)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Ensures(this.ILCloningContext != null);
            Contract.Ensures(this.Cloners != null);

            this.ILCloningContext = ilCloningContext;
            this.Cloners = new Cloners();
        }

        /// <summary>
        /// Gets or sets the context for IL cloning.
        /// </summary>
        private ILCloningContext ILCloningContext { get; set; }

        /// <summary>
        /// Gets or sets the cloners created during visit operations.
        /// </summary>
        public Cloners Cloners { get; private set; }

        /// <summary>
        /// Gathers all cloners for the given cloning source and target
        /// </summary>
        /// <param name="source">Cloning source to gather cloners for.</param>
        /// <param name="target">Cloning target to gather cloners for.</param>
        public void Visit(TypeDefinition sourceType, TypeDefinition targetType)
        {
            Contract.Requires(sourceType != null);
            Contract.Requires(targetType != null);

            this.Cloners.AddCloner(new TypeCloner(this.ILCloningContext, targetType, sourceType));

            foreach (var sourceNestedType in sourceType.NestedTypes)
            {
                var targetNestedType = new TypeDefinition(sourceNestedType.Namespace, sourceNestedType.Name, 0);
                targetType.NestedTypes.Add(targetNestedType);

                this.Visit(
                    (IGenericParameterProvider)sourceNestedType,
                    (IGenericParameterProvider)targetNestedType);
                
                this.Visit(sourceNestedType, targetNestedType);
            }

            var voidReference = targetType.Module.Import(typeof(void));

            foreach (var sourceField in sourceType.Fields)
            {
                var targetField = new FieldDefinition(sourceField.Name, 0, voidReference);
                targetType.Fields.Add(targetField);
                this.Visit(sourceField, targetField);
            }

            foreach (var sourceMethod in sourceType.Methods)
            {
                if (sourceMethod.Name == ".cctor" &&
                    sourceMethod.IsStatic &&
                    sourceMethod.DeclaringType == ILCloningContext.RootSource)
                {
                    // TODO should static constructors be supported on the root type?
                    throw new WeavingException(string.Format(
                        "Configured mixin implementation cannot have a type initializer (i.e. static constructor): [{0}]",
                        this.ILCloningContext.RootSource.FullName));
                }

                if (sourceMethod.IsConstructor &&
                    sourceMethod.DeclaringType == ILCloningContext.RootSource)
                {
                    if (sourceMethod.HasParameters)
                    {
                        // TODO support constructors for the root type in some meaningful way
                        throw new WeavingException(string.Format(
                            "Configured mixin implementation cannot use constructors: [{0}]",
                            this.ILCloningContext.RootSource.FullName));
                    }

                    this.VisitRootSourceDefaultConstructor(sourceMethod, targetType.Methods.Where(method => method.IsConstructor));

                    continue;
                }

                var targetMethod = new MethodDefinition(sourceMethod.Name, 0, voidReference);
                targetType.Methods.Add(targetMethod);
                this.Visit(sourceMethod, targetMethod);
            }

            foreach (var sourceProperty in sourceType.Properties)
            {
                var targetProperty = new PropertyDefinition(sourceProperty.Name, 0, voidReference);
                targetType.Properties.Add(targetProperty);
                this.Visit(sourceProperty, targetProperty);
            }

            foreach (var sourceEvent in sourceType.Events)
            {
                var targetEvent = new EventDefinition(sourceEvent.Name, 0, voidReference);
                targetType.Events.Add(targetEvent);
                this.Visit(sourceEvent, targetEvent);
            }
        }

        /// <summary>
        /// Visits the default source constructor to look for the instance initialization code which
        /// takes care of initializing fields that are initialized in their declarations.
        /// </summary>
        /// <param name="sourceConstructor">Source's default constructor.</param>
        /// <param name="targetConstructors">All target constructors.</param>
        private void VisitRootSourceDefaultConstructor(MethodDefinition sourceConstructor, IEnumerable<MethodDefinition> targetConstructors)
        {
            Contract.Requires(sourceConstructor != null);
            Contract.Requires(sourceConstructor.DeclaringType == this.ILCloningContext.RootSource);
            Contract.Requires(!sourceConstructor.Parameters.Any());
            Contract.Requires(sourceConstructor.HasBody);
            Contract.Requires(targetConstructors != null);

            var sourceConstructorBody = sourceConstructor.Body;
            Contract.Assert(sourceConstructorBody != null);
            Contract.Assert(sourceConstructorBody.Instructions != null);
            Contract.Assert(sourceConstructorBody.Instructions.Any());

            // we want the collection of instructions between the first ldarg and the call into the base constructor.
            var sourceInstruction = sourceConstructorBody.Instructions[0];
            if (sourceInstruction.OpCode != OpCodes.Ldarg_0 || sourceInstruction.Operand != null)
            {
                throw new InvalidOperationException("The first instruction in a mixin implementation's default constructor wasn't the expected ldarg.0");
            }

            var sourceInitializationInstructions = new List<Instruction>();
            var objectConstructorFullName = this.ILCloningContext.RootSource.Module.Import(typeof(object).GetConstructor(new Type[0])).FullName;
            int? baseConstructorCallInstructionIndex = default(int?);
            for (int i = 1; i < sourceConstructorBody.Instructions.Count && !baseConstructorCallInstructionIndex.HasValue; i++)
            {
                // we have a rule that an implementation must have only a default constructor and that it must inherit from object
                // all we have to do is find the first call to object's constructor
                sourceInstruction = sourceConstructorBody.Instructions[i];
                if (sourceInstruction.OpCode == OpCodes.Call &&
                    sourceInstruction.Operand != null &&
                    sourceInstruction.Operand is MethodReference &&
                    ((MethodReference)sourceInstruction.Operand).FullName == objectConstructorFullName)
                {
                    baseConstructorCallInstructionIndex = i;
                }
                else
                {
                    sourceInitializationInstructions.Add(sourceInstruction);
                }
            }

            if (!baseConstructorCallInstructionIndex.HasValue)
            {
                // if more constructors were allowed, this might be possible in constructors that call through to other constructors
                // under current assumptions, this shouldn't happen
                throw new InvalidOperationException("Could not find base constructor call in mixin implementation.");
            }

            // TODO enforce the rule about no logic in the source's default constructor

            // sanity check
            Contract.Assert(sourceInitializationInstructions.Count == baseConstructorCallInstructionIndex - 1);

            if (!sourceInitializationInstructions.Any())
            {
                // nothing to do if there are no source initialization instructions
                return;
            }
            
            // find target constructors that call into their base constructor
            var targetBaseTypeConstructorFullNames =
                from method in this.ILCloningContext.RootTarget.BaseType.Resolve().Methods
                where method.IsConstructor
                select method.FullName;

            // filter target constructors by those that call the base constructor
            var initializingTargetConstructors =
                from method in targetConstructors
                where method.HasBody && method.Body.Instructions.Any(instruction =>
                    instruction.OpCode == OpCodes.Call &&
                    instruction.Operand != null &&
                    instruction.Operand is MethodReference &&
                    targetBaseTypeConstructorFullNames.Contains(((MethodReference)instruction.Operand).FullName))
                select method;

            // a valid type should have at least one constructor that calls into a base constructor
            if (!initializingTargetConstructors.Any())
            {
                throw new InvalidOperationException("Could not find any target constructors that call into a base constructor.");
            }

            // we're going to insert the initializing instruction clone targets into the initializing constructors after the first instruction
            var instructionCloners = new List<InstructionCloner>(sourceInitializationInstructions.Count * initializingTargetConstructors.Count());
            foreach (var initializingTargetConstructor in initializingTargetConstructors)
            {
                var ilProcessor = initializingTargetConstructor.Body.GetILProcessor();
                var firstInstructionInTargetConstructor = initializingTargetConstructor.Body.Instructions[0];

                // go backwards through the source initialization instructions
                // this makes it so that every new instruction is added just after the first instruction in the target
                for (int i = sourceInitializationInstructions.Count - 1; i >= 0; i++)
                {
                    //Instruction targetInstruction = InstructionCloner.CreateCloningTargetFor(this.ILCloningContext, ilProcessor, sourceInstruction);
                    //ilProcessor.Append(targetInstruction);
                    //instructionCloners.Add(new InstructionCloner(this, targetInstruction, sourceInstruction));
                }
            }
            this.Cloners.AddCloners(instructionCloners);
        }

        /// <summary>
        /// Gathers all cloners for the given cloning source and target
        /// </summary>
        /// <param name="source">Cloning source to gather cloners for.</param>
        /// <param name="target">Cloning target to gather cloners for.</param>
        private void Visit(FieldDefinition sourceField, FieldDefinition targetField)
        {
            Contract.Requires(sourceField != null);
            Contract.Requires(targetField != null);

            this.Cloners.AddCloner(new FieldCloner(this.ILCloningContext, targetField, sourceField));
        }

        /// <summary>
        /// Gathers all cloners for the given cloning source and target
        /// </summary>
        /// <param name="source">Cloning source to gather cloners for.</param>
        /// <param name="target">Cloning target to gather cloners for.</param>
        private void Visit(MethodDefinition sourceMethod, MethodDefinition targetMethod)
        {
            Contract.Requires(sourceMethod != null);
            Contract.Requires(targetMethod != null);

            var methodSignatureCloner = new MethodSignatureCloner(this.ILCloningContext, targetMethod, sourceMethod);
            this.Cloners.AddCloner(methodSignatureCloner);

            Contract.Assert(methodSignatureCloner.ParameterCloners != null);
            this.Cloners.AddCloners(methodSignatureCloner.ParameterCloners);

            this.Visit(
                (IGenericParameterProvider)sourceMethod,
                (IGenericParameterProvider)targetMethod);

            if (sourceMethod.HasBody)
            {
                var methodBodyCloner = new MethodBodyCloner(methodSignatureCloner, targetMethod.Body, sourceMethod.Body);
                this.Cloners.AddCloner(methodBodyCloner);
                this.Cloners.AddCloners(methodBodyCloner.VariableCloners);
                this.Cloners.AddCloners(methodBodyCloner.InstructionCloners);
            }
        }

        /// <summary>
        /// Gathers all cloners for the given cloning source and target
        /// </summary>
        /// <param name="source">Cloning source to gather cloners for.</param>
        /// <param name="target">Cloning target to gather cloners for.</param>
        private void Visit(PropertyDefinition sourceProperty, PropertyDefinition targetProperty)
        {
            Contract.Requires(sourceProperty != null);
            Contract.Requires(targetProperty != null);

            var propertyCloner = new PropertyCloner(this.ILCloningContext, targetProperty, sourceProperty);
            this.Cloners.AddCloner(propertyCloner);
        }

        /// <summary>
        /// Gathers all cloners for the given cloning source and target
        /// </summary>
        /// <param name="source">Cloning source to gather cloners for.</param>
        /// <param name="target">Cloning target to gather cloners for.</param>
        private void Visit(EventDefinition sourceEvent, EventDefinition targetEvent)
        {
            Contract.Requires(sourceEvent != null);
            Contract.Requires(targetEvent != null);

            this.Cloners.AddCloner(new EventCloner(this.ILCloningContext, targetEvent, sourceEvent));
        }

        /// <summary>
        /// Gathers all cloners for the given cloning source and target.
        /// </summary>
        /// <param name="source">Cloning source to gather cloners for.</param>
        /// <param name="target">Cloning target to gather cloners for.</param>
        private void Visit(IGenericParameterProvider sourceGenericParameterProvider, IGenericParameterProvider targetGenericParameterProvider)
        {
            var voidReference = targetGenericParameterProvider.Module.Import(typeof(void));
            foreach (var sourceGenericParameter in sourceGenericParameterProvider.GenericParameters)
            {
                targetGenericParameterProvider.GenericParameters.Add(new GenericParameter(voidReference)); // this is just a placeholder since null is not allowed
                this.Cloners.AddCloner(new GenericParameterCloner(
                    this.ILCloningContext,
                    () => targetGenericParameterProvider.GenericParameters[sourceGenericParameter.Position],
                    targetGenericParameter => targetGenericParameterProvider.GenericParameters[sourceGenericParameter.Position] = targetGenericParameter,
                    () => sourceGenericParameter));
            }
        }
    }
}
