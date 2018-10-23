using Scribe.Connector.Common.Reflection;
using Scribe.Connector.Common.Reflection.Actions;

namespace CDK.Entities
{
    [ObjectDefinition]
    [Query]
    [CreateWith]
    public class Account
    {
        [PropertyDefinition]
        public string Name { get; set; }
        [PropertyDefinition]
        public int Number { get; set; }
    }
}
