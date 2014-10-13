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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Bix.Mixers.Fody.TestMixinInterfaces
{
    [Serializable]
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public sealed class TestMixedAttribute : Attribute, ISerializable, IXmlSerializable
    {
        public TestMixedAttribute() { }

        public TestMixedAttribute(int constructorArgument)
        {
            this.ConstructorArgument = constructorArgument;
        }

        private TestMixedAttribute(SerializationInfo info, StreamingContext context)
        {
            this.ConstructorArgument = info.GetInt32("ConstructorArgument");
            this.NamedArgument = info.GetInt32("NamedArgument");
            var typeName = info.GetString("NamedTypeArgument");
            if(typeName != null)
            {
                this.NamedTypeArgument = Type.GetType(typeName);
            }
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("ConstructorArgument", this.ConstructorArgument);
            info.AddValue("NamedArgument", this.NamedArgument);
            info.AddValue("NamedTypeArgument", this.NamedTypeArgument == null ? null : this.NamedTypeArgument.AssemblyQualifiedName);
        }

        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            writer.WriteElementString("ConstructorArgument", this.ConstructorArgument.ToString());
            writer.WriteElementString("NamedArgument", this.NamedArgument.ToString());
            writer.WriteElementString("NamedTypeArgument", this.NamedTypeArgument == null ? string.Empty : this.NamedTypeArgument.AssemblyQualifiedName);
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            if (!reader.Read()) { return; }
            this.ConstructorArgument = int.Parse(reader.Value);
            if (!reader.Read()) { return; }
            this.NamedArgument = int.Parse(reader.Value);
            if (!reader.Read()) { return; }
            if(!string.IsNullOrWhiteSpace(reader.Value))
            {
                this.NamedTypeArgument = Type.GetType(reader.Value);
            }
        }

        public int ConstructorArgument { get; set; }

        public int NamedArgument { get; set; }

        public Type NamedTypeArgument { get; set; }
    }
}
