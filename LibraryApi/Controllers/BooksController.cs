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

    // Add a new book
    [HttpPost]
    public async Task<ActionResult<BookReadDto>> Add(BookCreateDto dto)
    {
        _logger.LogInformation("POST /api/books called. Title={Title}, Author={Author}", dto.Title, dto.Author);

        if (string.IsNullOrWhiteSpace(dto.Title) ||
            string.IsNullOrWhiteSpace(dto.Author) ||
            string.IsNullOrWhiteSpace(dto.ISBN))
        {
            _logger.LogWarning("Validation failed while creating book");
            return BadRequest(new { message = "Title, Author, and ISBN are required." });
        }

        var book = new Book
        {
            Title = dto.Title.Trim(),
            Author = dto.Author.Trim(),
            ISBN = dto.ISBN.Trim(),
            PublishedDate = dto.PublishedDate
        };

        var created = await _repo.AddAsync(book);

        _logger.LogInformation("Book created successfully with Id={BookId}", created.Id);

        var readDto = new BookReadDto(created.Id, created.Title, created.Author, created.ISBN, created.PublishedDate);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, readDto);
    }


    // Retrieve all books (with optional search & pagination)
    [HttpGet]
    public async Task<ActionResult<List<BookReadDto>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation(
            "GET /api/books called. Search={Search}, Page={PageNumber}, Size={PageSize}",
            search, pageNumber, pageSize
        );

        var books = await _repo.GetAllAsync(search, pageNumber, pageSize);
        var result = books
            .Select(b => new BookReadDto(b.Id, b.Title, b.Author, b.ISBN, b.PublishedDate))
            .ToList();

        _logger.LogInformation("Returned {Count} books", result.Count);

        return Ok(result);
    }

    // Retrieve a single book by Id
    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookReadDto>> GetById(int id)
    {
        _logger.LogInformation("GET /api/books/{Id} called", id);

        var book = await _repo.GetByIdAsync(id);
        if (book is null)
        {
            _logger.LogWarning("Book with Id={Id} not found", id);
            return NotFound(new { message = "Book not found." });
        }

        return Ok(new BookReadDto(
            book.Id,
            book.Title,
            book.Author,
            book.ISBN,
            book.PublishedDate
        ));
    }

    // Update an existing book
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, BookUpdateDto dto)
    {
        _logger.LogInformation("PUT /api/books/{Id} called", id);

        if (string.IsNullOrWhiteSpace(dto.Title) ||
            string.IsNullOrWhiteSpace(dto.Author) ||
            string.IsNullOrWhiteSpace(dto.ISBN))
        {
            _logger.LogWarning("Validation failed while updating book. Id={Id}", id);
            return BadRequest(new { message = "Title, Author, and ISBN are required." });
        }

        var book = await _repo.GetByIdAsync(id);
        if (book is null)
        {
            _logger.LogWarning("Update failed. Book with Id={Id} not found", id);
            return NotFound(new { message = "Book not found." });
        }

        book.Title = dto.Title.Trim();
        book.Author = dto.Author.Trim();
        book.ISBN = dto.ISBN.Trim();
        book.PublishedDate = dto.PublishedDate;

        var ok = await _repo.UpdateAsync(book);
        if (!ok)
        {
            _logger.LogError("Update failed for Book Id={Id}", id);
            return StatusCode(500, new { message = "Update failed." });
        }

        _logger.LogInformation("Book with Id={Id} updated successfully", id);
        return NoContent();
    }


    // Delete a book
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogInformation("DELETE /api/books/{Id} called", id);

        var ok = await _repo.DeleteAsync(id);
        if (!ok)
        {
            _logger.LogWarning("Delete failed. Book with Id={Id} not found", id);
            return NotFound(new { message = "Book not found." });
        }

        _logger.LogInformation("Book with Id={Id} deleted successfully", id);
        return NoContent();
    }
}
