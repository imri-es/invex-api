using Google.Apis.Auth;
using invex_api.DTOs;
using invex_api.Enums;
using invex_api.Models;
using invex_api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace invex_api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly TokenService _tokenService;
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        TokenService tokenService,
        IHttpClientFactory httpClientFactory
    )
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null)
            return Unauthorized("Invalid username or password");

        var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

        if (!result.Succeeded)
            return Unauthorized("Invalid username or password");

        return Ok(CreateAuthResponse(user));
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto model)
    {
        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName,
            AccessRole = UserRole.Newbie,
        };

        var result = await _userManager.CreateAsync(user, model.Password);

        if (result.Succeeded)
        {
            return Ok(CreateAuthResponse(user));
        }

        return BadRequest(result.Errors);
    }

    private class SocialPayload
    {
        public required string Email { get; set; }
        public required string Name { get; set; }
        public required string Subject { get; set; }
    }

    private class GoogleUserDto
    {
        public string Sub { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Name { get; set; }
    }

    [HttpPost("auth-social")]
    public async Task<IActionResult> AuthSocial([FromBody] AuthSocialDto model)
    {
        SocialPayload? payload = null; // Changed type from GoogleJsonWebSignature.Payload?

        if (model.Provider.ToLower() == "google")
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", model.Token);

            var response = await client.GetAsync("https://www.googleapis.com/oauth2/v3/userinfo");

            if (!response.IsSuccessStatusCode)
                return BadRequest("Invalid Google Token");

            var googleUser = await response.Content.ReadFromJsonAsync<GoogleUserDto>();
            if (googleUser == null || string.IsNullOrEmpty(googleUser.Email))
                return BadRequest("Could not retrieve email from Google");

            payload = new SocialPayload
            {
                Email = googleUser.Email,
                Name = googleUser.Name ?? googleUser.Email,
                Subject = googleUser.Sub,
            };
        }
        else if (model.Provider.ToLower() == "facebook")
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(
                $"https://graph.facebook.com/me?fields=id,name,email&access_token={model.Token}"
            );

            if (!response.IsSuccessStatusCode)
                return BadRequest("Invalid Facebook Token");

            var fbUser = await response.Content.ReadFromJsonAsync<FacebookUserDto>();
            if (fbUser == null || string.IsNullOrEmpty(fbUser.Email))
                return BadRequest("Could not retrieve email from Facebook");

            payload = new SocialPayload
            {
                Email = fbUser.Email,
                Name = fbUser.Name,
                Subject = fbUser.Id,
            };
        }
        else if (model.Provider.ToLower() == "github")
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("invex-api");
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", model.Token);

            var response = await client.GetAsync("https://api.github.com/user");
            if (!response.IsSuccessStatusCode)
                return BadRequest("Invalid GitHub Token");

            var ghUser = await response.Content.ReadFromJsonAsync<GitHubUserDto>();
            if (ghUser == null)
                return BadRequest("Invalid GitHub User");

            if (string.IsNullOrEmpty(ghUser.Email))
            {
                // Fetch emails if missing (private)
                var emailResponse = await client.GetAsync("https://api.github.com/user/emails");
                if (emailResponse.IsSuccessStatusCode)
                {
                    var emails = await emailResponse.Content.ReadFromJsonAsync<
                        List<GitHubEmailDto>
                    >();
                    var primary =
                        emails?.FirstOrDefault(e => e.Primary && e.Verified)
                        ?? emails?.FirstOrDefault(e => e.Verified);
                    if (primary != null)
                        ghUser.Email = primary.Email;
                }
            }

            if (string.IsNullOrEmpty(ghUser.Email))
                return BadRequest("Could not retrieve verified email from GitHub");

            payload = new SocialPayload
            {
                Email = ghUser.Email,
                Name = ghUser.Name ?? ghUser.Login,
                Subject = ghUser.Id.ToString(),
            };
        }
        else
        {
            return BadRequest("Provider not supported");
        }

        if (payload == null)
            return BadRequest("Invalid Token Payload");

        var user = await _userManager.FindByEmailAsync(payload.Email);

        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = payload.Email,
                Email = payload.Email,
                FullName = payload.Name,
                SocialNetworkType = model.Provider,
                SocialNetworkId = payload.Subject,
                AccessRole = UserRole.Newbie,
            };

            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);
        }
        else
        {
            // Update existing user info if needed
            user.LastLogin = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
        }

        return Ok(CreateAuthResponse(user));
    }

    [HttpPost("reset-password")]
    public IActionResult ResetPassword()
    {
        return Ok();
    }

    private AuthResponseDto CreateAuthResponse(ApplicationUser user)
    {
        return new AuthResponseDto
        {
            Token = _tokenService.CreateToken(user),
            Email = user.Email ?? "",
            FullName = user.FullName,
            AccessRole = user.AccessRole,
        };
    }
}
