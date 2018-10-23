namespace ConnectorTemplate.Custom
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using Scribe.Core.ConnectorApi;
    using Scribe.Core.ConnectorApi.Metadata;
    using Scribe.Core.ConnectorApi.Query;
    /// <summary>
    /// This is a class that can help add Runtime properties to Scribe Connectors.
    /// There are really thre parts:
    /// 1. Metadata
    /// 2. Query: Getting a dictionary into a data entity
    /// 3. Operations: Getting data from a Data Entity into Dictionaries(by type)
    /// </summary>
    public class DynamicProperties
    {
        private readonly IReadOnlyDictionary<string, IPropertyDefinition> customPropertiesLookup;

        private readonly IReadOnlyDictionary<string, SupportedCustomType> propertyNamesAndTypes;

        public DynamicProperties(IReadOnlyList<SupportedCustomType> propertyTypes)
            : this(propertyTypes.ToDictionary(Name, Id)) { }

        private DynamicProperties(IReadOnlyDictionary<string, SupportedCustomType> propertyNamesAndTypes)
        {
            if (propertyNamesAndTypes.Values.Distinct().Count() != propertyNamesAndTypes.Count)
                throw new ArgumentException("The types must be unique.");

            this.propertyNamesAndTypes = propertyNamesAndTypes;

            var customTypes = propertyNamesAndTypes.ToDictionary(
                kv => kv,
                kv => new KeyValObjDef(CustomType.GetCustomType(kv.Value)));

            var propDefs = customTypes.Select(kv => new DictionaryPropDef(kv.Key.Key, kv.Value));

            this.customPropertiesLookup = new ReadOnlyDictionary<string, IPropertyDefinition>(
                propDefs.ToDictionary(pd => pd.FullName, pd => (IPropertyDefinition)pd));

            this.ExtraObjectDefinitionsDic = new ReadOnlyDictionary<string, IObjectDefinition>(
                customTypes.Values.ToDictionary(o => o.FullName, o => (IObjectDefinition)Id(o)));
        }

        /// <summary>
        /// Add these properties to any entities that you want to have custom properties.
        /// </summary>
        public IReadOnlyList<IPropertyDefinition> CustomPropertiesMetadata =>
            this.customPropertiesLookup.Values.ToList();

        /// <summary>
        /// Add these ObjectDefinitions to the list of object definitions (these are used by the properties)
        /// </summary>
        public IReadOnlyList<IObjectDefinition> ExtraObjectDefinitions =>
            this.ExtraObjectDefinitionsDic.Values.ToList().AsReadOnly();

        internal IReadOnlyDictionary<string, IObjectDefinition> ExtraObjectDefinitionsDic { get; }

        /// <summary>
        /// When building a data entity, pass in the dictionary of values and add the EntityChildren to your DataEntity.Children.
        /// If it already had Children, you should merge them.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="customProps"></param>
        /// <returns></returns>
        public EntityChildren CreateCustomPropertyiesForDataEntity<T>(IReadOnlyDictionary<string, object> customProps)
        {
            var children = new EntityChildren();

            var ints = new Dictionary<string, long?>();
            var doubles = new Dictionary<string, double?>();
            var dateTimes = new Dictionary<string, DateTime?>();
            var guids = new Dictionary<string, Guid?>();
            var bools = new Dictionary<string, bool?>();
            var strings = new Dictionary<string, string>();

            var reversed = this.propertyNamesAndTypes.ToDictionary(x => x.Value, x => x.Key);
            foreach (var prop in customProps)
            {
                var ty = prop.Value.GetType();

                if (TryGetSupportedType(ty, out var supported))
                    switch (supported)
                    {
                        case SupportedCustomType.Int:
                            ints.Add(prop.Key, CustomProperties.ConvertToInt(prop.Value));
                            break;
                        case SupportedCustomType.Double:
                            doubles.Add(prop.Key, CustomProperties.ConvertToDouble(prop.Value));
                            break;
                        case SupportedCustomType.DateTime:
                            dateTimes.Add(prop.Key, CustomProperties.ConvertToDateTime(prop.Value));
                            break;
                        case SupportedCustomType.Guid:
                            guids.Add(prop.Key, CustomProperties.ConvertToGuid(prop.Value));
                            break;
                        case SupportedCustomType.String:
                            strings.Add(prop.Key, CustomProperties.ConvertToString(prop.Value));
                            break;
                        case SupportedCustomType.Bool:
                            bools.Add(prop.Key, CustomProperties.ConvertToBool(prop.Value));
                            break;
                    }
            }

            if (ints.Count > 0)
            {
                var objDefName = CustomType.Int.DictionaryItemName;
                var des = ints.Select(kv => MakeKeyValDataEntity(kv.Key, kv.Value, objDefName)).ToList();
                var propName = reversed[SupportedCustomType.Int];
                children.Add(propName, des);
            }

            if (doubles.Count > 0)
            {
                var objDefName = CustomType.Double.DictionaryItemName;
                var des = ints.Select(kv => MakeKeyValDataEntity(kv.Key, kv.Value, objDefName)).ToList();
                var propName = reversed[SupportedCustomType.Double];
                children.Add(propName, des);
            }

            if (dateTimes.Count > 0)
            {
                var objDefName = CustomType.DateTime.DictionaryItemName;
                var des = ints.Select(kv => MakeKeyValDataEntity(kv.Key, kv.Value, objDefName)).ToList();
                var propName = reversed[SupportedCustomType.DateTime];
                children.Add(propName, des);
            }

            if (guids.Count > 0)
            {
                var objDefName = CustomType.Guid.DictionaryItemName;
                var des = ints.Select(kv => MakeKeyValDataEntity(kv.Key, kv.Value, objDefName)).ToList();
                var propName = reversed[SupportedCustomType.Guid];
                children.Add(propName, des);
            }

            if (strings.Count > 0)
            {
                var objDefName = CustomType.String.DictionaryItemName;
                var des = ints.Select(kv => MakeKeyValDataEntity(kv.Key, kv.Value, objDefName)).ToList();
                var propName = reversed[SupportedCustomType.String];
                children.Add(propName, des);
            }

            if (bools.Count > 0)
            {
                var objDefName = CustomType.Bool.DictionaryItemName;
                var des = ints.Select(kv => MakeKeyValDataEntity(kv.Key, kv.Value, objDefName)).ToList();
                var propName = reversed[SupportedCustomType.Bool];
                children.Add(propName, des);
            }

            return children;
        }

        /// <summary>
        /// When trying to extract values from a data entity pass in the data entity and then use the typed dictionaries on the
        /// CustomProperties class to find the values.
        /// </summary>
        /// <param name="de"></param>
        /// <returns></returns>
        public CustomProperties GetFromEntity(DataEntity de)
        {
            return new CustomProperties(this.propertyNamesAndTypes, de);
        }

        private static T Id<T>(T t) { return t; }

        private static DataEntity MakeKeyValDataEntity(string key, object val, string objDefName)
        {
            var props = new EntityProperties { { KeyPropDef.KeyName, key }, { ValuePropDef.ValueName, val } };

            return new DataEntity(objDefName) { Properties = props };
        }

        private static string Name(SupportedCustomType t) { return t + "Properties"; }

        private static bool TryGetSupportedType(Type t, out SupportedCustomType supported)
        {
            if (t == typeof(int)
                || t == typeof(short)
                || t == typeof(long))
            {
                supported = SupportedCustomType.Int;
                return true;
            }

            if (t == typeof(bool))
            {
                supported = SupportedCustomType.Bool;
                return true;
            }

            if (t == typeof(DateTime))
            {
                supported = SupportedCustomType.DateTime;
                return true;
            }

            if (t == typeof(Guid))
            {
                supported = SupportedCustomType.Guid;
                return true;
            }

            if (t == typeof(float)
                || t == typeof(double))
            {
                supported = SupportedCustomType.Double;
                return true;
            }

            if (t == typeof(string))
            {
                supported = SupportedCustomType.String;
                return true;
            }

            supported = 0;
            return false;
        }
    }
}