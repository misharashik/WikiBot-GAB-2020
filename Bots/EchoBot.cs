// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Azure; //Namespace for CloudConfigurationManager
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class EchoBot : ActivityHandler {

        // TODO: Move to config
	    private static readonly string ConnectionString =
		    @"DefaultEndpointsProtocol=https;AccountName=aqualiabot;AccountKey=dsIIpg4gXB+Vhz1A6RNRm1Pd4dsK5JSkVuWoFE9+U20eoEZGvECHXUSkHSSbCRpQkWLPgFA7P7bYAQYXH1kq9g==;BlobEndpoint=https://aqualiabot.blob.core.windows.net/;QueueEndpoint=https://aqualiabot.queue.core.windows.net/;TableEndpoint=https://aqualiabot.table.core.windows.net/;FileEndpoint=https://aqualiabot.file.core.windows.net/;";

	    private static readonly string QueueName = "messages";

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {

	        // Parse the connection string and return a reference to the storage account.
            //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConnectionString);
            
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConnectionString);

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference(QueueName);
            queue.CreateIfNotExists();

            CloudQueueMessage message = new CloudQueueMessage(turnContext.Activity.Text);
            await queue.AddMessageAsync(message, cancellationToken).ConfigureAwait(false);

	        //var v = queue.PeekMessages(5);

            var replyText = $"Echo: {turnContext.Activity.Text}\n{message.AsString}";
            await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var welcomeText = "Hello and welcome!";
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }
    }
}
