namespace LibraryApi.Dtos.Books;

public record BookUpdateDto(string Title, string Author, string ISBN, DateTime PublishedDate);
