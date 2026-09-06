using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhotographerApp.Core.DTOs;
using PhotographerApp.Core.Interfaces;

namespace PhotographerApp.API.Controllers.Content;

[ApiController]
[Route("api/[controller]")]
public class ContactController : ControllerBase
{
    private readonly IContactService _contactService;
    private readonly IValidator<SubmitContactMessageRequest> _validator;

    public ContactController(IContactService contactService, IValidator<SubmitContactMessageRequest> validator)
    {
        _contactService = contactService;
        _validator = validator;
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<ApiResponse>> Submit(
        [FromBody] SubmitContactMessageRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequest(new ApiResponse
            {
                Success = false,
                Message = "Invalid request",
                Errors = validation.Errors.Select(e => e.ErrorMessage).ToList(),
            });
        }

        await _contactService.SubmitAsync(request, cancellationToken);
        return Ok(new ApiResponse { Success = true, Message = "Message received" });
    }
}
