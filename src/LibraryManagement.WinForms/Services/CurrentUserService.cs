using LibraryManagement.Application.Abstractions;
using LibraryManagement.Domain.Enums;

namespace LibraryManagement.WinForms.Services;

// Автоматически входит как администратор — авторизация не требуется.
public class CurrentUserService : ICurrentUserService
{
    public int? UserId { get; private set; }
    public string? Username { get; private set; }
    public UserRole? Role { get; private set; }
    public bool IsAuthenticated => UserId.HasValue;

    public CurrentUserService()
    {
        UserId = 1;
        Username = "admin";
        Role = UserRole.Administrator;
    }

    public void SignIn(int userId, string username, UserRole role)
    {
        UserId = userId;
        Username = username;
        Role = role;
    }

    public void SignOut()
    {
        UserId = null;
        Username = null;
        Role = null;
    }
}
