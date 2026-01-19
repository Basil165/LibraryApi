using LibraryApi.Domain;

namespace LibraryApi.Repositories;

public interface IBookRepository
{
    Task<Book> AddAsync(Book book);
    Task<List<Book>> GetAllAsync(string? search, int pageNumber, int pageSize);
    Task<Book?> GetByIdAsync(int id);
    Task<bool> UpdateAsync(Book book);
    Task<bool> DeleteAsync(int id);
}
