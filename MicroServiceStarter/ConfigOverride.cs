using System;
using Newtonsoft.Json;

namespace MicroServiceStarter
{
    public class ConfigOverride : IEquatable<ConfigOverride>
    {
        /// <summary>
        /// Used to select a property for modify or delete operations.
        /// </summary>
        [JsonProperty("propertySelector")]
        public string PropertySelector { get; set; }

        /// <summary>
        /// Only used when adding a property.
        /// </summary>
        [JsonProperty("propertyName")]
        public string PropertyName { get; set; }

        /// <summary>
        /// The value for the property in the case of creation or modification.
        /// </summary>
        [JsonProperty("propertyValue")]
        public object PropertyValue { get; set; }

        /// <summary>
        /// Operations to be performed.  Add, Modify, Delete are valid options.
        /// </summary>
        [JsonProperty("operation")]
        public string Operation { get; set; }


        public bool Equals(ConfigOverride other)
        {
            return other.PropertySelector == PropertySelector;
        }

        public override bool Equals(object obj)
        {
            return obj is ConfigOverride confOverride && Equals(confOverride);
        }

        public override int GetHashCode()
        {
            return (PropertySelector != null ? PropertySelector.GetHashCode() : 0);
        }
    }
}
