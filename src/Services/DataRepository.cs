using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

using parishdirectoryapi.Models;
using Amazon.DynamoDBv2.Model;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace parishdirectoryapi.Services
{

    public interface IDataRepository
    {
        Task<bool> AddChurch(Church church);
        Task<Church> GetChurch(string churchId);
        Task<bool> UpdateChurch(Church church);

        Task<bool> CreateFamily(string churchId, string familyId);
        Task<bool> DeleteFamily(string churchId, string familyId);
        Task<bool> UpdateFamily(Family family);
        Task<Family> GetFamily(string churchId, string familyId);

        Task<bool> AddMembers(string churchId, IEnumerable<Member> members);
        Task<bool> UpdateMember(string churchId, Member member);
        Task<bool> RemoveMember(string churchId, string memberId);

        Task<IEnumerable<Member>> GetMembers(string churchId, IEnumerable<string> memberIds);
        Task<IEnumerable<Family>> GetFamilies(string churchId);
    }

    public class DataRepository : IDataRepository
    {
        private class DDBTableNames
        {
            public const string CHURCHTABLE_NAME = "Churches";
            public const string FAMILYTABLE_NAME = "Families";
            public const string MEMBERTABLE_NAME = "Members";
        }

        private ILogger<DataRepository> _logger;
        private IDynamoDBContext DDBContext;

        private Table ChurchesTable => Table.LoadTable(new AmazonDynamoDBClient(), DDBTableNames.CHURCHTABLE_NAME);
        private Table FamiliesTable => Table.LoadTable(new AmazonDynamoDBClient(), DDBTableNames.FAMILYTABLE_NAME);
        private Table MembersTable => Table.LoadTable(new AmazonDynamoDBClient(), DDBTableNames.MEMBERTABLE_NAME);

        public DataRepository(ILogger<DataRepository> logger)
        {
            _logger = logger;

            AWSConfigsDynamoDB.Context.TypeMappings[typeof(Church)] = new Amazon.Util.TypeMapping(typeof(Church), DDBTableNames.CHURCHTABLE_NAME);
            AWSConfigsDynamoDB.Context.TypeMappings[typeof(Family)] = new Amazon.Util.TypeMapping(typeof(Family), DDBTableNames.FAMILYTABLE_NAME);

            var config = new DynamoDBContextConfig { Conversion = DynamoDBEntryConversion.V2 };
            DDBContext = new DynamoDBContext(new AmazonDynamoDBClient(), config);
        }

        async Task<bool> IDataRepository.AddMembers(string churchId, IEnumerable<Member> members)
        {
            var batchWrite = DDBContext.CreateBatchWrite<Member>(new DynamoDBOperationConfig
            {
                IgnoreNullValues = true
            });

            batchWrite.AddPutItems(members);
            try
            {
                await batchWrite.ExecuteAsync();
            }
            catch
            {
                return false;
            }
            return true;
        }

        async Task<bool> IDataRepository.CreateFamily(string churchId, string familyId)
        {
            var family = new Family { ChurchId = churchId, FamilyId = familyId };
            var document = DDBContext.ToDocument(family);
            var config = new PutItemOperationConfig { ConditionalExpression = GetAttributeNotExists(nameof(family.FamilyId)) };
            try
            {
                _logger.LogInformation($"Creating family with churchId :{family.ChurchId} and familyId:{family.FamilyId}");
                await FamiliesTable.PutItemAsync(document, config);
                return true;
            }
            catch (ConditionalCheckFailedException)
            {
                _logger.LogInformation($"Family with churchId :{family.ChurchId} and familyId:{family.FamilyId} already exits");
                return false;
            }
        }

        async Task<bool> IDataRepository.UpdateFamily(Family family)
        {
            var document = DDBContext.ToDocument(family);
            var config = new PutItemOperationConfig { ConditionalExpression = GetAttributeExists(nameof(family.FamilyId)) };
            try
            {
                _logger.LogInformation($"Replacing details of family with churchId :{family.ChurchId} and familyId:{family.FamilyId}");
                await FamiliesTable.PutItemAsync(document, config);
                return true;
            }
            catch (ConditionalCheckFailedException)
            {
                _logger.LogInformation($"Family with churchId :{family.ChurchId} and familyId:{family.FamilyId} doesn't exits");
                return false;
            }
        }

        async Task<bool> IDataRepository.DeleteFamily(string churchId, string familyId)
        {
            var family = new Family { ChurchId = churchId, FamilyId = familyId };
            var document = DDBContext.ToDocument(family);

            var deletedFamily = await FamiliesTable.DeleteItemAsync(document);
            return deletedFamily != null;
        }

        async Task<bool> IDataRepository.RemoveMember(string churchId, string memberId)
        {
            var member = new Member { ChurchId = churchId, MemberId = memberId };
            var document = DDBContext.ToDocument(member);

            var deletedMember = await MembersTable.DeleteItemAsync(document);
            return deletedMember != null;
        }

        async Task<IEnumerable<Member>> IDataRepository.GetMembers(string churchId, IEnumerable<string> memberIds)
        {
            var config = new DynamoDBOperationConfig();
            config.QueryFilter = new List<ScanCondition> { new ScanCondition("MemberId", ScanOperator.In, memberIds) };

            var query = DDBContext.QueryAsync<Member>(churchId, config);
            var members = await query.GetNextSetAsync();
            while (!query.IsDone)
            {
                var rem = await query.GetRemainingAsync();
                members.AddRange(rem);
            }
            return members;
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

        async Task<bool> IDataRepository.UpdateMember(string churchId, Member member)
        {
            var expr = new Expression
            {
                ExpressionStatement = "attribute_exists(MemberId)"
            };

            var document = DDBContext.ToDocument(member);
            try
            {
                await MembersTable.PutItemAsync(document,
                    new PutItemOperationConfig() { ConditionalExpression = expr });
                return true;
            }
            catch (ConditionalCheckFailedException)
            {
                _logger.LogInformation($"Member with id {member.ChurchId} and {member.MemberId} doesnot exists");
                return false;
            }
        }

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
                await ChurchesTable.PutItemAsync(document, config);
                return true;
            }
            catch (ConditionalCheckFailedException)
            {
                _logger.LogInformation($"Church with id {church.ChurchId} already exists");
                return false;
            }
        }

        async Task<bool> IDataRepository.UpdateChurch(Church church)
        {
            var document = DDBContext.ToDocument(church);
            var config = new PutItemOperationConfig { ConditionalExpression = GetAttributeExists(nameof(church.ChurchId)) };
            try
            {
                _logger.LogInformation($"Replacing church with churchId {church.ChurchId}");
                await ChurchesTable.PutItemAsync(DDBContext.ToDocument(church), config);
                return true;
            }
            catch (ConditionalCheckFailedException)
            {
                _logger.LogInformation($"Church with id {church.ChurchId} doesnot exists");
                return false;
            }
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
    }
}