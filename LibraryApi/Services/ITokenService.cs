using LibraryApi.Domain;

namespace LibraryApi.Services;

public interface ITokenService
{
    string CreateToken(AppUser user);
}
