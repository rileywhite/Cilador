using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.ILCloning
{
    internal class MemberSourceWithRootKeyedCollection<TMemberSourceWithRoot, TMemberDefinition> : KeyedCollection<string, TMemberSourceWithRoot>
        where TMemberSourceWithRoot : MemberSourceWithRootBase<TMemberDefinition>
        where TMemberDefinition :  MemberReference, IMemberDefinition, Mono.Cecil.ICustomAttributeProvider
    {
        public static string GetKeyFor(TMemberSourceWithRoot item)
        {
            Contract.Requires(item != null);
            Contract.Requires(item.Source != null);
            return GetKeyFor(item.Source);
        }

        public static string GetKeyFor(TMemberDefinition item)
        {
            Contract.Requires(item != null);
            return string.Format("{0}, {1}", item.FullName, item.Module.FullyQualifiedName);
        }

        protected override string GetKeyForItem(TMemberSourceWithRoot item)
        {
            if (item == null) { throw new ArgumentNullException("item"); }
            if (item.Source == null) { throw new ArgumentException("Source must not be null", "item"); }
            return GetKeyFor(item);
        }
    }
}
