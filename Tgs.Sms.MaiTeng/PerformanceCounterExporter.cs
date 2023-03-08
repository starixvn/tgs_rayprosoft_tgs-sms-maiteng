using OpenTelemetry;
using System.Diagnostics;

namespace Tgs.Sms.MaiTeng;

public class PerformanceCounterExporter : BaseExporter<Activity>
{
    public override ExportResult Export(in Batch<Activity> batch)
    {
        foreach (var activity in batch)
        {
            var counterAction = (Action?)activity.GetCustomProperty("PerformanceCounter");

            counterAction?.Invoke();
        }

        return ExportResult.Success;
    }
}

