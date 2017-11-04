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


using System.Reflection;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Cilador.Fody")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("30cfc55f-7af9-4fa2-9e9e-08c6078a2884")]

[assembly: AssemblyDescription(
@"Create your own custom, rich C# mixins with Cilador! Mixins are the perfect DRY solution for sharing code without abusing inheritance.

Supports:
Mixins containing fields, methods, properties, events, and nested types.
Generics mixins and mixin members, so long as the top-level mixin implementation is closed. (Members and nested types can be open.)
Public, private, protected, internal, and protected internal members.
Static members.
Custom attributes on members.
Virtual members.
Abstract nested types and abstract members within these nested types.
Generic nested types and generic members.
Parameterless constructors for mixin implementations.
Type initializers (i.e. static constructors) in mixin implementations

Unsupported:
Parameters on mixin implemenation constructors.
Unmanaged code calls (extern)
Security attributes
Mixins implementing multiple interfaces
Mixins with base types other than object
Value type mixins

Unhandled:
Naming collisions

Please consider this version of Cilador.Fody to be pre-release.")]
