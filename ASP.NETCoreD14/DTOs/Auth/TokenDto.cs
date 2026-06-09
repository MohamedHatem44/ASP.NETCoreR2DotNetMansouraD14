namespace ASP.NETCoreD14.DTOs.Auth
{
    public record TokenDto(string AccessToken, int DurationInMinutes, string TokenType = "Bearer");
}
