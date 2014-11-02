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

namespace Bix.Mixers.Fody.Core
{
    /// <summary>
    /// Raised when an error caused by misconfiguration occurs that should
    /// stop the build. Fody provides the error information in the output
    /// without a full stack trace, so enough information should be provided
    /// in the message to allow the user to correct the problem.
    /// </summary>
    /// <remarks>
    /// This should only be thrown for errors whose cause is known, such as
    /// for unsupported behavior. It should not be used a a catchall exception
    /// since it suppresses the stack trace from being displayed the user and,
    /// therefore, from being passed on in a bug report.
    /// </remarks>
    [Serializable]
    public sealed class WeavingException : Exception
    {
        /// <summary>
        /// Creates a new <see cref="WeavingException"/>
        /// </summary>
        public WeavingException()
            : base() { }

        /// <summary>
        /// Creates a new <see cref="WeavingException"/> with the given message
        /// </summary>
        /// <param name="message">Error message for display to the user</param>
        public WeavingException(string message)
        : base(message){ }

        /// <summary>
        /// Creates a new <see cref="WeavingException"/> with the given message
        /// and inner exception
        /// </summary>
        /// <param name="message">Error message for display to the user</param>
        /// <param name="innerException">Original cause of the exception</param>
        public WeavingException(string message, Exception innerException)
            : base(message, innerException) { }

        /// <summary>
        /// Deserializes a <see cref="WeavingException"/>
        /// </summary>
        /// <param name="info">Deserialization info</param>
        /// <param name="context">Deserialization context</param>
        private WeavingException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }
}
