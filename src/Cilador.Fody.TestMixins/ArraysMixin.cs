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

using Cilador.Fody.TestMixinInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cilador.Fody.TestMixins
{
    using Cilador.Fody.TestMixinInterfaces;

    public class ArraysMixin : IEmptyInterface
    {
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
