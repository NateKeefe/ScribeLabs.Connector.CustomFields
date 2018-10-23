namespace ConnectorTemplate.Custom {
    using System.Collections.Generic;

    using Scribe.Core.ConnectorApi.Metadata;

    public class KeyValObjDef : IObjectDefinition
    {
        public KeyValObjDef(CustomType t)
        {
            this.FullName = t.DictionaryItemName;
            this.Name = t.DictionaryItemName;
            this.Description = "A collection of values indexed by runtime names.";
            this.PropertyDefinitions = new List<IPropertyDefinition>{ new KeyPropDef(), new ValuePropDef(t.FullName)}; 
        }

        public string FullName { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool Hidden { get; set; }

        public List<string> SupportedActionFullNames { get; set; } = new List<string>();

        public List<IPropertyDefinition> PropertyDefinitions { get; set; }

        public List<IRelationshipDefinition> RelationshipDefinitions { get; set; } = new List<IRelationshipDefinition>();
    }
}