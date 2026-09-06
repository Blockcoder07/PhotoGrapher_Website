using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhotographerApp.Core.DTOs;
using PhotographerApp.Core.Interfaces;

namespace PhotographerApp.API.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class ContactMessagesController : ControllerBase
{
    private readonly IContactService _contactService;

    public ContactMessagesController(IContactService contactService)
    {
        _contactService = contactService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<ContactMessageDto>>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _contactService.GetMessagesAsync(page, pageSize, cancellationToken);
        return Ok(new ApiResponse<PaginatedResponse<ContactMessageDto>>
        {
            Success = true,
            Message = "Messages retrieved",
            Data = result,
        });
    }

    [HttpPatch("{id}/read")]
    public async Task<ActionResult<ApiResponse>> SetRead(
        int id,
        [FromBody] MarkContactMessageReadRequest request,
        CancellationToken cancellationToken)
    {
        var success = await _contactService.SetReadAsync(id, request.IsRead, cancellationToken);
        if (!success)
            return NotFound(new ApiResponse { Success = false, Message = "Message not found" });

        return Ok(new ApiResponse { Success = true, Message = "Updated" });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> Delete(int id, CancellationToken cancellationToken)
    {
        var success = await _contactService.DeleteAsync(id, cancellationToken);
        if (!success)
            return NotFound(new ApiResponse { Success = false, Message = "Message not found" });

        return Ok(new ApiResponse { Success = true, Message = "Deleted" });
    }
}
