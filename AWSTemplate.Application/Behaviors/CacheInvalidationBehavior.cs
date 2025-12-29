using AWSTemplate.Application.Abstractions;
using AWSTemplate.Application.Abstractions.Caching;
using MediatR;

public class CacheInvalidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICacheService _cache;

    public CacheInvalidationBehavior(ICacheService cache)
    {
        _cache = cache;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();
        if (typeof(TRequest).Name.EndsWith("Command"))
        {
            await _cache.RemoveAsync("cache:GetAllItemsQuery");

            if (request is IHasId hasId)
            {
                var itemKey = $"cache:GetItemByIdQuery:{hasId.Id.GetHashCode()}";
                await _cache.RemoveAsync(itemKey);
            }
        }

        return response;
    }
}
