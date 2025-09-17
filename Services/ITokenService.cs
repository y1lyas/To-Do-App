using ToDoApp.Domain;

namespace ToDoApp.Services
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}
