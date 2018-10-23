namespace ConnectorTemplate.Custom {
    using Scribe.Core.ConnectorApi.Metadata;

    public class KeyPropDef : IPropertyDefinition
    {
        internal const string KeyName = "Key";

        public string FullName { get; set; } = KeyName;

        public string Name { get; set; } = KeyName;

        public string Description { get; set; } = "The key name to index a value.";

        public string PropertyType { get; set; } = "System.String";

        public int MinOccurs { get; set; } = 1;

        public int MaxOccurs { get; set; } = 1;

        public int Size { get; set; }

        public int NumericScale { get; set; }

        public int NumericPrecision { get; set; }

        public string PresentationType { get; set; } = "";

        public bool Nullable { get; set; } = true;

        public bool IsPrimaryKey { get; set; }

        public bool UsedInQuerySelect { get; set; } = true;

        public bool UsedInQueryConstraint { get; set; } = false;

        public bool UsedInActionInput { get; set; } = true;

        public bool UsedInActionOutput { get; set; } = true;

        public bool UsedInLookupCondition { get; set; }

        public bool UsedInQuerySequence { get; set; }

        public bool RequiredInActionInput { get; set; }
    }
}