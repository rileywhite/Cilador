using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.Tests.Common
{
    internal static class TestPaths
    {
        public static string GetTestSolutionDirectory()
        {
            return Path.GetFullPath(@"..\..\..\..\");
        }

        private static readonly string TestProjectDirectoryRelativeToExecutingAssemblyFormat = @"..\..\..\Fody.Test{0}";

        public static string GetTestProjectDirectory()
        {
            return Path.GetFullPath(string.Format(TestProjectDirectoryRelativeToExecutingAssemblyFormat, "Target"));
        }

        private static readonly string TestAssemblyPathRelativeToExecutingAssemblyFormat =
            TestProjectDirectoryRelativeToExecutingAssemblyFormat + @"\bin\{1}\Bix.Mixers.Fody.Test{0}.dll";

        public static string GetTestTargetPath()
        {
            return GetTestPath("Target");
        }

        public static string GetTestSourcePath()
        {
            return GetTestPath("Source");
        }

        private static string GetTestPath(string name)
        {
            var targetPathFormat = Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(new Uri(typeof(TestPaths).Assembly.CodeBase).LocalPath),
                string.Format(TestAssemblyPathRelativeToExecutingAssemblyFormat, name, "{0}")));
#if (DEBUG)
            return string.Format(targetPathFormat, "Debug");
#else
            return string.Format(targetPathFormat, "Release");
#endif
        }

        public static string GetAddinDirectory()
        {
            return Path.GetDirectoryName(GetTestSourcePath());
        }
    }
}
