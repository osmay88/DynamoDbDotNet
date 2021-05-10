using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DynamoNet
{
    class Program
    {
        class HighLevelItemCRUD
        {

            static async Task Main(string[] args)
            {
                AmazonDynamoDBConfig clientConfig = new AmazonDynamoDBConfig();
                clientConfig.ServiceURL = "http://dynamodb:8000";
                AmazonDynamoDBClient client = new AmazonDynamoDBClient("AKIA5A4IE3H45OEUD3HH", "FaxdILrQUM2czZ1DtfPinUHZPJDabJmHA+7W7DcJ", clientConfig);
                try
                {
                    DynamoDBContext context = new DynamoDBContext(client);
                    await CreateTables();
                    await TestCRUDOperations(context);
                }
                catch (AmazonDynamoDBException e)
                {
                    Console.WriteLine(e.Message);
                }
                catch (AmazonServiceException e)
                {
                    Console.WriteLine(e.Message);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            private static async Task TestCRUDOperations(DynamoDBContext context)
            {
                int bookID = 1001; // Some unique value.
                Book myBook = new Book
                {
                    Id = bookID,
                    Title = "object persistence-AWS SDK for.NET SDK-Book 1001",
                    ISBN = "111-1111111001",
                    BookAuthors = new List<string> { "Author 1", "Author 2" },
                };

                // Save the book.
                await context.SaveAsync(myBook);
                // Retrieve the book.
                Book bookRetrieved = await context.LoadAsync<Book>(bookID);

                // Update few properties.
                bookRetrieved.ISBN = "222-2222221001";
                bookRetrieved.BookAuthors = new List<string> { " Author 1", "Author x" }; // Replace existing authors list with this.
                await context.SaveAsync(bookRetrieved);

                // Retrieve the updated book. This time add the optional ConsistentRead parameter using DynamoDBContextConfig object.
                Book updatedBook = await context.LoadAsync<Book>(bookID, new DynamoDBContextConfig
                {
                    ConsistentRead = true
                });

                // Delete the book.
                await context.DeleteAsync<Book>(bookID);
                // Try to retrieve deleted book. It should return null.
                Book deletedBook = await context.LoadAsync<Book>(bookID, new DynamoDBContextConfig
                {
                    ConsistentRead = true
                });
                if (deletedBook == null)
                    Console.WriteLine("Book is deleted");
            }

            private static async Task CreateTables()
            {
                AmazonDynamoDBConfig clientConfig = new AmazonDynamoDBConfig();
                clientConfig.ServiceURL = "http://dynamodb:8000";
                AmazonDynamoDBClient client = new AmazonDynamoDBClient("AKIA5A4IE3H45OEUD3HH", "FaxdILrQUM2czZ1DtfPinUHZPJDabJmHA+7W7DcJ", clientConfig);
                string tableName = "ProductCatalog";

                var request = new CreateTableRequest
                {
                    TableName = tableName,
                    AttributeDefinitions = new List<AttributeDefinition>()
                    {
                        new AttributeDefinition
                        {
                            AttributeName = "Id",
                            AttributeType = "N"
                        }
                    },
                    KeySchema = new List<KeySchemaElement>()
                    {
                        new KeySchemaElement
                        {
                            AttributeName = "Id",
                            KeyType = "HASH"  //Partition key
                        }
                    },
                    ProvisionedThroughput = new ProvisionedThroughput
                    {
                        ReadCapacityUnits = 10,
                        WriteCapacityUnits = 5
                    }
                };

                var response = await client.CreateTableAsync(request);
            }
        }

        [DynamoDBTable("ProductCatalog")]
        public class Book
        {
            [DynamoDBHashKey] //Partition key
            public int Id
            {
                get; set;
            }
            [DynamoDBProperty]
            public string Title
            {
                get; set;
            }
            [DynamoDBProperty]
            public string ISBN
            {
                get; set;
            }
            [DynamoDBProperty("Authors")] //String Set datatype
            public List<string> BookAuthors
            {
                get; set;
            }
        }
    }
}
