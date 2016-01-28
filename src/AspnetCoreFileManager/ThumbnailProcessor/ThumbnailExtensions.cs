using Microsoft.AspNet.Builder;

namespace AspnetCoreFileManager.ThumbnailProcessor
{
    public static class ThumbnailExtensions
    {
        public static IApplicationBuilder UseThumbnailProcessor(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ThumbnailProcessorMiddleware>();
        }
    }
}