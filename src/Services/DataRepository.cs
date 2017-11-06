using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

using parishdirectoryapi.Models;
using Amazon.DynamoDBv2.Model;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System;
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

        private ILogger<DataRepository> _logger;
        private IDynamoDBContext DDBContext;

        private Table ChurchesTable => Table.LoadTable(new AmazonDynamoDBClient(), DDBTableNames.CHURCHTABLE_NAME);
        private Table FamiliesTable => Table.LoadTable(new AmazonDynamoDBClient(), DDBTableNames.FAMILYTABLE_NAME);
        private Table MembersTable => Table.LoadTable(new AmazonDynamoDBClient(), DDBTableNames.MEMBERTABLE_NAME);

        public DataRepository(ILogger<DataRepository> logger)
        {
            _logger = logger;

            AWSConfigsDynamoDB.Context.TypeMappings[typeof(Church)] = new TypeMapping(typeof(Church), DDBTableNames.CHURCHTABLE_NAME);
            AWSConfigsDynamoDB.Context.TypeMappings[typeof(Family)] = new TypeMapping(typeof(Family), DDBTableNames.FAMILYTABLE_NAME);

            var config = new Amazon.DynamoDBv2.DataModel.DynamoDBContextConfig { Conversion = DynamoDBEntryConversion.V2 };
            DDBContext = new DynamoDBContext(new AmazonDynamoDBClient(), config);
        }

        async Task<bool> IDataRepository.AddMembers(IEnumerable<Member> members)
        {
            var batchWrite = MembersTable.CreateBatchWrite();
            foreach (var member in members)
            {
                var document = DDBContext.ToDocument(member);
                batchWrite.AddDocumentToPut(document);
            }
            await batchWrite.ExecuteAsync();
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
            var batchGet = MembersTable.CreateBatchGet();
            foreach (var memberId in memberIds)
            {
                batchGet.AddKey(churchId, memberId);
            }

            await batchGet.ExecuteAsync();

            return batchGet.Results.Select(doc => DDBContext.FromDocument<Member>(doc));
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

        async Task<bool> IDataRepository.UpdateMember(Member member)
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