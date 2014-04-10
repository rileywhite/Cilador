using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using MethodBase = System.Reflection.MethodBase;
using ParameterInfo = System.Reflection.ParameterInfo;
using PropertyInfo = System.Reflection.PropertyInfo;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using Mono.Collections.Generic;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Bix.Mixers.Fody.Core
{
    public static class CoreExtensions
    {
        public static T FromXElement<T>(this XElement xElement)
        {
            return (T)xElement.FromXElement(typeof(T));
        }

        public static object FromXElement(this XElement xElement, Type objectType)
        {
            Contract.Requires(objectType != null);

            using (var reader = xElement.CreateReader())
            {
                var xmlSerializer = new XmlSerializer(objectType);
                return xmlSerializer.Deserialize(reader);
            }
        }
    }
}
