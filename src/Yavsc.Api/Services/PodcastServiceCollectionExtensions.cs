using Yavsc.Services;

namespace Yavsc.Api.Services;

public static class PodcastServiceCollectionExtensions
{
    public static IServiceCollection AddPodcasts(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PodcastOptions>(configuration.GetSection("Podcast"));
        services.AddSingleton<ILiveProcessor, LiveProcessor>();
        services.AddSingleton<IPodcastReplayRemuxer, FfmpegPodcastReplayRemuxer>();
        services.AddHostedService<PodcastReplayWorker>();
        services.AddOpenApi("podcasts", options => options.ShouldInclude = description => description.GroupName == "podcasts");
        return services;
    }
}
