using AWSTemplate.Application.Abstractions.Persistence;
using AWSTemplate.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AWSTemplate.Infrastructure.Persistence;

public class ItemRepository : IItemRepository
{
    private readonly ApplicationDbContext _db;

    public ItemRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<Item?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Items
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<Item>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Items
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Item item, CancellationToken cancellationToken = default)
    {
        await _db.Items.AddAsync(item, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Item item, CancellationToken cancellationToken = default)
    {
        _db.Items.Update(item);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Item item, CancellationToken cancellationToken = default)
    {
        _db.Items.Remove(item);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
