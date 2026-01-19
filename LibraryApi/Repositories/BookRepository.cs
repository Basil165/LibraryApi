using LibraryApi.Data;
using LibraryApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Repositories;

public class BookRepository : IBookRepository
{
    private readonly AppDbContext _db;

    public BookRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Book> AddAsync(Book book)
    {
        _db.Books.Add(book);
        await _db.SaveChangesAsync();
        return book;
    }

    public async Task<List<Book>> GetAllAsync(string? search, int pageNumber, int pageSize)
    {
        var query = _db.Books.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(b => b.Title.Contains(search) || b.Author.Contains(search));
        }

        //  pagination
        pageNumber = pageNumber < 1 ? 1 : pageNumber;
        pageSize = pageSize < 1 ? 10 : pageSize;
        pageSize = pageSize > 100 ? 100 : pageSize;

        return await query
            .OrderBy(b => b.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public Task<Book?> GetByIdAsync(int id)
    {
        return _db.Books.FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<bool> UpdateAsync(Book book)
    {
        _db.Books.Update(book);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var book = await _db.Books.FirstOrDefaultAsync(b => b.Id == id);
        if (book is null) return false;

        _db.Books.Remove(book);
        return await _db.SaveChangesAsync() > 0;
    }
}
