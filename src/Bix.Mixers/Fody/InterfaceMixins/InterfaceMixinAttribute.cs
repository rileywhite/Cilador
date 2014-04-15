using Bix.Mixers.Fody.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.InterfaceMixins
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class InterfaceMixinAttribute : Attribute, IMixCommandAttribute
    {
        public InterfaceMixinAttribute(Type @interface)
        {
            Contract.Requires(@interface != null);
            this.Interface = @interface;
        }

        private Type @interface;
        public Type Interface
        {
            get { return this.@interface; }
            set
            {
                Contract.Requires(value != null);
                Contract.Requires(value.IsInterface);
                this.@interface = value;
            }
        }
    }
}
