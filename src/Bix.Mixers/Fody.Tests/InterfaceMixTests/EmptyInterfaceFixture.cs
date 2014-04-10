using Bix.Mixers.Fody.Core;
using Bix.Mixers.Fody.InterfaceMixing;
using Bix.Mixers.Fody.Tests.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Bix.Mixers.Fody.Tests.InterfaceMixTests
{
    [TestFixture]
    internal class EmptyInterfaceFixture
    {
        [Test]
        public void CanAddInterface()
        {
            var config = new BixMixersType();

            config.MixCommandConfig = new MixCommandConfigTypeBase[]
            {
                new InterfaceMixConfigType
                {
                    InterfaceMap = new InterfaceMapType[]
                    {
                        new InterfaceMapType
                        {
                            Interface = "Bix.Mixers.Fody.TestInterfaces.IEmptyInterface, Bix.Mixers.Fody.TestInterfaces, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null",
                            Template = "Bix.Mixers.Fody.TestSource.EmptyInterfaceTemplate, Bix.Mixers.Fody.TestSource, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
                        }
                    }
                },
            };

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget(config.ToXElement());
            var targetType = assembly.GetType("Bix.Mixers.Fody.TestTarget.EmptyInterfaceTarget");
            Assert.That(typeof(Bix.Mixers.Fody.TestInterfaces.IEmptyInterface).IsAssignableFrom(targetType));

            // TODO Removing unused assembly dependencies would be a nice feature, but I'm not sure how to do it, in general (e.g. maybe the source assembly should truly be referenced)
            //Assert.That(!assembly.GetReferencedAssemblies().Any(referencedAssembly => referencedAssembly.Name == "Bix.Mixers.Fody"));
            //Assert.That(!assembly.GetReferencedAssemblies().Any(referencedAssembly => referencedAssembly.Name == "Bix.Mixers.Fody.TestSource"));
        }
    }
}
