using Bix.Mixers.Fody.ILCloning;
using Bix.Mixers.Fody.TestMixinInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.TestMixins
{
    public class SecurityDeclarationOnNestedTypeMixin : IEmptyInterface
    {
        [Skip]
        public SecurityDeclarationOnNestedTypeMixin() { }

        public class InnerTypeWithoutSecurityDeclaration
        {
            [SecurityPermission(SecurityAction.PermitOnly)]
            public class InnerInnerTypeWithSecurityDeclaration { }
        }
    }
}
