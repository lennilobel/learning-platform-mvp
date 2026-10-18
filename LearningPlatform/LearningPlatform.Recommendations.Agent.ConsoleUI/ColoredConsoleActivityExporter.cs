using System.Diagnostics;
using OpenTelemetry;

namespace LearningPlatform.Recommendations.Agent.ConsoleUI
{
	// Let's define our own OpenTelemetry exporter so we can color-code the output and visually distinguish agent
	// output from telemetry output (the standard console exporter does not provide a hook for doing this).
	//
	// We derive from BaseExporter<Activity> to define how completed activities are written to the console.
	// In Program.cs, the exporter is wrapped by SimpleActivityExportProcessor, which exports each completed
	// activity synchronously. That's useful for a learning/demo console app because the telemetry appears
	// immediately. For production, a batching processor would generally be preferable.

	internal sealed class ColoredConsoleActivityExporter : BaseExporter<Activity>
	{
		private static readonly object _consoleLock = new();

		private readonly ConsoleColor _color;

		public ColoredConsoleActivityExporter(ConsoleColor color)
		{
			this._color = color;
		}

		public override ExportResult Export(in Batch<Activity> batch)
		{
			// OpenTelemetry recommends that custom exporters suppress instrumentation while exporting, so that exporter activity cannot
			// accidentally generate more telemetry and recurse. So we suppress additional telemetry during the lifetime of this method.
			using var scope = SuppressInstrumentationScope.Begin();

			lock (_consoleLock)
			{
				var originalColor = Console.ForegroundColor;

				try
				{
					Console.ForegroundColor = this._color;

					foreach (var activity in batch)
					{
						Console.WriteLine($"Activity: {activity.DisplayName}");
						Console.WriteLine($"  Source:   {activity.Source.Name}");
						Console.WriteLine($"  TraceId:  {activity.TraceId}");
						Console.WriteLine($"  SpanId:   {activity.SpanId}");
						Console.WriteLine($"  Duration: {activity.Duration}");

						foreach (var tag in activity.TagObjects)
						{
							Console.WriteLine($"  {tag.Key}: {tag.Value}");
						}

						Console.WriteLine();
					}
				}
				finally
				{
					Console.ForegroundColor = originalColor;
				}
			}

			return ExportResult.Success;
		}
	}
}