using AWSTemplate.Application.Abstractions.Persistence;
using AWSTemplate.Application.Items.DTOs;
using AWSTemplate.Domain.Entities;
using MediatR;

namespace AWSTemplate.Application.Items.Commands;

public record CreateItemCommand(string Name, string Description) : IRequest<ItemDto>;

public class CreateItemCommandHandler : IRequestHandler<CreateItemCommand, ItemDto>
{
    private readonly IItemRepository _repo;

    public CreateItemCommandHandler(IItemRepository repo)
    {
        _repo = repo;
    }

    public async Task<ItemDto> Handle(CreateItemCommand request, CancellationToken cancellationToken)
    {
        var item = new Item
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description
        };

        await _repo.AddAsync(item, cancellationToken);

        return new ItemDto(item.Id, item.Name, item.Description);
    }
}