using Bix.Mixers.Fody.Core;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Bix.Mixers.Fody.Tests.Common
{
    internal static class ModuleWeaverHelper
    {
        public static ModuleWeaver GetModuleWeaver(XElement config)
        {
            Contract.Requires(config != null);
            Contract.Ensures(Contract.Result<ModuleWeaver>() != null);
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<ModuleWeaver>().AddinDirectoryPath));
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<ModuleWeaver>().AssemblyFilePath));
            Contract.Ensures(File.Exists(Contract.Result<ModuleWeaver>().AssemblyFilePath));
            Contract.Ensures(Contract.Result<ModuleWeaver>().AssemblyResolver != null);
            Contract.Ensures(Contract.Result<ModuleWeaver>().Config != null);
            Contract.Ensures(Contract.Result<ModuleWeaver>().DefineConstants != null);
            Contract.Ensures(Contract.Result<ModuleWeaver>().LogDebug != null);
            Contract.Ensures(Contract.Result<ModuleWeaver>().LogError != null);
            Contract.Ensures(Contract.Result<ModuleWeaver>().LogErrorPoint != null);
            Contract.Ensures(Contract.Result<ModuleWeaver>().LogInfo != null);
            Contract.Ensures(Contract.Result<ModuleWeaver>().LogWarning != null);
            Contract.Ensures(Contract.Result<ModuleWeaver>().LogWarningPoint != null);
            Contract.Ensures(Contract.Result<ModuleWeaver>().ModuleDefinition != null);
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<ModuleWeaver>().ProjectDirectoryPath));
            Contract.Ensures(!string.IsNullOrWhiteSpace(Contract.Result<ModuleWeaver>().SolutionDirectoryPath));

            var moduleWeaver = new ModuleWeaver();
            moduleWeaver.AddinDirectoryPath = TestPaths.GetAddinDirectory();
            Contract.Assert(Directory.Exists(moduleWeaver.AddinDirectoryPath));
            moduleWeaver.AssemblyFilePath = TestPaths.GetTestTargetPath();
            moduleWeaver.AssemblyResolver = new DefaultAssemblyResolver();
            moduleWeaver.Config = config;
            moduleWeaver.DefineConstants = new List<string>();
            moduleWeaver.LogDebug = m => { };
            moduleWeaver.LogError = m => { };
            moduleWeaver.LogErrorPoint = (m, p) => { };
            moduleWeaver.LogInfo = m => { };
            moduleWeaver.LogWarning = m => { };
            moduleWeaver.LogWarningPoint = (m, p) => { };
            moduleWeaver.ModuleDefinition = ModuleDefinition.ReadModule(moduleWeaver.AssemblyFilePath);
            moduleWeaver.ProjectDirectoryPath = TestPaths.GetTestProjectDirectory();
            Contract.Assert(Directory.Exists(moduleWeaver.ProjectDirectoryPath));
            moduleWeaver.SolutionDirectoryPath = TestPaths.GetTestSolutionDirectory();
            Contract.Assert(Directory.Exists(moduleWeaver.SolutionDirectoryPath));
            return moduleWeaver;
        }

        public static Assembly WeaveAndLoadTestTarget(XElement config)
        {
            Contract.Requires(config != null);
            Contract.Ensures(Contract.Result<Assembly>() != null);

            return AppDomain.CurrentDomain.Load(ModuleWeaverHelper.GetRawWeavedAssembly(config));
        }

        public static byte[] GetRawWeavedAssembly(XElement config)
        {
            Contract.Requires(config != null);
            Contract.Ensures(Contract.Result<byte[]>() != null);

            using(var memoryStream = new MemoryStream())
            {
                WeaveTestTarget(config).Write(memoryStream);
                return memoryStream.GetBuffer();
            }
        }

        public static ModuleDefinition WeaveTestTarget(XElement config)
        {
            Contract.Requires(config != null);
            Contract.Ensures(Contract.Result<ModuleDefinition>() != null);

            var moduleWeaver = GetModuleWeaver(config);
            moduleWeaver.Execute();

            return moduleWeaver.ModuleDefinition;
        }

        public static XElement CreateConfig(params object[] entries)
        {
            Contract.Requires(entries != null);
            Contract.Ensures(Contract.Result<XElement>() != null);

            var config = new XElement("Weavers");

            foreach (var entry in entries)
            {
                if (entry == null) { continue; }

                var xElement = new XElement("Bix.Mixers", entry.ToXElement());
                xElement.Add(new XAttribute("MixCommand", "InterfaceMix"));

                config.Add(xElement);
            }

            return config;
        }
    }
}
