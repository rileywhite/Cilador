using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.Tests.Common
{
    internal static class TestContent
    {
        public static BindingFlags BindingFlagsForMixedMembers =
            BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        public static string GetTestSolutionDirectory()
        {
            return Path.GetFullPath(@"..\..\..\..\");
        }

        private static readonly string TestProjectDirectoryRelativeToExecutingAssemblyFormat = @"..\..\..\Fody.Test{0}";

        public static string GetTestProjectDirectory()
        {
            return Path.GetFullPath(string.Format(TestProjectDirectoryRelativeToExecutingAssemblyFormat, "MixinTargets"));
        }

        private static readonly string TestAssemblyPathRelativeToExecutingAssemblyFormat =
            TestProjectDirectoryRelativeToExecutingAssemblyFormat + @"\bin\{1}\Bix.Mixers.Fody.Test{0}.dll";

        public static string GetTestTargetsPath()
        {
            return GetTestPath("MixinTargets");
        }

        public static string GetTestMixinsPath()
        {
            return GetTestPath("Mixins");
        }

        private static string GetTestPath(string name)
        {
            var targetPathFormat = Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(new Uri(typeof(TestContent).Assembly.CodeBase).LocalPath),
                string.Format(TestAssemblyPathRelativeToExecutingAssemblyFormat, name, "{0}")));
#if (DEBUG)
            return string.Format(targetPathFormat, "Debug");
#else
            return string.Format(targetPathFormat, "Release");
#endif
        }

        public static string GetAddinDirectory()
        {
            return Path.GetDirectoryName(GetTestMixinsPath());
        }
    }
}
