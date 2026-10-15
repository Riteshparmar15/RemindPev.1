using Microsoft.AspNetCore.Mvc;
using RemindPe.Models.DTOs;
using RemindPe.Services;

namespace RemindPe.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    /// <summary>
    /// GET /api/transactions
    /// Retrieves a paginated and filterable list of all transactions across customers.
    /// Supports customerId, type, date range, page, and pageSize filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<TransactionDetailDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResultDto<TransactionDetailDto>>> GetTransactions([FromQuery] TransactionFilterDto filter)
    {
        var result = await _transactionService.GetTransactionsAsync(filter);
        return Ok(result);
    }

    /// <summary>
    /// GET /api/transactions/{id}
    /// Retrieves a single transaction record by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TransactionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionResponseDto>> GetTransaction(int id)
    {
        var tx = await _transactionService.GetTransactionByIdAsync(id);
        return Ok(tx);
    }

    /// <summary>
    /// POST /api/transactions
    /// Adds a new Credit (Maine Diye / Udhar) or Debit (Maine Liye / Jama) transaction entry.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TransactionResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TransactionResponseDto>> CreateTransaction([FromBody] CreateTransactionDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var created = await _transactionService.CreateTransactionAsync(dto);
        return CreatedAtAction(nameof(GetTransaction), new { id = created.Id }, created);
    }

    /// <summary>
    /// DELETE /api/transactions/{id}
    /// Deletes a specific transaction from the ledger.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTransaction(int id)
    {
        await _transactionService.DeleteTransactionAsync(id);
        return NoContent();
    }
}
