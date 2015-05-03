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

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable NotAccessedField.Local

using Cilador.Fody.DtoProjector;
using System;

namespace Cilador.Fody.TestDtoProjectorTargets
{
    [DtoProjector]
    public class Properties
    {
        #region DtoMember

        [DtoMember]
        public int PublicGetPublicSet { get; set; }
        [DtoMember]
        public int PublicGetPrivateSet { get; private set; }
        [DtoMember]
        public int PublicGetInternalSet { get; internal set; }
        [DtoMember]
        public int PublicGetProtectedSet { get; protected set; }
        [DtoMember]
        public int PublicGetProtectedInternalSet { get; protected internal set; }

        [DtoMember]
        public int PrivateGetPublicSet { private get; set; }
        [DtoMember]
        private int PrivateGetPrivateSet { get; set; }
        [DtoMember]
        internal int PrivateGetInternalSet { private get; set; }
        [DtoMember]
        protected int PrivateGetProtectedSet { private get; set; }
        [DtoMember]
        protected internal int PrivateGetProtectedInternalSet { private get; set; }

        [DtoMember]
        public int InternalGetPublicSet { internal get; set; }
        [DtoMember]
        internal int InternalGetPrivateSet { get; private set; }
        [DtoMember]
        internal int InternalGetInternalSet { get; set; }
        [DtoMember]
        protected internal int InternalGetProtectedInternalSet { internal get; set; }

        [DtoMember]
        public int ProtectedGetPublicSet { protected get; set; }
        [DtoMember]
        protected int ProtectedGetPrivateSet { get; private set; }
        [DtoMember]
        protected int ProtectedGetProtectedSet { get; set; }
        [DtoMember]
        protected internal int ProtectedGetProtectedInternalSet { protected get; set; }

        [DtoMember]
        public int ProtectedInternalGetPublicSet { protected internal get; set; }
        [DtoMember]
        protected internal int ProtectedInternalGetPrivateSet { get; private set; }
        [DtoMember]
        protected internal int ProtectedInternalGetInternalSet { get; internal set; }
        [DtoMember]
        protected internal int ProtectedInternalGetProtectedSet { get; protected set; }
        [DtoMember]
        protected internal int ProtectedInternalGetProtectedInternalSet { get; set; }

#pragma warning disable 0649

        private int publicGet;
        [DtoMember]
        public int PublicGet { get { return this.publicGet; } }

        private int privateGet;
        [DtoMember]
        private int PrivateGet { get { return this.privateGet; } }

        private int internalGet;
        [DtoMember]
        internal int InternalGet { get { return this.internalGet; } }

        private int protectedGet;
        [DtoMember]
        protected int ProtectedGet { get { return this.protectedGet; } }

        private int protectedInternalGet;
        [DtoMember]
        protected internal int ProtectedInternalGet { get { return this.protectedInternalGet; } }

        private int publicSet;
        [DtoMember]
        public int PublicSet { set { this.publicSet = value; } }

        private int privateSet;
        [DtoMember]
        private int PrivateSet { set { this.privateSet = value; } }

        private int internalSet;
        [DtoMember]
        internal int InternalSet { set { this.internalSet = value; } }

        private int protectedSet;
        [DtoMember]
        protected int ProtectedSet { set { this.protectedSet = value; } }

        private int protectedInternalSet;
        [DtoMember]
        protected internal int ProtectedInternalSet { set { this.protectedInternalSet = value; } }

        #endregion

        #region Not DtoMember

        public int PublicGetPublicSetNotDtoMember { get; set; }
        public int PublicGetPrivateSetNotDtoMember { get; private set; }
        public int PublicGetInternalSetNotDtoMember { get; internal set; }
        public int PublicGetProtectedSetNotDtoMember { get; protected set; }
        public int PublicGetProtectedInternalSetNotDtoMember { get; protected internal set; }

        public int PrivateGetPublicSetNotDtoMember { private get; set; }
        private int PrivateGetPrivateSetNotDtoMember { get; set; }
        internal int PrivateGetInternalSetNotDtoMember { private get; set; }
        protected int PrivateGetProtectedSetNotDtoMember { private get; set; }
        protected internal int PrivateGetProtectedInternalSetNotDtoMember { private get; set; }

        public int InternalGetPublicSetNotDtoMember { internal get; set; }
        internal int InternalGetPrivateSetNotDtoMember { get; private set; }
        internal int InternalGetInternalSetNotDtoMember { get; set; }
        protected internal int InternalGetProtectedInternalSetNotDtoMember { internal get; set; }

        public int ProtectedGetPublicSetNotDtoMember { protected get; set; }
        protected int ProtectedGetPrivateSetNotDtoMember { get; private set; }
        protected int ProtectedGetProtectedSetNotDtoMember { get; set; }
        protected internal int ProtectedGetProtectedInternalSetNotDtoMember { protected get; set; }

        public int ProtectedInternalGetPublicSetNotDtoMember { protected internal get; set; }
        protected internal int ProtectedInternalGetPrivateSetNotDtoMember { get; private set; }
        protected internal int ProtectedInternalGetInternalSetNotDtoMember { get; internal set; }
        protected internal int ProtectedInternalGetProtectedSetNotDtoMember { get; protected set; }
        protected internal int ProtectedInternalGetProtectedInternalSetNotDtoMember { get; set; }

        private int publicGetNotDtoMember;
        public int PublicGetNotDtoMember { get { return this.publicGetNotDtoMember; } }

        private int privateGetNotDtoMember;
        private int PrivateGetNotDtoMember { get { return this.privateGetNotDtoMember; } }

        private int internalGetNotDtoMember;
        internal int InternalGetNotDtoMember { get { return this.internalGetNotDtoMember; } }

        private int protectedGetNotDtoMember;
        protected int ProtectedGetNotDtoMember { get { return this.protectedGetNotDtoMember; } }

        private int protectedInternalGetNotDtoMember;
        protected internal int ProtectedInternalGetNotDtoMember { get { return this.protectedInternalGetNotDtoMember; } }

        private int publicSetNotDtoMember;
        public int PublicSetNotDtoMember { set { this.publicSetNotDtoMember = value; } }

        private int privateSetNotDtoMember;
        private int PrivateSetNotDtoMember { set { this.privateSetNotDtoMember = value; } }

        private int internalSetNotDtoMember;
        internal int InternalSetNotDtoMember { set { this.internalSetNotDtoMember = value; } }

        private int protectedSetNotDtoMember;
        protected int ProtectedSetNotDtoMember { set { this.protectedSetNotDtoMember = value; } }

        private int protectedInternalSetNotDtoMember;
        protected internal int ProtectedInternalSetNotDtoMember { set { this.protectedInternalSetNotDtoMember = value; } }

        #endregion
    }
}
