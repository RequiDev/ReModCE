using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ReModCE.Core
{
    public class DynamicContractResolver : DefaultContractResolver
    {
        private readonly List<string> _propertyNamesToExclude;

        public DynamicContractResolver(List<string> propertyNamesToExclude)
        {
            _propertyNamesToExclude = propertyNamesToExclude;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var properties = base.CreateProperties(type, memberSerialization);

            // only serializer properties that are not named after the specified property.
            properties =
                properties.Where(p => !_propertyNamesToExclude.Contains(p.PropertyName)).ToList();

            return properties;
        }
    }
}
