using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhotographerApp.Core.DTOs;
using PhotographerApp.Core.Interfaces;

namespace PhotographerApp.API.Controllers.Admin;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize(Roles = "Admin")]
public class FilmsController : ControllerBase
{
    private readonly IFilmService _filmService;
    private readonly IFileStorageService _fileStorageService;

    public FilmsController(IFilmService filmService, IFileStorageService fileStorageService)
    {
        _filmService = filmService;
        _fileStorageService = fileStorageService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<FilmDto>>>> GetAllFilms(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _filmService.GetAllFilmsForAdminAsync(page, pageSize, cancellationToken);
        return Ok(new ApiResponse<PaginatedResponse<FilmDto>>
        {
            Success = true,
            Message = "Films retrieved",
            Data = result
        });
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<FilmDto>>> CreateFilm(
        [FromBody] CreateFilmRequest request,
        CancellationToken cancellationToken = default)
    {
        var film = await _filmService.CreateFilmAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAllFilms), new { id = film?.Id },
            new ApiResponse<FilmDto> { Success = true, Message = "Film created", Data = film });
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<FilmDto>>> UpdateFilm(
        int id,
        [FromBody] UpdateFilmRequest request,
        CancellationToken cancellationToken = default)
    {
        var film = await _filmService.UpdateFilmAsync(id, request, cancellationToken);
        if (film == null)
            return NotFound(new ApiResponse { Success = false, Message = "Film not found" });

        return Ok(new ApiResponse<FilmDto> { Success = true, Message = "Film updated", Data = film });
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteFilm(
        int id,
        CancellationToken cancellationToken = default)
    {
        var success = await _filmService.DeleteFilmAsync(id, cancellationToken);
        if (!success)
            return NotFound(new ApiResponse { Success = false, Message = "Film not found" });

        return Ok(new ApiResponse { Success = true, Message = "Film deleted" });
    }

    [HttpPost("upload")]
    public async Task<ActionResult<ApiResponse<string>>> UploadPoster(
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new ApiResponse { Success = false, Message = "No file provided" });

        if (!_fileStorageService.ValidateImageFile(file, out var errorMessage))
            return BadRequest(new ApiResponse { Success = false, Message = errorMessage });

        try
        {
            using (var stream = file.OpenReadStream())
            {
                var filePath = await _fileStorageService.SavePhotoAsync(stream, file.FileName, cancellationToken);
                return Ok(new ApiResponse<string>
                {
                    Success = true,
                    Message = "File uploaded successfully",
                    Data = filePath
                });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse
            {
                Success = false,
                Message = $"Upload failed: {ex.Message}"
            });
        }
    }

    [HttpGet("{id}/gallery")]
    public async Task<ActionResult<ApiResponse<List<FilmGalleryImageDto>>>> GetGallery(
        int id,
        CancellationToken cancellationToken = default)
    {
        var images = await _filmService.GetGalleryImagesAsync(id, cancellationToken);
        return Ok(new ApiResponse<List<FilmGalleryImageDto>>
        {
            Success = true,
            Message = "Gallery retrieved",
            Data = images
        });
    }

    [HttpPost("{id}/gallery")]
    public async Task<ActionResult<ApiResponse<FilmGalleryImageDto>>> AddGalleryImage(
        int id,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new ApiResponse { Success = false, Message = "No file provided" });

        if (!_fileStorageService.ValidateImageFile(file, out var errorMessage))
            return BadRequest(new ApiResponse { Success = false, Message = errorMessage });

        try
        {
            string imagePath;
            using (var stream = file.OpenReadStream())
            {
                imagePath = await _fileStorageService.SavePhotoAsync(stream, file.FileName, cancellationToken);
            }

            var image = await _filmService.AddGalleryImageAsync(id, imagePath, cancellationToken);
            if (image == null)
                return NotFound(new ApiResponse { Success = false, Message = "Film not found" });

            return Ok(new ApiResponse<FilmGalleryImageDto>
            {
                Success = true,
                Message = "Photo added",
                Data = image
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ApiResponse
            {
                Success = false,
                Message = $"Upload failed: {ex.Message}"
            });
        }
    }

    [HttpDelete("{id}/gallery/{imageId}")]
    public async Task<ActionResult<ApiResponse>> DeleteGalleryImage(
        int id,
        int imageId,
        CancellationToken cancellationToken = default)
    {
        var imagePath = await _filmService.DeleteGalleryImageAsync(id, imageId, cancellationToken);
        if (imagePath == null)
            return NotFound(new ApiResponse { Success = false, Message = "Photo not found" });

        await _fileStorageService.DeleteFileAsync(imagePath, cancellationToken);
        return Ok(new ApiResponse { Success = true, Message = "Photo deleted" });
    }

    [HttpPut("{id}/gallery/reorder")]
    public async Task<ActionResult<ApiResponse>> ReorderGallery(
        int id,
        [FromBody] ReorderGalleryImagesRequest request,
        CancellationToken cancellationToken = default)
    {
        await _filmService.ReorderGalleryImagesAsync(id, request.OrderedImageIds, cancellationToken);
        return Ok(new ApiResponse { Success = true, Message = "Reordered" });
    }
}
