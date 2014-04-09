using NUnit.ConsoleRunner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            Runner.Main(new[] { Assembly.GetExecutingAssembly().Location });
        }
    }
}
