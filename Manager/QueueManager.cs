using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EchoBot.Model;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Queue;

namespace EchoBot.Manager
{
	public interface IQueueManager {

		Task AddMessageAsync(MessageModel megModel, CancellationToken cancellationToken = default);
		Task<ResponseModel> GetMessage(string id);
	}

	public class QueueManager : IQueueManager
	{
		private static readonly string ConnectionString;
		private static readonly string MessageQueueName;
		private static readonly string ResponseQueueName;

		private readonly CloudQueueClient _cloudQueueClient;

		static QueueManager() {
			//TODO: Read from config
			ConnectionString =
				@"DefaultEndpointsProtocol=https;AccountName=aqualiabot;AccountKey=dsIIpg4gXB+Vhz1A6RNRm1Pd4dsK5JSkVuWoFE9+U20eoEZGvECHXUSkHSSbCRpQkWLPgFA7P7bYAQYXH1kq9g==;BlobEndpoint=https://aqualiabot.blob.core.windows.net/;QueueEndpoint=https://aqualiabot.queue.core.windows.net/;TableEndpoint=https://aqualiabot.table.core.windows.net/;FileEndpoint=https://aqualiabot.file.core.windows.net/;";
			
			MessageQueueName = "messages";
			ResponseQueueName = "responses";
		}

		public QueueManager()
		{
			var cloudStorageAccount = CloudStorageAccount.Parse(ConnectionString);
			_cloudQueueClient = cloudStorageAccount.CreateCloudQueueClient();

		}

		public async Task AddMessageAsync(MessageModel megModel, CancellationToken cancellationToken = default) {

			CloudQueue queue = _cloudQueueClient.GetQueueReference(MessageQueueName);
			queue.CreateIfNotExists();

			CloudQueueMessage message = new CloudQueueMessage(Newtonsoft.Json.JsonConvert.SerializeObject(megModel));
			await queue.AddMessageAsync(message, cancellationToken).ConfigureAwait(false);
		}

		public async Task<ResponseModel> GetMessage(string id)
		{

			CloudQueue queue = _cloudQueueClient.GetQueueReference(ResponseQueueName);
			if (!(await queue.ExistsAsync())) {
				return null;
			};

			ResponseModel response = null;
			while (true) {

				var msg = await queue.GetMessageAsync();
				if (msg == null) {

					Thread.Sleep(1000);
					continue;
				}
				response = Newtonsoft.Json.JsonConvert.DeserializeObject<ResponseModel>(msg.AsString);

				if (response.Id != id) {
					await queue.AddMessageAsync(msg);

					Thread.Sleep(1000);
					continue;
				}

				break;
			}

			return response;
		}
	}
}
