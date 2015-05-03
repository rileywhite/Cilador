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

using Cilador.Fody.Config;
using Cilador.Fody.Core;
using Mono.Cecil;
using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using Mono.Cecil.Cil;

namespace Cilador.Fody.DtoProjector
{

    [Export(typeof(IWeave))]
    [ExportMetadata("AttributeType", typeof(DtoProjectorAttribute))]
    [ExportMetadata("ConfigType", typeof(DtoProjectorConfigType))]
    internal class DtoProjectorWeave : IWeave
    {
        private DtoProjectorConfigType config;
        /// <summary>
        /// Gets or sets the configuration for this weave.
        /// </summary>
        public DtoProjectorConfigType Config
        {
            get { return this.config; }
            private set
            {
                value = value ?? new DtoProjectorConfigType();
                value.DtoProjectorMap = value.DtoProjectorMap ?? new DtoProjectorMapType[0];
                this.config = value;
            }
        }

        /// <summary>
        /// Gets or sets whether this weave has been initialized.
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// Initializes the weave.
        /// </summary>
        /// <param name="weavingContext">Weaving context that is initializing the weave.</param>
        /// <param name="weaveConfig">Must be null for this type of weave.</param>
        public void Initialize(IWeavingContext weavingContext, WeaveConfigTypeBase weaveConfig)
        {
            this.IsInitialized = true;
        }

        /// <summary>
        /// Projects a DTO type as a nested type inside of the target type.
        /// </summary>
        /// <param name="weavingContext">Weaving context within which the weave is running.</param>
        /// <param name="target">Target type.</param>
        /// <param name="weaveAttribute">Attribute that decorated the target type.</param>
        public void Weave(IWeavingContext weavingContext, TypeDefinition target, CustomAttribute weaveAttribute)
        {
            TypeDefinition dtoType = new TypeDefinition(
                target.Namespace, "Dto",
                TypeAttributes.Class | TypeAttributes.NestedPublic,
                target.Module.Import(typeof(object)));
            target.NestedTypes.Add(dtoType);

            var dtoMemberAttributeType = target.Module.Import(typeof (DtoMemberAttribute)).Resolve();

            foreach (var property in target.Properties)
            {
                for (var i = property.CustomAttributes.Count - 1; i >= 0; --i)
                {
                    var attribute = property.CustomAttributes[i];
                    if (attribute.AttributeType.Resolve() == dtoMemberAttributeType)
                    {
                        property.CustomAttributes.RemoveAt(i);

                        var field = new FieldDefinition(property.Name, FieldAttributes.Public, property.PropertyType);
                        dtoType.Fields.Add(field);
                    }
                }
            }
        }
    }
}
