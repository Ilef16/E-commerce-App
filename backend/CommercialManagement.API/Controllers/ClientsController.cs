using CommercialManagement.API.DTOs;
using CommercialManagement.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CommercialManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController(IClientService clientService) : ControllerBase
{
    /// <summary>
    /// Retourne la liste paginée des clients.
    /// </summary>
    /// <param name="page">Numéro de page (défaut : 1)</param>
    /// <param name="pageSize">Nombre d'éléments par page (défaut : 20, max : 100)</param>
    /// <param name="q">Terme de recherche (nom, prénom ou email)</param>
    /// <param name="ct">Jeton d'annulation</param>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ClientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? q = null,
        CancellationToken ct = default)
    {
        if (page < 1)
            return BadRequest(new ProblemDetails
            {
                Title = "Paramètre invalide",
                Detail = "Le paramètre 'page' doit être supérieur ou égal à 1.",
                Status = StatusCodes.Status400BadRequest
            });

        if (pageSize < 1 || pageSize > 100)
            return BadRequest(new ProblemDetails
            {
                Title = "Paramètre invalide",
                Detail = "Le paramètre 'pageSize' doit être compris entre 1 et 100.",
                Status = StatusCodes.Status400BadRequest
            });

        var result = await clientService.GetPagedAsync(page, pageSize, q, ct);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var client = await clientService.GetByIdAsync(id, ct);
        return client is null ? NotFound() : Ok(client);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] ClientWriteDto input, CancellationToken ct)
    {
        try
        {
            var client = await clientService.CreateAsync(input, ct);
            return CreatedAtAction(nameof(GetById), new { id = client.Id }, client);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails { Title = "Client déjà existant", Detail = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] ClientWriteDto input, CancellationToken ct)
    {
        try
        {
            var client = await clientService.UpdateAsync(id, input, ct);
            return client is null ? NotFound() : Ok(client);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails { Title = "Email déjà utilisé", Detail = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        return await clientService.DeleteAsync(id, ct) ? NoContent() : NotFound();
    }
}
