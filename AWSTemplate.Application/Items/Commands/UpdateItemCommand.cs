using AWSTemplate.Application.Abstractions.Persistence;
using AWSTemplate.Application.Items.DTOs;
using MediatR;

namespace AWSTemplate.Application.Items.Commands;

public record UpdateItemCommand(Guid Id, string Name, string Description) : IRequest<ItemDto?>;

public class UpdateItemCommandHandler : IRequestHandler<UpdateItemCommand, ItemDto?>
{
    private readonly IItemRepository _repo;

    public UpdateItemCommandHandler(IItemRepository repo)
    {
        _repo = repo;
    }

    public async Task<ItemDto?> Handle(UpdateItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (item == null) return null;

        item.Name = request.Name;
        item.Description = request.Description;

        await _repo.UpdateAsync(item, cancellationToken);

        return new ItemDto(item.Id, item.Name, item.Description);
    }
}