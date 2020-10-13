<img src="patcheetah.png" height="100" alt="SimplePatch">
A flexible, multi-platform and extensible solution for implementing partial patching in REST API.


[![Nuget](https://img.shields.io/nuget/v/Patcheetah.JsonNET.svg)](https://www.nuget.org/packages/Patcheetah.JsonNET) - [Patcheetah.JsonNET](https://www.nuget.org/packages/Patcheetah.JsonNET) | .NET Core 2.1/3.1 | .NET Framework 4.5+

[![Nuget](https://img.shields.io/nuget/v/Patcheetah.SystemText.svg)](https://www.nuget.org/packages/Patcheetah.SystemText) - [Patcheetah.SystemText](https://www.nuget.org/packages/Patcheetah.SystemText) | .NET Core 3.1 

[![Nuget](https://img.shields.io/nuget/v/Patcheetah.Swagger.NET21.svg)](https://www.nuget.org/packages/Patcheetah.Swagger.NET21) - [Patcheetah.Swagger.NET21](https://www.nuget.org/packages/Patcheetah.Swagger.NET21) | Swagger support for .NET Core 2.1 

[![Nuget](https://img.shields.io/nuget/v/Patcheetah.Swagger.NET31.svg)](https://www.nuget.org/packages/Patcheetah.Swagger.NET31) - [Patcheetah.Swagger.NET31](https://www.nuget.org/packages/Patcheetah.Swagger.NET31) | Swagger support for .NET Core 3.1 


## Summary
- [Quick Start](#quick-start)
- [About](#about)
- [Usage](#usage)
   - [Initialization](#initialization)
   - [Patching](#patching)
- [Configuration](#configuration)
   - [Case sensitivity](#case-sensitivity)
   - [Nested patching (RFC7396)](#nested-patching-rfc7396)
   - [Required properties](#required-properties)
   - [Ignored properties](#ignored-properties)
   - [Property aliases](#property-aliases)
   - [Key properties and Upsert methods](#key-properties-and-upsert-methods)
   - [Attributes](#attributes)
   - [Mapping](#mapping)
     - [Global mapping](#global-mapping)  
     - [Specific type mapping](#specific-type-mapping)
     - [Single property mapping](#single-property-mapping)
     - [Mapping execution order](#mapping-execution-order)
- [Extensions](#extensions)
   - [Intervention into property patching](#intervention-into-property-patching)
   - [Custom attributes](#custom-attributes)
- [Swagger support](#swagger-support)
     
## Quick start

For using with JSON.NET (Newtonsoft.Json) (.NET Core 2.1/3.1 | .NET Framework 4.5+):

    PM> Install-Package Patcheetah.JsonNET

For using with System.Text.Json (.NET Core 3.1):

    PM> Install-Package Patcheetah.SystemText

Then add in your `Startup.cs` or `Global.asax` (or another place where your application starts) the next call:

    PatchEngine.Init();
    
You're breathtaking!

## About

The ultimate extendable solution for partial patching via REST APIs.

Why Patcheetah?

- Really easy start. Only one parameterless call to start working;

- Support for .NET Core and .NET Framework;

- Support for JSON.Net or System.Text.Json as parsing engines;

- Support for RFC7396 json-merge-patch standard;

- Useful helpers out of the box;

- Swagger support;

- Extensibility.

  

## Usage

  

#### Initialization

You don't need any special configurations tofor start to use Patcheetah. All that you need is `PatchEngine.Init()`; call somewhere in `Startup.cs` or `Global.asax`.

#### Patching
```C#
[HttpPatch("{id}")]
public async Task<IActionResult> PatchOne(PatchObject<User> patch)
{
	var userToPatch = await _usersProvider.TryGet(patch.GetValue(x => x.Id));

	if (userToPatch == null)
	{
		return BadRequest("User not found");
	} 

	patch.ApplyTo(userToPatch);

	return Ok(userToPatch);
}
```
## Configuration

Let's talk about the default configuration first.

The default settings are as follows:

 - Case insensitive;
 - Patching "as is". Nested patching is disabled;
 - Attributes are disabled.
 
To configure patch engine use parametrised overload of default method `PatchEngine.Init()`.
```C#
PatchEngine.Init(config =>
{
    config.SetCaseSensitivity(true);
    config
        .ConfigureEntity<User>()
		.Required(x => x.LastSeenFrom)
		.IgnoreOnPatching(x => x.Login)
		.UseJsonAlias(x => x.PersonalInfo, "Personal")
		.SetKey(x => x.Id);
});
 ```
### Case sensitivity
In Patcheetah you have two levels of case sensitivity configuration. 
The first level is global case sensitivity. 
```C#
PatchEngine.Init(config =>
{
	config.SetCaseSensitivity(true);
});
```
Keep in mind that global parameter has lower priority than the same single entity parameter. 
The second level is a case sensistivity of single entity. It can be setup during entity configuration method calling. Just pass bool value as parameter.
```C#
PatchEngine.Init(config =>
{
	config.ConfigureEntity<User>(true);
});
```
### Nested patching (RFC7396)
Patcheetah supports nested patching according to RFC7396 standard.
You can read more about that standard here.
To enable nested patching just use `EnableNestedPatching()` method in patch config.
```C#
PatchEngine.Init(config =>
{
	config.EnableNestedPatching();
});
```
**If nested property type has been configured, nested patching performs according to config rules.**

### Required properties
If you want to make some properties required in patch request, you can use `Required(propExpression)` method which is available in the entity's configuration method chain. If required property missed in patch request, `RequiredPropertiesMissedException` throws.
```C#
PatchEngine.Init(config =>
{
	config
		.ConfigureEntity<User>()
		.Required(x => x.LastSeenFrom);
});
```
### Ignored properties
Some properties can be skipped during patching. Even if the request contains them, they're ignored. To enable this feature use `IgnoreOnPatching(propExpression)` method which is available in the entity's configuration method chain.
```C#
PatchEngine.Init(config =>
{
	config
		.ConfigureEntity<User>()
		.IgnoreOnPatching(x => x.Login);
});
```
### Property aliases
Sometimes we need to do a trick with json property names substitution. E.g. Json.NET (Newtonsoft.Json) has an attribute `JsonProperty(propAlias)` that allows to use different names in json object and serialized C# object. Patcheetah has a special method for you to do the same.
```C#
PatchEngine.Init(config =>
{
	config
		.ConfigureEntity<User>()
		.UseJsonAlias(x => x.PersonalInfo, "Personal");
});
```
### Key properties and Upsert methods
Key properties is not an necessary killer-feature, but useful helper. If you use classic standard approach of patching that looks like this
```C#
[HttpPatch("{id}")]
public IActionResult Patch([FromRoute] string id, PatchObject<User> patch)
{
	// ...
}
```
maybe you don't need this feature. But if instead you're using the Upsert approach, which is incredibly handy for your small home projects, you absolutely need this feature. 
In most cases, our entities have an Id (more precisely, a key). You can set a key property to use it in Upsert methods or other appropriate situations.
```C#
PatchEngine.Init(config =>
{
	config
		.ConfigureEntity<User>()
		.SetKey(x => x.Id);
});
```
Then in Upsert, you can use the `HasKey` property to determine if your patch request has a key or not.
```C#
[HttpPatch("{id}")]
public IActionResult Patch(PatchObject<User> patch)
{
	if (patch.HasKey)
	{
		var entityToPatch = _usersService.Get(patch.GetValue(x => x.Id);
		patch.ApplyTo(entityToPatch);
		return Ok();
	}
	var newUser = entityToPatch.CreateNewEntity(Guid.NewGuid().ToString());
	_usersService.Create(newUser);
	return Ok();
}
```
The `HasKey` property returns false if the property specified as a key has no value in the patch request. If so, you can use the `CreateNewEntity(keyPropertyValue)` method to create the entity from scratch and populate all the fields contained in the patch request. You can also set the value of the key property of the new entity by passing it as a parameter to this method. Be aware that if a patch request has this property value, it is overwritten. If you don't need to set the key, use parameterless overload `CreateNewEntity()`.

### Attributes
Attributes is the one of the most funny and cozy features in Patcheetah. Instead of the above methods, you can customize your entities by simply placing attributes above their properties. **You even don't need to call `config.ConfigureEntity<User>()`!** Let's take a look.

- `[CaseSensitivePatching]` (set above class definition) is analogous to `ConfigureEntity<TEntity>(true)`;

- `[IgnoreOnPatching]` (or you can use `JsonIgnore` in Patcheetah.JsonNET) is analogous to `IgnoreOnPatching(propExpression)`;

- `[JsonAlias]` (or you can use `JsonProperty(property)` in Patcheetah.JsonNET) analogous to `UseJsonAlias(propExpression, alias)`;

- `[PatchingKey]` is analogous to `SetKey(propExpression)`;

- `[RequiredOnPatching]` is analogous to `Required(propExpression)`;

You can configure your entities by fluent config methods and attributes at one time, **but keep in mind that attributes has higher priority than rules which set by methods**.

**But that's not all. Patcheetah allows to create your own attributes.** Find out more about it here.

### Mapping
Sometimes it's not enough to just patch. What if you need to dynamically change the prop value according to some conditions? E.g. round a number after the decimal point to a certain precision? Patcheetah allows to do that using three-levels mapping.

 - Global mapping;
 - Mapping for specific type;
 - Single property mapping.

##### Global mapping
```C#
PatchEngine.Init(config =>
{
	config.SetGlobalMapping(val => 
	{
		if (val is double doubleVal)
		{
			var rounded = Math.Round(doubleVal, 2);
			return MappingResult.MapTo(rounded);
		}
		return MappingResult.Skip(val);
	});
});
```
 Be aware that global mapping executes every property patch case.
 
 ##### Specific type mapping
```C#
PatchEngine.Init(config =>
{
	config.SetMappingForType<double>(val => 
	{
		var rounded = Math.Round(val , 2);
		return MappingResult.MapTo(rounded);
	});
});
```
##### Single property mapping
```C#
PatchEngine.Init(config =>
{
	config
		.ConfigureEntity<Product>()
		.UseMapping(x => x.Price, val => 
		{
			var rounded = Math.Round(val, 2);
			return MappingResult.MapTo(rounded);
		});
});
```
##### Mapping execution order
Global mapping => Specific type mapping => Single entity mapping

## Extensions
If you need to extend the patch (for example, do some value manipulation depending on the configuration before starting the mapping, or add your own attributes, or call some side methods on property patching), you can use Patcheetah's extensibility functionality. But keep in mind that Patcheetah is not responsible for anything you do in this case!

### Intervention into property patching
At first we need to understand how patching works in Patcheetah. There is essential steps of patching.

If property isn't configured:
 - **Executing PrePatchProcessingFunction**;
 - Property patching;

If property is configured:
 - Resolving all config instructions;
 - **Executing PrePatchProcessingFunction**;
 - Constructing new nested object value if needed;
 - Mapping;
 - Property patching;
 
As you can see, we have a special place for intervention into patch process. Let's take a closer look.
`PrePatchProcessingFunction` is similar with mapping, but it calls earlier and has access to propery config, old property value and patched entity. You can look at this as an advanced version of the mapping.
```C#
PatchEngine.Init(config =>
{
	config
		.SetPrePatchProcessingFunction(context =>
		{
			if (context.Entity is AuctionLot && context.PropertyConfiguration.Name == "Price")
			{

				if ((decimal)context.NewValue < (decimal)context.OldValue)
				{
					throw new Exception("New auction lot price can't be less than previous!");
				}

				return context.NewValue;
			}
		});
});
```
`PrePatchProcessingFunction` accepts the `PatchContext` parameter that has `NewValue`, `OldValue`, `Entity` and `PropertyConfiguration` fields. Returned value of this method should be a new value for property (modified or not).

`PrePatchProcessingFunction` was designed for custom attributes logic execution, but you can use it however you want. **But be careful, overweight of this method can create performance problems, and careless handling can damage your data. Use this functionality wisely!**

       
### Custom attributes
If you want to add your own attributes with its own behavior, you should use `ICustomAttributesConfigurator` mechanic.
Steps:

 - Create your own implementation of  `ICustomAttributesConfigurator`;
 - Register it using `SetCustomAttributesConfigurator(yourImpl)` method of patch engine config;
 - Read your attributes and do some actions via `PrePatchProcessingFunction`.

In one of previous examples we rounded digits using mapping. But what if we need to round values ​​in several properties of different entities? Let's create an attribute that will do this.
```C#
[AttributeUsage(AttributeTargets.Property)]
public RoundValueAttribute : Attribute
{
	public const string PARAMETER_NAME = "RoundValue";
	public int Precision { get; }

	public RoundValueAttribute(int precision)
	{
		Precision = precision;
	}
}
```
Then we need to create an implementation of `ICustomAttributesConfigurator`. It's a special interface with only one method Configure that will be called on property configuring process.
```C#
public class CustomAttributesConfigurator : ICustomAttributesConfigurator
{
	public void Configure(PropertyInfo property, IEnumerable<Attribute> propertyAttributes, EntityConfigAccessor configAccessor)
	{
		var roundAttribute = propertyAttributes.FirstOrDefault(attr => attr is RoundValueAttribute);
		if (roundAttribute != null)
		{
			configAccessor
				.GetPropertyConfiguration(property.Name)
				.ExtraSettings
				.Add(RoundValueAttribute.PARAMETER_NAME, (roundAttribute as RoundValueAttribute).Precision);
		}
	}
}
```
As you can see, we are using the `GetPropertyConfiguration(propName)` method to access the configuration of the property we need. The property configuration has a special `ExtraSettings` property, which is of type `Dictionary<string, object>` and which represents a storage for your additional settings. You can get access to it in `PrePatchProcessingFunction`  and do any business with it.
```C#
PatchEngine.Init(config =>
{
	config
		.SetPrePatchProcessingFunction(context =>
		{
			if (context.PropertyConfiguration.ExtraSettings.ContainsKey(RoundValueAttribute.PARAMETER_NAME))
			{
				if (!(context.NewValue is double))
				{
					return context.NewValue;
				}

				var precision = (int)context.PropertyConfiguration.ExtraSettings[RoundValueAttribute.PARAMETER_NAME];

				return Math.Round((double)context.NewValue, precision);
			}

			return context.NewValue;
		});
});
```
In this example, we check that the `ExtraSettings` dictionary contains a parameter named "RoundValue". If so, we get its value as the precision for rounding. After that, if the new value is of the correct type, we return it rounded. Otherwise, an unchanged value will be returned.
Now you can use your new attribute in your classes.
```C#
public class Route
{
	public string Id { get; set; }

	public string Title { get; set; }

	public string StartPoint { get; set; }

	public string DestinationPoint { get; set; }

	[RoundValue(2)]
	public double Distance { get; set; }
}
```
Gongratulations, that's all! The `Distance` property value will be rounded to two decimal places on patching.

## Swagger support
Since `PatchObject<T>` implements the `IDictionary<string, object>` interface, it can't be parsed by swagger schema generator correctly. Instead of description of the model used as generic parameter, you will see something like this:
```
{
	"prop1": "string",
	"prop2": "string",
	"prop3": "string"
}
```
Not very clear, right?
To fix that issue you need to follow 2 steps:

 1. `PM> Install-Package Patcheetah.Swagger.NET31` for .NET core 3.1 or `PM> Install-Package Patcheetah.Swagger.NET21` for .NET core 2.1
 2. `services.AddPatchObjectSwaggerSupport()` in `ConfigureServices` method of `Startup.cs`

That's all! Now you have complete support of `PatchObject<T>` by swagger gen.
