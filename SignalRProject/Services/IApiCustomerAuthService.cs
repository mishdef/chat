using SignalRProject.DTO;

namespace SignalRProject.Services
{
    public interface IApiCustomerAuthService
    {
        Task<bool> IsEmailExistsAsync(string email);
        
        Task<ClientDTO?> RegisterAsync(RegistrationRequestDTO registrationRequestDTO);

        Task<LoginResponceDTO?> LoginAsync(LoginRequestDTO loginRequestDTO);
    }
}
