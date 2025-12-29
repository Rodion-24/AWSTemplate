using AWSTemplate.Application.Abstractions.Persistence;
using AWSTemplate.Application.Items.DTOs;
using MediatR;

namespace AWSTemplate.Application.Items.Queries;

public record GetItemByIdQuery(Guid Id) : IRequest<ItemDto?>;

public class GetItemByIdQueryHandler
    : IRequestHandler<GetItemByIdQuery, ItemDto?>
{
    private readonly IItemRepository _repo;

    public GetItemByIdQueryHandler(IItemRepository repo)
    {
        _repo = repo;
    }

    public async Task<ItemDto?> Handle(GetItemByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (item == null) return null;

        return new ItemDto(
            item.Id,
            item.Name,
            item.Description
        );
    }

}