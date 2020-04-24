// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EchoBot.Manager;
using EchoBot.Model;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class EchoBot : ActivityHandler {

	    private readonly IQueueManager _queueManager;

	    public EchoBot(IQueueManager queueManager) {
		    _queueManager = queueManager;
	    }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken) {
	        var msg = new MessageModel {
		        Id = turnContext.Activity.Recipient.Id,
		        Role = turnContext.Activity.Recipient.Role,
		        Text = turnContext.Activity.Text
	        };

            await _queueManager.AddMessageAsync(msg, cancellationToken).ConfigureAwait(false);

	        ResponseModel response = await _queueManager.GetMessage(turnContext.Activity.Recipient.Id).ConfigureAwait(false);

            string replyText = $"Echo: {turnContext.Activity.Text} Response: {response?.Text}";
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
