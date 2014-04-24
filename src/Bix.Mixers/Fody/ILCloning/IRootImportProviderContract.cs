using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.ILCloning
{
    [ContractClassFor(typeof(IRootImportProvider))]
    internal abstract class IRootImportProviderContract : IRootImportProvider
    {
        public TypeDefinition RootSource
        {
            get
            {
                Contract.Ensures(Contract.Result<TypeDefinition>() != null);
                throw new NotSupportedException();
            }
        }

        public TypeDefinition RootTarget
        {
            get
            {
                Contract.Ensures(Contract.Result<TypeDefinition>() != null);
                throw new NotSupportedException();
            }
        }

        public TItem DynamicRootImport<TItem>(TItem item)
        {
            Contract.Ensures(item == null || Contract.Result<TItem>() != null);
            throw new NotSupportedException();
        }

        public TypeReference RootImport(TypeReference type)
        {
            Contract.Ensures(type == null || Contract.Result<TypeReference>() != null);
            throw new NotSupportedException();
        }

        public FieldReference RootImport(FieldReference field)
        {
            Contract.Ensures(field == null || Contract.Result<FieldReference>() != null);
            throw new NotSupportedException();
        }

        public MethodReference RootImport(MethodReference method)
        {
            Contract.Ensures(method == null || Contract.Result<MethodReference>() != null);
            throw new NotSupportedException();
        }
    }
}
