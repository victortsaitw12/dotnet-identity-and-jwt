using System;

namespace test.api.Models.DTOs;

public class AuthResponseDto
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    public DateTime? Expiration { get; set; }
}
