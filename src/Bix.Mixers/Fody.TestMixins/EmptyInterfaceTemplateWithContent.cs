using Bix.Mixers.Fody.TestInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.TestMixins
{
    public class EmptyInterfaceTemplateWithContent : IEmptyInterface
    {
        public EmptyInterfaceTemplateWithContent(int someValue)
        {
            this.SomeValue = someValue;
        }

        public int SomeValue { get; set; }

        public void SomeMethod()
        {
            Console.WriteLine("EmptyInterfaceTargetWithContent.SomeMethod() here.");
        }
    }
}
