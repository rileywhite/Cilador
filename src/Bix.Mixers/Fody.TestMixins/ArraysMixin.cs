using Bix.Mixers.Fody.ILCloning;
using Bix.Mixers.Fody.TestMixinInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.TestMixins
{
    public class ArraysMixin : IEmptyInterface
    {
        [Skip]
        public ArraysMixin() { }

        public int[] Array;
        public int[][] ArrayOfArry;
        public int[][][][][][][][][][][][] ArraysOfArrays;
        public int[,] Array2;
        public int[,,,,,,,,,,,,,,,,] Array17;
        public int[,][,] Array2OfArray2;
        public int[,,,,][,,][,,,,,,,][][,,][,,,,,][,,,][,,,][,,,,,,][,,,,,,,][,,,,,,][,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,] VeryImpressiveArray;
    }
}
