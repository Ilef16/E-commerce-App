using CommercialManagement.API.DTOs;
using CommercialManagement.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CommercialManagement.API.Controllers;

[ApiController]
[Route("api/orders")]
public class CommandesController(ICommandeService commandeService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CommandeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct) => Ok(await commandeService.GetAllAsync(ct));

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
    public async Task<IActionResult> Create(CommandeWriteDto input, CancellationToken ct)
    {
        try
        {
            var commande = await commandeService.CreateAsync(input, ct);
            return CreatedAtAction(nameof(GetById), new { id = commande.Id }, commande);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails { Title = "Commande invalide", Detail = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CommandeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, CommandeWriteDto input, CancellationToken ct)
    {
        try
        {
            var commande = await commandeService.UpdateAsync(id, input, ct);
            return commande is null ? NotFound() : Ok(commande);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails { Title = "Commande invalide", Detail = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        try
        {
            return await commandeService.DeleteAsync(id, ct) ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails { Title = "Commande non supprimable", Detail = ex.Message });
        }
    }

    [HttpPost("{id:int}/validate")]
    [ProducesResponseType(typeof(CommandeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Validate(int id, CancellationToken ct)
    {
        try
        {
            var commande = await commandeService.ValidateAsync(id, ct);
            return commande is null ? NotFound() : Ok(commande);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails { Title = "Commande non valide", Detail = ex.Message });
        }
    }
}
