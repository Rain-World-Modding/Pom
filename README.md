# Pom

Port of henpemaz' mighty PlacedObjectsManager to Downpour, with some additional ergonomics (such as attribute-usable enumfields).

**NEW**: Mod now comes with a second module, EffExt! This one allows you to add functionality to room effects, and should be much simpler to use.
## Usage

### POM

To add a new placed object, follow these steps:

- Create a class deriving from `Pom.Pom.ManagedData`. This class will hold all the automatically serialized data that will be attached to your object and changed when you click your object's handles and buttons in devinterface.
	- Each field you intend to use as a serialized data field, you denote with an appropriate `ManagedField` attribute (they are all nested types inside Pom.Pom). Float fields are denoted with `FloatField`, ExtEnum fields are with `ExtEnumField` etc.
- [optional] Create a class deriving from UpdatableAndDeletable, with a constructor of one of the following forms: `PlacedObject, Room`, `Room, PlacedObject`, `Room`. This will be instantiated in rooms where your placedobject is added. `data` of the `PlacedObject` passed to the constructor will always be of your type in previous step, you can cast to it to use that data. 
- [optional] Create a class deriving from `Pom.Pom.ManagedRepresentation`. Doing this allows you to (in theory) create your own custom visual representation in devUI, but is hardly ever needed. Use Pom.Pom.ManagedRepresentation by default.
- Use one of the following static functions found in `Pom.Pom` to register your object:

| Function	| Desctiption	|
| ---		| ---			|
| `RegisterManagedObject` `<UAD,DATA,REPR>` `(string key, bool singleInstance)` | Register a full object. Generic parameters are: your UpdatableAndDeletable type,  your `ManagedData` type, and your representation type (Pom.Pom.ManagedRepresentation for default repr). `key` is the name of your object, set `singleInstance` to `true` if you want only one of your `UAD` to be created per room. |
| `RegisterEmptyObjectType` `<DATA,REPR>` `(string key)` | Register an "empty" object that will not create an UpdatableAndDeletable. Generic parameters are:  your `ManagedData` type and your representation type (Pom.Pom.ManagedRepresentation for default repr). `key` is the name of your object. |

You're done! Your object will now appear in POM category of devtools objects tab.

### EffExt

To use EffExt, add these to your mod project.

This imports the EffExt namespace into your source file:

```cs
using EffExt;
```

This creates and registers an effect definition (a record describing what fields and controls your effect will have):

```cs

public void OnEnable() {

	//... somewhere inside your mod's OnEnable:
	EffectDefinitionBuilder builder = new EffectDefinitionBuilder("my_effect_name");

	builder
		.AddIntField("intfield", 0, 10, 5, "An integer") // this add integer cycle buttons
		.AddFloatField("floatfield", 0f, 10f, 0.1f, 1f, "A float") //this adds a float slider
		.AddBoolField("boolfield", true, "A bool") //this adds a bool flipper button
		.AddStringField("stringfield", "example_string", "A string") //this adds a text input field
		.SetUADFactory(MyEffectSpawner) //this takes a callback that allows you to spawn stuff into rooms where your effect is applied
		.SetEffectInitializer(MyInitializer) //this is similar to SetUADFactory, but doesn't add anything ro the room
		.SetCategory("POMEffectsExamples") //this sets a devtools menu category for your effect
		.Register(); //this is the last step of the builder. You must call it in order for your effect to be added.
	//...
}
```

This is an UAD factory used in the previous code block. After you pass it to `SetUADFactory`, it will be called every time a room loads with your effect in it. It can return null if the effect should not be added:

```cs
public UpdatableAndDeletable MyEffectSpawner(Room room, EffectExtraData data, bool firstTimeRealized) {
	return new MyEffectObject(room, data);
}
```

This is an effect initializer, much like an UADFactory, except it doesn't return anything (it's optional):

```cs
public void MyEffectSpawner(Room room, EffectExtraData data, bool firstTimeRealized) {
	Logger.LogWarning("ploo");
}
```

Finally, in your effect's entity class, initialized by the factory, you can use methods of EffectExtraData in order to retrieve your settings:

```cs
public class MyEffectObject : UpdatableAndDeletable {
	private EffectExtraData data;
	public MyEffectObject(Room room, EffectExtraData data) {
		this.data = data;
		//... constructor stuff
	}
	public override void Update(bool eu) {
		int i = data.GetInt("intfield");
		float f = data.GetFloat("floatfield");
		bool b = data.GetBool("boolfield");
		string s = data.GetString("stringfield");
	}
}
```

For a slighty more detailed example, check [Examples.cs source file](./EffExt/Examples.cs).