using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Bix.Mixers.Fody.Core
{
    public sealed class WeavingException : Exception
    {
        public WeavingException()
            : base() { }

        public WeavingException(string message)
        : base(message){ }

        public WeavingException(string message, Exception innerException)
            : base(message, innerException) { }

        private WeavingException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
