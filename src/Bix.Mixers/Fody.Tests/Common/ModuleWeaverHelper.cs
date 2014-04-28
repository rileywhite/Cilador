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

using Bix.Mixers.Fody.Config;
using Bix.Mixers.Fody.Core;
using Mono.Cecil;
using NUnit.Framework;
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
            moduleWeaver.AddinDirectoryPath = TestContent.GetAddinDirectory();
            Contract.Assert(Directory.Exists(moduleWeaver.AddinDirectoryPath));
            moduleWeaver.AssemblyFilePath = TestContent.GetTestTargetsPath();
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
            moduleWeaver.ProjectDirectoryPath = TestContent.GetTestProjectDirectory();
            Contract.Assert(Directory.Exists(moduleWeaver.ProjectDirectoryPath));
            moduleWeaver.SolutionDirectoryPath = TestContent.GetTestSolutionDirectory();
            Contract.Assert(Directory.Exists(moduleWeaver.SolutionDirectoryPath));
            return moduleWeaver;
        }

        public static Assembly WeaveAndLoadTestTarget(BixMixersConfigType config, params Tuple<string, string>[] fodyWeaverTaskProperties)
        {
            Contract.Requires(config != null);
            Contract.Ensures(Contract.Result<Assembly>() != null);

            return AppDomain.CurrentDomain.Load(ModuleWeaverHelper.GetRawWeavedAssembly(BuildXElementConfig(config, fodyWeaverTaskProperties)));
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

            bool isVerified;
            string verificationOutput;
            AssemblyVerifier.RunVerifyProcessAndCollectOutput(moduleWeaver.AssemblyFilePath, out isVerified, out verificationOutput);
            Assert.That(isVerified, string.Format("Unprocessed assembly could not be verified: \n{0}", verificationOutput));

            moduleWeaver.Execute();

            var tempProcessedAssemblyPath = Path.Combine(Path.GetDirectoryName(moduleWeaver.AssemblyFilePath), string.Format("{0}.dll", Path.GetRandomFileName()));
            try
            {
                moduleWeaver.ModuleDefinition.Write(tempProcessedAssemblyPath);
                AssemblyVerifier.RunVerifyProcessAndCollectOutput(tempProcessedAssemblyPath, out isVerified, out verificationOutput);
                Assert.That(isVerified, string.Format("Processed assembly could not be verified: \n{0}", verificationOutput));
            }
            finally
            {
                if (File.Exists(tempProcessedAssemblyPath))
                {
                    try
                    {
                        File.Delete(tempProcessedAssemblyPath);
                    }
                    catch { }
                }
            }

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
                xElement.Add(new XAttribute("MixCommand", "InterfaceMixin"));

                config.Add(xElement);
            }

            return config;
        }

        public static XElement BuildXElementConfig(BixMixersConfigType config, params Tuple<string, string>[] fodyWeaverTaskProperties)
        {
            Contract.Requires(config != null);
            Contract.Ensures(Contract.Result<XElement>() != null);

            var xmlConfig = new XElement("Bix.Mixers", config.ToXElement());
            if(fodyWeaverTaskProperties != null && fodyWeaverTaskProperties.Length > 0)
            {
                foreach(var property in fodyWeaverTaskProperties)
                {
                    xmlConfig.Add(new XAttribute(property.Item1, property.Item2));
                }
            }
            return xmlConfig;
        }
    }
}
