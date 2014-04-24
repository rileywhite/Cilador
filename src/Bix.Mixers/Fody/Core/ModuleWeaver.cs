/***************************************************************************/
// Copyright 2013-2014 Riley White
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
/***************************************************************************/

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
    public class ModuleWeaver : IDisposable, IPartImportsSatisfiedNotification, IWeavingContext
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
            Contract.Assert(this.BixMixersConfig != null);
            Contract.Assert(this.MixCommands != null);

            foreach (var mixCommand in this.MixCommands)
            {
                var mixCommandConfig = this.BixMixersConfig.MixCommandConfig.FirstOrDefault(config => config.GetType() == mixCommand.Metadata.ConfigType);
                if (mixCommandConfig == null)
                {
                    if (this.LogWarning != null)
                    {
                        this.LogWarning(string.Format("Ignoring mix command with no configuration: [{0}]", mixCommand.GetType().AssemblyQualifiedName));
                    }
                    continue;
                }
                mixCommand.Value.Initialize(this, mixCommandConfig);
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
                Contract.Ensures(this.Config != null);
                Contract.Ensures(this.BixMixersConfig != null);
                Contract.Ensures(this.Container != null);

                this.config = value;
                this.BixMixersConfig = ReadBixMixersConfig(value);

                this.Container.ComposeParts(this);
            }
        }

        public static BixMixersConfigType ReadBixMixersConfig(XElement config)
        {
            Contract.Requires(config != null);
            Contract.Ensures(Contract.Result<BixMixersConfigType>() != null);

            var children = config.Elements();
            if (children.Count() != 1)
            {
                throw new WeavingException("Bix.Mixers config in FodyWeavers.xml should have exactly one child");
            }

            var firstChild = children.First();
            if (firstChild.Name.NamespaceName != "urn:Bix:Mixers:Fody:Core" ||
                firstChild.Name.LocalName != "BixMixersConfig")
            {
                throw new WeavingException("Child of Bix.Mixers config in FodyWeavers.xml should be BixMixersConfig in namespace urn:Bix:Mixers:Fody:Core");
            }

            BixMixersConfigType deserializedConfig;
            try
            {
                deserializedConfig = firstChild.FromXElement<BixMixersConfigType>();
            }
            catch (Exception e)
            {
                throw new WeavingException(
                    "Element urn:Bix:Mixers:Fody:Core:BixMixersConfig in FodyWeavers.xml could not be deserialized into type of BixMixersConfigType",
                    e);
            }

            return deserializedConfig;
        }

        public BixMixersConfigType BixMixersConfig { get; private set; }
        
        public Action<string> LogDebug { get; set; }
        
        public Action<string> LogInfo { get; set; }
        
        public Action<string> LogWarning { get; set; }
        
        public Action<string, SequencePoint> LogWarningPoint { get; set; }
        
        public Action<string> LogError { get; set; }
        
        public Action<string, SequencePoint> LogErrorPoint { get; set; }
        
        public IAssemblyResolver AssemblyResolver { get; set; }
        
        public ModuleDefinition ModuleDefinition { get; set; }

        public List<string> DefineConstants { get; set; }

        IReadOnlyCollection<string> IWeavingContext.DefineConstants
        {
            get { return this.DefineConstants; }
        }
        
        public string AssemblyFilePath { get; set; }
        
        public string ProjectDirectoryPath { get; set; }
        
        public string AddinDirectoryPath { get; set; }
        
        public string SolutionDirectoryPath { get; set; }

        public void Execute()
        {
            var mixCommandAttributeInterfaceType = this.ModuleDefinition.Import(typeof(IMixCommandAttribute)).Resolve();

            var mixMap = new Dictionary<TypeDefinition, List<CustomAttribute>>();
            foreach (var type in this.ModuleDefinition.Types)
            {
                foreach(var attribute in type.CustomAttributes)
                {
                    var attributeTypeDefinition = attribute.AttributeType.Resolve();

                    foreach (var attributeInterfaceType in attributeTypeDefinition.Interfaces)
                    {
                        if(attributeInterfaceType.Resolve() == mixCommandAttributeInterfaceType)
                        {
                            List<CustomAttribute> mixAttributesForType;
                            if(!mixMap.TryGetValue(type.Resolve(), out mixAttributesForType))
                            {
                                mixAttributesForType = new List<CustomAttribute>();
                                mixMap[type.Resolve()] = mixAttributesForType;
                            }

                            mixAttributesForType.Add(attribute);

                            continue;
                        }
                    }
                }
            }

            foreach(var mixes in mixMap)
            {
                var mixedType = mixes.Key;
                Contract.Assert(mixedType != null);

                foreach (var commandAttribute in mixes.Value)
                {
                    mixedType.CustomAttributes.Remove(commandAttribute);
                }


                foreach(var commandAttribute in mixes.Value)
                {
                    var commandAttributeType = commandAttribute.AttributeType.Resolve();
                    var mixCommand = this.MixCommands.FirstOrDefault(
                        command => this.ModuleDefinition.Import(command.Metadata.AttributeType).Resolve() == commandAttributeType && command.Value.IsInitialized);

                    if(mixCommand == null)
                    {
                        throw new InvalidOperationException(
                            string.Format("Cannot find a configured mix command for type [{0}] and command [{1}]", mixedType.FullName, commandAttributeType.FullName));
                    }

                    mixCommand.Value.Mix(this, mixedType, commandAttribute);
                }
            }
        }
    }
}
