using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Modelee.Configuration;
using Modelee.Exceptions;
using Newtonsoft.Json;
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
            var missedRequiredProps = config.RequiredProperties.Except(data.Keys).ToArray();

            if (missedRequiredProps.Any())
            {
                throw new RequiredPropertiesMissedException(missedRequiredProps);
            }

            var newModelCreated = false;

            if (model == null)
            {
                newModelCreated = true;
                model = Activator.CreateInstance(type);
            }

            if (!newModelCreated)
            {
                foreach (var ignored in config.IgnoredProperties)
                {
                    data.Remove(ignored);
                }
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
                var newValue = data[propertyName];

                if (config.SimplePatchingUsed)
                {
                    property.SetValue(model, newValue.ToObject(property.PropertyType));

                    continue;
                }

                var oldValue = property.GetValue(model);

                if (config.KeyPropertyName == propertyName)
                {
                    if (!(typeof(IComparable).IsAssignableFrom(property.PropertyType)))
                    {
                        throw new InvalidKeyException(property.PropertyType);
                    }

                    if (!newModelCreated)
                    {
                        var valueOfIncomingKey = newValue.ToObject(property.PropertyType);

                        if (config.StrictKeyUsed && (oldValue as IComparable).CompareTo(valueOfIncomingKey) != 0)
                        {
                            throw new KeyConcurrenceException(valueOfIncomingKey);
                        }
                    }
                }

                if (!newModelCreated && config.BeforePatchCallbacks.ContainsKey(property.Name))
                {
                    config.BeforePatchCallbacks[property.Name]?.Invoke(new PropertyChangedEventArgs
                    {
                        Entity = model,
                        SourceValue = oldValue,
                        TargetValue = newValue
                    });
                }

                // This is the most complicated part for understanding
                // Mechanic written below uses modelee configs for recursive patching 
                if (config.UseModeleeConfigProperties.Contains(propertyName))
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
                        (currentValue as IEnumerable)?.GetEnumerator().Reset();
                        var originNotPatchedElements = new List<object>();

                        // If property in patch object is not an array, it means that something went wrong
                        if (!(newValue is JArray jArray))
                        {
                            throw new ArgumentException("The argument is not valid. Array expected.", propertyName);
                        }

                        var keys = jArray
                            .Select(x => x is JObject jObj ?
                                (jObj.ContainsKey(elementTypeConfig.KeyPropertyName) ?
                                    jObj[elementTypeConfig.KeyPropertyName] :
                                    null) :
                                null)
                            .Where(x => x != null)
                            .Select(x => x.ToObject<object>())
                            .ToList();

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
                                        var enumerator = (currentValue as IEnumerable).GetEnumerator();
                                        // Iterating by them
                                        while (enumerator.MoveNext())
                                        {
                                            // Get current value of origin element and its key property
                                            var elemVal = enumerator.Current;
                                            var elemKeyProp = elemVal.GetType().GetProperty(elementTypeConfig.KeyPropertyName);

                                            // Checking that key property implements IComparable
                                            if (typeof(IComparable).IsAssignableFrom(elemKeyProp.PropertyType))
                                            {
                                                var comparableKeyVal = elemKeyProp.GetValue(elemVal) as IComparable;

                                                // If key prop values are not equals
                                                if (comparableKeyVal == null || comparableKeyVal.CompareTo(jObject[elementTypeConfig.KeyPropertyName].ToObject(elemKeyProp.PropertyType)) != 0)
                                                {
                                                    if (!originNotPatchedElements.Any(x => Object.ReferenceEquals(x, elemVal)) && !keys.Any(x => comparableKeyVal?.CompareTo(x) == 0))
                                                    {
                                                        // Add origin collection value to list for non-patching
                                                        originNotPatchedElements.Add(elemVal);
                                                    }
                                                }
                                                else
                                                {
                                                    // Else assign current origin array element to stub object
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
                        }).ToList();

                        arrayElements.AddRange(originNotPatchedElements);

                        var listInstance = Activator.CreateInstance(property.PropertyType);
                        var listVal = (IList)listInstance;
                        arrayElements.ForEach(x => listVal.Add(x));

                        property.SetValue(model, listVal);

                        continue;
                    }

                    newValue = JToken.FromObject(ModelBuilder.Build(newValue.ToObject<Dictionary<string, JToken>>(), property.PropertyType, currentValue));
                }

                property.SetValue(model, newValue.ToObject(property.PropertyType));

                if (!newModelCreated && config.AfterPatchCallbacks.ContainsKey(property.Name))
                {
                    config.AfterPatchCallbacks[property.Name]?.Invoke(new PropertyChangedEventArgs
                    {
                        Entity = model,
                        SourceValue = oldValue,
                        TargetValue = newValue
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

        private static bool ValidateAndDeserialize(JToken jToken, Type type, out object result)
        {
            result = null;
            var missedProps = 0;
            var deserialized = JsonConvert.DeserializeObject(jToken.ToString(), type, new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Error,
                Error = (s, e) =>
                {
                    missedProps++;
                    e.ErrorContext.Handled = true;
                }
            });

            if (missedProps >= type.GetProperties().Where(x => x.CanWrite).Count())
            {
                return false;
            }

            result = deserialized;
            return true;
        }
    }
}
