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
        public int[][] ArrayOfArray;
        public int[][][][][][][][][][][][] ArraysOfArrays;
        public int[,] Array2;
        public int[,,,,,,,,,,,,,,,,] Array17;
        public int[,][,] Array2OfArray2;
        public int[,,,,][,,][,,,,,,,][][,,][,,,,,][,,,][,,,][,,,,,,][,,,,,,,][,,,,,,][,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,,] VeryImpressiveArray;

        public class InnerType { }
        public InnerType[] InnerTypeArray;
        public InnerType[][] InnerTypeArrayOfArry;
        public InnerType[][][][][][][][][][][][] InnerTypeArraysOfArrays;
        public InnerType[,] InnerTypeArray2;
        public InnerType[, , , , , , , , , , , , , , , ,] InnerTypeArray17;
        public InnerType[,][,] InnerTypeArray2OfArray2;
        public InnerType[, , , ,][, ,][, , , , , , ,][][, ,][, , , , ,][, , ,][, , ,][, , , , , ,][, , , , , , ,][, , , , , ,][, , , , , , , , , , , , , , , , , , , , , , , , , , , , , , ,] InnerTypeVeryImpressiveArray;
        public List<InnerType>[] InnerTypeListArray;
        public List<InnerType>[][] InnerTypeArrayOfListArray;
        public List<InnerType>[][][][][][][][][][][][] InnerTypeListArraysOfArrays;
        public List<InnerType>[,] InnerTypeListArray2;
        public List<InnerType>[, , , , , , , , , , , , , , , ,] InnerTypeListArray17;
        public List<InnerType>[,][,] InnerTypeListArray2OfArray2;
        public List<InnerType>[, , , ,][, ,][, , , , , , ,][][, ,][, , , , ,][, , ,][, , ,][, , , , , ,][, , , , , , ,][, , , , , ,][, , , , , , , , , , , , , , , , , , , , , , , , , , , , , , ,] InnerTypeListVeryImpressiveArray;
    }
}
