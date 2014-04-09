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
        public InterfaceMixCommandMixer(Type interfaceType, Type templateType, TypeDefinition target)
        {
            Contract.Requires(interfaceType != null);
            Contract.Requires(interfaceType.IsInterface);
            Contract.Requires(templateType != null);
            Contract.Requires(templateType.IsClass);
            Contract.Requires(interfaceType.IsAssignableFrom(templateType));
            Contract.Requires(target != null);
            Contract.Requires(target.Module != null);
            Contract.Requires(!target.IsValueType);
            Contract.Requires(!target.IsPrimitive);

            Contract.Ensures(this.InterfaceType != null);
            Contract.Ensures(this.InterfaceType.IsInterface);
            Contract.Ensures(this.TemplateType != null);
            Contract.Ensures(this.TemplateType.IsClass);
            Contract.Ensures(this.InterfaceType.IsAssignableFrom(templateType));
            Contract.Ensures(this.Target != null);
            Contract.Ensures(this.Target.IsClass);
            Contract.Ensures(this.TargetModule != null);
            Contract.Ensures(this.Source != null);

            this.InterfaceType = interfaceType;
            this.TemplateType = templateType;
            this.Source = new TypeWithRespectToModule(templateType, target);
            this.TargetModule = target.Module;
            this.Target = target;
        }

        public Type InterfaceType { get; private set; }

        public Type TemplateType { get; private set; }

        private TypeWithRespectToModule Source { get; set; }

        private ModuleDefinition TargetModule { get; set; }

        private TypeDefinition target;
        public TypeDefinition Target
        {
            get { return this.target; }
            private set
            {
                Contract.Requires(value != null);
                Contract.Ensures(this.Target != null);

                if (value.Interfaces.Any(@interface => @interface.Resolve() == this.Source.MemberDefinition))
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
