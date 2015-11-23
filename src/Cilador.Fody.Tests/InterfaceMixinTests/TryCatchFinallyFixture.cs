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
using Cilador.Fody.TestMixinInterfaces;
using Cilador.Fody.TestMixins;
using Cilador.Fody.TestMixinTargets;
using Cilador.Fody.Tests.Common;
using NUnit.Framework;
using System;

namespace Cilador.Fody.Tests.InterfaceMixinTests
{
    [TestFixture]
    public class TryCatchFinallyFixture
    {
        [Test]
        public void TryCatchFinallyFlowIsCorrect()
        {
            var config = new CiladorConfigType
            {
                WeaveConfig = new WeaveConfigTypeBase[]
                {
                    new InterfaceMixinConfigType
                    {
                        InterfaceMixinMap = new[]
                        {
                            new InterfaceMixinMapType
                            {
                                Interface = typeof (IEmptyInterface).GetShortAssemblyQualifiedName(),
                                Mixin = typeof (TryCatchFinallyMixin).GetShortAssemblyQualifiedName()
                            }
                        }
                    },
                }
            };


            var assembly = ModuleWeaverHelper.WeaveAndLoadTestTarget("Cilador.Fody.TestMixinTargets", config);
            var targetType = assembly.GetType(typeof(EmptyInterfaceTarget).FullName);
            Assert.That(typeof(IEmptyInterface).IsAssignableFrom(targetType));
            targetType.ValidateMemberCountsAre(1, 1, 9, 0, 0, 1);

            var instance = (IEmptyInterface)targetType.GetConstructor(new Type[0]).Invoke(new object[0]);

            Assert.That(targetType.GetField("FieldSetBeforeTryBlock").GetValue(instance), Is.False);
            Assert.That(targetType.GetField("FieldSetBeforeThrowInTryBlock").GetValue(instance), Is.False);
            Assert.That(targetType.GetField("FieldSetAfterThrowInTryBlock").GetValue(instance), Is.False);
            Assert.That(targetType.GetField("FieldSetInApplicationExceptionCatchBlock").GetValue(instance), Is.False);
            Assert.That(targetType.GetField("FieldSetInMyExceptionCatchBlock").GetValue(instance), Is.False);
            Assert.That(targetType.GetField("FieldSetInExceptionCatchBlock").GetValue(instance), Is.False);
            Assert.That(targetType.GetField("FieldSetInFinallyBlock").GetValue(instance), Is.False);
            Assert.That(targetType.GetField("FieldSetAfterFinallyBlock").GetValue(instance), Is.False);
            Assert.That(targetType.GetField("TypeOfCaughtException").GetValue(instance), Is.Null);

            targetType.GetMethod("CatchExceptionWithFinallyBlock").Invoke(instance, new object[0]);

            Assert.That(targetType.GetField("FieldSetBeforeTryBlock").GetValue(instance), Is.True);
            Assert.That(targetType.GetField("FieldSetBeforeThrowInTryBlock").GetValue(instance), Is.True);
            Assert.That(targetType.GetField("FieldSetAfterThrowInTryBlock").GetValue(instance), Is.False);
            Assert.That(targetType.GetField("FieldSetInApplicationExceptionCatchBlock").GetValue(instance), Is.False);
            Assert.That(targetType.GetField("FieldSetInMyExceptionCatchBlock").GetValue(instance), Is.True);
            Assert.That(targetType.GetField("FieldSetInExceptionCatchBlock").GetValue(instance), Is.False);
            Assert.That(targetType.GetField("FieldSetInFinallyBlock").GetValue(instance), Is.True);
            Assert.That(targetType.GetField("FieldSetAfterFinallyBlock").GetValue(instance), Is.True);
            Assert.That(targetType.GetField("TypeOfCaughtException").GetValue(instance), Is.EqualTo(targetType.GetNestedType("MyException")));
        }
    }
}
