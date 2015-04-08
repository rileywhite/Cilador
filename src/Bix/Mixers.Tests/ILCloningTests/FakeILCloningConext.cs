using Bix.Mixers.ILCloning;
using Mono.Cecil;
using System;

namespace Bix.Mixers.Tests.ILCloningTests
{
    /// <summary>
    /// Fake IL cloning context.
    /// </summary>
    internal class FakeILCloningConext : IILCloningContext
    {
        public FakeILCloningConext()
        {
            this.RootImportObjectDelegate = new Func<object, object>(item => item);
        }

        public TypeDefinition RootSource { get; set; }
        public TypeDefinition RootTarget { get; set; }

        public TItem DynamicRootImport<TItem>(TItem item)
        {
            return (TItem)this.RootImport((dynamic)item);
        }

        public Func<object, object> RootImportObjectDelegate { get; set; }
        private object RootImport(object item)
        {
            var handler = this.RootImportObjectDelegate;
            if (handler != null) { return handler(item); }
            return item;
        }

        public Func<TypeReference, TypeReference> RootImportTypeDelegate { get; set; }
        public TypeReference RootImport(TypeReference type)
        {
            var handler = this.RootImportTypeDelegate;
            if (handler != null) { return handler(type); }
            return type;
        }

        public Func<MethodReference, MethodReference> RootImportMethodDelegate { get; set; }
        public MethodReference RootImport(MethodReference method)
        {
            var handler = this.RootImportMethodDelegate;
            if (handler != null) { return handler(method); }
            return method;
        }

        public Func<FieldReference, FieldReference> RootImportFieldDelegate { get; set; }
        public FieldReference RootImport(FieldReference field)
        {
            var handler = this.RootImportFieldDelegate;
            if (handler != null) { return handler(field); }
            return field;
        }
    }
}
