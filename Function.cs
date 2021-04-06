using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.DynamoDBEvents;
using Amazon.DynamoDBv2.DocumentModel;
using Newtonsoft.Json;
using Amazon.DynamoDBv2.Model;
using Amazon.DynamoDBv2;
using Amazon.Lambda.Serialization.SystemTextJson;
using Amazon.Lambda.Serialization.Json;
//using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace getStatsByType2021
{
    public class ratingObject
    {
        public string type;
        public int count;
        //public int avgRating;
        //public string itemId;
        public int rating;
        //public string itemId;
        //public string description;
        //public int rating;
        //public string type;
        //public string company;
    }
    public class Function
    {
        private static AmazonDynamoDBClient client = new AmazonDynamoDBClient();
        public async Task<List<ratingObject>> FunctionHandler(DynamoDBEvent input, ILambdaContext context)
        {
            Table table = Table.LoadTable(client, "RatingsByType");
            List<ratingObject> items = new List<ratingObject>();
            List<DynamoDBEvent.DynamodbStreamRecord> records = (List<DynamoDBEvent.DynamodbStreamRecord>)input.Records;
            if (records.Count > 0)
            {
                DynamoDBEvent.DynamodbStreamRecord record = records[0];
                if (record.EventName.Equals("INSERT"))
                {
                    Document myDoc = Document.FromAttributeMap(record.Dynamodb.NewImage);
                    ratingObject myItem = JsonConvert.DeserializeObject<ratingObject>(myDoc.ToJson());

                    var request = new UpdateItemRequest
                    {
                        TableName = "RatingsByType",
                        Key = new Dictionary<string, AttributeValue>
                        {
                            { "type", new AttributeValue { S = myItem.type} }
                        },
                        AttributeUpdates = new Dictionary<string, AttributeValueUpdate>()
                        {
                            {
                                "count",
                                new AttributeValueUpdate { Action = "ADD", Value = new AttributeValue { N = "1"} }
                            },
                            {
                                "totalRating",
                                new AttributeValueUpdate { Action = "ADD", Value = new AttributeValue { N = myItem.rating.ToString() }}
                            },
                            //{
                            //    "avgRating",
                            //    new AttributeValueUpdate { Action = "ADD", Value = new AttributeValue { N = (myItem.rating/myItem.count + 1).ToString() }}
                            //},
                        },
                    };
                    await client.UpdateItemAsync(request);
                }
            }
            return items;
        }
    }
}
