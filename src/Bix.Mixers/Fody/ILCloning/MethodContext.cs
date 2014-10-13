using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Bix.Mixers.Fody.ILCloning
{
    internal class MethodContext
    {
        public MethodContext(MethodBodyCloner methodBodyCloner) : this(
            methodBodyCloner.ILCloningContext,
            Tuple.Create(methodBodyCloner.Source.ThisParameter, methodBodyCloner.Target.ThisParameter),
            methodBodyCloner.SignatureCloner.ParameterCloners,
            methodBodyCloner.VariableCloners,
            methodBodyCloner.InstructionCloners)
        {
            Contract.Requires(methodBodyCloner != null);
            Contract.Requires(methodBodyCloner.ILCloningContext != null);
            Contract.Ensures(this.ILCloningContext != null);
        }

        public MethodContext(
            ILCloningContext ilCloningContext,
            Tuple<ParameterDefinition, ParameterDefinition> thisParameterSourceAndTarget,
            IEnumerable<Tuple<ParameterDefinition, ParameterDefinition>> parameterSourceAndTargets,
            IEnumerable<Tuple<VariableDefinition, VariableDefinition>> variableSourceAndTargets,
            IEnumerable<Tuple<Instruction, Instruction>> instructionSourceAndTargets)
        {
            Contract.Requires(ilCloningContext != null);
            Contract.Requires(thisParameterSourceAndTarget != null);
            Contract.Requires(parameterSourceAndTargets != null);
            Contract.Requires(variableSourceAndTargets != null);
            Contract.Requires(instructionSourceAndTargets != null);
            Contract.Ensures(this.ILCloningContext != null);

            this.ILCloningContext = ilCloningContext;
            this.ThisParameterSourceAndTarget = thisParameterSourceAndTarget;
            this.ParameterSourceAndTargets = parameterSourceAndTargets;
            this.VariableSourceAndTargets = variableSourceAndTargets;
            this.InstructionSourceAndTargets = instructionSourceAndTargets;
        }

        private Tuple<ParameterDefinition, ParameterDefinition> ThisParameterSourceAndTarget { get; set; }
        private IEnumerable<Tuple<ParameterDefinition, ParameterDefinition>> ParameterSourceAndTargets { get; set; }
        private IEnumerable<Tuple<VariableDefinition, VariableDefinition>> VariableSourceAndTargets { get; set; }
        private IEnumerable<Tuple<Instruction, Instruction>> InstructionSourceAndTargets { get; set; }

        /// <summary>
        /// Gets or sets the context for IL cloning.
        /// </summary>
        public ILCloningContext ILCloningContext { get; set; }

        public Instruction RootImport(Instruction source)
        {
            var instructionCloner = this.InstructionSourceAndTargets.FirstOrDefault(cloner => cloner.Item1== source);
            if (instructionCloner == null)
            {
                throw new InvalidOperationException("Could not locate a instruction for copying an operand");
            }
            return instructionCloner.Item2;
        }

        public VariableDefinition RootImport(VariableDefinition source)
        {
            var variableCloner = this.VariableSourceAndTargets.FirstOrDefault(cloner => cloner.Item1 == source);
            if (variableCloner == null)
            {
                throw new InvalidOperationException("Could not locate a variable for copying an instruction");
            }
            return variableCloner.Item2;
        }

        public ParameterDefinition RootImport(ParameterDefinition source)
        {
            if (source == this.ThisParameterSourceAndTarget.Item1)
            {
                return this.ThisParameterSourceAndTarget.Item2;
            }

            var parameterCloner =
                this.ParameterSourceAndTargets.FirstOrDefault(cloner => cloner.Item1 == source);
            if (parameterCloner == null)
            {
                throw new InvalidOperationException("Failed to find a parameter cloner matching the operand in an instruction");
            }

            return parameterCloner.Item2;
        }
    }
}
