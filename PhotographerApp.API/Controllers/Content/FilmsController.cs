using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PhotographerApp.Core.DTOs;
using PhotographerApp.Core.Interfaces;

namespace PhotographerApp.API.Controllers.Content;

[ApiController]
[Route("api/[controller]")]
public class FilmsController : ControllerBase
{
    private readonly IFilmService _filmService;

    public FilmsController(IFilmService filmService)
    {
        _filmService = filmService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResponse<FilmDto>>>> GetFilms(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _filmService.GetFilmsAsync(page, pageSize, cancellationToken);
        return Ok(new ApiResponse<PaginatedResponse<FilmDto>>
        {
            Success = true,
            Message = "Films retrieved",
            Data = result
        });
    }

    [AllowAnonymous]
    [HttpGet("hero")]
    public async Task<ActionResult<ApiResponse<List<FilmDto>>>> GetHeroFilms(CancellationToken cancellationToken = default)
    {
        var films = await _filmService.GetHeroFilmsAsync(cancellationToken);
        return Ok(new ApiResponse<List<FilmDto>>
        {
            Success = true,
            Message = "Hero films retrieved",
            Data = films
        });
    }

    [AllowAnonymous]
    [HttpGet("map")]
    public async Task<ActionResult<ApiResponse<List<FilmMapDto>>>> GetFilmsForMap(CancellationToken cancellationToken = default)
    {
        var films = await _filmService.GetFilmsForMapAsync(cancellationToken);
        return Ok(new ApiResponse<List<FilmMapDto>>
        {
            Success = true,
            Message = "Map data retrieved",
            Data = films
        });
    }

    [AllowAnonymous]
    [HttpGet("{slug}")]
    public async Task<ActionResult<ApiResponse<FilmDetailDto>>> GetFilmBySlug(string slug, CancellationToken cancellationToken = default)
    {
        var film = await _filmService.GetFilmBySlugAsync(slug, cancellationToken);
        if (film == null)
            return NotFound(new ApiResponse { Success = false, Message = "Film not found" });

        return Ok(new ApiResponse<FilmDetailDto>
        {
            Success = true,
            Message = "Film retrieved",
            Data = film
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<FilmDto>>> CreateFilm(
        [FromBody] CreateFilmRequest request,
        CancellationToken cancellationToken = default)
    {
        var film = await _filmService.CreateFilmAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetFilmBySlug), new { slug = film?.Slug },
            new ApiResponse<FilmDto>
            {
                Success = true,
                Message = "Film created",
                Data = film
            });
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<FilmDto>>> UpdateFilm(
        int id,
        [FromBody] UpdateFilmRequest request,
        CancellationToken cancellationToken = default)
    {
        var film = await _filmService.UpdateFilmAsync(id, request, cancellationToken);
        if (film == null)
            return NotFound(new ApiResponse { Success = false, Message = "Film not found" });

        return Ok(new ApiResponse<FilmDto>
        {
            Success = true,
            Message = "Film updated",
            Data = film
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> DeleteFilm(int id, CancellationToken cancellationToken = default)
    {
        var success = await _filmService.DeleteFilmAsync(id, cancellationToken);
        if (!success)
            return NotFound(new ApiResponse { Success = false, Message = "Film not found" });

        return Ok(new ApiResponse { Success = true, Message = "Film deleted" });
    }
}
