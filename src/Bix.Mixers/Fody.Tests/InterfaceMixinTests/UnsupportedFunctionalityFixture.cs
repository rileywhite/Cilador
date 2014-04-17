using Bix.Mixers.Fody.Core;
using Bix.Mixers.Fody.InterfaceMixins;
using Bix.Mixers.Fody.TestMixinInterfaces;
using Bix.Mixers.Fody.TestMixins;
using Bix.Mixers.Fody.Tests.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.Tests.InterfaceMixinTests
{
    [TestFixture]
    public class UnsupportedFunctionalityFixture
    {
        [Test]
        public void CannotUseAbstractMixin()
        {
            var config = new BixMixersConfigType();

            config.MixCommandConfig = new MixCommandConfigTypeBase[]
            {
                new InterfaceMixinConfigType
                {
                    InterfaceMap = new InterfaceMapType[]
                    {
                        new InterfaceMapType
                        {
                            Interface = typeof(IEmptyInterface).GetShortAssemblyQualifiedName(),
                            Mixin = typeof(AbstractMixin).GetShortAssemblyQualifiedName()
                        }
                    }
                },
            };

            Assert.Throws<WeavingException>(
                () => ModuleWeaverHelper.WeaveAndLoadTestTarget(config),
                string.Format("Mixin source type cannot be abstract: [{0}]", typeof(AbstractMixin).FullName));
        }
    }
}
