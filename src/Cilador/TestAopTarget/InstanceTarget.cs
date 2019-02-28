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
    public class InstanceTarget
    {
        public static Dictionary<string, object[]> ThingsThatHaveRun { get; set; }

        public void RunWithReplacement(string arg)
        {
            ThingsThatHaveRun.Add("Instance RunWithReplacement}", new object[] { arg });
        }

        public void RunWithoutReplacement(string arg)
        {
            ThingsThatHaveRun.Add("Instance RunWithoutReplacement", new object[] { arg });
        }

        public void RunAutoForwardingWithoutArgs()
        {
            ThingsThatHaveRun.Add("Instance RunAutoForwardingWithoutArgs", new object[0]);
        }

        public void RunAutoForwardingWithMultipleArgs(object obj, string str, int i, double d)
        {
            ThingsThatHaveRun.Add("Instance RunAutoForwardingWithMultipleArgs", new object[] { obj, str, i, d });
        }
    }
}
