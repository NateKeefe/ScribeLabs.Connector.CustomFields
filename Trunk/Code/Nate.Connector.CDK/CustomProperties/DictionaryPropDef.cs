namespace ConnectorTemplate.Custom {
    using Scribe.Core.ConnectorApi.Metadata;

    public class DictionaryPropDef : IPropertyDefinition
    {
        public DictionaryPropDef(string name, KeyValObjDef objDef)
        {
            this.FullName = name;
            this.Name = name;
            this.Description = "A collection of key value pairs.";
            this.PropertyType = objDef.FullName;

        }
        public string FullName { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string PropertyType { get; set; }

        public int MinOccurs { get; set; } = 0;

        public int MaxOccurs { get; set; } = -1; // This makes it a collection

        public int Size { get; set; }

        public int NumericScale { get; set; }

        public int NumericPrecision { get; set; }

        public string PresentationType { get; set; } = string.Empty;

        public bool Nullable { get; set; }

        public bool IsPrimaryKey { get; set; }

        public bool UsedInQuerySelect { get; set; } = true;

        public bool UsedInQueryConstraint { get; set; }

        public bool UsedInActionInput { get; set; } = true;

        public bool UsedInActionOutput { get; set; } = true;

        public bool UsedInLookupCondition { get; set; }

        public bool UsedInQuerySequence { get; set; }

        public bool RequiredInActionInput { get; set; }
    }
}