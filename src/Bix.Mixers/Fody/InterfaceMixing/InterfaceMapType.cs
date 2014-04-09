using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.InterfaceMixing
{
    public partial class InterfaceMapType
    {
        private Type interfaceType;
        public Type InterfaceType
        {
            get
            {
                if (this.interfaceType == null)
                {
                    this.interfaceType = Type.GetType(this.Interface);
                    if(this.interfaceType == null)
                    {
                        throw new InvalidOperationException(string.Format(
                            "Cannot resolve interface type [{0}] for InterfaceMix command configuration",
                            this.Interface));
                    }
                }
                return this.interfaceType;
            }
        }

        private Type templateType;
        public Type TemplateType
        {
            get
            {
                if (this.templateType == null)
                {
                    this.templateType = Type.GetType(this.Template);
                    if (this.templateType == null)
                    {
                        throw new InvalidOperationException(string.Format(
                            "Cannot resolve template type [{0}] for InterfaceMix command configuration",
                            this.Template));
                    }
                }
                return this.templateType;
            }
        }
    }
}
