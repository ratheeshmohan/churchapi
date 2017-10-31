using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;

namespace parishdirectoryapi.Controllers
{
    public static class DynamodbWrapper
    {
        private static class DDBTableNames
        {
            public const string CHURCHTABLE_NAME = "Churches";
            public const string FAMILYTABLE_NAME = "Families";
        }
        
        public static IDynamoDBContext DDBContext { get; private set; }
        public static Table ChurchesTable => Table.LoadTable(new AmazonDynamoDBClient(), DDBTableNames.CHURCHTABLE_NAME);
        public static Table FamiliesTable => Table.LoadTable(new AmazonDynamoDBClient(), DDBTableNames.FAMILYTABLE_NAME);

        static DynamodbWrapper()
        {
            AWSConfigsDynamoDB.Context.TypeMappings[typeof(Models.Church)] = new Amazon.Util.TypeMapping(typeof(Models.Church), DDBTableNames.CHURCHTABLE_NAME);
            AWSConfigsDynamoDB.Context.TypeMappings[typeof(Models.Family)] = new Amazon.Util.TypeMapping(typeof(Models.Church), DDBTableNames.FAMILYTABLE_NAME);

            var config = new DynamoDBContextConfig { Conversion = DynamoDBEntryConversion.V2 };
            DDBContext = new DynamoDBContext(new AmazonDynamoDBClient(), config);
        }
    }
}