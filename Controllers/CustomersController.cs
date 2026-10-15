using Microsoft.AspNetCore.Mvc;
using RemindPe.Models.DTOs;
using RemindPe.Services;

namespace RemindPe.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ITransactionService _transactionService;
    private readonly IDashboardService _dashboardService;

    public CustomersController(
        ICustomerService customerService,
        ITransactionService transactionService,
        IDashboardService dashboardService)
    {
        _customerService = customerService;
        _transactionService = transactionService;
        _dashboardService = dashboardService;
    }

    /// <summary>
    /// GET /api/customers
    /// Retrieves all customer accounts with calculated total credit, total debit, and net balance.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CustomerResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CustomerResponseDto>>> GetCustomers([FromQuery] string? search)
    {
        var result = await _customerService.GetAllCustomersAsync(search);
        return Ok(result);
    }

    /// <summary>
    /// GET /api/customers/stats
    /// Provides aggregated dashboard financial metrics across all customer ledgers.
    /// </summary>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
    {
        var stats = await _dashboardService.GetDashboardStatsAsync();
        return Ok(stats);
    }

    /// <summary>
    /// GET /api/customers/{id}
    /// Retrieves single customer profile with comprehensive ledger transaction history.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CustomerDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerDetailDto>> GetCustomer(int id)
    {
        var detail = await _customerService.GetCustomerByIdAsync(id);
        return Ok(detail);
    }

    /// <summary>
    /// POST /api/customers
    /// Creates a new customer ledger profile.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CustomerResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CustomerResponseDto>> CreateCustomer([FromBody] CreateCustomerDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await _customerService.CreateCustomerAsync(dto);
        return CreatedAtAction(nameof(GetCustomer), new { id = created.Id }, created);
    }

    /// <summary>
    /// PUT /api/customers/{id}
    /// Updates an existing customer profile (name, phone).
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CustomerResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CustomerResponseDto>> UpdateCustomer(int id, [FromBody] UpdateCustomerDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var updated = await _customerService.UpdateCustomerAsync(id, dto);
        return Ok(updated);
    }

    /// <summary>
    /// GET /api/customers/{id}/transactions
    /// Retrieves all ledger transactions for a specific customer account.
    /// </summary>
    [HttpGet("{id:int}/transactions")]
    [ProducesResponseType(typeof(IEnumerable<TransactionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<TransactionResponseDto>>> GetCustomerTransactions(int id)
    {
        var transactions = await _transactionService.GetTransactionsByCustomerIdAsync(id);
        return Ok(transactions);
    }

    /// <summary>
    /// DELETE /api/customers/{id}
    /// Deletes a customer and cascades all associated transactions.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        await _customerService.DeleteCustomerAsync(id);
        return NoContent();
    }
}
