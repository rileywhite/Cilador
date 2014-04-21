using Bix.Mixers.Fody.ILCloning;
using Bix.Mixers.Fody.TestMixinInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.TestMixins
{
    public class DynamicMixin : IEmptyInterface
    {
        [Skip]
        public DynamicMixin() { }

#pragma warning disable 169
        private dynamic dynamicField;
#pragma warning restore 169

        public dynamic DynamicProperty { get; set; }

        public dynamic DynamicParameterAndReturnMethod(dynamic arg0) { return arg0; }

        public void DynamicLocalVariableMethod()
        {
            dynamic dynamicVariable = "I am dynamic!";
        }

        public string DynamicInvokingMethod()
        {
            dynamic dynamicVariable = "I am dynamic!";
            var stringVariable = dynamicVariable.ToString();
            dynamicVariable = new List<int>();
            dynamicVariable.Add(1);
            return dynamicVariable[0].ToString();
        }

#pragma warning disable 67
        public event EventHandler<dynamic> EventWithDynamicHandler;
#pragma warning restore 67
    }
}
