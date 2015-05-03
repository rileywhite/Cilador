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

using Cilador.ILCloning;
using Mono.Cecil;
using Mono.Cecil.Cil;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Cilador.Tests.ILCloningTests
{
    [TestFixture]
    internal class ILCloningExtensionsFixture
    {
        private IAssemblyResolver Resolver { get; set; }
        private AssemblyDefinition CurrentAssembly { get; set; }
        private ModuleDefinition CurrentModule { get; set; }

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            this.Resolver = new DefaultAssemblyResolver();
            this.CurrentAssembly = this.Resolver.Resolve(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
            Assert.That(this.CurrentAssembly, Is.Not.Null);
            this.CurrentModule = this.CurrentAssembly.MainModule;
            Assert.That(this.CurrentModule, Is.Not.Null);
        }

        private class TestCloner : ICloner<object>
        {
            public IILCloningContext ILCloningContext { get; set; }

            public bool IsCloned { get; private set; }

            public void Clone()
            {
                this.IsCloned = true;
            }

            public object Source
            {
                get { return new object(); }
            }

            public object Target
            {
                get { return new object(); }
            }
        }

        [Test]
        public void CloneAllTest()
        {
            var cloners = new TestCloner[0];

            cloners.CloneAll();
            Assert.That(cloners, Is.Empty);

            cloners = new TestCloner[] { new TestCloner() };
            cloners.CloneAll();
            Assert.That(cloners, Has.Length.EqualTo(1));
            Assert.That(cloners, Has.All.Property("IsCloned").True);

            cloners = new TestCloner[] { new TestCloner(), new TestCloner(), new TestCloner() };
            cloners.CloneAll();
            Assert.That(cloners, Has.Length.EqualTo(3));
            Assert.That(cloners, Has.All.Property("IsCloned").True);
        }


        private class Nested
        {
            public class Level1_1
            {
                public class Level1_1_1
                {
                    public class Level1_1_1_1
                    {
                    }
                }
            }

            public class Level1_2
            {
                public class Level1_2_1<T>
                {
                    public class Level1_2_1_1
                    {
                        public int SomeInt;
                    }
                }
            }
        }

        [Test]
        public void IsNestedWithinTest()
        {
            var nestedType = this.CurrentModule.Import(typeof(Nested)).Resolve();
            var level1_1Type = this.CurrentModule.Import(typeof(Nested.Level1_1)).Resolve();
            var level1_1_1Type = this.CurrentModule.Import(typeof(Nested.Level1_1.Level1_1_1)).Resolve();
            var level1_1_1_1Type = this.CurrentModule.Import(typeof(Nested.Level1_1.Level1_1_1.Level1_1_1_1)).Resolve();
            var level1_2Type = this.CurrentModule.Import(typeof(Nested.Level1_2)).Resolve();
            var level1_2_1TType = this.CurrentModule.Import(typeof(Nested.Level1_2.Level1_2_1<>)).Resolve();
            var level1_2_1TTypeReference = this.CurrentModule.Import(typeof(Nested.Level1_2.Level1_2_1<>));
            var level1_2_1T_1Type = this.CurrentModule.Import(typeof(Nested.Level1_2.Level1_2_1<>.Level1_2_1_1)).Resolve();
            var level1_2_1T_1TypeReference = this.CurrentModule.Import(typeof(Nested.Level1_2.Level1_2_1<>.Level1_2_1_1));

            var level1_2_1IntTypeReference = this.CurrentModule.Import(typeof(Nested.Level1_2.Level1_2_1<int>));
            var level1_2_1Int_1TypeReference = this.CurrentModule.Import(typeof(Nested.Level1_2.Level1_2_1<int>.Level1_2_1_1));

            // not nested within self
            Assert.That(nestedType.IsNestedWithin(nestedType), Is.False);

            // direct nesting is correct
            Assert.That(level1_1Type.IsNestedWithin(nestedType), Is.True);
            Assert.That(nestedType.IsNestedWithin(level1_1Type), Is.False);

            // deep nesting is correct
            Assert.That(level1_1_1_1Type.IsNestedWithin(nestedType), Is.True);
            Assert.That(nestedType.IsNestedWithin(level1_1_1_1Type), Is.False);

            // not confused by sibling types
            Assert.That(level1_1Type.IsNestedWithin(level1_2Type), Is.False);
            Assert.That(level1_1_1Type.IsNestedWithin(level1_2Type), Is.False);
            Assert.That(level1_1_1_1Type.IsNestedWithin(level1_2Type), Is.False);
            Assert.That(level1_2Type.IsNestedWithin(level1_1Type), Is.False);
            Assert.That(level1_2_1TType.IsNestedWithin(level1_1Type), Is.False);
            Assert.That(level1_2_1T_1Type.IsNestedWithin(level1_1Type), Is.False);

            // works with generic types and open generic type references
            Assert.That(level1_2_1TType.IsNestedWithin(level1_2Type), Is.True);
            Assert.That(level1_2_1TType.IsNestedWithin(nestedType), Is.True);

            Assert.That(level1_2_1TTypeReference.IsNestedWithin(level1_2Type), Is.True);
            Assert.That(level1_2_1TTypeReference.IsNestedWithin(nestedType), Is.True);

            Assert.That(level1_2_1T_1Type.IsNestedWithin(level1_2_1TType), Is.True);
            Assert.That(level1_2_1T_1Type.IsNestedWithin(level1_2Type), Is.True);
            Assert.That(level1_2_1T_1Type.IsNestedWithin(nestedType), Is.True);

            Assert.That(level1_2_1T_1TypeReference.IsNestedWithin(level1_2_1TType), Is.True);
            Assert.That(level1_2_1T_1TypeReference.IsNestedWithin(level1_2Type), Is.True);
            Assert.That(level1_2_1T_1TypeReference.IsNestedWithin(nestedType), Is.True);

            // works with closed generic type references
            Assert.That(level1_2_1IntTypeReference.IsNestedWithin(level1_2Type), Is.True);
            Assert.That(level1_2_1IntTypeReference.IsNestedWithin(nestedType), Is.True);

            Assert.That(level1_2_1Int_1TypeReference.IsNestedWithin(level1_2_1TType), Is.True);
            Assert.That(level1_2_1Int_1TypeReference.IsNestedWithin(level1_2Type), Is.True);
            Assert.That(level1_2_1Int_1TypeReference.IsNestedWithin(nestedType), Is.True);
        }

        [Test]
        public void SignatureEqualsTest()
        {
            // just check some existing methods that match and fail expectations
            var ilCloningContext = new FakeILCloningConext
            {
                RootSource = this.CurrentModule.Import(typeof(int)).Resolve(),
                RootTarget = this.CurrentModule.Import(typeof(double)).Resolve(),
            };

            var intTryParseMethod = this.CurrentModule.Import(typeof(int).GetMethod("Parse", new Type[] { typeof(string) }));
            var intTryParseWithExtraParametersMethod = this.CurrentModule.Import(typeof(double).GetMethod("Parse", new Type[] { typeof(string), typeof(IFormatProvider) }));
            var doubleTryParseMethod = this.CurrentModule.Import(typeof(double).GetMethod("Parse", new Type[] { typeof(string) }));
            var decimalTryParseMethod = this.CurrentModule.Import(typeof(decimal).GetMethod("Parse", new Type[] { typeof(string) }));

            Assert.IsTrue(doubleTryParseMethod.SignatureEquals(intTryParseMethod, ilCloningContext));
            Assert.IsFalse(doubleTryParseMethod.SignatureEquals(intTryParseWithExtraParametersMethod, ilCloningContext));
            Assert.IsFalse(intTryParseMethod.SignatureEquals(doubleTryParseMethod, ilCloningContext));
            Assert.IsFalse(doubleTryParseMethod.SignatureEquals(decimalTryParseMethod, ilCloningContext));

            // now use a generic method
            ilCloningContext = new FakeILCloningConext
            {
                RootSource = this.CurrentModule.Import(typeof(List<>)).Resolve(),
                RootTarget = this.CurrentModule.Import(typeof(Queue<>)).Resolve(),
            };

            var listMethod = this.CurrentModule.Import(typeof(List<>).GetMethods().First(method => method.Name == "GetEnumerator" && method.ContainsGenericParameters));
            var queueMethod = this.CurrentModule.Import(typeof(Queue<>).GetMethods().First(method => method.Name == "GetEnumerator" && method.ContainsGenericParameters));
            var queueClosedMethod = this.CurrentModule.Import(typeof(Queue<int>).GetMethod("GetEnumerator"));

            Assert.IsTrue(queueMethod.SignatureEquals(listMethod, ilCloningContext));
            Assert.IsFalse(listMethod.SignatureEquals(queueMethod, ilCloningContext));
            Assert.IsFalse(queueMethod.SignatureEquals(queueClosedMethod, ilCloningContext));
        }

        [Test]
        public void IsStoreVariableOpCodeTest()
        {
            foreach (var code in Enum.GetValues(typeof(Code)))
            {
                Assert.AreEqual(code.ToString().StartsWith("Stloc"), ((Code)code).IsStoreVariableOpCode());
            }
        }

        [Test]
        public void IsLoadVariableOpCodeTest()
        {
            foreach (var code in Enum.GetValues(typeof(Code)))
            {
                Assert.AreEqual(
                    code.ToString().StartsWith("Ldloc") && !code.ToString().StartsWith("Ldloca"),
                    ((Code)code).IsLoadVariableOpCode());
            }
        }

        [Test]
        public void IsLoadVariableAddressOpCodeTest()
        {
            foreach (var code in Enum.GetValues(typeof(Code)))
            {
                Assert.AreEqual(code.ToString().StartsWith("Ldloca"), ((Code)code).IsLoadVariableAddressOpCode());
            }
        }

        private static readonly FieldInfo VariableDefinitionIndexField =
            typeof(VariableDefinition).GetField("index", BindingFlags.NonPublic | BindingFlags.Instance);

        private VariableDefinition CreateTestVariableDefinition(int index)
        {
            VariableDefinition variable = new VariableDefinition(this.CurrentModule.Import(typeof(object)));
            VariableDefinitionIndexField.SetValue(variable, index);
            return variable;
        }

        [TestCase(Code.Stloc, 18)]
        [TestCase(Code.Stloc_S, 0)]
        [TestCase(Code.Ldloc, 3)]
        [TestCase(Code.Ldloc_S, 5)]
        [TestCase(Code.Ldloca, 14)]
        [TestCase(Code.Ldloca_S, 115)]
        [TestCase(Code.Stloc_0, 0)]
        [TestCase(Code.Ldloc_0, 0)]
        [TestCase(Code.Stloc_1, 1)]
        [TestCase(Code.Ldloc_1, 1)]
        [TestCase(Code.Stloc_2, 2)]
        [TestCase(Code.Ldloc_2, 2)]
        [TestCase(Code.Stloc_3, 3)]
        [TestCase(Code.Ldloc_3, 3)]
        [TestCase(Code.Nop, null)]
        [TestCase(Code.Stloc, -5, ExpectedException = typeof(InvalidOperationException))]
        public void TryGetVariableIndexTest(Code code, int? expectedVariableIndex)
        {
            var opCodeField = typeof(OpCodes).GetField(
                code.ToString(),
                BindingFlags.Public | BindingFlags.Static);
            Assert.IsNotNull(opCodeField);

            OpCode opCode = (OpCode)opCodeField.GetValue(null);

            Instruction instruction;
            if ((opCode.OperandType == OperandType.InlineVar || opCode.OperandType == OperandType.ShortInlineVar) && expectedVariableIndex.HasValue)
            {
                VariableDefinition operand = this.CreateTestVariableDefinition(expectedVariableIndex.Value);
                instruction = Instruction.Create(opCode, operand);
            }
            else
            {
                instruction = Instruction.Create(opCode);
            }

            int? variableIndex;
            Assert.AreEqual(expectedVariableIndex.HasValue, instruction.TryGetVariableIndex(out variableIndex));
            Assert.AreEqual(expectedVariableIndex, variableIndex);
        }

        [Test]
        public void ApplyLocalVariableTranslationErrorsTest()
        {
            // send a non-store/load instruction in
            // should just be sent back
            var instruction = Instruction.Create(OpCodes.Nop);
            Assert.AreEqual(instruction, instruction.ApplyLocalVariableTranslation(5, new VariableDefinition[0]));

            // check the error case of the variable not being found
            // this error can only happen for the short stloc_0-3 and ldloc_0-3 that are being translated out of the 0-3 range
            // and it should never happen in valid code
            Assert.Catch<InvalidOperationException>(
                () => Instruction.Create(OpCodes.Stloc_0).ApplyLocalVariableTranslation(10, new VariableDefinition[0]));
            Assert.Catch<InvalidOperationException>(
                () => Instruction.Create(OpCodes.Stloc_1).ApplyLocalVariableTranslation(10, new VariableDefinition[0]));
            Assert.Catch<InvalidOperationException>(
                () => Instruction.Create(OpCodes.Stloc_2).ApplyLocalVariableTranslation(10, new VariableDefinition[0]));
            Assert.Catch<InvalidOperationException>(
                () => Instruction.Create(OpCodes.Stloc_3).ApplyLocalVariableTranslation(10, new VariableDefinition[0]));
            Assert.Catch<InvalidOperationException>(
                () => Instruction.Create(OpCodes.Ldloc_0).ApplyLocalVariableTranslation(10, new VariableDefinition[0]));
            Assert.Catch<InvalidOperationException>(
                () => Instruction.Create(OpCodes.Ldloc_1).ApplyLocalVariableTranslation(10, new VariableDefinition[0]));
            Assert.Catch<InvalidOperationException>(
                () => Instruction.Create(OpCodes.Ldloc_2).ApplyLocalVariableTranslation(10, new VariableDefinition[0]));
            Assert.Catch<InvalidOperationException>(
                () => Instruction.Create(OpCodes.Ldloc_3).ApplyLocalVariableTranslation(10, new VariableDefinition[0]));
        }

        [Test]
        public void ApplyLocalVariableTranslationStore0_3Test()
        {
            // translate a few store instructions
            int maxIndex = 10;
            var NoOperandStoreOpCodesWithInitialIndex = new Tuple<OpCode, int>[] {
                Tuple.Create(OpCodes.Stloc_0, 0),
                Tuple.Create(OpCodes.Stloc_1, 1),
                Tuple.Create(OpCodes.Stloc_2, 2),
                Tuple.Create(OpCodes.Stloc_3, 3),
            };
            foreach (var opCodeAndIninitialIndex in NoOperandStoreOpCodesWithInitialIndex)
            {
                for (int translation = -opCodeAndIninitialIndex.Item2;
                    translation < maxIndex - opCodeAndIninitialIndex.Item2;
                    translation++)
                {
                    var instruction = Instruction.Create(opCodeAndIninitialIndex.Item1);
                    var variables = new VariableDefinition[] { this.CreateTestVariableDefinition(opCodeAndIninitialIndex.Item2) };
                    var translatedInstruction = instruction.ApplyLocalVariableTranslation(translation, variables);

                    // the sent instruction should be returned when no translation occurs
                    Assert.AreEqual(translation == 0, instruction.Equals(translatedInstruction));

                    // for the lowest range, check the actual new index, which is embedded in the opcode
                    int expectedIndex = opCodeAndIninitialIndex.Item2 + translation;
                    if (expectedIndex <= 3)
                    {
                        int? actualIndex;
                        Assert.IsTrue(translatedInstruction.TryGetVariableIndex(out actualIndex));
                        Assert.IsTrue(actualIndex.HasValue);
                        Assert.AreEqual(expectedIndex, actualIndex.Value);
                    }
                    else
                    {
                        // for 4-255, just check the opcode for stloc.s
                        // the index is part of the VariableDefinition operand, so it will be changed
                        // as part of the variable cloning
                        Assert.AreEqual(Code.Stloc_S, translatedInstruction.OpCode.Code);
                    }

                    // bump the translation out of range of stloc.s
                    translatedInstruction = instruction.ApplyLocalVariableTranslation(translation + 256 + maxIndex, variables);
                    Assert.AreEqual(Code.Stloc, translatedInstruction.OpCode.Code);
                }
            }
        }

        [Test]
        public void ApplyLocalVariableTranslationStore4_255Test()
        {
            var variables = new VariableDefinition[] { this.CreateTestVariableDefinition(45) };
            var instruction = Instruction.Create(OpCodes.Stloc_S, variables[0]);

            Assert.AreEqual(Code.Stloc_0, instruction.ApplyLocalVariableTranslation(-45, variables).OpCode.Code);
            Assert.AreEqual(Code.Stloc_1, instruction.ApplyLocalVariableTranslation(-44, variables).OpCode.Code);
            Assert.AreEqual(Code.Stloc_2, instruction.ApplyLocalVariableTranslation(-43, variables).OpCode.Code);
            Assert.AreEqual(Code.Stloc_3, instruction.ApplyLocalVariableTranslation(-42, variables).OpCode.Code);

            Assert.AreEqual(instruction, instruction.ApplyLocalVariableTranslation(-41, variables));
            Assert.AreEqual(instruction, instruction.ApplyLocalVariableTranslation(0, variables));
            Assert.AreEqual(instruction, instruction.ApplyLocalVariableTranslation(210, variables));

            var translatedInstruction = instruction.ApplyLocalVariableTranslation(211, variables);
            Assert.AreEqual(Code.Stloc, translatedInstruction.OpCode.Code);
            Assert.AreEqual(variables[0], translatedInstruction.Operand);

            translatedInstruction = instruction.ApplyLocalVariableTranslation(1000, variables);
            Assert.AreEqual(Code.Stloc, translatedInstruction.OpCode.Code);
            Assert.AreEqual(variables[0], translatedInstruction.Operand);
        }

        [Test]
        public void ApplyLocalVariableTranslationStore256Test()
        {
            var variables = new VariableDefinition[] { this.CreateTestVariableDefinition(392) };
            var instruction = Instruction.Create(OpCodes.Stloc, variables[0]);

            Assert.AreEqual(Code.Stloc_0, instruction.ApplyLocalVariableTranslation(-392, variables).OpCode.Code);
            Assert.AreEqual(Code.Stloc_1, instruction.ApplyLocalVariableTranslation(-391, variables).OpCode.Code);
            Assert.AreEqual(Code.Stloc_2, instruction.ApplyLocalVariableTranslation(-390, variables).OpCode.Code);
            Assert.AreEqual(Code.Stloc_3, instruction.ApplyLocalVariableTranslation(-389, variables).OpCode.Code);

            Assert.AreEqual(instruction, instruction.ApplyLocalVariableTranslation(-136, variables));
            Assert.AreEqual(instruction, instruction.ApplyLocalVariableTranslation(0, variables));
            Assert.AreEqual(instruction, instruction.ApplyLocalVariableTranslation(210, variables));

            var translatedInstruction = instruction.ApplyLocalVariableTranslation(-388, variables);
            Assert.AreEqual(Code.Stloc_S, translatedInstruction.OpCode.Code);
            Assert.AreEqual(variables[0], translatedInstruction.Operand);

            translatedInstruction = instruction.ApplyLocalVariableTranslation(-211, variables);
            Assert.AreEqual(Code.Stloc_S, translatedInstruction.OpCode.Code);
            Assert.AreEqual(variables[0], translatedInstruction.Operand);
        }

        [Test]
        public void ApplyLocalVariableTranslationLoad0_3Test()
        {
            // translate some load instructions
            int maxIndex = 10;
            var NoOperandStoreOpCodesWithInitialIndex = new Tuple<OpCode, int>[] {
                Tuple.Create(OpCodes.Ldloc_0, 0),
                Tuple.Create(OpCodes.Ldloc_1, 1),
                Tuple.Create(OpCodes.Ldloc_2, 2),
                Tuple.Create(OpCodes.Ldloc_3, 3),
            };
            foreach (var opCodeAndIninitialIndex in NoOperandStoreOpCodesWithInitialIndex)
            {
                for (int translation = -opCodeAndIninitialIndex.Item2;
                    translation < maxIndex - opCodeAndIninitialIndex.Item2;
                    translation++)
                {
                    var instruction = Instruction.Create(opCodeAndIninitialIndex.Item1);
                    var variables = new VariableDefinition[] { this.CreateTestVariableDefinition(opCodeAndIninitialIndex.Item2) };
                    var translatedInstruction = instruction.ApplyLocalVariableTranslation(translation, variables);

                    // the sent instruction should be returned when no translation occurs
                    Assert.AreEqual(translation == 0, instruction.Equals(translatedInstruction));

                    // for the lowest range, check the actual new index, which is embedded in the opcode
                    int expectedIndex = opCodeAndIninitialIndex.Item2 + translation;
                    if (expectedIndex <= 3)
                    {
                        int? actualIndex;
                        Assert.IsTrue(translatedInstruction.TryGetVariableIndex(out actualIndex));
                        Assert.IsTrue(actualIndex.HasValue);
                        Assert.AreEqual(expectedIndex, actualIndex.Value);
                    }
                    else
                    {
                        // for 4-255, just check the opcode for ldloc.s
                        // the index is part of the VariableDefinition operand, so it will be changed
                        // as part of the variable cloning
                        Assert.AreEqual(Code.Ldloc_S, translatedInstruction.OpCode.Code);
                    }

                    // bump the translation out of range of ldloc.s
                    translatedInstruction = instruction.ApplyLocalVariableTranslation(translation + 256 + maxIndex, variables);
                    Assert.AreEqual(Code.Ldloc, translatedInstruction.OpCode.Code);
                }
            }
        }

        [Test]
        public void ApplyLocalVariableTranslationLoad4_255Test()
        {
            var variables = new VariableDefinition[] { this.CreateTestVariableDefinition(45) };
            var instruction = Instruction.Create(OpCodes.Ldloc_S, variables[0]);

            Assert.AreEqual(Code.Ldloc_0, instruction.ApplyLocalVariableTranslation(-45, variables).OpCode.Code);
            Assert.AreEqual(Code.Ldloc_1, instruction.ApplyLocalVariableTranslation(-44, variables).OpCode.Code);
            Assert.AreEqual(Code.Ldloc_2, instruction.ApplyLocalVariableTranslation(-43, variables).OpCode.Code);
            Assert.AreEqual(Code.Ldloc_3, instruction.ApplyLocalVariableTranslation(-42, variables).OpCode.Code);

            Assert.AreEqual(instruction, instruction.ApplyLocalVariableTranslation(-41, variables));
            Assert.AreEqual(instruction, instruction.ApplyLocalVariableTranslation(0, variables));
            Assert.AreEqual(instruction, instruction.ApplyLocalVariableTranslation(210, variables));

            var translatedInstruction = instruction.ApplyLocalVariableTranslation(211, variables);
            Assert.AreEqual(Code.Ldloc, translatedInstruction.OpCode.Code);
            Assert.AreEqual(variables[0], translatedInstruction.Operand);

            translatedInstruction = instruction.ApplyLocalVariableTranslation(1000, variables);
            Assert.AreEqual(Code.Ldloc, translatedInstruction.OpCode.Code);
            Assert.AreEqual(variables[0], translatedInstruction.Operand);
        }

        [Test]
        public void ApplyLocalVariableTranslationLoad256Test()
        {
            var variables = new VariableDefinition[] { this.CreateTestVariableDefinition(392) };
            var instruction = Instruction.Create(OpCodes.Ldloc, variables[0]);

            Assert.AreEqual(Code.Ldloc_0, instruction.ApplyLocalVariableTranslation(-392, variables).OpCode.Code);
            Assert.AreEqual(Code.Ldloc_1, instruction.ApplyLocalVariableTranslation(-391, variables).OpCode.Code);
            Assert.AreEqual(Code.Ldloc_2, instruction.ApplyLocalVariableTranslation(-390, variables).OpCode.Code);
            Assert.AreEqual(Code.Ldloc_3, instruction.ApplyLocalVariableTranslation(-389, variables).OpCode.Code);

            Assert.AreEqual(instruction, instruction.ApplyLocalVariableTranslation(-136, variables));
            Assert.AreEqual(instruction, instruction.ApplyLocalVariableTranslation(0, variables));
            Assert.AreEqual(instruction, instruction.ApplyLocalVariableTranslation(210, variables));

            var translatedInstruction = instruction.ApplyLocalVariableTranslation(-388, variables);
            Assert.AreEqual(Code.Ldloc_S, translatedInstruction.OpCode.Code);
            Assert.AreEqual(variables[0], translatedInstruction.Operand);

            translatedInstruction = instruction.ApplyLocalVariableTranslation(-211, variables);
            Assert.AreEqual(Code.Ldloc_S, translatedInstruction.OpCode.Code);
            Assert.AreEqual(variables[0], translatedInstruction.Operand);
        }

        [Test]
        public void ApplyLocalVariableTranslationLoadAddress4_255Test()
        {
            var variables = new VariableDefinition[] { this.CreateTestVariableDefinition(45) };
            var instruction = Instruction.Create(OpCodes.Ldloca_S, variables[0]);

            Assert.AreEqual(instruction, instruction.ApplyLocalVariableTranslation(-45, variables));
            Assert.AreEqual(instruction, instruction.ApplyLocalVariableTranslation(-44, variables));
            Assert.AreEqual(instruction, instruction.ApplyLocalVariableTranslation(-43, variables));
            Assert.AreEqual(instruction, instruction.ApplyLocalVariableTranslation(-42, variables));
            Assert.AreEqual(instruction, instruction.ApplyLocalVariableTranslation(-41, variables));
            Assert.AreEqual(instruction, instruction.ApplyLocalVariableTranslation(0, variables));
            Assert.AreEqual(instruction, instruction.ApplyLocalVariableTranslation(210, variables));

            var translatedInstruction = instruction.ApplyLocalVariableTranslation(211, variables);
            Assert.AreEqual(Code.Ldloca, translatedInstruction.OpCode.Code);
            Assert.AreEqual(variables[0], translatedInstruction.Operand);

            translatedInstruction = instruction.ApplyLocalVariableTranslation(1000, variables);
            Assert.AreEqual(Code.Ldloca, translatedInstruction.OpCode.Code);
            Assert.AreEqual(variables[0], translatedInstruction.Operand);
        }

        [Test]
        public void ApplyLocalVariableTranslationLoadAddress256Test()
        {
            var variables = new VariableDefinition[] { this.CreateTestVariableDefinition(392) };
            var instruction = Instruction.Create(OpCodes.Ldloca, variables[0]);

            Assert.AreEqual(instruction, instruction.ApplyLocalVariableTranslation(-136, variables));
            Assert.AreEqual(instruction, instruction.ApplyLocalVariableTranslation(0, variables));
            Assert.AreEqual(instruction, instruction.ApplyLocalVariableTranslation(210, variables));

            var translatedInstruction = instruction.ApplyLocalVariableTranslation(-388, variables);
            Assert.AreEqual(Code.Ldloca_S, translatedInstruction.OpCode.Code);
            Assert.AreEqual(variables[0], translatedInstruction.Operand);

            translatedInstruction = instruction.ApplyLocalVariableTranslation(-211, variables);
            Assert.AreEqual(Code.Ldloca_S, translatedInstruction.OpCode.Code);
            Assert.AreEqual(variables[0], translatedInstruction.Operand);

            translatedInstruction = instruction.ApplyLocalVariableTranslation(-389, variables);
            Assert.AreEqual(Code.Ldloca_S, translatedInstruction.OpCode.Code);
            Assert.AreEqual(variables[0], translatedInstruction.Operand);

            translatedInstruction = instruction.ApplyLocalVariableTranslation(-390, variables);
            Assert.AreEqual(Code.Ldloca_S, translatedInstruction.OpCode.Code);
            Assert.AreEqual(variables[0], translatedInstruction.Operand);

            translatedInstruction = instruction.ApplyLocalVariableTranslation(-391, variables);
            Assert.AreEqual(Code.Ldloca_S, translatedInstruction.OpCode.Code);
            Assert.AreEqual(variables[0], translatedInstruction.Operand);

            translatedInstruction = instruction.ApplyLocalVariableTranslation(-392, variables);
            Assert.AreEqual(Code.Ldloca_S, translatedInstruction.OpCode.Code);
            Assert.AreEqual(variables[0], translatedInstruction.Operand);
        }
    }
}
