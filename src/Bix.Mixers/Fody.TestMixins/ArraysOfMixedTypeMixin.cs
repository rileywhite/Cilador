using Bix.Mixers.Fody.ILCloning;
using Bix.Mixers.Fody.TestMixinInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.TestMixins
{
    public class ArraysOfMixedTypeMixin : IEmptyInterface
    {
        [Skip]
        public ArraysOfMixedTypeMixin() { }

        public class InnerType { }

        public InnerType[] Array;
        public InnerType[][] ArrayOfArry;
        public InnerType[][][][][][][][][][][][] ArraysOfArrays;
        public InnerType[,] Array2;
        public InnerType[,,,,,,,,,,,,,,,,] Array17;
        public InnerType[,][,] Array2OfArray2;
        public InnerType[,,,,][,,][,,,,,,,][][,,][,,,,,][,,,][,,,][,,,,,,][,,,,,,,][,,,,,,][,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,] VeryImpressiveArray;
    }
}
