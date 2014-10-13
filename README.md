#Welcome to Bix.Mixers!

Bix.Mixers is a [Fody](http://github.com/Fody/Fody) plugin, available through
[NuGet](https://www.nuget.org/packages/Bix.Mixers.Fody/), that provides the
ability to write custom mixins for C#.

What does this mean? I'm glad you asked.

#Concept

"Mixin" is a term that means different things to different people. For the purposes of this project,
the term denotes a behavior that is added to potentially unrelated types without using inheritence.

There are two parts to a Bix.Mixers mixin.

1. Mixin Definition: C# interface that provides a way to identify the mixin and that provides the
minimum set of properties, methods, and events that will be added to a mixin target type.

2. Mixin Implementation: C# class that implements a mixin definition interface. With the exception
of constructors and field initial values, the entire contents of a mixin definition will be added
to a mixin target type.

#Usage
See [Bix.Mixers.Fody-Examples](http://github.com/rileywhite/Bix.Mixers.Fody-Examples)
for working examples. Read on for a short introduction.

##Create a mixin definition interface in the assembly that will contain mixin definitions.

    namespace MyMixinDefinitions
    {
        public interface IHelloWorld
        {
            string Hello();
        }
    }

There is nothing special about this interface except that it cannot inherit from another interface.
You can even choose an existing interface from your own code, the .NET framework, or a third party
library if you like.

##In a different assembly, create a mixin implementation class that implements the mixin definition interface.

    namespace MyMixins
    {
        public class HelloMixin : IHelloWorld
        {
            public string Hello()
            {
                return "Hello World";
            }
        }
    }

You have to follow a few rules for the implementation, but it's pretty basic.

 * Create a non-generic `class` type.
 * Implement the mixin interface and only the mixin interface.
 * Do not inherit from another type.
 * Do not add any constructors.

Outside of that, you can do what you want. You can include fields, properties, methods,
events, and even nested types. Methods and nested types can even be generic. Visibility
will transfer over to the mixin target, as will custom attributes, and virtual/abstract
modifiers (only nested types can be abstract, though).

There are some unsupported things. Although methods and nested types can be generic,
the implementation class cannot be generic, for example, but most things will work.
Experiment a bit, and [create an issue](https://github.com/rileywhite/Bix.Mixers.Fody/issues)
if something you want is missing.

The mixin implementation assembly must reference the mixin definition assembly since the definition
interface must be implemented.

##Add an InterfaceMixinAttribute to your target type in a third assembly. Specify the mixin definition interface in the attribute constructor.

    namespace MyApplicationLibrary
    {
        [InterfaceMixin(typeof(IHelloWorld))]
        public class Target
        {
        }
    }

This is the assembly that should contain types used by your application. It must reference
the mixin definition assembly, but it need not reference the mixin implementation assembly.

Types inside this assembly cannot use the mixin without casting or reflection, but types
outside of this assembly can use the mixin functionality. Intellisense doesn't work, unfortunately,
and Visual Studio may complain, but when it comes time to compile, it'll all work.

##Configure mixins for the target type assembly.

Add NuGet packages for Fody and Bix.Mixers.Fody to the target type assembly. Modify FodyWeavers.xml to map the mixin definition
to the mixin implementation.

    <?xml version="1.0" encoding="utf-8" ?>
    <Weavers>
      <Bix.Mixers>
        <BixMixersConfig xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns="urn:Bix:Mixers:Fody:Core" xmlns:bmfim="urn:Bix:Mixers:Fody:InterfaceMixins">
          <MixCommandConfig xsi:type="bmfim:InterfaceMixinConfigType" xmlns="">
            <InterfaceMap Interface="MyMixinDefinitions.IHelloWorld, MyMixinDefinitions" Mixin="MyMixins.HelloMixin, MyMixins" />
          </MixCommandConfig>
        </BixMixersConfig>
      </Bix.Mixers>
    </Weavers>

You can consider everything but the `InterfaceMap` elements to be boilerplate.

Include an `InterfaceMap` element specifying each mixin definition `Interface`. The type you supply for the `Mixin`
will be used to implement the interface in each mixin target. Use the assembly qualified names of all types. In
the simplest case, that's just `<My.Namespaces.TypeName>, <AssemblyFilenameWithoutExtension>`, but it can get
[more complex](http://msdn.microsoft.com/en-us/library/k8xx4k69.aspx).

##Make sure Fody can find your mixin implementations.

While the mixin definition assembly must be referenced by the target project, the implementation assembly may not be.
If this is the case, then you'll need to put the assembly into a location where it can be found by Bix.Mixers.

Create a Tools directory in your Visual Studio solution directory. Add the following command to your mixin
implementation project post-build action.

    copy "$(TargetPath)" "$(SolutionDir)Tools"

You may need to manually change the build order of the projects in your solution to ensure that the file exists
before the target assembly is compiled.

Alternatively, you can copy all assembly files manually; however, this may affect compilation if all assembly
projects are in the same solution.

##Compile and test

Code in a fourth assembly that references the target assembly and mixin definition assembly can now call mixed in code.

    namespace MyApplication
    {
        internal class Program
        {
            public static void Main(string[] args)
            {
                Console.WriteLine(new Target().Hello());
            }
        }
    }


#Additional Information
Bix.Mixers.Fody was originally developed as part of [Bix](https://github.com/rileywhite/Bix).
Because it is doing something outside of the scope of Bix, it has been made independent. As Bix needs new
code weaving functionality, Bix.Mixers.Fody will be expanded to provide it.

I may [blog](http://statisticsandlies.com/tags/bix) about Bix and Bix.Mixers.Fody from time to time.
