using SignalRProject.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SignalRProject.DTO;
using SignalRProject.Data;
using SignalRProject.Model;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SignalRProject.Services
{
    public class ApiCustomerAuthService : IApiCustomerAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;

        public ApiCustomerAuthService(IConfiguration configuration, UserManager<User> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }

        public Task<bool> IsEmailExistsAsync(string email)
        {
            return _userManager.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<LoginResponceDTO?> LoginAsync(LoginRequestDTO loginRequestDTO)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(loginRequestDTO.Login)
                   ?? await _userManager.FindByNameAsync(loginRequestDTO.Login)
                   ?? await _userManager.Users.FirstOrDefaultAsync(u => u.NickName == loginRequestDTO.Login);

                if (user == null || !await _userManager.CheckPasswordAsync(user, loginRequestDTO.Password))
                {
                    throw new InvalidOperationException("Login or password is incorrect");
                }

                var token = GenerateJwtToken(user);

                return new LoginResponceDTO()
                {
                    User = new ClientDTO()
                    {
                        ProfilePictureUrl = user.ProfilePictureUrl,
                        Username = user.NickName,
                        Email = user.Email,
                    },
                    Token = token
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to login the user. Inner: {ex.Message}", ex);
            }
        }

        public async Task<ClientDTO?> RegisterAsync(RegistrationRequestDTO registrationRequestDTO)
        {
            try
            {
                if (await IsEmailExistsAsync(registrationRequestDTO.Email) == true || registrationRequestDTO.Email == null)
                {
                    throw new InvalidOperationException($"User with email {registrationRequestDTO.Email} already exists");
                }

                var dto = registrationRequestDTO;

                User user = new User()
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    NickName = dto.Nickname
                };

                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Validation failed: {errors}");
                }

                return new ClientDTO()
                {
                    ProfilePictureUrl = user.ProfilePictureUrl,
                    Username = user.NickName,
                    Email = user.Email
                };
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to register user: {ex.Message}", ex);
            }
        }

        private string GenerateJwtToken(User user)
        {
            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("JwtSettings")["Secret"]);

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.UserName ?? user.Email)
                }),
                Expires = DateTime.UtcNow.AddDays(90),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
