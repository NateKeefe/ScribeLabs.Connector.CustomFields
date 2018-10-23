using ConnectorTemplate.Custom;
using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CDK
{
    public class MetadataExt : IMetadataProvider
    {
        private IMetadataProvider m;
        private IReadOnlyDictionary<string, IObjectDefinition> objects;

        public MetadataExt(IMetadataProvider m)
        {
            this.m = m;
            var originals = m.RetrieveObjectDefinitions(true, true);

            var supported = new[] { SupportedCustomType.String, SupportedCustomType.DateTime };
            var dyn = new DynamicProperties(supported);
            originals.ToList().ForEach(od => od.PropertyDefinitions.AddRange(dyn.CustomPropertiesMetadata));

            var extras = dyn.ExtraObjectDefinitions;
            var objDefs = originals.Concat(extras);
            this.objects = objDefs.ToDictionary(x => x.FullName, x => x);
        }

        public void Dispose()
        {
            
        }

        public void ResetMetadata()
        {
            this.m.ResetMetadata();
        }

        public IEnumerable<IActionDefinition> RetrieveActionDefinitions()
        {
            return this.m.RetrieveActionDefinitions();
        }

        public IMethodDefinition RetrieveMethodDefinition(string objectName, bool shouldGetParameters = false)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IMethodDefinition> RetrieveMethodDefinitions(bool shouldGetParameters = false)
        {
            throw new NotImplementedException();
        }

        public IObjectDefinition RetrieveObjectDefinition(string objectName, bool shouldGetProperties = false, bool shouldGetRelations = false)
        {
            if (this.objects.TryGetValue(objectName, out var od))
            {
                return od;
            }

            return null;
        }

        public IEnumerable<IObjectDefinition> RetrieveObjectDefinitions(bool shouldGetProperties = false, bool shouldGetRelations = false)
        {
            return this.objects.Values;
        }
    }
}
