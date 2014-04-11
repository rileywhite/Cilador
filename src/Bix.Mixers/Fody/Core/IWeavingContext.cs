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
        Action<string> LogDebug { get; set; }
        
        Action<string> LogInfo { get; set; }
        
        Action<string> LogWarning { get; set; }
        
        Action<string, SequencePoint> LogWarningPoint { get; set; }
        
        Action<string> LogError { get; set; }
        
        Action<string, SequencePoint> LogErrorPoint { get; set; }
        
        IAssemblyResolver AssemblyResolver { get; set; }
        
        ModuleDefinition ModuleDefinition { get; set; }
        
        List<string> DefineConstants { get; set; }
        
        string AssemblyFilePath { get; set; }
        
        string ProjectDirectoryPath { get; set; }
        
        string AddinDirectoryPath { get; set; }
        
        string SolutionDirectoryPath { get; }
    }
}
