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

using Cilador.Fody.Config;
using Cilador.Fody.TestDtoProjectorTargets;
using Cilador.Fody.Tests.Common;
using NUnit.Framework;
using System;

namespace Cilador.Fody.Tests.DtoProjectorTests
{
    /// <summary>
    /// Tests standard usage of DTO projector weaving.
    /// </summary>
    [TestFixture]
    internal class DtoProjectorFixture
    {
        [Test]
        public void CorrectPropertiesAreRepresentedAndAreReadWriteInDto()
        {
            var config = new CiladorConfigType();
            config.WeaveConfig = new WeaveConfigTypeBase[0];

            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget("Cilador.Fody.TestDtoProjectorTargets", config);
            var targetType = assembly.GetType(typeof(Properties).FullName);

            Type dtoType = targetType.GetNestedType("Dto");
            Assert.That(dtoType, Is.Not.Null);

            Assert.That(dtoType.GetFields().Length, Is.EqualTo(33));
            var fieldNames = new string[]
            {
                "PublicGetPublicSet",
                "PublicGetPrivateSet",
                "PublicGetInternalSet",
                "PublicGetProtectedSet",
                "PublicGetProtectedInternalSet",
                "PrivateGetPublicSet",
                "PrivateGetPrivateSet",
                "PrivateGetInternalSet",
                "PrivateGetProtectedSet",
                "PrivateGetProtectedInternalSet",
                "InternalGetPublicSet",
                "InternalGetPrivateSet",
                "InternalGetInternalSet",
                "InternalGetProtectedInternalSet",
                "ProtectedGetPublicSet",
                "ProtectedGetPrivateSet",
                "ProtectedGetProtectedSet",
                "ProtectedGetProtectedInternalSet",
                "ProtectedInternalGetPublicSet",
                "ProtectedInternalGetPrivateSet",
                "ProtectedInternalGetInternalSet",
                "ProtectedInternalGetProtectedSet",
                "ProtectedInternalGetProtectedInternalSet",
                "PublicGet",
                "PrivateGet",
                "InternalGet",
                "ProtectedGet",
                "ProtectedInternalGet",
                "PublicSet",
                "PrivateSet",
                "InternalSet",
                "ProtectedSet",
                "ProtectedInternalSet"
            };

            foreach (var fieldName in fieldNames)
            {
                var field = dtoType.GetField(fieldName);
                Assert.That(field, Is.Not.Null);
                Assert.That(field.IsPublic);
            }
        }
    }
}
