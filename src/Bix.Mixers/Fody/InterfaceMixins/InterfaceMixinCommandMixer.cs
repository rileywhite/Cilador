using Bix.Mixers.Fody.Core;
using Bix.Mixers.Fody.ILCloning;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.InterfaceMixins
{
    internal class InterfaceMixinCommandMixer
    {
        public InterfaceMixinCommandMixer(TypeDefinition interfaceType, TypeDefinition mixinType, TypeDefinition target)
        {
            Contract.Requires(interfaceType != null);
            Contract.Requires(mixinType != null);
            Contract.Requires(target != null);
            Contract.Requires(target.Module != null);
            Contract.Requires(!target.IsValueType);
            Contract.Requires(!target.IsPrimitive);

            Contract.Ensures(this.InterfaceType != null);
            Contract.Ensures(this.MixinType != null);
            Contract.Ensures(this.Target != null);
            Contract.Ensures(this.Target.IsClass);
            Contract.Ensures(this.TargetModule != null);
            Contract.Ensures(this.SourceWithRoot != null);

            if (!interfaceType.IsInterface)
            {
                throw new WeavingException(string.Format("Configured mixin interface type is not an interface: [{0}]", interfaceType.FullName));
            }

            if (!mixinType.Interfaces.Any(@interface => @interface.FullName == interfaceType.FullName))
            {
                throw new WeavingException(string.Format(
                    "The mixin source [{0}] must implement the interface specified in the interfaceType argument [{1}]",
                    mixinType.FullName,
                    interfaceType.FullName));
            }

            this.InterfaceType = interfaceType;
            this.MixinType = mixinType;
            this.SourceWithRoot = TypeSourceWithRoot.CreateWithRootSourceAndTarget(mixinType, target);
            this.TargetModule = target.Module;
            this.Target = target;
        }

        public TypeDefinition InterfaceType { get; private set; }

        public TypeDefinition MixinType { get; private set; }

        private TypeSourceWithRoot SourceWithRoot { get; set; }

        private ModuleDefinition TargetModule { get; set; }

        private TypeDefinition target;
        public TypeDefinition Target
        {
            get { return this.target; }
            private set
            {
                Contract.Requires(value != null);
                Contract.Ensures(this.Target != null);

                if (value.Interfaces.Any(@interface => @interface.Resolve() == this.SourceWithRoot.Source))
                {
                    throw new ArgumentException("Cannot set a target type that already implements the interface to be mixed", "value");
                }

                this.target = value;
            }
        }

        public void Execute()
        {
            this.Target.Interfaces.Add(this.TargetModule.Import(this.InterfaceType));
            var typeCloner = new TypeCloner(this.Target, this.SourceWithRoot);
            typeCloner.CloneStructure();
            typeCloner.CloneLogic();
        }
    }
}
