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

using Cilador.Fody.TestMixinInterfaces;
using System;
using System.Runtime.Serialization;

namespace Cilador.Fody.TestMixins
{
    using Cilador.Fody.TestMixinInterfaces;

    public class TryCatchFinallyMixin : IEmptyInterface
    {
        public bool FieldSetBeforeTryBlock;
        public bool FieldSetBeforeThrowInTryBlock;
        public bool FieldSetAfterThrowInTryBlock;
        public bool FieldSetInApplicationExceptionCatchBlock;
        public bool FieldSetInMyExceptionCatchBlock;
        public bool FieldSetInExceptionCatchBlock;
        public Type TypeOfCaughtException;
        public bool FieldSetInFinallyBlock;
        public bool FieldSetAfterFinallyBlock;

        public void CatchExceptionWithFinallyBlock()
        {
            this.FieldSetBeforeTryBlock = true;

            try
            {
                this.FieldSetBeforeThrowInTryBlock = true;
                throw new MyException();

#pragma warning disable 162
                this.FieldSetAfterThrowInTryBlock = true;
#pragma warning restore 162
            }
            catch(ApplicationException ae)
            {
                this.FieldSetInApplicationExceptionCatchBlock = true;
                this.TypeOfCaughtException = ae.GetType();
            }
            catch (MyException me)
            {
                this.FieldSetInMyExceptionCatchBlock = true;
                this.TypeOfCaughtException = me.GetType();
            }
            catch (Exception e)
            {
                this.FieldSetInExceptionCatchBlock = true;
                this.TypeOfCaughtException = e.GetType();
            }
            finally
            {
                this.FieldSetInFinallyBlock = true;
            }

            this.FieldSetAfterFinallyBlock = true;
        }

        public class MyException : Exception
        {
            public MyException() { }
            public MyException(string message) : base(message) { }
            public MyException(string message, Exception innerException) : base(message, innerException) { }
            public MyException(SerializationInfo info, StreamingContext context) : base(info, context) { }
        }
    }
}
