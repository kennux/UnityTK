# Prototypes

The prototypes system can be used to parse objects using a UnityTK serializer.
They were implemented as a substitute for unity prefabs / scriptableobjects and used mainly together with UnityECS.

Prototypes support polymorphism, custom types and collections.

## Example

C# Datamodel
---
```
namespace UnityTK.Examples.Prototypes
{
	public class TestPrototypeSpec : TestPrototype
	{
		public int testField;
	}

	public class TestPrototype : IPrototype
	{
		[PrototypeDataSerializable] // This makes types serializable
		public struct TestStruct
		{
			public int test;
		}
		
		[PrototypeDataSerializable] // This makes types serializable
		public class TestBase
		{
			public string baseStr;
		}
		
		[PrototypeDataSerializable] // This makes types serializable
		public class SpecializedClass : TestBase
		{
			public int lul;
		}

		public string id;
		public int someInt;
		public TestPrototype someOtherPrototype = null;
		public TestStruct _struct;
		public TestBase testBase;
		
		public List<TestBase> list;

		string IPrototype.identifier { get {return id;} set {id = value;} }
	}
}
```

PrototypesParser
----

The core class of the prototype system. It exposes the ability to parse prototypes.
It provides the ability to parse files in batch or one file at once. See the "Examples" section for usage.

XML
---
```
<PrototypeContainer Type="TestPrototype">
    <!-- Create prototype with identifier 'Test', this prototype is abstract and will not actually be created but instead just used as template! -->
    <Prototype Id="Test" Abstract="True">
        <!-- Set some data -->
        <someInt>123</someInt>
        <_struct>
            <test>321</test>
        </_struct>
        <testBase>
            <baseStr>test string</baseStr>
        </testBase>
    </Prototype>
    <!-- Create prototype with identifier 'Test2', deriving from 'Test' and with a special class (TestPrototypeSpec) -->
    <Prototype Id="Test2" Type="TestPrototypeSpec" Inherits="Test2">
        <testBase>
            <baseStr>test string</baseStr>
        </testBase>
        <_struct>
            <test>456</test> <!-- This will override the value inherited from Test! -->
        </_struct>
        <!-- This will change the type of testBase to a specialized type deriving from it -->
        <!-- Additionally this will provide a value for the field "lul", the field "baseStr" will be inherited from "Test" -->
        <testBase Type="SpecializedClass">
            <lul>879</lul> 
        </testBase>
    </Prototype>
    <Prototype Id="Test3">
        <someOtherPrototype>Test2</someOtherPrototype>
    </Prototype>
</PrototypeContainer>
```

C# Parsing example
---
```
var parser = new PrototypeParser();
parser.Parse(this.xml.text, "DIRECT PARSE", new PrototypeParseParameters()
{
	standardNamespace = "UnityTK.Examples.Prototypes"
});

var errors = parser.GetParsingErrors();
this.loadedPrototypes = parser.GetPrototypes().Cast<TestPrototype>().ToArray();

// this.loadedPrototypes will now contain 2 prototypes, the one previously created and deriving from "Test" ("Test2") and "Test3".
// this.loadedPrototypes[0].identifier == "Test2"
// this.loadedPrototypes[0]._struct.test == 456
// this.loadedPrototypes[0].testBase.baseStr == "test string"
// this.loadedPrototypes[1].someOtherPrototype == this.loadedPrototypes[0] <- Reference equality!
// ...

if (errors.Count == 0)
{
	Debug.Log("Parsed successfully " + loadedPrototypes.Length+ " objects!");
}
else
{
	foreach (var error in errors)
		error.DoUnityDebugLog();
}
```

# Deserialization

Deserialization works the same way as Prototypes do, just that you'll need to use the serialization API directly.

# Serialization

The UnityTK serialization system can be used to serialize and deserialize arbitrary objects at runtime to any arbitrary format (given, there's a serializer implementation for it).
A good starting point for working with the unity tk serializer are the unit tests [here](https://github.com/kennux/UnityTK/tree/master/Assets/UnityTK/Code/EditorCode/Tests/Serialization)
## Attributes

### UnityTKNonSerialized / System.NonSerialized

As the name suggests, fields with this attribute will not be serialized at all.
The UnityTKNonSerialized attribute exists because there can be cases where you want a field serialized by the UnityTK serializer but not the unity editor for example.
Both will be interpreted the same way.

### AlwaysSerialized

This attribute can be used when an object reference field referencing null should be explicitly serialized.
If this field is not set, the serializer will ignore fields with null as value.

# XML-Serializer

This is currently the only serialized UnityTK implements, I don't think unity will ever implement a different serializer but the API should be abstract enough to do so quite easily.


# Script reference

IXMLDataSerializer
---

This interface can be used to extend the xml serializer by adding a new data serializers.
Whenever the serializer comes across a field in xml, it will look for a serializer for the type.
If a serializer is found it will be called to deserialize the data.

When implementing new serializers, you only have to create the implementing class somewhere and thats it!
The prototypes cache will automatically find them via reflection and create an instance for them.

## Collections

Collections are supported to some extend.
I only tested List<T> and System.Array, but any collection implementing either ISet<>, ICollection<> or IList should work.

