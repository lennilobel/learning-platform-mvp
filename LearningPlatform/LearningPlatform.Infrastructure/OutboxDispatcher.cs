using LearningPlatform.BusinessLogic.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LearningPlatform.Infrastructure
{
	// We put the dispatcher class in the infrastructure project because it is host/runtime infrastructure
	public class OutboxDispatcher : BackgroundService       // OutboxDispatcher is a singleton hosted service, so scoped dependencies such as DbContext must be resolved from an IServiceScopeFactory-created scope.
	{
		private readonly ILogger<OutboxDispatcher> _logger;
		private readonly IServiceScopeFactory _serviceScopeFactory;     // IServiceScopeFactory is used to create a scope for the scoped services (like DbContext) that we need to use in this background service

		public OutboxDispatcher(
			ILogger<OutboxDispatcher> logger,
			IServiceScopeFactory serviceScopeFactory)
		{
			this._logger = logger;
			this._serviceScopeFactory = serviceScopeFactory;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			this._logger.LogInformation("Starting OutboxDispatcher background service.");

			while (!stoppingToken.IsCancellationRequested)
			{
				using var scope = this._serviceScopeFactory.CreateScope();

				var outboxService = scope.ServiceProvider.GetRequiredService<IOutboxService>();

				await outboxService.ProcessOutbox(stoppingToken);

				// Wait for a short period before checking for new messages again
				this._logger.LogInformation("OutboxDispatcher sleeping for 5 seconds before checking for new messages.");
				await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
			}
		}
	}
}
