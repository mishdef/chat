using Microsoft.AspNetCore.Mvc;
using SignalRProject.DTO;
using SignalRProject.Services;

namespace SignalRProject.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IApiCustomerAuthService _authService;

        public AuthController(IApiCustomerAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<ClientDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<ClientDTO>>> Register([FromBody] RegistrationRequestDTO user)
        {
            try
            {
                if (user == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Registration data is required"));
                }

                if (await _authService.IsEmailExistsAsync(user.Email))
                {
                    return Conflict(ApiResponse<object>.Conflict("User with this phone number already exists"));
                }

                var newUser = await _authService.RegisterAsync(user);

                if (newUser == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Registration failed"));
                }

                var responce = ApiResponse<ClientDTO>.Created(newUser, "User created sucsessfuly");

                return CreatedAtAction(nameof(Register), responce);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.BadRequest(ex.Message));
            }
            catch (Exception ex)
            {
                var errorResponce = ApiResponse<object>.InternalServerError(ex, "Unexpected error while registrating the user");

                return StatusCode(500, errorResponce);
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<LoginRequestDTO>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<LoginResponceDTO>>> Login([FromBody] LoginRequestDTO user)
        {
            try
            {
                if (user == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Login data is required"));
                }

                var loginResponse = await _authService.LoginAsync(user);

                if (loginResponse == null)
                {
                    return BadRequest(ApiResponse<object>.BadRequest("Login failed"));
                }

                var responce = ApiResponse<LoginResponceDTO>.Ok(loginResponse, "Login successful");

                return Ok(responce);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.BadRequest(ex.Message));
            }
            catch (Exception ex)
            {
                var errorResponce = ApiResponse<object>.InternalServerError(ex, "Unexpected error while login the user");

                return StatusCode(500, errorResponce);
            }
        }
    }
}
