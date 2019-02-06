///***************************************************************************/
//// Copyright 2013-2018 Riley White
//// 
//// Licensed under the Apache License, Version 2.0 (the "License");
//// you may not use this file except in compliance with the License.
//// You may obtain a copy of the License at
//// 
////     http://www.apache.org/licenses/LICENSE-2.0
//// 
//// Unless required by applicable law or agreed to in writing, software
//// distributed under the License is distributed on an "AS IS" BASIS,
//// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//// See the License for the specific language governing permissions and
//// limitations under the License.
///***************************************************************************/

//using Cilador.Aop.Core;
//using Cilador.Aop.Transform;
//using Cilador.Clone;
//using Cilador.Graph.Factory;
//using Mono.Cecil;
//using System;
//using System.Diagnostics.Contracts;

//namespace Cilador.Aop.Decorate
//{
//    public class PropertyDecorator<T> : IConceptWeaver<PropertyDefinition>
//    {
//        public PropertyDecorator(
//            IAssemblyResolver resolver,
//            CilGraphGetter graphGetter,
//            Action<T> getterDecoration = null,
//            Func<T> setterDecoration = null,
//            Func<string, string> decorationNameGenerator = null)
//        {
//            Contract.Requires(resolver != null);
//            Contract.Requires(graphGetter != null);
//            Contract.Requires(getterDecoration != null || setterDecoration != null);
//            Contract.Ensures(this.Resolver != null);
//            Contract.Ensures(this.GraphGetter != null);
//            Contract.Ensures(this.GetterDecoration != null || this.SetterDecoration != null);
//            Contract.Ensures(this.DecorationNameGenerator == null);

//            this.Resolver = resolver;
//            this.GraphGetter = graphGetter;
//            this.GetterDecoration = getterDecoration;
//            this.SetterDecoration = setterDecoration;
//            this.DecorationNameGenerator = decorationNameGenerator ?? (sourceName => sourceName);
//        }

//        public IAssemblyResolver Resolver { get; }
//        public CilGraphGetter GraphGetter { get; }
//        public Action<T> GetterDecoration { get; }
//        public Func<T> SetterDecoration { get; }
//        public Func<string, string> DecorationNameGenerator { get; }

//        public void Weave(PropertyDefinition target)
//        {
//            var targetAssembly = target.Module.Assembly;
//            var decorationName = this.DecorationNameGenerator(target.Name);
//            var isTargetReplacedByDecorator = decorationName == target.Name;

//            var decorationTarget = CloneDecorationIntoTargetLocation(target, decorationName, isTargetReplacedByDecorator);

//        }

//        private PropertyDefinition CloneDecorationIntoTargetLocation(PropertyDefinition target, string decorationName, bool isTargetReplacedByDecorator)
//        {
//            using (var getterAdviceAssembly = this.Resolver.Resolve(AssemblyNameReference.Parse(this.GetterDecoration.Target.GetType().Assembly.FullName)))
//            using (var setterAdviceAssembly = this.Resolver.Resolve(AssemblyNameReference.Parse(this.SetterDecoration.Target.GetType().Assembly.FullName)))
//            {
//                var getterDecorationSource = getterAdviceAssembly.MainModule.ImportReference(this.GetterDecoration.Method).Resolve();
//                var setterDecorationSource = getterAdviceAssembly.MainModule.ImportReference(this.GetterDecoration.Method).Resolve();

//                var adviceGraph = this.GraphGetter.Get(getterDecorationSource);
//                var cloningContext = new CloningContext(adviceGraph, getterDecorationSource.DeclaringType, target.DeclaringType);

//                // if the names are the same, then we're replacing the target, so move it to a randomly named location
//                if (isTargetReplacedByDecorator)
//                {
//                    target.Name = $"cilador_{Guid.NewGuid().ToString("N")}";
//                }

//                PropertyDefinition getterDecorationTarget = null;
//                cloningContext.InlineWeaves.Add(new WeavableConcept<object>(
//                    new PointCut<object>(s => s == getterDecorationSource),
//                    new TransformAdvisor<object>(
//                    t =>
//                    {
//                        getterDecorationTarget = (PropertyDefinition)t;
//                        getterDecorationTarget.Name = decorationName;
//                        getterDecorationTarget.Attributes = target.Attributes;
//                    })));

//                Contract.Assert(getterDecorationTarget != null);

//                cloningContext.Execute();
//            }

//            return decorationTarget;
//        }
//    }
//}
