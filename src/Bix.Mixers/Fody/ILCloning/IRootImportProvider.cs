using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.ILCloning
{
    [ContractClass(typeof(IRootImportProviderContract))]
    internal interface IRootImportProvider
    {
        TypeDefinition RootTarget { get; }

        TItem DynamicRootImport<TItem>(TItem item);

        TypeReference RootImport(TypeReference type);

        FieldReference RootImport(FieldReference field);

        PropertyReference RootImport(PropertyReference property);

        MethodReference RootImport(MethodReference method);

        EventReference RootImport(EventReference @event);
    }
}
