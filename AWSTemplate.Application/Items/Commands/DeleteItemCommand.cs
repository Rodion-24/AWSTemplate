using AWSTemplate.Application.Abstractions.Persistence;
using MediatR;

namespace AWSTemplate.Application.Items.Commands;

public record DeleteItemCommand(Guid Id) : IRequest<bool>;

public class DeleteItemCommandHandler : IRequestHandler<DeleteItemCommand, bool>
{
    private readonly IItemRepository _repo;

    public DeleteItemCommandHandler(IItemRepository repo)
    {
        _repo = repo;
    }

    public async Task<bool> Handle(DeleteItemCommand request, CancellationToken cancellationToken)
    {
        var item = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (item == null) return false;

        await _repo.DeleteAsync(item, cancellationToken);
        return true;
    }
}
