/***************************************************************************/
// Copyright 2013-2018 Riley White
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
using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics.Contracts;

namespace Cilador.Fody.Core
{
    /// <summary>
    /// Holds information from <see cref="ExportMetadataAttribute"/> items on <see cref="IWeave"/> MEF exports.
    /// </summary>
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class WeaveMeta : ExportAttribute, IWeaveMeta
    {
        public WeaveMeta(IDictionary<string, object> properties)
        {
            if (!properties.TryGetValue(nameof(AttributeType), out var attributeType))
            {
                throw new ArgumentException($"No value with key {nameof(AttributeType)} was supplied.", nameof(properties));
            }
            this.AttributeType = (Type)attributeType;

            if(properties.TryGetValue(nameof(ConfigType), out var configType))
            {
                this.ConfigType = (Type)configType;
            }
        }

        /// <summary>
        /// For a weaves exported through MEF, this identifies the custom attribute type
        /// that both indicates that the exported command should be invoked on the annotated type
        /// and contains any command arguments for the invocation. The type specified here must
        /// implemente <see cref="IWeaveAttribute"/>.
        /// </summary>
        public Type AttributeType { get; }

        /// <summary>
        /// For a weave exported through MEF, this identifies the configuration type
        /// that can be found within <see cref="CiladorConfigType.WeaveConfig"/>,
        /// which is part of the configuration information that is serialized and embedded
        /// within the FodyWeavers.xml configuraiton file. The type specified here must
        /// inherit from <see cref="WeaveConfigTypeBase"/>.
        /// </summary>
        public Type ConfigType { get; }
    }
}
