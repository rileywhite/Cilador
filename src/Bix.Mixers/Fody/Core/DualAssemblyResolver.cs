/***************************************************************************/
// Copyright 2013-2014 Riley White
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

using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.Core
{
    /// <summary>
    /// Special case of a composite assembly resolver that only contains two inner resolvers.
    /// </summary>
    internal class DualAssemblyResolver : IAssemblyResolver
    {
        /// <summary>
        /// Gets or sets the first reolver that is checked in resolve attempts.
        /// </summary>
        public IAssemblyResolver Resolver1 { get; set; }

        /// <summary>
        /// Gets or sets the second reolver that is checked in resolve attempts.
        /// </summary>
        public IAssemblyResolver Resolver2 { get; set; }

        /// <summary>
        /// Resolves an assembly
        /// </summary>
        /// <param name="fullName">Name of assembly to resolve.</param>
        /// <param name="parameters">Additional settings to use in resolving the assembly.</param>
        /// <returns>Resolved assembly.</returns>
        public AssemblyDefinition Resolve(string fullName, ReaderParameters parameters)
        {
            foreach (var resolver in new IAssemblyResolver[] { this.Resolver1, this.Resolver2 })
            {
                if (resolver != null)
                {
                    try
                    {
                        var assembly = resolver.Resolve(fullName, parameters);
                        if (assembly != null) { return assembly; }
                    }
                    catch { }
                }
            }

            return null;
        }

        /// <summary>
        /// Resolves an assembly
        /// </summary>
        /// <param name="fullName">Name of assembly to resolve.</param>
        /// <returns>Resolved assembly.</returns>
        public AssemblyDefinition Resolve(string fullName)
        {
            foreach (var resolver in new IAssemblyResolver[] { this.Resolver1, this.Resolver2 })
            {
                if (resolver != null)
                {
                    try
                    {
                        var assembly = resolver.Resolve(fullName);
                        if (assembly != null) { return assembly; }
                    }
                    catch { }
                }
            }

            return null;
        }

        /// <summary>
        /// Resolves an assembly
        /// </summary>
        /// <param name="name">Assembly name to resolve.</param>
        /// <param name="parameters">Additional settings to use in resolving the assembly.</param>
        /// <returns>Resolved assembly.</returns>
        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            foreach (var resolver in new IAssemblyResolver[] { this.Resolver1, this.Resolver2 })
            {
                if (resolver != null)
                {
                    try
                    {
                        var assembly = resolver.Resolve(name, parameters);
                        if (assembly != null) { return assembly; }
                    }
                    catch { }
                }
            }

            return null;
        }

        /// <summary>
        /// Resolves an assembly
        /// </summary>
        /// <param name="name">Assembly name to resolve.</param>
        /// <returns>Resolved assembly.</returns>
        public AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            foreach (var resolver in new IAssemblyResolver[] { this.Resolver1, this.Resolver2 })
            {
                if (resolver != null)
                {
                    try
                    {
                        var assembly = resolver.Resolve(name);
                        if (assembly != null) { return assembly; }
                    }
                    catch { }
                }
            }

            return null;
        }
    }
}
