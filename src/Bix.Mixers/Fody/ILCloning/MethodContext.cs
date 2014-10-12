using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Bix.Mixers.Fody.ILCloning
{
    internal class MethodContext
    {
        public MethodContext(MethodBodyCloner methodBodyCloner)
        {
            Contract.Requires(methodBodyCloner != null);
            Contract.Requires(methodBodyCloner.ILCloningContext != null);
            Contract.Ensures(this.ILCloningContext != null);

            this.MethodBodyCloner = methodBodyCloner;
        }

        private MethodBodyCloner MethodBodyCloner { get; set; }

        /// <summary>
        /// Gets or sets the context for IL cloning.
        /// </summary>
        public ILCloningContext ILCloningContext { get { return this.MethodBodyCloner.ILCloningContext; } }

        public Instruction RootImport(Instruction source)
        {
            var instructionCloner = this.MethodBodyCloner.InstructionCloners.FirstOrDefault(cloner => cloner.Source == source);
            if (instructionCloner == null)
            {
                throw new InvalidOperationException("Could not locate a instruction for copying an operand");
            }
            return instructionCloner.Target;
        }

        public VariableDefinition RootImport(VariableDefinition source)
        {
            var variableCloner = this.MethodBodyCloner.VariableCloners.FirstOrDefault(cloner => cloner.Source == source);
            if (variableCloner == null)
            {
                throw new InvalidOperationException("Could not locate a variable for copying an instruction");
            }
            return variableCloner.Target;
        }

        public ParameterDefinition RootImport(ParameterDefinition source)
        {
            if (source == this.MethodBodyCloner.Source.ThisParameter)
            {
                return this.MethodBodyCloner.Target.ThisParameter;
            }

            var parameterCloner =
                this.MethodBodyCloner.SignatureCloner.ParameterCloners.FirstOrDefault(cloner => cloner.Source == source);
            if (parameterCloner == null)
            {
                throw new InvalidOperationException("Failed to find a parameter cloner matching the operand in an instruction");
            }

            return parameterCloner.Target;
        }
    }
}
