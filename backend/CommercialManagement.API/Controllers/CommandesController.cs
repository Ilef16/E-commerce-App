using CommercialManagement.API.Constants;
using CommercialManagement.API.DTOs;
using CommercialManagement.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CommercialManagement.API.Controllers;

[ApiController]
[Route("api/orders")]
public class CommandesController(ICommandeService commandeService) : ControllerBase
{
    /// <summary>Returns a paginated list of orders, most recent first.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CommandeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = BusinessConstants.DefaultPageSize,
        CancellationToken ct = default)
    {
        if (page < 1 || pageSize < 1 || pageSize > BusinessConstants.MaxPageSize)
            return BadRequest(Problem($"page >= 1 et pageSize entre 1 et {BusinessConstants.MaxPageSize}.", "Paramètres invalides"));

        return Ok(await commandeService.GetPagedAsync(page, pageSize, ct));
    }

    /// <summary>Returns a single order by id, including its lines.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CommandeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var commande = await commandeService.GetByIdAsync(id, ct);
        return commande is null ? NotFound() : Ok(commande);
    }

    /// <summary>Creates a new order in Brouillon status.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CommandeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CommandeWriteDto input, CancellationToken ct)
    {
        try
        {
            var commande = await commandeService.CreateAsync(input, ct);
            return CreatedAtAction(nameof(GetById), new { id = commande.Id }, commande);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(Problem(ex.Message, "Commande invalide"));
        }
    }

    /// <summary>Updates a Brouillon order. Validated or cancelled orders cannot be modified.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CommandeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, CommandeWriteDto input, CancellationToken ct)
    {
        try
        {
            var commande = await commandeService.UpdateAsync(id, input, ct);
            return commande is null ? NotFound() : Ok(commande);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(Problem(ex.Message, "Modification impossible"));
        }
    }

    /// <summary>Deletes a Brouillon order. Validated or cancelled orders cannot be deleted.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        try
        {
            return await commandeService.DeleteAsync(id, ct) ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(Problem(ex.Message, "Suppression impossible"));
        }
    }

    /// <summary>
    /// Validates a Brouillon order: checks stock availability then decrements stock atomically.
    /// </summary>
    [HttpPost("{id:int}/validate")]
    [ProducesResponseType(typeof(CommandeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Validate(int id, CancellationToken ct)
    {
        try
        {
            var commande = await commandeService.ValidateAsync(id, ct);
            return commande is null ? NotFound() : Ok(commande);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(Problem(ex.Message, "Validation impossible"));
        }
    }

    private static ProblemDetails Problem(string detail, string title) =>
        new() { Title = title, Detail = detail };
}
