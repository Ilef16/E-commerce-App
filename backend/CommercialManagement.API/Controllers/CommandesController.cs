using CommercialManagement.API.Constants;
using CommercialManagement.API.DTOs;
using CommercialManagement.API.Helpers;
using CommercialManagement.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CommercialManagement.API.Controllers;

[ApiController]
[Route("api/orders")]
public class CommandesController(ICommandeService commandeService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CommandeDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = BusinessConstants.DefaultPageSize,
        CancellationToken ct = default)
    {
        var paginationError = Pagination.Validate(page, pageSize);
        if (paginationError is not null)
            return BadRequest(ApiProblem.Create(paginationError, "Paramètres invalides", StatusCodes.Status400BadRequest));

        return Ok(await commandeService.GetPagedAsync(page, pageSize, ct));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CommandeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var commande = await commandeService.GetByIdAsync(id, ct);
        return commande is null ? NotFound() : Ok(commande);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CommandeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CommandeWriteDto input, CancellationToken ct)
    {
        return await Execute(async () => await commandeService.CreateAsync(input, ct), created: true);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CommandeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(int id, CommandeWriteDto input, CancellationToken ct)
    {
        return await Execute(() => commandeService.UpdateAsync(id, input, ct));
    }

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
            return BadRequest(ApiProblem.Create(ex.Message, "Suppression impossible", StatusCodes.Status400BadRequest));
        }
    }

    [HttpPost("{id:int}/validate")]
    [ProducesResponseType(typeof(CommandeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Validate(int id, CancellationToken ct)
    {
        return await Execute(() => commandeService.ValidateAsync(id, ct), errorTitle: "Validation impossible");
    }

    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(typeof(CommandeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel(int id, CancellationToken ct)
    {
        return await Execute(() => commandeService.CancelAsync(id, ct), errorTitle: "Annulation impossible");
    }

    private async Task<IActionResult> Execute(
        Func<Task<CommandeDto?>> action,
        bool created = false,
        string errorTitle = "Commande invalide")
    {
        try
        {
            var commande = await action();
            if (commande is null) return NotFound();
            return created
                ? CreatedAtAction(nameof(GetById), new { id = commande.Id }, commande)
                : Ok(commande);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiProblem.Create(ex.Message, errorTitle, StatusCodes.Status400BadRequest));
        }
    }
}
