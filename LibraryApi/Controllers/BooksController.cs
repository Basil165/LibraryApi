using LibraryApi.Domain;
using LibraryApi.Dtos.Books;
using LibraryApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryApi.Controllers;

[ApiController]
[Route("api/books")]
[Authorize]

public class BooksController : ControllerBase
{
    private readonly IBookRepository _repo;
    private readonly ILogger<BooksController> _logger;

    public BooksController(IBookRepository repo, ILogger<BooksController> logger)
    {
        _repo = repo;
        _logger = logger;
    }


    [HttpPost]
    public async Task<ActionResult<BookReadDto>> Add(BookCreateDto dto)
    {
        _logger.LogInformation("POST /api/books called. Title={Title}, Author={Author}", dto.Title, dto.Author);
        if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Author))
            return BadRequest(new { message = "Title and Author are required." });

        var book = new Book
        {
            Title = dto.Title.Trim(),
            Author = dto.Author.Trim(),
            ISBN = dto.ISBN.Trim(),
            PublishedDate = dto.PublishedDate
        };

        var created = await _repo.AddAsync(book);
        var readDto = new BookReadDto(created.Id, created.Title, created.Author, created.ISBN, created.PublishedDate);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, readDto);
    }

    [HttpGet]
    public async Task<ActionResult<List<BookReadDto>>> GetAll([FromQuery] string? search, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var books = await _repo.GetAllAsync(search, pageNumber, pageSize);
        var result = books.Select(b => new BookReadDto(b.Id, b.Title, b.Author, b.ISBN, b.PublishedDate)).ToList();
        _logger.LogInformation("Returned {Count} books", result.Count);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookReadDto>> GetById(int id)
    {
        var book = await _repo.GetByIdAsync(id);
        if (book is null) return NotFound(new { message = "Book not found." });

        return Ok(new BookReadDto(book.Id, book.Title, book.Author, book.ISBN, book.PublishedDate));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, BookUpdateDto dto)
    {
        var book = await _repo.GetByIdAsync(id);
        if (book is null) return NotFound(new { message = "Book not found." });

        book.Title = dto.Title.Trim();
        book.Author = dto.Author.Trim();
        book.ISBN = dto.ISBN.Trim();
        book.PublishedDate = dto.PublishedDate;

        var ok = await _repo.UpdateAsync(book);
        if (!ok) return StatusCode(500, new { message = "Update failed." });

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _repo.DeleteAsync(id);
        if (!ok) return NotFound(new { message = "Book not found." });

        return NoContent();
    }
}
