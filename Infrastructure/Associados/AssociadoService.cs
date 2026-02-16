using Application.Exceptions;
using Application.Features.Associados;
using Domain.Entities;
using Infrastructure.Constants;
using Infrastructure.Contexts;
using Infrastructure.Identity;
using Infrastructure.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Associados;

public class AssociadoService(
    ApplicationDbContext context,
    UserManager<ApplicationUser> userManager,
    ILogger<AssociadoService> logger) : IAssociadoService
{
    private readonly ApplicationDbContext _context = context;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ILogger<AssociadoService> _logger = logger;

    public async Task<string> CreateAsync(CreateAssociadoRequest request)
    {
        _logger.LogInformation("CreateAsync called for Email={Email}, CPF={CPF}", request?.Email, request?.CPF);

        try
        {
            if (request.Password != request.ConfirmPassword)
            {
                throw new ConflictException(["Senhas não conferem."]);
            }

            if (await _userManager.FindByEmailAsync(request.Email) is not null)
            {
                throw new ConflictException(["Email já está em uso."]);
            }

            if (await _context.Associados.AnyAsync(a => a.CPF == request.CPF))
            {
                throw new ConflictException(["CPF já cadastrado."]);
            }

            // Extract first/last name from FullName
            var nameParts = (request.FullName ?? string.Empty).Trim().Split(' ', 2);
            var firstName = nameParts.Length > 0 ? nameParts[0] : string.Empty;
            var lastName = nameParts.Length > 1 ? nameParts[1] : firstName;

            // Create ApplicationUser
            var newUser = new ApplicationUser
            {
                FirstName = firstName,
                LastName = lastName,
                Email = request.Email,
                UserName = request.Email,
                PhoneNumber = request.PhoneNumber,
                IsActive = true,
                EmailConfirmed = true
            };

            var identityResult = await _userManager.CreateAsync(newUser, request.Password);
            if (!identityResult.Succeeded)
            {
                throw new IdentityException(IdentityHelper.GetIdentityResultErrorDescriptions(identityResult));
            }

            // Assign Basic role
            await _userManager.AddToRoleAsync(newUser, RoleConstants.Basic);

            // Create Associado linked to the user
            var associado = new Associado
            {
                FullName = request.FullName,
                CPF = request.CPF,
                DateOfBirth = request.DateOfBirth,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                City = request.City,
                State = request.State,
                ZipCode = request.ZipCode,
                Position = request.Position,
                UserId = newUser.Id
            };

            await _context.Associados.AddAsync(associado);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Database update error when creating associado for Email={Email}", request?.Email);
                // Convert common DB errors into user-friendly ConflictException
                throw new ConflictException(["Erro ao salvar associado. Verifique dados duplicados (CPF/Email) e tente novamente."]);
            }

            return associado.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating associado for Email={Email}", request?.Email);
            throw;
        }
    }

    public async Task<string> UpdateAsync(UpdateAssociadoRequest request, string id)
    {
        var associado = await _context.Associados
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new NotFoundException(["Associado não encontrado."]);

        associado.FullName = request.FullName;
        associado.PhoneNumber = request.PhoneNumber;
        associado.Address = request.Address;
        associado.City = request.City;
        associado.State = request.State;
        associado.ZipCode = request.ZipCode;
        associado.Position = request.Position;
        associado.UpdatedAt = DateTime.UtcNow;

        // Update the linked user's name and phone
        var user = await _userManager.FindByIdAsync(associado.UserId);
        if (user is not null)
        {
            var nameParts = request.FullName.Trim().Split(' ', 2);
            user.FirstName = nameParts[0];
            user.LastName = nameParts.Length > 1 ? nameParts[1] : nameParts[0];
            user.PhoneNumber = request.PhoneNumber;
            await _userManager.UpdateAsync(user);
        }

        _context.Associados.Update(associado);
        await _context.SaveChangesAsync();

        return associado.Id;
    }

    public async Task<string> DeleteAsync(string id)
    {
        var associado = await _context.Associados
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new NotFoundException(["Associado não encontrado."]);

        // Deactivate the linked user
        var user = await _userManager.FindByIdAsync(associado.UserId);
        if (user is not null)
        {
            user.IsActive = false;
            await _userManager.UpdateAsync(user);
        }

        _context.Associados.Remove(associado);
        await _context.SaveChangesAsync();

        return id;
    }

    public async Task<AssociadoResponse> GetByIdAsync(string id)
    {
        var associado = await _context.Associados
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new NotFoundException(["Associado não encontrado."]);

        var user = await _userManager.FindByIdAsync(associado.UserId);

        return MapToResponse(associado, user);
    }

    public async Task<List<AssociadoResponse>> GetAllAsync()
    {
        _logger.LogInformation("GetAllAsync called (AssociadoService)");

        var associados = await _context.Associados.ToListAsync();

        var responses = new List<AssociadoResponse>();
        foreach (var associado in associados)
        {
            var user = await _userManager.FindByIdAsync(associado.UserId);
            responses.Add(MapToResponse(associado, user));
        }

        return responses;
    }

    private static AssociadoResponse MapToResponse(Associado associado, ApplicationUser? user)
    {
        return new AssociadoResponse
        {
            Id = associado.Id,
            FullName = associado.FullName,
            CPF = associado.CPF,
            DateOfBirth = associado.DateOfBirth,
            PhoneNumber = associado.PhoneNumber,
            Address = associado.Address,
            City = associado.City,
            State = associado.State,
            ZipCode = associado.ZipCode,
            Position = associado.Position,
            Email = user?.Email ?? string.Empty,
            UserId = associado.UserId,
            IsActive = user?.IsActive ?? false,
            CreatedAt = associado.CreatedAt,
            UpdatedAt = associado.UpdatedAt
        };
    }
}
