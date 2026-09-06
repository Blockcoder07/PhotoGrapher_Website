using PhotographerApp.Core.DTOs;

namespace PhotographerApp.Core.Interfaces;

public interface IContactService
{
    Task SubmitAsync(SubmitContactMessageRequest request, CancellationToken cancellationToken = default);

    /// <summary>Admin inbox listing, newest first.</summary>
    Task<PaginatedResponse<ContactMessageDto>> GetMessagesAsync(int pageNumber = 1, int pageSize = 20, CancellationToken cancellationToken = default);
    Task<bool> SetReadAsync(int id, bool isRead, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}
