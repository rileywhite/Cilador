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
using System.Xml.Linq;
using System.Xml.Serialization;
using Mono.Cecil;

namespace Cilador.Fody.Core
{
    /// <summary>
    /// Extension methods closely associated with funcitonality in <see cref="Cilador.Fody.Core"/>.
    /// </summary>
    public static class CoreExtensions
    {
        /// <summary>
        /// Desrializes an object into the given type.
        /// </summary>
        /// <typeparam name="T">Type to deserialize</typeparam>
        /// <param name="xElement">XML element from which to deserialize</param>
        /// <returns>Deserialized element</returns>
        public static T FromXElement<T>(this XElement xElement)
        {
            return (T)xElement.FromXElement(typeof(T));
        }

        /// <summary>
        /// Desrializes an object into the given type.
        /// </summary>
        /// <param name="xElement">XML element from which to deserialize</param>
        /// <param name="objectType">Type to deserialize</param>
        /// <returns>Deserialized element</returns>
        public static object FromXElement(this XElement xElement, Type objectType)
        {
            Contract.Requires(objectType != null);

            using (var reader = xElement.CreateReader())
            {
                var xmlSerializer = new XmlSerializer(objectType);
                return xmlSerializer.Deserialize(reader);
            }
        }

        /// <summary>
        /// Gets the resolved <see cref="TypeDefinition"/> for a given <see cref="Type"/>.
        /// </summary>
        /// <param name="weavingContext">Context to use when resolving type information.</param>
        /// <param name="type">Type to resolve.</param>
        /// <returns>Resolved type.</returns>
        public static TypeDefinition GetTypeDefinition(this IWeavingContext weavingContext, Type type)
        {
            Contract.Requires(weavingContext != null);
            Contract.Requires(type != null);
            Contract.Ensures(Contract.Result<TypeDefinition>() != null);

            var assemblyDefinition = weavingContext.AssemblyResolver.Resolve(type.Assembly.FullName);
            if (assemblyDefinition == null)
            {
                throw new ArgumentException("Cannot resolve assembly for type: " + type.AssemblyQualifiedName, "type");
            }

            var typeDefinition = assemblyDefinition.MainModule.GetType(type.FullName);
            if(typeDefinition == null)
            {
                throw new ArgumentException("Cannot find type within resolved assembly: " + type.AssemblyQualifiedName, "type");
            }

            return typeDefinition;
        }

        /// <summary>
        /// Gets the resolved <see cref="TypeDefinition"/> for a given assembly qualified type name.
        /// </summary>
        /// <param name="weavingContext">Context to use when resolving type information.</param>
        /// <param name="assemblyQualifiedTypeName">Assembly qualified name of the type to resolve.</param>
        /// <returns>Resolved type.</returns>
        public static TypeDefinition GetTypeDefinition(this IWeavingContext weavingContext, string assemblyQualifiedTypeName)
        {
            Contract.Requires(weavingContext != null);
            Contract.Requires(assemblyQualifiedTypeName != null);
            Contract.Ensures(Contract.Result<TypeDefinition>() != null);

            var nameParts = assemblyQualifiedTypeName.Split(new[] { ',' }, 2, StringSplitOptions.None);
            if (nameParts.Length != 2)
            {
                throw new ArgumentException("Expected type name and assembly name separated by a comma: " + assemblyQualifiedTypeName, "assemblyQualifiedTypeName");
            }

            var assemblyDefinition = weavingContext.AssemblyResolver.Resolve(nameParts[1]);
            if (assemblyDefinition == null)
            {
                throw new ArgumentException("Cannot resolve assembly for type: " + assemblyQualifiedTypeName, "assemblyQualifiedTypeName");
            }

            var typeDefinition = assemblyDefinition.MainModule.GetType(nameParts[0]);
            if (typeDefinition == null)
            {
                throw new ArgumentException("Cannot find type within resolved assembly: " + assemblyQualifiedTypeName, "assemblyQualifiedTypeName");
            }

            return typeDefinition;
        }
    }
}
