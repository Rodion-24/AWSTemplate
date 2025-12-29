using AWSTemplate.Application.Abstractions.Caching;
using MediatR;

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICacheService _cache;

    public CachingBehavior(ICacheService cache)
    {
        _cache = cache;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (typeof(TRequest).Name.EndsWith("Query"))
        {
            var key = GetCacheKey(request);
            var cached = await _cache.GetAsync<TResponse>(key);
            if (cached != null) return cached;

            var response = await next();
            await _cache.SetAsync(key, response, TimeSpan.FromMinutes(5));

            return response;
        }

        return await next();
    }

    private string GetCacheKey(TRequest request)
    {
        return $"cache:{request.GetType().Name}:{request.GetHashCode()}";
    }
}
