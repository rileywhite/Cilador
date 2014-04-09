using Bix.Mixers.Fody.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.InterfaceMixing
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class InterfaceMixAttribute : Attribute, IMixCommandAttribute
    {
        public InterfaceMixAttribute(Type @interface)
        {
            this.Interface = @interface;
            this.Group = "";
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

        public string Name
        {
            get { return "InterfaceMix"; }
        }

        public string Group { get; set; }
    }
}
