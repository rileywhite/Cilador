/***************************************************************************/
// Copyright 2013-2015 Riley White
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
using System.Diagnostics.Contracts;
using Mono.Cecil;

namespace Bix.Mixers.Fody.Core
{
    /// <summary>
    /// Special case of a composite assembly resolver that only contains two inner resolvers.
    /// </summary>
    internal class DualAssemblyResolver : IAssemblyResolver
    {
        /// <summary>
        /// Creates a new <see cref="DualAssemblyResolver"/>.
        /// </summary>
        /// <param name="weavingContext">Weaving context to use in assembly resolution.</param>
        public DualAssemblyResolver(IWeavingContext weavingContext)
        {
            Contract.Requires(weavingContext != null);
            Contract.Ensures(this.WeavingContext != null);

            this.WeavingContext = weavingContext;
        }

        /// <summary>
        /// Gets or sets the weaving context to use in item resolution
        /// </summary>
        private IWeavingContext WeavingContext { get; set; }

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
            foreach (var resolver in new[] { this.Resolver1, this.Resolver2 })
            {
                if (resolver != null)
                {
                    try
                    {
                        var assembly = resolver.Resolve(fullName, parameters);
                        if (assembly != null) { return assembly; }
                    }
                    catch { /* Fail silently */ }
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
            return this.Resolve(fullName, new ReaderParameters { AssemblyResolver = this, MetadataResolver = this.WeavingContext.MetadataResolver });
        }

        /// <summary>
        /// Resolves an assembly
        /// </summary>
        /// <param name="name">Assembly name to resolve.</param>
        /// <param name="parameters">Additional settings to use in resolving the assembly.</param>
        /// <returns>Resolved assembly.</returns>
        public AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            foreach (var resolver in new[] { this.Resolver1, this.Resolver2 })
            {
                if (resolver != null)
                {
                    try
                    {
                        var assembly = resolver.Resolve(name, parameters);
                        if (assembly != null) { return assembly; }
                    }
                    catch { /* Fail silently */ }
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
            return this.Resolve(name, new ReaderParameters { AssemblyResolver = this, MetadataResolver = this.WeavingContext.MetadataResolver });
        }
    }
}
