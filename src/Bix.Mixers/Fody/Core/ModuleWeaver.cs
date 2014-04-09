using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Bix.Mixers.Fody.Core
{
    public class ModuleWeaver : IDisposable, IPartImportsSatisfiedNotification
    {
        #region Construction and Disposal

        ~ModuleWeaver()
        {
            this.Dispose(false);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                lock (this.containerLock)
                {
                    if (container != null) { container.Dispose(); }
                }
            }
        }

        #endregion

        private CompositionContainer container;
        private object containerLock = new object();
        private CompositionContainer Container
        {
            get
            {
                if (this.container == null)
                {
                    lock (this.containerLock)
                    {
                        var catalog = new AggregateCatalog();

                        catalog.Catalogs.Add(new AssemblyCatalog(this.GetType().Assembly));
                        //catalog.Catalogs.Add(new DirectoryCatalog(AppDomain.CurrentDomain.BaseDirectory));

                        this.container = new CompositionContainer(catalog);
                    }
                }
                return this.container;
            }
        }

        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            Contract.Assert(this.MixCommands != null);

            foreach (var mixCommand in this.MixCommands)
            {
                var config = this.Config.Descendants().SingleOrDefault(
                    element => element.FirstAttribute.Value == mixCommand.Metadata.Name);

                if (config == null || !config.Descendants().Any())
                {
                    this.LogWarning(string.Format("Ignoring mix command with no configuration: [{0}]", mixCommand.Metadata.Name));
                    continue;
                }
                mixCommand.Value.Initialize(config.Descendants().First());
            }
        }

        [ImportMany(typeof(IMixCommand), AllowRecomposition = true)]
        private IEnumerable<Lazy<IMixCommand, IMixCommandData>> MixCommands { get; set; }

        private XElement config;
        public XElement Config
        {
            get { return this.config; }
            set
            {
                Contract.Requires(this.Config == null);
                Contract.Requires(value != null);
                Contract.Ensures(this.Container != null);
                this.config = value;
                this.Container.ComposeParts(this);
            }
        }
        
        public Action<string> LogDebug { get; set; }
        
        public Action<string> LogInfo { get; set; }
        
        public Action<string> LogWarning { get; set; }
        
        public Action<string, SequencePoint> LogWarningPoint { get; set; }
        
        public Action<string> LogError { get; set; }
        
        public Action<string, SequencePoint> LogErrorPoint { get; set; }
        
        public IAssemblyResolver AssemblyResolver { get; set; }
        
        public ModuleDefinition ModuleDefinition { get; set; }
        
        public List<string> DefineConstants { get; set; }
        
        public string AssemblyFilePath { get; set; }
        
        public string ProjectDirectoryPath { get; set; }
        
        public string AddinDirectoryPath { get; set; }
        
        public string SolutionDirectoryPath { get; set; }

        public void Execute()
        {
            var mixCommandAttributeInterfaceType = this.ModuleDefinition.Import(typeof(IMixCommandAttribute)).Resolve();
            var typeTypeDefinition = this.ModuleDefinition.Import(typeof(Type)).Resolve();

            var mixMap = new Dictionary<TypeDefinition, List<Tuple<string, IMixCommandAttribute, CustomAttribute>>>();
            foreach (var type in this.ModuleDefinition.Types)
            {
                foreach(var attribute in type.CustomAttributes)
                {
                    var attributeTypeDefinition = attribute.AttributeType.Resolve();

                    foreach (var attributeInterfaceType in attributeTypeDefinition.Interfaces)
                    {
                        if(attributeInterfaceType.Resolve() == mixCommandAttributeInterfaceType)
                        {
                            List<Tuple<string, IMixCommandAttribute, CustomAttribute>> mixCommandsForType;
                            if(!mixMap.TryGetValue(type.Resolve(), out mixCommandsForType))
                            {
                                mixCommandsForType = new List<Tuple<string, IMixCommandAttribute, CustomAttribute>>();
                                mixMap[type.Resolve()] = mixCommandsForType;
                            }

                            Type attributeType = Type.GetType(string.Format(
                                "{0}, {1}",
                                attributeTypeDefinition.FullName,
                                attributeTypeDefinition.Module.Assembly.FullName));

                            Type[] constructorParameterTypes;
                            object[] constructorArguments;
                            if (!attribute.HasConstructorArguments)
                            {
                                constructorParameterTypes = new Type[0];
                                constructorArguments = new object[0];
                            }
                            else
                            {
                                Contract.Assert(attribute.Constructor.Parameters.Count == attribute.ConstructorArguments.Count);

                                constructorParameterTypes = new Type[attribute.Constructor.Parameters.Count];
                                constructorArguments = new object[attribute.ConstructorArguments.Count];
                                for (int i = 0; i < attribute.Constructor.Parameters.Count; i++)
                                {
                                    var parameterTypeDefinition = attribute.Constructor.Parameters[i].ParameterType.Resolve();
                                    constructorParameterTypes[i] = Type.GetType(string.Format(
                                        "{0}, {1}",
                                        parameterTypeDefinition.FullName,
                                        parameterTypeDefinition.Module.Assembly.FullName));

                                    var argument = attribute.ConstructorArguments[i];
                                    if (argument.Type.Resolve() == typeTypeDefinition)
                                    {
                                        var argumentValue = ((TypeReference)argument.Value).Resolve();
                                        constructorArguments[i] = Type.GetType(string.Format(
                                            "{0}, {1}",
                                            argumentValue.FullName,
                                            argumentValue.Module.Assembly.FullName));
                                    }
                                    else
                                    {
                                        constructorArguments[i] = argument.Value;
                                    }
                                }

                            }

                            var attributeConstructor = attributeType.GetConstructor(constructorParameterTypes);
                            if (attributeConstructor == null)
                            {
                                throw new InvalidOperationException(string.Format(
                                    "Unable to find matching constructor for attribute [{0}] on type [{1}]",
                                    attribute.AttributeType.FullName,
                                    type.FullName));
                            }

                            var argumentAttribute = (IMixCommandAttribute)attributeConstructor.Invoke(constructorArguments);
                            Contract.Assert(argumentAttribute != null);

                            mixCommandsForType.Add(Tuple.Create(argumentAttribute.Name, argumentAttribute, attribute));

                            continue;
                        }
                    }
                    // this attribute has exactly one argument, which is the inteface type
                    //Contract.Assert(interfaceMixAttribute.ConstructorArguments.Count == 1);
                    //Contract.Assert(interfaceMixAttribute.ConstructorArguments[0].Type is TypeReference);
                    //Contract.Assert(interfaceMixAttribute.ConstructorArguments[0].Value != null);

                    //var interfaceTypeReference = (TypeReference)interfaceMixAttribute.ConstructorArguments[0].Value;
                    //interfaceTypeReference = interfaceTypeReference.Resolve();

                    //var interfaceType = Type.GetType(interfaceTypeReference.FullName + ", " + interfaceTypeReference.Module.Assembly.FullName); 

                    //if(interfaceType == null)
                    //{
                    //    using (var memoryStream = new MemoryStream())
                    //    {
                    //        interfaceTypeReference.Module.Assembly.Write(memoryStream);
                    //        var assembly = AppDomain.CurrentDomain.Load(memoryStream.GetBuffer());
                    //        interfaceType = assembly.GetType(interfaceTypeReference.FullName);
                    //    }
                    //}

                    //if(!interfaceType.IsInterface)
                    //{
                    //    throw new InvalidOperationException(string.Format(
                    //        "InterfaceMix attribute for {0} should specify an interface type. {1} is not an interface type",
                    //        type.FullName,
                    //        interfaceType.FullName));
                    //}

                    //foreach(var xElement in this.Config.Elements())
                    //{
                    //    var configType = Type.GetType(xElement.Name.LocalName);
                    //    var serializer = new XmlSerializer(configType);
                    //    var config = xElement.Elements().First().FromXElement(configType);
                    //}

                    //Contract.Assert(mixCommandAttribute.HasProperties);
                    //var nameProperty = mixCommandAttribute.Properties.SingleOrDefault(property => property.Name == "Name");
                    //Contract.Assert(!default(CustomAttributeNamedArgument).Equals(nameProperty));
                }
            }

            foreach(var mixes in mixMap)
            {
                var mixedType = mixes.Key;
                Contract.Assert(mixedType != null);

                foreach (var mixNameAndAttributes in mixes.Value)
                {
                    mixedType.CustomAttributes.Remove(mixNameAndAttributes.Item3);
                }


                foreach(var mixNameAndAttributes in mixes.Value)
                {
                    var mixCommand = this.MixCommands.FirstOrDefault(
                        command => command.Metadata.Name.Equals(mixNameAndAttributes.Item1) && command.Value.IsInitialized);

                    if(mixCommand == null)
                    {
                        throw new InvalidOperationException(
                            string.Format("Cannot find a configured mix command for type [{0}] and command [{1}]", mixedType.FullName, mixNameAndAttributes.Item1));
                    }

                    mixCommand.Value.Mix(mixedType, mixNameAndAttributes.Item2);
                }
            }
        }
    }
}
