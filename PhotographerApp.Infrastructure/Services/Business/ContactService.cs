using Microsoft.EntityFrameworkCore;
using PhotographerApp.Core.DTOs;
using PhotographerApp.Core.Entities;
using PhotographerApp.Core.Interfaces;
using PhotographerApp.Infrastructure.Data;

namespace PhotographerApp.Infrastructure.Services.Business;

public class ContactService : IContactService
{
    private readonly ApplicationDbContext _context;

    public ContactService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SubmitAsync(SubmitContactMessageRequest request, CancellationToken cancellationToken = default)
    {
        _context.ContactMessages.Add(new ContactMessage
        {
            Name = request.Name,
            Email = request.Email,
            Subject = request.Subject,
            Message = request.Message,
        });

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<PaginatedResponse<ContactMessageDto>> GetMessagesAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var query = _context.ContactMessages.OrderByDescending(m => m.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var messages = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new ContactMessageDto
            {
                Id = m.Id,
                Name = m.Name,
                Email = m.Email,
                Phone = m.Phone,
                Subject = m.Subject,
                Message = m.Message,
                EventType = m.EventType,
                EventDate = m.EventDate,
                IsRead = m.IsRead,
                CreatedAt = m.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        return new PaginatedResponse<ContactMessageDto>
        {
            Items = messages,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
        };
    }

    public async Task<bool> SetReadAsync(int id, bool isRead, CancellationToken cancellationToken = default)
    {
        var message = await _context.ContactMessages.FindAsync(new object[] { id }, cancellationToken);
        if (message == null) return false;

        message.IsRead = isRead;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var message = await _context.ContactMessages.FindAsync(new object[] { id }, cancellationToken);
        if (message == null) return false;

        message.IsDeleted = true;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
