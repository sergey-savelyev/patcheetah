using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Modelee.Configuration;
using Modelee.Exceptions;
using Modelee.Mapping;
using Newtonsoft.Json.Linq;

namespace Modelee
{
    internal class EntityBuilder<TEntity> where TEntity : class, new()
    {
        private readonly IDictionary<string, JToken> _patchData;
        private readonly EntityConfig _config;

        public EntityBuilder(IDictionary<string, JToken> patchData, EntityConfig config)
        {
            _patchData = patchData;
            _config = config;
        }

        public void PatchEntity(TEntity model)
        {
            BuildInternal(_patchData, typeof(TEntity), _config, model);
        }

        public TEntity BuildEntity()
        {
            var result = BuildInternal(_patchData, typeof(TEntity), _config);

            return result as TEntity;
        }

        public TEntity BuildEntity(TEntity model)
        {
            var result = BuildInternal(_patchData, typeof(TEntity), _config, model);

            return result as TEntity;
        }

        private object BuildInternal(IDictionary<string, JToken> data, Type type, object model)
        {
            var config = ConfigurationContainer.Instance.GetConfig(type);

            return BuildInternal(data, type, config, model);
        }

        private object BuildInternal(IDictionary<string, JToken> data, Type type, EntityConfig config, object entityToPatch = null)
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

            var newEntityCreated = false;

            if (entityToPatch == null)
            {
                newEntityCreated = true;
                entityToPatch = Activator.CreateInstance(type);
            }

            if (!newEntityCreated)
            {
                foreach (var ignored in configuredProperties.Where(x => x.Ignored).Select(x => x.Name))
                {
                    data.Remove(ignored);
                }
            }

            foreach (var property in properties)
            {
                ProcessProperty(property, entityToPatch, data, config, newEntityCreated);
            }

            return entityToPatch;
        }

        public static IEnumerable<Type> GetGenericIEnumerables(Type type)
        {
            return type
                .GetInterfaces()
                .Where(t => t.IsGenericType
                    && t.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                .Select(t => t.GetGenericArguments()[0]);
        }

        private void ProcessProperty(PropertyInfo property, object entityToPatch, IDictionary<string, JToken> patchData, EntityConfig config, bool entityBuiltFromScratch)
        {
            // If config is null, we gonna use simple patching
            if (config == null)
            {
                if (!patchData.ContainsKey(property.Name))
                {
                    return;
                }

                property.SetValue(entityToPatch, patchData[property.Name].ToObject(property.PropertyType));

                return;
            }

            var propertyConfig = config[property.Name];
            var propertyName = propertyConfig?.Name ?? property.Name;

            if (!patchData.ContainsKey(propertyName))
            {
                return;
            }

            var newValue = patchData[propertyName];
            var oldValue = property.GetValue(entityToPatch);

            if (propertyConfig == null)
            {
                SetValue(entityToPatch, property, newValue);
                return;
            }

            if (config.KeyPropertyName == propertyName)
            {
                if (newValue == null)
                {
                    throw new InvalidKeyException("Key property value can't be null");
                }

                if (!typeof(IComparable).IsAssignableFrom(property.PropertyType))
                {
                    throw new InvalidKeyException(property.PropertyType);
                }

                if (!entityBuiltFromScratch)
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
                var defaultValue = Activator.CreateInstance(property.PropertyType);
                property.SetValue(entityToPatch, defaultValue);

                return;
            }

            if (!entityBuiltFromScratch)
            {
                InvokeBeforePatchCallback(propertyConfig, entityToPatch, oldValue, newValue);
            }

            var mapped = JToken.FromObject(MapValue(newValue.ToObject(property.PropertyType), property, propertyConfig));

            if (propertyConfig.HasInternalConfig)
            {
                if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                {
                    PatchCollectionProperty(entityToPatch, property, propertyName, oldValue, newValue);

                    return;
                }

                newValue = JToken.FromObject(BuildInternal(newValue.ToObject<Dictionary<string, JToken>>(), property.PropertyType, oldValue));
            }

            property.SetValue(entityToPatch, newValue.ToObject(property.PropertyType));

            if (!entityBuiltFromScratch)
            {
                InvokeAfterPatchCallback(propertyConfig, entityToPatch, oldValue, newValue);
            }
        }

        private void PatchCollectionProperty(object entityToPatch, PropertyInfo property, string jsonPropertyName, object previousPropertyValue, object newPropertyValue)
        {
            // Let's get type of array elements, config for them and check that collection contains at least one element
            var elementType = GetGenericIEnumerables(property.PropertyType).First();
            var elementTypeConfig = ConfigurationContainer.Instance.GetConfig(elementType);
            var elementKeyPropertyName = elementTypeConfig.KeyPropertyName;

            if (elementKeyPropertyName == null)
            {
                throw new InvalidKeyException($"Type {elementType} should contain key property for successful deep patching");
            }

            var collectionHasElements = (previousPropertyValue as IEnumerable)?.GetEnumerator().MoveNext() ?? false;
            (previousPropertyValue as IEnumerable)?.GetEnumerator().Reset();
            var originNotPatchedElements = new List<object>();

            // If property in patch object is not an array, it means that something went wrong
            if (!(newPropertyValue is JArray jArray))
            {
                throw new ArgumentException("The argument is not valid. Array expected.", jsonPropertyName);
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
                            var enumerator = (previousPropertyValue as IEnumerable).GetEnumerator();
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
                                        if (!originNotPatchedElements.Any(x => object.ReferenceEquals(x, elemVal)) && !keys.Any(x => comparableKeyVal?.CompareTo(x) == 0))
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

            property.SetValue(entityToPatch, listVal);
        }

        private void InvokeBeforePatchCallback(PropertyConfiguration config, object patchedEntity, object oldPropertyValue, object newPropertyValue)
        {
            config.BeforePatchCallback?.Invoke(new PropertyChangedEventArgs
            {
                Entity = patchedEntity,
                SourceValue = oldPropertyValue,
                TargetValue = newPropertyValue
            });
        }

        private void InvokeAfterPatchCallback(PropertyConfiguration config, object patchedEntity, object oldPropertyValue, object newPropertyValue)
        {
            config.AfterPatchCallback?.Invoke(new PropertyChangedEventArgs
            {
                Entity = patchedEntity,
                SourceValue = oldPropertyValue,
                TargetValue = newPropertyValue
            });
        }

        private object MapValue(object value, PropertyInfo propertyInfo, PropertyConfiguration configuration)
        {
            var typeName = propertyInfo.PropertyType.Name;
            var globalHandler = MappingHandlersContainer.Instance.GlobalMappingHandler;
            var typeHandler = MappingHandlersContainer.Instance.GetHandler(propertyInfo.PropertyType);
            var propertyHandler = configuration.MappingHandler;

            if (globalHandler != null)
            {
                var globalHandlerResult = globalHandler(value);
                if (!globalHandlerResult.Skip)
                    value = globalHandlerResult.Value;
            }

            if (typeHandler != null)
            {
                var typeHandlerResult = typeHandler(value);
                if (!typeHandlerResult.Skip)
                    value = typeHandlerResult.Value;
            }

            if (propertyHandler != null)
            {
                var propertyHandlerResult = propertyHandler(value);
                if (!propertyHandlerResult.Skip)
                    value = propertyHandlerResult.Value;
            }

            return value;
        }

        private void SetValue(object model, PropertyInfo propertyInfo, JToken value)
        {
            propertyInfo.SetValue(model, value?.ToObject(propertyInfo.PropertyType));
        }
    }
}
