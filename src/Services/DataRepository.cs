using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

using parishdirectoryapi.Models;
using Amazon.DynamoDBv2.Model;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using Amazon.Util;

namespace parishdirectoryapi.Services
{
    public class DataRepository : IDataRepository
    {
        private class DDBTableNames
        {
            public const string CHURCHTABLE_NAME = "Churches";
            public const string FAMILYTABLE_NAME = "Families";
            public const string MEMBERTABLE_NAME = "Members";
        }

        private readonly Table _churchesTable;
        private readonly Table _familiesTable;
        private readonly Table _membersTable;
        private readonly IDynamoDBContext DDBContext;
        private readonly ILogger<DataRepository> _logger;

        public DataRepository(ILogger<DataRepository> logger)
        {
            _logger = logger;

            AWSConfigsDynamoDB.Context.TypeMappings[typeof(Church)] = new TypeMapping(typeof(Church), DDBTableNames.CHURCHTABLE_NAME);
            AWSConfigsDynamoDB.Context.TypeMappings[typeof(Family)] = new TypeMapping(typeof(Family), DDBTableNames.FAMILYTABLE_NAME);
            AWSConfigsDynamoDB.Context.TypeMappings[typeof(Member)] = new TypeMapping(typeof(Family), DDBTableNames.MEMBERTABLE_NAME);

            var config = new Amazon.DynamoDBv2.DataModel.DynamoDBContextConfig
            {
                Conversion = DynamoDBEntryConversion.V2,
                IgnoreNullValues = true
            };
            DDBContext = new DynamoDBContext(new AmazonDynamoDBClient(), config);

            _churchesTable = DDBContext.GetTargetTable<Church>(new DynamoDBOperationConfig { IgnoreNullValues = true });
            _familiesTable = DDBContext.GetTargetTable<Family>(new DynamoDBOperationConfig { IgnoreNullValues = true });
            _membersTable = DDBContext.GetTargetTable<Member>(new DynamoDBOperationConfig { IgnoreNullValues = true });
        }

        #region Members

        async Task<bool> IDataRepository.AddMembers(IEnumerable<Member> members)
        {
            var batchWrite = _membersTable.CreateBatchWrite();
            foreach (var member in members)
            {
                var document = DDBContext.ToDocument(member);
                batchWrite.AddDocumentToPut(document);
            }
            await batchWrite.ExecuteAsync();
            return true;
        }

        Task<bool> IDataRepository.UpdateMember(Member member)
        {
            return UpdateTable(_membersTable, member, GetAttributeExists(nameof(member.MemberId)));
        }

        async Task<bool> IDataRepository.RemoveMember(string churchId, IEnumerable<string> memberIds)
        {
            var batchWrite = _membersTable.CreateBatchWrite();
            foreach (var memberId in memberIds)
            {
                batchWrite.AddKeyToDelete(churchId, memberId);
            }
            await batchWrite.ExecuteAsync();
            return true;
        }

        async Task<IEnumerable<Member>> IDataRepository.GetMembers(string churchId, IEnumerable<string> memberIds)
        {
            var batchGet = _membersTable.CreateBatchGet();
            foreach (var memberId in memberIds)
            {
                batchGet.AddKey(churchId, memberId);
            }

            await batchGet.ExecuteAsync();

            return batchGet.Results.Select(doc => DDBContext.FromDocument<Member>(doc));
        }

        #endregion Members

        #region Family

        async Task<bool> IDataRepository.AddFamily(Family family)
        {
            var document = DDBContext.ToDocument(family);
            var config = new PutItemOperationConfig { ConditionalExpression = GetAttributeNotExists(nameof(family.FamilyId)) };
            try
            {
                _logger.LogInformation($"Creating family with churchId :{family.ChurchId} and familyId:{family.FamilyId}");
                await _familiesTable.PutItemAsync(document, config);
                return true;
            }
            catch (ConditionalCheckFailedException)
            {
                _logger.LogInformation($"Family with churchId :{family.ChurchId} and familyId:{family.FamilyId} already exits");
                return false;
            }
        }

        Task<bool> IDataRepository.UpdateFamily(Family family)
        {
            return UpdateTable(_familiesTable, family, GetAttributeExists(nameof(family.FamilyId)));
        }

        async Task<bool> IDataRepository.DeleteFamily(string churchId, string familyId)
        {
            var family = new Family { ChurchId = churchId, FamilyId = familyId };
            var document = DDBContext.ToDocument(family);

            var deletedFamily = await _familiesTable.DeleteItemAsync(document);
            return deletedFamily != null;
        }


        async Task<IEnumerable<Family>> IDataRepository.GetFamilies(string churchId)
        {
            var query = DDBContext.QueryAsync<Family>(churchId);

            var families = await query.GetNextSetAsync();
            while (!query.IsDone)
            {
                var rem = await query.GetRemainingAsync();
                families.AddRange(rem);
            }
            return families;
        }

        Task<Family> IDataRepository.GetFamily(string churchId, string familyId)
        {
            _logger.LogInformation($"Looking up details of family with ChurchId:{churchId} and FamilyId:{familyId}");

            var family = DDBContext.LoadAsync<Family>(churchId, familyId);

            _logger.LogInformation($"Found family with ChurchId:{churchId} and FamilyId:{familyId} = {family != null}");
            return family;
        }


        #endregion Family

        #region CHURCH

        Task<Church> IDataRepository.GetChurch(string churchId)
        {
            _logger.LogInformation($"Getting details of church {churchId}");

            var church = DDBContext.LoadAsync<Church>(churchId);

            _logger.LogInformation($"Found church with id {churchId} = {church != null}");
            return church;
        }

        async Task<bool> IDataRepository.AddChurch(Church church)
        {
            var document = DDBContext.ToDocument(church);
            var config = new PutItemOperationConfig { ConditionalExpression = GetAttributeNotExists(nameof(church.ChurchId)) };

            try
            {
                _logger.LogInformation($"Adding a new church with churchId {church.ChurchId}");
                await _churchesTable.PutItemAsync(document, config);
                return true;
            }
            catch (ConditionalCheckFailedException)
            {
                _logger.LogInformation($"Church with id {church.ChurchId} already exists");
                return false;
            }
        }

        Task<bool> IDataRepository.UpdateChurch(Church church)
        {
            return UpdateTable(_churchesTable, church, GetAttributeExists(nameof(church.ChurchId)));
        }

        #endregion CHURCH

        private Expression GetAttributeNotExists(string attributeName)
        {
            return new Expression
            {
                ExpressionStatement = $"attribute_not_exists({attributeName})"
            };
        }

        private Expression GetAttributeExists(string attributeName)
        {
            return new Expression
            {
                ExpressionStatement = $"attribute_exists({attributeName})"
            };
        }

        private async Task<bool> UpdateTable<T>(Table table, T item, Expression condtionalExpression)
        {
            var document = DDBContext.ToDocument(item);
            var config = new UpdateItemOperationConfig { ConditionalExpression = condtionalExpression };

            try
            {
                await table.UpdateItemAsync(document, config);
                return true;
            }
            catch (ConditionalCheckFailedException)
            {
                return false;
            }
        }
    }
}