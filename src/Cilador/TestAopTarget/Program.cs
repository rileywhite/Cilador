/***************************************************************************/
// Copyright 2013-2019 Riley White
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

using System;

namespace Cilador.TestAopTarget
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var p = new Program();
            p.Run(args);
            p.RunAgain(args);
            p.RunAutoForwardingWithoutArgs();
            p.RunAutoForwardingWithMultipleArgs(new object(), "testestest", 897389, 3873.3829877832);
        }

        public void Run(string[] args)
        {
            Console.WriteLine("Hello World!");
        }

        public void RunAgain(string[] args)
        {
            Console.WriteLine("Hello Again World!");
        }

        public void RunAutoForwardingWithoutArgs()
        {
            Console.WriteLine("Hello Without Args!");
        }

        public void RunAutoForwardingWithMultipleArgs(object obj, string str, int i, double d)
        {
            Console.WriteLine($"Hello With Multiple Args: {obj?.ToString() ?? string.Empty}, {str ?? string.Empty}, {i}, {d}!");
        }
    }
}
