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

using System.Reflection;

[assembly: AssemblyCompany("Riley White")]
[assembly: AssemblyProduct("Bix.Mixers.Fody")]
[assembly: AssemblyCopyright("Copyright © Riley White 2013-2014")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyVersion(CommonAssemblyInfo.Version)]
[assembly: AssemblyFileVersion(CommonAssemblyInfo.Version)]

[assembly: AssemblyDescription(
@"Create your own custom, rich C# mixins with Bix.Mixers! Mixins are the perfect DRY solution for sharing code without abusing inheritance.

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
Naming collisions")]

internal static class CommonAssemblyInfo
{
    public const string Version = "0.1.5.0";
}
