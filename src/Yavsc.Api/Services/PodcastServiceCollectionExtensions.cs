using Yavsc.Services;

namespace Yavsc.Api.Services;

public static class PodcastServiceCollectionExtensions
{
    public static IServiceCollection AddPodcasts(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PodcastOptions>(configuration.GetSection("Podcast"));
        services.Configure<PodcastObsOptions>(configuration.GetSection("Podcast:Obs"));
        services.AddScoped<IPodcastObsService, PodcastObsService>();
        services.AddHttpClient<IPodcastObsRelay, PodcastObsRelay>(client => client.Timeout = TimeSpan.FromSeconds(10))
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });
        services.AddHostedService<PodcastObsWorker>();
        services.AddSingleton<ILiveProcessor, LiveProcessor>();
        services.AddScoped<IPodcastScheduleService, PodcastScheduleService>();
        services.AddSingleton<IPodcastReplayRemuxer, FfmpegPodcastReplayRemuxer>();
        services.AddHostedService<PodcastReplayWorker>();
        services.AddOpenApi("podcasts", options => options.ShouldInclude = description => description.GroupName == "podcasts");
        return services;
    }
}
