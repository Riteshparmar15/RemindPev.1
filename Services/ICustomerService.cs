using RemindPe.Models.DTOs;

namespace RemindPe.Services;

public interface ICustomerService
{
    Task<IEnumerable<CustomerResponseDto>> GetAllCustomersAsync(string? search = null);
    Task<CustomerDetailDto> GetCustomerByIdAsync(int id);
    Task<CustomerResponseDto> CreateCustomerAsync(CreateCustomerDto dto);
    Task<CustomerResponseDto> UpdateCustomerAsync(int id, UpdateCustomerDto dto);
    Task DeleteCustomerAsync(int id);
}
