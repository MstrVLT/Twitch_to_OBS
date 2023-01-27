using System.Reflection;
using GenericHostConsoleApp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TwitchLib.EventSub.Websockets.Extensions;

await Host.CreateDefaultBuilder(args)
    .UseContentRoot(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
    .ConfigureLogging(logging =>
    {
        // Add any 3rd party loggers like NLog or Serilog
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddLogging();
        services.AddTwitchLibEventSubWebsockets();

        services
            .AddHostedService<WebsocketHostedService>()
            .AddSingleton<IOBSService, OBSService>()
            .AddSingleton<ITwitchService, TwitchService>();

        services.AddOptions<OBSSettings>().Bind(hostContext.Configuration.GetSection("OBS"));
        services.AddOptions<TwitchSettings>().Bind(hostContext.Configuration.GetSection("Twitch"));
    })
    .RunConsoleAsync();
