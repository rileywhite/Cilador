using Bix.Mixers.Fody.ILCloning;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.InterfaceMixing
{
    internal class InterfaceMixCommandMixer
    {
        public InterfaceMixCommandMixer(TypeDefinition interfaceType, TypeDefinition templateType, TypeDefinition target)
        {
            Contract.Requires(interfaceType != null);
            Contract.Requires(interfaceType.IsInterface);
            Contract.Requires(templateType != null);
            Contract.Requires(templateType.IsClass);
            Contract.Requires(target != null);
            Contract.Requires(target.Module != null);
            Contract.Requires(!target.IsValueType);
            Contract.Requires(!target.IsPrimitive);

            Contract.Ensures(this.InterfaceType != null);
            Contract.Ensures(this.InterfaceType.IsInterface);
            Contract.Ensures(this.TemplateType != null);
            Contract.Ensures(this.TemplateType.IsClass);
            Contract.Ensures(this.Target != null);
            Contract.Ensures(this.Target.IsClass);
            Contract.Ensures(this.TargetModule != null);
            Contract.Ensures(this.Source != null);

            if (!templateType.Interfaces.Any(@interface => @interface.Resolve() == interfaceType))
            {
                throw new ArgumentException("Must implement the interface specified in the interfaceType argmuent", "templateType");
            }

            this.InterfaceType = interfaceType;
            this.TemplateType = templateType;
            this.Source = new TypeSourceWithRoot(templateType, target);
            this.TargetModule = target.Module;
            this.Target = target;
        }

        public TypeDefinition InterfaceType { get; private set; }

        public TypeDefinition TemplateType { get; private set; }

        private TypeSourceWithRoot Source { get; set; }

        private ModuleDefinition TargetModule { get; set; }

        private TypeDefinition target;
        public TypeDefinition Target
        {
            get { return this.target; }
            private set
            {
                Contract.Requires(value != null);
                Contract.Ensures(this.Target != null);

                if (value.Interfaces.Any(@interface => @interface.Resolve() == this.Source.Source))
                {
                    throw new ArgumentException("Cannot set a target type that already implements the interface to be mixed", "value");
                }

                this.target = value;
            }
        }

        public void Execute()
        {
            this.Target.Interfaces.Add(this.TargetModule.Import(this.InterfaceType));
            new TypeCloner(this.Target, this.Source).Clone();
        }
    }
}
