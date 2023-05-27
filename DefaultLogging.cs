using NLog;
using NLog.Targets;

namespace Zomlib;

public static class DefaultLogging
{
    public static void Setup()
    {
        const string logLayout = "${date:format=HH\\:mm\\:ss:universalTime=true} [${level:uppercase=true} @ ${logger:shortName=true}] ${message:withException=true:exceptionSeparator=\n\n}";

        LogManager.AutoShutdown = true;
        LogManager.Setup()
            .SetupLogFactory(config => config.SetTimeSourcAccurateUtc())
            .LoadConfiguration(setup => setup.ForLogger()
                .WriteTo(new ColoredConsoleTarget()
                {
                    DetectConsoleAvailable = true,
                    Layout = logLayout,
                    AutoFlush = true,
                    UseDefaultRowHighlightingRules = true,
                })
            );

    }
}
