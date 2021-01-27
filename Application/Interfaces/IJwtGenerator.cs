using Domain;

namespace Services.Interfaces
{
    public interface IJwtGenerator
    {
        string CreateToken(AppUser user);
    }
}