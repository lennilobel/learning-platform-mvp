using LearningPlatform.Infrastructure;

namespace LearningPlatform.LearningProgressWorker
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = Host.CreateApplicationBuilder(args);

			// Register all services required by the dedicated Learning Progress Worker,
			// including SQL persistence, Service Bus consumption, and the LearningProgressConsumer hosted service.
			builder.Services.AddLearningProgressWorker(builder.Configuration);

			var host = builder.Build();

			host.Run();
		}
	}
}
