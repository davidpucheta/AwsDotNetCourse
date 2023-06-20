using System.Runtime.Serialization.Formatters;
using System.Threading.Channels;
using Amazon.SQS;
using Amazon.SQS.Model;
using SqsConsumer;

var cts = new CancellationTokenSource();

var sqsClient = new AmazonSQSClient();

var queueUrlResponse = await sqsClient.GetQueueUrlAsync("customers");

var receivedMessageRequest = new ReceiveMessageRequest()
{
    QueueUrl = queueUrlResponse.QueueUrl,
    AttributeNames = new List<string>{ "All" },
    MessageAttributeNames = new List<string> { "All" }
};

while (!cts.IsCancellationRequested)
{
    var response = await sqsClient.ReceiveMessageAsync(receivedMessageRequest, cts.Token);

    foreach (var message in response.Messages)
    {
        Console.WriteLine(message.MessageId);
        Console.WriteLine(message.Body);

        await sqsClient.DeleteMessageAsync(queueUrlResponse.QueueUrl, message.ReceiptHandle);
    }

    await Task.Delay(3000);
}