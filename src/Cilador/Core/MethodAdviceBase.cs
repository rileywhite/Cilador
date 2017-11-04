/***************************************************************************/
// Copyright 2013-2017 Riley White
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

using Cilador.Graph;
using System;

namespace Cilador.Core
{
    public abstract class MethodAdviceBase
    {
        public static string AdviseMethodName { get { return nameof(MethodAdviceBase.Advise); } }

        protected dynamic InvokeMethod()
        {
            return null;
        }

        protected virtual dynamic Advise()
        {
            try
            {
                this.OnBefore();
                var returnValue = this.InvokeMethod();
                this.OnAfter();
                return returnValue;
            }
            catch (Exception e)
            {
                this.OnException(e);
                throw;
            }
            finally
            {
                this.OnExit();
            }
        }

        protected virtual void OnBefore() { }

        protected virtual void OnAfter() { }

        protected virtual void OnException(Exception e) { }

        protected virtual void OnExit() { }
    }
}
