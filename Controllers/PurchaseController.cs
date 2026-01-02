using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using purchase_service.Models.DTOs;
using purchase_service.Models.DTOs.Validators;
using purchase_service.Services;

namespace purchase_service.Controllers;

[ApiController]
[Route("purchases")]
public class PurchaseController : ControllerBase
{
    private readonly IPurchaseService _purchaseService;
    private readonly ILogger<PurchaseController> _logger;
    private readonly IValidator<CreatePurchaseRequest> _createValidator;
    private readonly IValidator<UpdatePurchaseStatusRequest> _updateValidator;

    public PurchaseController(
        IPurchaseService purchaseService,
        ILogger<PurchaseController> logger,
        IValidator<CreatePurchaseRequest> createValidator,
        IValidator<UpdatePurchaseStatusRequest> updateValidator)
    {
        _purchaseService = purchaseService;
        _logger = logger;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>
    /// Creates a new purchase
    /// </summary>
    /// <param name="request">Purchase creation request</param>
    /// <returns>Purchase ID</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PurchaseResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreatePurchase([FromBody] CreatePurchaseRequest request)
    {
        // Validate request
        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", errors));
        }

        try
        {
            var response = await _purchaseService.CreatePurchaseAsync(request);
            return CreatedAtAction(
                nameof(CreatePurchase),
                new { purchaseId = response.PurchaseId },
                ApiResponse<PurchaseResponse>.SuccessResponse(response, "Purchase created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while creating purchase");
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating purchase");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while creating the purchase"));
        }
    }

    /// <summary>
    /// Updates the status of a purchase
    /// </summary>
    /// <param name="purchaseId">Purchase ID</param>
    /// <param name="request">Status update request</param>
    /// <returns>Success response</returns>
    [HttpPut("{purchaseId}/status")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdatePurchaseStatus(int purchaseId, [FromBody] UpdatePurchaseStatusRequest request)
    {
        // Validate request
        var validationResult = await _updateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<object>.ErrorResponse("Validation failed", errors));
        }

        try
        {
            await _purchaseService.UpdatePurchaseStatusAsync(purchaseId, request);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Purchase status updated successfully"));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation while updating purchase status");
            
            // Check if it's a not found error
            if (ex.Message.Contains("does not exist"))
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message));
            }
            
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating purchase status");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while updating the purchase status"));
        }
    }
}

