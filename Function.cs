using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Newtonsoft.Json;

using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace Assignment7GetTypeStats
{
    [Serializable]
    public class StatsClass
    {
        public string type;
        public double totalRating;
        public double count;
        public double averageRating;
    }
    public class Function
    {
        private static AmazonDynamoDBClient client = new AmazonDynamoDBClient();
        private string tableName = "RatingsByType";
        public async Task<StatsClass> FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
        {
            string type = "";
            Dictionary<string, string> dict = (Dictionary<string, string>)input.QueryStringParameters;
            dict.TryGetValue("type", out type);
            GetItemResponse res = await client.GetItemAsync(tableName, new Dictionary<string, AttributeValue>
                {
                    {"type", new AttributeValue { S = type } }
                }
            );
            Document myDoc = Document.FromAttributeMap(res.Item);
            StatsClass myItem = JsonConvert.DeserializeObject<StatsClass>(myDoc.ToJson());
            myItem.averageRating = Math.Round(myItem.totalRating / myItem.count, 1);
            return myItem;
        }
    }
}
