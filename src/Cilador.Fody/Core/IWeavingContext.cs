/***************************************************************************/
// Copyright 2013-2016 Riley White
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

using Cilador.Fody.Config;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;

namespace Cilador.Fody.Core
{

    /// <summary>
    /// Represents a read-only context with configuration data, logging actions, and target assembly data
    /// </summary>
    public interface IWeavingContext
    {
        #region Configuration

        /// <summary>
        /// Gets the strongly typed Cilador configuration object.
        /// </summary>
        CiladorConfigType CiladorConfig { get; }

        /// <summary>
        /// Gets the contants defined by the build
        /// </summary>
        IReadOnlyCollection<string> DefineConstants { get; }

        /// <summary>
        /// Gets the path of the target assembly file.
        /// </summary>
        string AssemblyFilePath { get; }

        /// <summary>
        /// Gets the path for the target assembly's project file
        /// </summary>
        string ProjectDirectoryPath { get; }

        /// <summary>
        /// Gets the path to the Cilador.Fody addin assembly
        /// </summary>
        string AddinDirectoryPath { get; }

        /// <summary>
        /// Gets the path to the target assembly's solution file
        /// </summary>
        string SolutionDirectoryPath { get; }

        #endregion

        #region Logging

        /// <summary>
        /// Gets the logger that displays debug-level output.
        /// </summary>
        Action<string> LogDebug { get; }

        /// <summary>
        /// Gets the logger that displays info-level output. In Visual
        /// Studio, logged items appear in the "Error List" as Message items.
        /// </summary>
        Action<string> LogInfo { get; }

        /// <summary>
        /// Gets the logger that displays warning-level output. In Visual
        /// Studio, logged items appear in the "Error List" as Warning items.
        /// </summary>
        Action<string> LogWarning { get; }

        /// <summary>
        /// Gets the logger that displays warning-level output for a given
        /// CIL instruction's sequence point. In Visual
        /// Studio, logged items appear in the "Error List" as Warning items.
        /// </summary>
        Action<string, SequencePoint> LogWarningPoint { get; }

        /// <summary>
        /// Gets the logger that displays error-level output. In Visual
        /// Studio, logged items appear in the "Error List" as Error items.
        /// </summary>
        Action<string> LogError { get; }

        /// <summary>
        /// Gets the logger that displays error-level output for a given
        /// CIL instruction's sequence point. In Visual
        /// Studio, logged items appear in the "Error List" as Error items.
        /// </summary>
        Action<string, SequencePoint> LogErrorPoint { get; }

        #endregion

        #region Target Assembly Data

        /// <summary>
        /// Gets the object that can resolve type members.
        /// </summary>
        IMetadataResolver MetadataResolver { get; }

        /// <summary>
        /// Gets the object that can find and load assemblies.
        /// </summary>
        IAssemblyResolver AssemblyResolver { get; }

        /// <summary>
        /// Gets the <see cref="ModuleDefinition"/> for the target assembly.
        /// </summary>
        ModuleDefinition ModuleDefinition { get; }

        #endregion
    }
}
