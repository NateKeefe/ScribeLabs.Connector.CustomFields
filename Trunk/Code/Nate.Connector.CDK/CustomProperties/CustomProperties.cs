namespace ConnectorTemplate.Custom {
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    using Scribe.Core.ConnectorApi;

    public class CustomProperties
    {
        public static long? ConvertToInt(object o)          => o is null ? null : (long?)Convert.ToInt64(o);
        public static DateTime? ConvertToDateTime(object o) => o is null ? null : (DateTime ?)Convert.ToDateTime(o);
        public static double? ConvertToDouble(object o)     => o is null ? null : (double?)Convert.ToDouble(o);
        public static string ConvertToString(object o)      => o is null ? null : Convert.ToString(o);
        public static bool? ConvertToBool(object o)         => o is null ? null : (bool?)Convert.ToBoolean(o);
        public static Guid? ConvertToGuid(object o)
        {
            switch (o)
            {
                case null: return null;
                case Guid g: return g;
                case string s when Guid.TryParse(s, out var g): return g;
                default:
                    throw new ArgumentOutOfRangeException($"The object '{o} : {o.GetType().FullName} is not null, a Guid or string that can be parsed as a Guid.");
            }
        }



        public static IReadOnlyDictionary<string, T> Empty<T>() { return new ReadOnlyDictionary<string, T>(new Dictionary<string, T>()); }

        public CustomProperties(IReadOnlyDictionary<string, SupportedCustomType> properties, DataEntity de)
        {
            var typeLookup = properties.ToDictionary(kv => kv.Value, kv => kv.Key);

            if (typeLookup.TryGetValue(SupportedCustomType.Int, out var propNameInts))
            {
                if (de.Children.TryGetValue(propNameInts, out var des))
                {
                    var d = des.Select(cde => MakeProp(ConvertToInt, cde))
                        .Where(kv => kv.Key != null)
                        .ToDictionary(kv => kv.Key, kv => kv.Value);
                    this.IntProperties = new ReadOnlyDictionary<string, long?>(d);

                }
            }
            else { this.IntProperties = Empty<long?>(); }

            if (typeLookup.TryGetValue(SupportedCustomType.Double, out var propNameDoubles))
            {
                if (de.Children.TryGetValue(propNameDoubles, out var des))
                {
                    var d = des.Select(cde => MakeProp(ConvertToDouble, cde))
                        .Where(kv => kv.Key != null)
                        .ToDictionary(kv => kv.Key, kv => kv.Value);
                    this.DoubleProperties = new ReadOnlyDictionary<string, double?>(d);

                }
            }
            else { this.DoubleProperties = Empty<double?>(); }

            if (typeLookup.TryGetValue(SupportedCustomType.DateTime, out var propNameDateTime))
            {
                if (de.Children.TryGetValue(propNameDateTime, out var des))
                {
                    var d = des.Select(cde => MakeProp(ConvertToDateTime, cde))
                        .Where(kv => kv.Key != null)
                        .ToDictionary(kv => kv.Key, kv => kv.Value);
                    this.DateTimeProperties = new ReadOnlyDictionary<string, DateTime?>(d);

                }
            }
            else { this.DateTimeProperties = Empty<DateTime?>(); }

            if (typeLookup.TryGetValue(SupportedCustomType.DateTime, out var propNameGuid))
            {
                if (de.Children.TryGetValue(propNameGuid, out var des))
                {
                    var d = des.Select(cde => MakeProp(ConvertToGuid, cde))
                        .Where(kv => kv.Key != null)
                        .ToDictionary(kv => kv.Key, kv => kv.Value);
                    this.GuidProperties = new ReadOnlyDictionary<string, Guid?>(d);

                }
            }
            else { this.GuidProperties = Empty<Guid?>(); }

            if (typeLookup.TryGetValue(SupportedCustomType.Bool, out var propNameBool))
            {
                if (de.Children.TryGetValue(propNameBool, out var des))
                {
                    var d = des.Select(cde => MakeProp(ConvertToBool, cde))
                        .Where(kv => kv.Key != null)
                        .ToDictionary(kv => kv.Key, kv => kv.Value);
                    this.BoolProperties = new ReadOnlyDictionary<string, bool?>(d);

                }
            }
            else { this.BoolProperties = Empty<bool?>(); }

            if (typeLookup.TryGetValue(SupportedCustomType.String, out var propNameString))
            {
                if (de.Children.TryGetValue(propNameString, out var des))
                {
                    var d = des.Select(cde => MakeProp(ConvertToString, cde))
                        .Where(kv => kv.Key != null)
                        .ToDictionary(kv => kv.Key, kv => kv.Value);
                    this.StringProperties = new ReadOnlyDictionary<string, string>(d);

                }
            }
            else { this.StringProperties = Empty<string>(); }

            try
            {
                this.AllProperties = DictionaryExt.X.ConcatAndCast(IntProperties)
                    .ConcatAndCast(DoubleProperties)
                    .ConcatAndCast(DateTimeProperties)
                    .ConcatAndCast(GuidProperties)
                    .ConcatAndCast(StringProperties)
                    .ConcatAndCast(BoolProperties);
            }
            catch (ArgumentException) { /* Duplicate keys should not break the whole thing */ }
        }

        private static KeyValuePair<string, T> MakeProp<T>(Func<object, T> convert, DataEntity de)
        {
            var props = de.Properties;
            if (props.TryGetValue(KeyPropDef.KeyName, out var name))
            {
                if (props.TryGetValue(ValuePropDef.ValueName, out var val))
                {
                    return new KeyValuePair<string, T>(name.ToString(), convert(val));
                }
            }

            return new KeyValuePair<string, T>(null, default);
        }

        public IReadOnlyDictionary<string, long?> IntProperties { get; }
        public IReadOnlyDictionary<string, double?> DoubleProperties { get; }
        public IReadOnlyDictionary<string, DateTime?> DateTimeProperties { get; }
        public IReadOnlyDictionary<string, Guid?> GuidProperties { get; }
        public IReadOnlyDictionary<string, bool?> BoolProperties { get; }
        public IReadOnlyDictionary<string, string> StringProperties { get; }
        public IReadOnlyDictionary<string, object> AllProperties { get; }
    }

    internal static class DictionaryExt
    {
        public static readonly IReadOnlyDictionary<string, object> X = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>());
        internal static IReadOnlyDictionary<string, object> ConcatAndCast<T>(
            this IReadOnlyDictionary<string, object> x,
            IReadOnlyDictionary<string, T> d)
        {
            return x.Concat(d.Select(kv => new KeyValuePair<string, object>(kv.Key, (object)kv.Value))).ToReadOnlyDic();
        }

        internal static IReadOnlyDictionary<string, object> ToReadOnlyDic(
            this IEnumerable<KeyValuePair<string, object>> x)
        {
            return new ReadOnlyDictionary<string, object>(x.ToDictionary(kv => kv.Key, kv => kv.Value));
        }
    }
}