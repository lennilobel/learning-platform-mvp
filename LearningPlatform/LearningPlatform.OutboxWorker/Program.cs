using LearningPlatform.Infrastructure;

namespace LearningPlatform.OutboxWorker
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = Host.CreateApplicationBuilder(args);

			// Register all services required by the dedicated Outbox Worker,
			// including SQL persistence, Service Bus publishing, and the OutboxDispatcher hosted service.
			builder.Services.AddOutboxWorker(builder.Configuration);

			var host = builder.Build();

			host.Run();
		}
	}
}
