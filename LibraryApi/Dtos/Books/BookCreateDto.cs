namespace LibraryApi.Dtos.Books;

public record BookCreateDto(string Title, string Author, string ISBN, DateTime PublishedDate);
