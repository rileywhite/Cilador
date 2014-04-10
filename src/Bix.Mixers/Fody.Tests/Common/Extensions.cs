using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Bix.Mixers.Fody.Tests.Common
{
    internal static class Extensions
    {
        public static XElement ToXElement(this object @object)
        {
            Contract.Requires(@object != null);
            Contract.Requires(@object.GetType().IsSerializable);

            var objectType = @object.GetType();
            using (var memoryStream = new MemoryStream())
            {
                var xmlSerializer = new XmlSerializer(objectType);
                xmlSerializer.Serialize(memoryStream, @object);
                memoryStream.Position = 0;
                var xElement = XElement.Load(memoryStream);
                return xElement;
            }
        }
    }
}
