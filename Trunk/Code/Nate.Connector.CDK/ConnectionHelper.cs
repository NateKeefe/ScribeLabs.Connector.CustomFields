using System;
using System.Collections.Generic;

using Scribe.Core.ConnectorApi.Exceptions;
using Scribe.Core.ConnectorApi.ConnectionUI;

namespace CDK
{

    public static class ConnectionHelper
    {
        #region Constants

        public class ConnectionProperties
        {
        }

        internal static class ConnectionPropertyKeys
        {
        }

        internal static class ConnectionPropertyLabels
        {
        }

        private const string HelpLink = "https://success.scribesoft.com/s/";

        #endregion

        public static ConnectionProperties GetConnectionProperties(IDictionary<string, string> propDictionary)
        {
            if (propDictionary == null)
                throw new InvalidConnectionException("Connection Properties are NULL");

            var connectorProps = new ConnectionProperties();

            return connectorProps;
        }

        private static string getRequiredPropertyValue(IDictionary<string, string> properties, string key, string label)
        {
            var value = getPropertyValue(properties, key);
            if (string.IsNullOrEmpty(value))
                throw new InvalidConnectionException(string.Format("A value is required for '{0}'", label));

            return value;
        }

        private static string getPropertyValue(IDictionary<string, string> properties, string key)
        {
            var value = "";
            properties.TryGetValue(key, out value);
            return value;
        }

        public static FormDefinition GetConnectionFormDefintion()
        {

            var formDefinition = new FormDefinition
            {
                CompanyName = Connector.CompanyName,
                CryptoKey = Connector.CryptoKey,
                HelpUri = new Uri(HelpLink)
            };

            return formDefinition;
        }
    }
}