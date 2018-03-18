/***************************************************************************/
// Copyright 2013-2018 Riley White
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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Cilador.Fody.Tests.Common
{
    internal static class TestContent
    {
        public static BindingFlags BindingFlagsForWeavedMembers =
            BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        public static string GetTestSolutionDirectory()
        {
            return Path.GetFullPath(@"..\..\..\");
        }

        private static readonly string TestProjectDirectoryRelativeToExecutingAssemblyFormat = @"..\..\..\{0}";

        private static readonly string TestAssemblyPathRelativeToExecutingAssemblyFormat =
            TestProjectDirectoryRelativeToExecutingAssemblyFormat + @"\bin\{1}\{0}.dll";

        public static string GetTestPath(string assemblyFilename)
        {
            var targetPathFormat = Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(new Uri(typeof(TestContent).Assembly.CodeBase).LocalPath),
                string.Format(TestAssemblyPathRelativeToExecutingAssemblyFormat, assemblyFilename, "{0}")));
#if (DEBUG)
            return string.Format(targetPathFormat, "Debug");
#else
            return string.Format(targetPathFormat, "Release");
#endif
        }

        public static string GetDirectory(string assemblyFilename)
        {
            return Path.GetDirectoryName(GetTestPath(assemblyFilename));
        }
    }
}
