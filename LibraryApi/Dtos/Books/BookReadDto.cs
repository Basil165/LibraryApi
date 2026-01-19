namespace LibraryApi.Dtos.Books;

public record BookReadDto(int Id, string Title, string Author, string ISBN, DateTime PublishedDate);
