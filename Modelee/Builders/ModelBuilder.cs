using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Modelee.Configuration;
using Modelee.Exceptions;
using Newtonsoft.Json.Linq;

namespace Modelee.Builders
{
    internal static class ModelBuilder
    {
        public static TEntity BuildFrom<TEntity>(PatchObject<TEntity> patchObject, TEntity model = null)
            where TEntity : class, new()
        {
            var result = ModelBuilder.Build(patchObject, typeof(TEntity), model);

            return result as TEntity;
        }

        private static object Build(IDictionary<string, JToken> data, Type type, object model = null)
        {
            var config = ConfigurationContainer.Instance.GetConfig(type);
            var properties = type.GetProperties();
            var missedRequiredProps = config.RequiredProperties.Except(data.Keys);

            if (missedRequiredProps.Any())
            {
                throw new RequiredPropertiesMissedException(missedRequiredProps);
            }

            foreach (var ignored in config.IgnoredProperties)
            {
                data.Remove(ignored);
            }

            var newModelCreated = false;

            if (model == null)
            {
                newModelCreated = true;
                model = Activator.CreateInstance(type);
            }

            foreach (var property in properties)
            {
                var propertyName = config.ViewModelAliasProperties.ContainsKey(property.Name) ?
                    config.ViewModelAliasProperties[property.Name] :
                    property.Name;

                if (!data.ContainsKey(propertyName))
                {
                    continue;
                }

                // pass this and model config to model builder if modelee config enabled
                var value = data[propertyName];
                var oldValue = property.GetValue(model);

                if (!newModelCreated && config.BeforePatchCallbacks.ContainsKey(property.Name))
                {
                    config.BeforePatchCallbacks[property.Name]?.Invoke(new PropertyChangedEventArgs
                    {
                        Entity = model,
                        SourceValue = oldValue,
                        TargetValue = value
                    });
                }

                // This is the most complicated part for understanding
                // Mechanic written below uses modelee configs for recursive patching 
                if (config.UseModeleeConfigProperties.Contains(property.Name))
                {
                    // At first we should remember original property value
                    var currentValue = property.GetValue(model);

                    // Then, if property implements IEnumerable, it means that we should have a deal with array in json patch object
                    // We'll check it below
                    if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                    {
                        // Let's get type of array elements, config for them and check that collection contains at least one element
                        var elementType = GetGenericIEnumerables(property.PropertyType).First();
                        var elementTypeConfig = ConfigurationContainer.Instance.GetConfig(elementType);
                        var collectionHasElements = (currentValue as IEnumerable)?.GetEnumerator().MoveNext() ?? false;

                        // If property in patch object is not an array, it means that something went wrong
                        if (!(value is JArray jArray))
                        {
                            throw new ArgumentException("The argument is not valid. Array expected.", propertyName);
                        }

                        // Let's iterate all the elements in patch object
                        var arrayElements = jArray.Select(element =>
                        {
                            // If it's a complex element
                            if (element is JObject jObject)
                            {
                                // Convert it into dictionary
                                var elementDataDictionary =
                                    new Dictionary<string, JToken>(element.ToObject<Dictionary<string, JToken>>(), elementTypeConfig.CaseSensitive ?
                                        StringComparer.Ordinal :
                                        StringComparer.OrdinalIgnoreCase);

                                // And create stub object for possible value with the same value of key-property
                                object elementOriginValue = null;

                                // If config has key property for this type and element of patch collection contains that property
                                if (!string.IsNullOrEmpty(elementTypeConfig.KeyPropertyName) && jObject.ContainsKey(elementTypeConfig.KeyPropertyName))
                                {
                                    // And origin object collection has elements
                                    if (collectionHasElements)
                                    {
                                        // Iterating by them
                                        while ((currentValue as IEnumerable).GetEnumerator().MoveNext())
                                        {
                                            // Get current value of origin element and its key property
                                            var elemVal = (currentValue as IEnumerable).GetEnumerator().Current;
                                            var elemKeyProp = elemVal.GetType().GetProperty(elementTypeConfig.KeyPropertyName);

                                            // Checking that key property implements IComparable
                                            if (typeof(IComparable).IsAssignableFrom(elemKeyProp.PropertyType))
                                            {
                                                var comparableKeyVal = elemKeyProp.GetValue(elemVal) as IComparable;

                                                // If key prop values are equals
                                                if (comparableKeyVal.CompareTo(jObject[elementTypeConfig.KeyPropertyName].ToObject(elemKeyProp.PropertyType)) == 0)
                                                {
                                                    // Assign current origin array element to stub object
                                                    elementOriginValue = elemVal;
                                                }
                                            }
                                        }
                                    }
                                }

                                // Pass stub object as origin value. If it's null, method will create a new one
                                return ModelBuilder.Build(elementDataDictionary, elementType, elementOriginValue);
                            }

                            return element.ToObject(elementType);
                        });

                        property.SetValue(model, arrayElements);

                        continue;
                    }

                    value = JToken.FromObject(ModelBuilder.Build(value.ToObject<Dictionary<string, JToken>>(), property.PropertyType, currentValue));
                }

                property.SetValue(model, value.ToObject(property.PropertyType));

                if (!newModelCreated && config.AfterPatchCallbacks.ContainsKey(property.Name))
                {
                    config.AfterPatchCallbacks[property.Name]?.Invoke(new PropertyChangedEventArgs
                    {
                        Entity = model,
                        SourceValue = oldValue,
                        TargetValue = value
                    });
                }
            }

            return model;
        }

        public static IEnumerable<Type> GetGenericIEnumerables(Type type)
        {
            return type
                .GetInterfaces()
                .Where(t => t.IsGenericType
                    && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .Select(t => t.GetGenericArguments()[0]);
        }
    }
}
