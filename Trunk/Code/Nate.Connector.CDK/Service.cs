using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

using Scribe.Core.ConnectorApi;
using Scribe.Core.ConnectorApi.Actions;
using Scribe.Core.ConnectorApi.Metadata;
using Scribe.Core.ConnectorApi.Query;
using Scribe.Core.ConnectorApi.Exceptions;
using Scribe.Core.ConnectorApi.Logger;
using Scribe.Connector.Common.Reflection.Data;

using CDK.Objects;
using CDK.Entities;
using CDK.Common;

namespace CDK
{
    class ConnectorService
    {
        #region Instaniation 
        private static Reflector reflector = new Reflector(Assembly.GetExecutingAssembly());
        public bool IsConnected { get; set; }
        public Guid ConnectorTypeId { get; }
        public void Dispose()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Connection
        public enum SupportedActions
        {
            Query,
            Create
        }

        public void Connect(ConnectionHelper.ConnectionProperties connectionProps)
        {
            IsConnected = true;
        }

        public void Disconnect()
        {
            IsConnected = false;
        }
        #endregion

        #region Operations
        public OperationResult Create(DataEntity dataEntity)
        {
            var entityName = dataEntity.ObjectDefinitionFullName;
            var operationResult = new OperationResult();

            switch (entityName)
            {
                case EntityNames.Folder:
                    var invoice810 = ToScribeModel<Account>(dataEntity);
                    break;
                default:
                    throw new ArgumentException($"{entityName} is not supported for Create.");
            }

            return operationResult;
        }

        private T ToScribeModel<T>(DataEntity input) where T : new()
        {
            T scribeModel;
            try
            {
                scribeModel = reflector.To<T>(input);
            }
            catch (Exception e)
            {
                throw new ArgumentException("Error translating from DataEntity to ScribeModel: " + e.Message, e);
            }
            return scribeModel;
        }
        #endregion

        #region Query
        public IEnumerable<DataEntity> ExecuteQuery(Query query)
        {
            var entityName = query.RootEntity.ObjectDefinitionFullName;
            var rawData = ReadFile(query);

            //deserialize file
            switch (entityName)
            {
                case EntityNames.Folder: return null; //fix me

                default:
                    throw new InvalidExecuteQueryException(
                        $"The {entityName} entity is not supported for query.");
            }
        }

        private static Dictionary<string, object> BuildConstraintDictionary(Expression queryExpression)
        {
            var constraints = new Dictionary<string, object>();

            if (queryExpression == null)
                return constraints;

            if (queryExpression.ExpressionType == ExpressionType.Comparison)
            {
                // only 1 filter
                addCompEprToConstraints(queryExpression as ComparisonExpression, ref constraints);
            }
            else if (queryExpression.ExpressionType == ExpressionType.Logical)
            {
                // Multiple filters
                addLogicalEprToConstraints(queryExpression as LogicalExpression, ref constraints);
            }
            else
                throw new InvalidExecuteQueryException("Unsupported filter type: " + queryExpression.ExpressionType.ToString());

            return constraints;
        }

        private static void addLogicalEprToConstraints(LogicalExpression exp, ref Dictionary<string, object> constraints)
        {
            if (exp.Operator != LogicalOperator.And)
                throw new InvalidExecuteQueryException("Unsupported operator in filter: " + exp.Operator.ToString());

            if (exp.LeftExpression.ExpressionType == ExpressionType.Comparison)
                addCompEprToConstraints(exp.LeftExpression as ComparisonExpression, ref constraints);
            else if (exp.LeftExpression.ExpressionType == ExpressionType.Logical)
                addLogicalEprToConstraints(exp.LeftExpression as LogicalExpression, ref constraints);
            else
                throw new InvalidExecuteQueryException("Unsupported filter type: " + exp.LeftExpression.ExpressionType.ToString());

            if (exp.RightExpression.ExpressionType == ExpressionType.Comparison)
                addCompEprToConstraints(exp.RightExpression as ComparisonExpression, ref constraints);
            else if (exp.RightExpression.ExpressionType == ExpressionType.Logical)
                addLogicalEprToConstraints(exp.RightExpression as LogicalExpression, ref constraints);
            else
                throw new InvalidExecuteQueryException("Unsupported filter type: " + exp.RightExpression.ExpressionType.ToString());
        }

        private static void addCompEprToConstraints(ComparisonExpression exp, ref Dictionary<string, object> constraints)
        {
            if (exp.Operator != ComparisonOperator.Equal)
                throw new InvalidExecuteQueryException(string.Format(StringMessages.OnlyEqualsOperatorAllowed, exp.Operator.ToString(), exp.LeftValue.Value));

            var constraintKey = exp.LeftValue.Value.ToString();
            if (constraintKey.LastIndexOf(".") > -1)
            {
                // need to remove "objectname." if present
                constraintKey = constraintKey.Substring(constraintKey.LastIndexOf(".") + 1);
            }
            constraints.Add(constraintKey, exp.RightValue.Value.ToString());
        }

        public static string ReadFile(Query query)
        {
            string rawData = "";
            var entityName = query.RootEntity.ObjectDefinitionFullName;
            var constraints = BuildConstraintDictionary(query.Constraints);

            constraints.TryGetValue("Filename", out var filename);
            if (filename == null) { throw new InvalidExecuteQueryException("Missing Filename filter."); }
            constraints.TryGetValue("Folder", out var folder);
            if (folder == null) { throw new InvalidExecuteQueryException("Missing Folder filter."); }
            if (folder.ToString().EndsWith("\\") == false) { folder = folder + "\\".ToString(); }
            try
            {
                rawData = File.ReadAllText(folder.ToString() + filename.ToString());
            }
            catch (Exception exp)
            {
                Logger.Write(Logger.Severity.Error,
                    $"Cannot find Folder or File when querying entity: {entityName}.",
                    exp.Message + exp.InnerException);
                throw new InvalidExecuteQueryException("Cannot find Folder or File: " + exp.Message);
            }
            return rawData;
        }
        #endregion

        #region Metadata
        public IMetadataProvider GetMetadataProvider()
        {
            return mp;
        }

        private static IMetadataProvider mp = new MetadataExt(reflector.GetMetadataProvider());



        public IEnumerable<IActionDefinition> RetrieveActionDefinitions()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IObjectDefinition> RetrieveObjectDefinitions(bool shouldGetProperties = false, bool shouldGetRelations = false)
        {
            throw new NotImplementedException();
        }

        public IObjectDefinition RetrieveObjectDefinition(string objectName, bool shouldGetProperties = false,
            bool shouldGetRelations = false)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IMethodDefinition> RetrieveMethodDefinitions(bool shouldGetParameters = false)
        {
            throw new NotImplementedException();
        }

        public IMethodDefinition RetrieveMethodDefinition(string objectName, bool shouldGetParameters = false)
        {
            throw new NotImplementedException();
        }

        public void ResetMetadata()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}