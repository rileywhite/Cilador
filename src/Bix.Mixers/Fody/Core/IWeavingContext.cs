using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.Core
{
    public interface IWeavingContext
    {
        BixMixersConfigType BixMixersConfig { get; }

        Action<string> LogDebug { get; }
        
        Action<string> LogInfo { get; }
        
        Action<string> LogWarning { get; set; }
        
        Action<string, SequencePoint> LogWarningPoint { get; }
        
        Action<string> LogError { get; }
        
        Action<string, SequencePoint> LogErrorPoint { get; }
        
        IAssemblyResolver AssemblyResolver { get; }
        
        ModuleDefinition ModuleDefinition { get; }

        IReadOnlyCollection<string> DefineConstants { get; }
        
        string AssemblyFilePath { get; }
        
        string ProjectDirectoryPath { get; }
        
        string AddinDirectoryPath { get; }
        
        string SolutionDirectoryPath { get; }
    }
}
