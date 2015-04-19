/***************************************************************************/
// Copyright 2013-2015 Riley White
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

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml.Linq;
using Cilador.Fody.Config;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Cilador.Fody.Core
{
    using Cilador.Fody.Config;

    /// <summary>
    /// The <see cref="ModuleWeaver.Execute()"/> method is invoked by Fody as a Visual
    /// Studio post-build step. This type serves to start the weaving process and to manage
    /// the configuration of the Fody weaver. It is also passed around as needed to other types
    /// in the aspect of a <see cref="IWeavingContext"/>.
    /// </summary>
    public sealed class ModuleWeaver : IDisposable, IPartImportsSatisfiedNotification, IWeavingContext
    {
        #region Construction and Disposal

        /// <summary>
        /// Creates a new <see cref="ModuleWeaver"/>.
        /// </summary>
        public ModuleWeaver()
        {
            Contract.Ensures(this.AssemblyResolver != null);
            Contract.Ensures(this.MetadataResolver != null);

            this.assemblyResolver = new DualAssemblyResolver(this);
            this.MetadataResolver = new MetadataResolver(this.AssemblyResolver);
        }

        /// <summary>
        /// Finalizer to release resources used by the <see cref="ModuleWeaver"/>
        /// </summary>
        ~ModuleWeaver()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Releases resources used by the <see cref="ModuleWeaver"/> in a
        /// deterministic manner
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases resources used by the <see cref="ModuleWeaver"/>
        /// </summary>
        /// <param name="isDisposing"><c>true</c> if this is being called explicitly through <see cref="Dispose()"/>, else <c>false</c> if being called by the finalizer during garbage collection.</param>
        private void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                lock (this.containerLock)
                {
                    if (this.container != null) { this.container.Dispose(); }
                }
            }
        }

        #endregion

        #region MEF Management

        /// <summary>
        /// Handles locking for MEF lookup
        /// </summary>
        private readonly object containerLock = new object();

        private CompositionContainer container;
        /// <summary>
        /// Gets the Microsoft Extensibility Framework container used by this object
        /// </summary>
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

                        this.container = new CompositionContainer(catalog);
                    }
                }
                return this.container;
            }
        }

        #endregion

        #region Configuration Handling

        /// <summary>
        /// Called by MEF when a part's imports have been satisfied and it is safe to use.
        /// For this type, it rebuilds some configuration information based on the imported mix commands.
        /// </summary>
        void IPartImportsSatisfiedNotification.OnImportsSatisfied()
        {
            Contract.Assert(this.CiladorConfig != null);
            Contract.Assert(this.MixCommands != null);

            foreach (var mixCommand in this.MixCommands)
            {
                var mixCommandConfig = this.CiladorConfig.MixCommandConfig.FirstOrDefault(config => config.GetType() == mixCommand.Metadata.ConfigType);
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

        /// <summary>
        /// Gets or sets the collection of mix commands found in the MEF container catalogs.
        /// </summary>
        [ImportMany(typeof(IMixCommand), AllowRecomposition = true)]
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private IEnumerable<Lazy<IMixCommand, IMixCommandData>> MixCommands { get; set; }

        private XElement config;
        /// <summary>
        /// Gets or sets the configuration element from FodyWeavers.xml.
        /// </summary>
        /// <remarks>When run as a Fody addin withing Visual Studio, Fody sets this value.</remarks>
        public XElement Config
        {
            get { return this.config; }
            set
            {
                Contract.Requires(this.Config == null);
                Contract.Requires(value != null);
                Contract.Ensures(this.Config != null);
                Contract.Ensures(this.CiladorConfig != null);
                Contract.Ensures(this.Container != null);

                this.config = value;
                this.CiladorConfig = ModuleWeaver.ReadCiladorConfig(value);

                this.Container.ComposeParts(this);
            }
        }

        /// <summary>
        /// Gets or sets the contants defined by the build
        /// </summary>
        /// <remarks>When run as a Fody addin withing Visual Studio, Fody sets this value.</remarks>
        public List<string> DefineConstants { get; set; }

        /// <summary>
        /// Gets the contants defined by the build
        /// </summary>
        IReadOnlyCollection<string> IWeavingContext.DefineConstants
        {
            get { return this.DefineConstants; }
        }

        /// <summary>
        /// Gets or sets the path of the target assembly file.
        /// </summary>
        /// <remarks>When run as a Fody addin withing Visual Studio, Fody sets this value.</remarks>
        public string AssemblyFilePath { get; set; }

        /// <summary>
        /// Gets or sets the path for the target assembly's project file
        /// </summary>
        /// <remarks>When run as a Fody addin withing Visual Studio, Fody sets this value.</remarks>
        public string ProjectDirectoryPath { get; set; }

        /// <summary>
        /// Gets or sets the path to the Cilador.Fody addin assembly
        /// </summary>
        /// <remarks>When run as a Fody addin withing Visual Studio, Fody sets this value.</remarks>
        public string AddinDirectoryPath { get; set; }

        /// <summary>
        /// Gets or sets the path to the target assembly's solution file
        /// </summary>
        /// <remarks>When run as a Fody addin withing Visual Studio, Fody sets this value.</remarks>
        public string SolutionDirectoryPath { get; set; }

        /// <summary>
        /// Deserializes a <see cref="CiladorConfigType"/> from within a config.
        /// <see cref="XElement"/>
        /// </summary>
        /// <remarks>
        /// The configuration type, <see cref="CiladorConfigType"/>, is primarily generated
        /// from MixCommandConfig.xsd.
        /// </remarks>
        /// <param name="config">Item that contains the serialized config element.</param>
        /// <returns>Deserialized configurtion object</returns>
        public static CiladorConfigType ReadCiladorConfig(XElement config)
        {
            Contract.Requires(config != null);
            Contract.Ensures(Contract.Result<CiladorConfigType>() != null);

            var childElements = config.Elements();
            var children = childElements as XElement[] ?? childElements.ToArray();
            if (children.Count() != 1)
            {
                throw new WeavingException("Cilador config in FodyWeavers.xml should have exactly one child");
            }

            var firstChild = children.First();
            if (firstChild.Name.NamespaceName != "urn:Cilador:Fody:Config" ||
                firstChild.Name.LocalName != "CiladorConfig")
            {
                throw new WeavingException("Child of Cilador config in FodyWeavers.xml should be CiladorConfig in namespace urn:Cilador:Fody:Config");
            }

            CiladorConfigType deserializedConfig;
            try
            {
                deserializedConfig = firstChild.FromXElement<CiladorConfigType>();
            }
            catch (Exception e)
            {
                throw new WeavingException(
                    "Element urn:Cilador:Fody:Config:CiladorConfig in FodyWeavers.xml could not be deserialized into type of CiladorConfigType",
                    e);
            }

            return deserializedConfig;
        }

        /// <summary>
        /// Gets or sets the strongly typed Cilador configuration object.
        /// </summary>
        public CiladorConfigType CiladorConfig { get; private set; }

        #endregion

        #region Logging

        /// <summary>
        /// Gets or sets the logger that displays debug-level output.
        /// </summary>
        /// <remarks>When run as a Fody addin withing Visual Studio, Fody sets this value.</remarks>
        public Action<string> LogDebug { get; set; }

        /// <summary>
        /// Gets or sets the logger that displays info-level output. In Visual
        /// Studio, logged items appear in the "Error List" as Message items.
        /// </summary>
        /// <remarks>When run as a Fody addin withing Visual Studio, Fody sets this value.</remarks>
        public Action<string> LogInfo { get; set; }

        /// <summary>
        /// Gets or sets the logger that displays warning-level output. In Visual
        /// Studio, logged items appear in the "Error List" as Warning items.
        /// </summary>
        /// <remarks>When run as a Fody addin withing Visual Studio, Fody sets this value.</remarks>
        public Action<string> LogWarning { get; set; }

        /// <summary>
        /// Gets or sets the logger that displays warning-level output for a given
        /// CIL instruction's sequence point. In Visual
        /// Studio, logged items appear in the "Error List" as Warning items.
        /// </summary>
        /// <remarks>When run as a Fody addin withing Visual Studio, Fody sets this value.</remarks>
        public Action<string, SequencePoint> LogWarningPoint { get; set; }

        /// <summary>
        /// Gets or sets the logger that displays error-level output. In Visual
        /// Studio, logged items appear in the "Error List" as Error items.
        /// </summary>
        /// <remarks>When run as a Fody addin withing Visual Studio, Fody sets this value.</remarks>
        public Action<string> LogError { get; set; }

        /// <summary>
        /// Gets or sets the logger that displays error-level output for a given
        /// CIL instruction's sequence point. In Visual
        /// Studio, logged items appear in the "Error List" as Error items.
        /// </summary>
        /// <remarks>When run as a Fody addin withing Visual Studio, Fody sets this value.</remarks>
        public Action<string, SequencePoint> LogErrorPoint { get; set; }

        #endregion

        #region Target Assembly Data

        private readonly DualAssemblyResolver assemblyResolver;
        /// <summary>
        /// Gets or sets the object that can find and load assemblies.
        /// </summary>
        /// <remarks>When run as a Fody addin withing Visual Studio, Fody sets this value.</remarks>
        public IAssemblyResolver AssemblyResolver
        {
            get { return this.assemblyResolver; }
            set
            {
                // only control the first resolver with this setter
                this.assemblyResolver.Resolver1 = value;
            }
        }

        /// <summary>
        /// Gets or sets the object that can resolve type members.
        /// </summary>
        public IMetadataResolver MetadataResolver { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="ModuleDefinition"/> for the target assembly.
        /// </summary>
        /// <remarks>When run as a Fody addin withing Visual Studio, Fody sets this value.</remarks>
        public ModuleDefinition ModuleDefinition { get; set; }

        #endregion

        #region Execution

        /// <summary>
        /// Executes the Cilador code weaving logic.
        /// </summary>
        /// <remarks>When run as a Fody addin withing Visual Studio, Fody invokes this method after setting all configuration data.</remarks>
        public void Execute()
        {
            this.assemblyResolver.Resolver2 = new WeavingContextAssemblyResolver(this);

            foreach (var mixCommandAttributesByTargetType in this.ExtractMixCommandAttributesByTargetTypes())
            {
                Contract.Assert(mixCommandAttributesByTargetType.Key != null);
                Contract.Assert(mixCommandAttributesByTargetType.Value != null);
                Contract.Assert(mixCommandAttributesByTargetType.Value.All(customAttribute => customAttribute != null));

                this.FindAndInvokeMixCommands(mixCommandAttributesByTargetType.Key, mixCommandAttributesByTargetType.Value);
            }
        }

        /// <summary>
        /// Looks at all types within the target assembly, and collects any that are annotated with mix command attributes.
        /// Mix command attributes are removed from types as they are gathered.
        /// </summary>
        /// <returns>Collection keyed by types the are annotated with at least one mix command attribute. Values are the collection of mix commands on the key item.</returns>
        private Dictionary<TypeDefinition, List<CustomAttribute>> ExtractMixCommandAttributesByTargetTypes()
        {
            Contract.Ensures(Contract.Result<Dictionary<TypeDefinition, List<CustomAttribute>>>().Keys.All(type => type != null));
            Contract.Ensures(!Contract.Result<Dictionary<TypeDefinition, List<CustomAttribute>>>().Values.Any(
                customAttributes => customAttributes == null || customAttributes.Any(customAttribute => customAttribute == null)));

            var mixCommandAttributeInterfaceType = this.ModuleDefinition.Import(typeof(IMixCommandAttribute)).Resolve();

            var mixCommandAttributesByTargetTypes = new Dictionary<TypeDefinition, List<CustomAttribute>>();
            foreach (var type in this.ModuleDefinition.Types)
            {
                Contract.Assert(type != null);
                Contract.Assert(type.CustomAttributes != null);

                for (var i = type.CustomAttributes.Count - 1; i >= 0; i--)
                {
                    var attribute = type.CustomAttributes[i];
                    Contract.Assert(attribute != null);
                    var attributeTypeDefinition = attribute.AttributeType.Resolve();
                    Contract.Assert(attributeTypeDefinition != null);

                    foreach (var attributeInterfaceType in attributeTypeDefinition.Interfaces)
                    {
                        if (attributeInterfaceType.Resolve() == mixCommandAttributeInterfaceType)
                        {
                            List<CustomAttribute> mixAttributesForType;
                            if (!mixCommandAttributesByTargetTypes.TryGetValue(type.Resolve(), out mixAttributesForType))
                            {
                                mixAttributesForType = new List<CustomAttribute>();
                                mixCommandAttributesByTargetTypes[type.Resolve()] = mixAttributesForType;
                            }

                            mixAttributesForType.Add(attribute);
                            type.CustomAttributes.RemoveAt(i);
                        }
                    }
                }
            }
            return mixCommandAttributesByTargetTypes;
        }

        /// <summary>
        /// Looks up commands indicated by mix commands attributes thorugh configuration, and then
        /// executes each command.
        /// </summary>
        /// <param name="targetType">Type which will be modified by mix commands</param>
        /// <param name="mixCommandAttributes">Collection of mix command attributes which indicate mix commands that will be applied the <paramref name="targetType"/></param>
        private void FindAndInvokeMixCommands(
            TypeDefinition targetType,
            List<CustomAttribute> mixCommandAttributes)
        {
            Contract.Requires(targetType != null);
            Contract.Requires(mixCommandAttributes != null);
            Contract.Requires(mixCommandAttributes.All(customAttribute => customAttribute != null));

            foreach (var mixCommandAttribute in mixCommandAttributes)
            {
                this.GetMixCommandFor(targetType, mixCommandAttribute).Mix(this, targetType, mixCommandAttribute);
            }
        }

        /// <summary>
        /// Finds the mix command that corresponds to a given mix command attribute for a type
        /// </summary>
        /// <param name="targetType">Type that is the target for the command</param>
        /// <param name="mixCommandAttribute">Attribute to find command for</param>
        /// <returns>Mix command that corresponds to the given attribute</returns>
        /// <exception cref="InvalidOperationException">No mix command is found that corresponds to the <paramref name="mixCommandAttribute"/></exception>
        private IMixCommand GetMixCommandFor(TypeDefinition targetType, CustomAttribute mixCommandAttribute)
        {
            Contract.Requires(targetType != null);
            Contract.Requires(mixCommandAttribute != null);
            Contract.Ensures(Contract.Result<IMixCommand>() != null);

            var mixCommandAttributeType = mixCommandAttribute.AttributeType.Resolve();
            try
            {
                return this.MixCommands.First(command =>
                    this.ModuleDefinition.Import(command.Metadata.AttributeType).Resolve() == mixCommandAttributeType &&
                    command.Value.IsInitialized).Value;
            }
            catch(Exception e)
            {
                throw new InvalidOperationException(
                    string.Format("Cannot find a configured mix command for type [{0}] and command [{1}]", targetType.FullName, mixCommandAttributeType.FullName),
                    e);
            }
        }

        #endregion
    }
}
