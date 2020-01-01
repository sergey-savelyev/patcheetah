using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Modelee.Configuration;
using Modelee.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Modelee.Builders
{
    internal class ModelBuilder<TEntity> where TEntity : class, new()
    {
        private readonly IDictionary<string, JToken> _patchData;
        private readonly EntityConfig _config;

        public ModelBuilder(IDictionary<string, JToken> patchData, EntityConfig config)
        {
            _patchData = patchData;
            _config = config;
        }

        public TEntity BuildModel()
        {
            var result = BuildInternal(_patchData, typeof(TEntity), _config);

            return result as TEntity;
        }

        public TEntity BuildModel(TEntity model)
        {
            var result = BuildInternal(_patchData, typeof(TEntity), _config, model);

            return result as TEntity;
        }

        private object BuildInternal(IDictionary<string, JToken> data, Type type, object model)
        {
            var config = ConfigurationContainer.Instance.GetConfig(type);

            return BuildInternal(data, type, config, model);
        }

        private object BuildInternal(IDictionary<string, JToken> data, Type type, EntityConfig config, object model = null)
        {
            var properties = type.GetProperties();
            var configuredProperties = config.ConfiguredProperties;
            var missedRequiredPropertyNames = configuredProperties
                .Where(x => x.Required)
                .Select(x => x.Name)
                .Except(data.Keys)
                .ToArray();

            if (missedRequiredPropertyNames.Any())
            {
                throw new RequiredPropertiesMissedException(missedRequiredPropertyNames);
            }

            var newModelCreated = false;

            if (model == null)
            {
                newModelCreated = true;
                model = Activator.CreateInstance(type);
            }

            if (!newModelCreated)
            {
                foreach (var ignored in configuredProperties.Where(x => x.Ignored).Select(x => x.Name))
                {
                    data.Remove(ignored);
                }
            }

            foreach (var property in properties)
            {
                // If config is null, we gonna use simple patching
                if (config == null)
                {
                    if (!data.ContainsKey(property.Name))
                    {
                        continue;
                    }

                    property.SetValue(model, data[property.Name].ToObject(property.PropertyType));

                    continue;
                }

                var propertyConfig = config[property.Name];
                var propertyName = propertyConfig?.Name ?? property.Name;

                if (!data.ContainsKey(propertyName))
                {
                    continue;
                }

                var newValue = data[propertyName];
                var oldValue = property.GetValue(model);

                if (propertyConfig == null)
                {
                    SetValue(model, property, newValue);
                    continue;
                }

                if (config.KeyPropertyName == propertyName)
                {
                    if (newValue == null)
                    {
                        throw new InvalidKeyException("Key property value can't be null");
                    }

                    if (!(typeof(IComparable).IsAssignableFrom(property.PropertyType)))
                    {
                        throw new InvalidKeyException(property.PropertyType);
                    }

                    if (!newModelCreated)
                    {
                        var valueOfIncomingKey = newValue.ToObject(property.PropertyType);

                        if (config.CheckKeyOnPatching && (oldValue as IComparable).CompareTo(valueOfIncomingKey) != 0)
                        {
                            throw new KeyConcurrenceException(valueOfIncomingKey);
                        }
                    }
                }

                if (newValue == null)
                {
                    property.SetValue(model, null);

                    continue;
                }

                if (!newModelCreated && propertyConfig.BeforePatchCallback != null)
                {
                    propertyConfig.BeforePatchCallback.Invoke(new PropertyChangedEventArgs
                    {
                        Entity = model,
                        SourceValue = oldValue,
                        TargetValue = newValue
                    });
                }

                // This is the most complicated part for understanding
                // Mechanic written below uses modelee configs for recursive patching 
                if (propertyConfig.HasModeleeCongig)
                {
                    // If property implements IEnumerable, it means that we should have a deal with array in json patch object
                    // We'll check it below
                    if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                    {
                        // Let's get type of array elements, config for them and check that collection contains at least one element
                        var elementType = GetGenericIEnumerables(property.PropertyType).First();
                        var elementTypeConfig = ConfigurationContainer.Instance.GetConfig(elementType);
                        var elementKeyPropertyName = elementTypeConfig.KeyPropertyName;

                        if (elementKeyPropertyName == null)
                        {
                            throw new InvalidKeyException($"Type {elementType} should contain key property for successful deep patching");
                        }

                        var collectionHasElements = (oldValue as IEnumerable)?.GetEnumerator().MoveNext() ?? false;
                        (oldValue as IEnumerable)?.GetEnumerator().Reset();
                        var originNotPatchedElements = new List<object>();

                        // If property in patch object is not an array, it means that something went wrong
                        if (!(newValue is JArray jArray))
                        {
                            throw new ArgumentException("The argument is not valid. Array expected.", propertyName);
                        }

                        var keys = jArray
                            .Select(x => x is JObject jObj ?
                                (jObj.ContainsKey(elementKeyPropertyName) ?
                                    jObj[elementKeyPropertyName] :
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
                                if (!string.IsNullOrEmpty(elementKeyPropertyName) && jObject.ContainsKey(elementKeyPropertyName))
                                {
                                    // And origin object collection has elements
                                    if (collectionHasElements)
                                    {
                                        var enumerator = (oldValue as IEnumerable).GetEnumerator();
                                        // Iterating by them
                                        while (enumerator.MoveNext())
                                        {
                                            // Get current value of origin element and its key property
                                            var elemVal = enumerator.Current;
                                            var elemKeyProp = elemVal.GetType().GetProperty(elementKeyPropertyName);

                                            // Checking that key property implements IComparable
                                            if (typeof(IComparable).IsAssignableFrom(elemKeyProp.PropertyType))
                                            {
                                                var comparableKeyVal = elemKeyProp.GetValue(elemVal) as IComparable;

                                                // If key prop values are not equals
                                                if (comparableKeyVal == null || comparableKeyVal.CompareTo(jObject[elementKeyPropertyName].ToObject(elemKeyProp.PropertyType)) != 0)
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
                                return BuildInternal(elementDataDictionary, elementType, elementOriginValue);
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

                    newValue = JToken.FromObject(BuildInternal(newValue.ToObject<Dictionary<string, JToken>>(), property.PropertyType, oldValue));
                }

                property.SetValue(model, newValue.ToObject(property.PropertyType));

                if (!newModelCreated && propertyConfig.AfterPatchCallback != null)
                {
                    propertyConfig.AfterPatchCallback.Invoke(new PropertyChangedEventArgs
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

        private void SetValue(object model, PropertyInfo propertyInfo, JToken value)
        {
            propertyInfo.SetValue(model, value?.ToObject(propertyInfo.PropertyType));
        }
    }
}
