namespace ConnectorTemplate.Custom {
    using System;

    public class CustomType
    {
        protected Type Type { get; }

        public string FullName => this.Type.FullName;

        public string DictionaryItemName => $"Key{this.Type.Name}Value";

        private CustomType(Type t) { this.Type = t; }

        /// <exception cref="ArgumentOutOfRangeException">Condition.</exception>
        public static CustomType GetCustomType(SupportedCustomType t)
        {
            switch (t)
            {
                case SupportedCustomType.Int: return Int;
                case SupportedCustomType.Double: return Double;
                case SupportedCustomType.DateTime: return DateTime;
                case SupportedCustomType.Guid: return Guid;
                case SupportedCustomType.String: return String;
                case SupportedCustomType.Bool: return Bool;
                default:
                    throw new ArgumentOutOfRangeException($"{t} : {t.GetType()} is not a supported custom type.");
            }
        }

        public static readonly CustomType Int = new CustomType(typeof(long?));
        public static readonly CustomType Double = new CustomType(typeof(double?));
        public static readonly CustomType DateTime = new CustomType(typeof(DateTime?));
        public static readonly CustomType Guid = new CustomType(typeof(Guid?));
        public static readonly CustomType String = new CustomType(typeof(string));
        public static readonly CustomType Bool = new CustomType(typeof(bool?));
    }
}