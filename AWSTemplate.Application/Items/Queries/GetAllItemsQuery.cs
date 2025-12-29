using AWSTemplate.Application.Abstractions.Persistence;
using AWSTemplate.Application.Items.DTOs;
using MediatR;
using System.Collections.Generic;

namespace AWSTemplate.Application.Items.Queries;

public record GetAllItemsQuery() : IRequest<List<ItemDto>>;

public class GetAllItemsQueryHandler : IRequestHandler<GetAllItemsQuery, List<ItemDto>>
{
    private readonly IItemRepository _repo;

    public GetAllItemsQueryHandler(IItemRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<ItemDto>> Handle(GetAllItemsQuery request, CancellationToken cancellationToken)
    {
        var items = await _repo.GetAllAsync(cancellationToken);
        return items.Select(item => new ItemDto(
            item.Id,
            item.Name,
            item.Description
        )).ToList();
    }
}