using System.Text.Json;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Options;

namespace Customers.Api.Messaging;

public class SqsMessenger : ISqsMessenger
{
    private readonly IAmazonSQS _amazonSQS;
    private readonly IOptions<QueueSettings> _queueSettings;
    private string? _queueUrl;

    public SqsMessenger(IAmazonSQS amazonSqs, IOptions<QueueSettings> queueSettings)
    {
        _amazonSQS = amazonSqs;
        _queueSettings = queueSettings;
    }

    public async Task<SendMessageResponse> SendMessageAsync<T>(T message)
    {
        var queueUrl = await GetQueueUrlResponseAsync<T>();

        var sendMessageRequest = new SendMessageRequest()
        {
            QueueUrl = queueUrl,
            MessageBody = JsonSerializer.Serialize(message),
            MessageAttributes = new Dictionary<string, MessageAttributeValue>()
            {
                {
                    "MessageType",
                    new MessageAttributeValue()
                    {
                        DataType = "String",
                        StringValue = typeof(T).Name
                    }
                }
            }
        };

        return await _amazonSQS.SendMessageAsync(sendMessageRequest);
    }

    private async Task<string> GetQueueUrlResponseAsync<T>()
    {
        if (_queueUrl is not null)
        {
            return _queueUrl;
        }


        var queueUrlResponse = await _amazonSQS.GetQueueUrlAsync(_queueSettings.Value.Name);
        _queueUrl = queueUrlResponse.QueueUrl;

        return _queueUrl;
    }
}