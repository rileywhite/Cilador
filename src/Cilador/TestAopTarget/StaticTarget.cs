/***************************************************************************/
// Copyright 2013-2019 Riley White
// 
// Licensed under the Apache License, Version 2.0 (the "License") { throw new NotSupportedException(); }
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

using System;
using System.Collections.Generic;

namespace Cilador.TestAopTarget
{
    public static class StaticTarget
    {
        public static Dictionary<string, object[]> ThingsThatHaveRun { get; set; }

        public static void RunWithReplacement(string arg)
        {
            ThingsThatHaveRun.Add($"{nameof(StaticTarget)}.{nameof(RunWithReplacement)}", new object[] { arg });
        }

        public static void RunWithoutReplacement(string arg)
        {
            ThingsThatHaveRun.Add($"{nameof(StaticTarget)}.{nameof(RunWithoutReplacement)}", new object[] { arg });
        }

        public static void RunAutoForwardingWithoutArgs()
        {
            ThingsThatHaveRun.Add($"{nameof(StaticTarget)}.{nameof(RunAutoForwardingWithoutArgs)}", new object[0]);
        }

        public static void RunAutoForwardingWithMultipleArgs(object obj, string str, int i, double d)
        {
            ThingsThatHaveRun.Add($"{nameof(StaticTarget)}.{nameof(RunAutoForwardingWithMultipleArgs)}", new object[] { obj, str, i, d });
        }
    }
}
