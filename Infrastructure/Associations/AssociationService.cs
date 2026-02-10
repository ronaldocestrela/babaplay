using Application.Exceptions;
using Application.Features.Associations;
using Domain.Entities;
using Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Associations;

public class AssociationService(ApplicationDbContext context) : IAssociationService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<string> CreateAsync(Association association)
    {
        await _context.Associations.AddAsync(association);
        await _context.SaveChangesAsync();
        return association.Id;
    }

    public async Task<string> DeleteAsync(Association association)
    {
        _context.Associations.Remove(association);
        await _context.SaveChangesAsync();
        return association.Id;
    }

    public async Task<List<Association>> GetAllAsync()
    {
        return await _context.Associations.ToListAsync();
    }

    public async Task<Association> GetByIdAsync(string associationId)
    {
        return await _context.Associations
            .Where(association => association.Id == associationId)
            .SingleOrDefaultAsync() 
            ?? throw new NotFoundException(["Association not found."]);
    }

    public async Task<Association> GetByNameAsync(string name)
    {
        return await _context.Associations
            .Where(association => association.Name == name)
            .SingleOrDefaultAsync() 
            ?? throw new NotFoundException(["Association not found."]);
    }

    public async Task<string> UpdateAsync(Association association)
    {
        _context.Associations.Update(association);
        await _context.SaveChangesAsync();
        return association.Id;
    }
}
