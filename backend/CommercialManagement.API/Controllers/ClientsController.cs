using CommercialManagement.API.Constants;
using CommercialManagement.API.DTOs;
using CommercialManagement.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CommercialManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController(IClientService clientService) : ControllerBase
{
    /// <summary>Returns a paginated list of clients, optionally filtered by a search term.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ClientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = BusinessConstants.DefaultPageSize,
        [FromQuery] string? q = null,
        CancellationToken ct = default)
    {
        if (page < 1)
            return BadRequest(Problem("Le paramètre 'page' doit être supérieur ou égal à 1."));

        if (pageSize < 1 || pageSize > BusinessConstants.MaxPageSize)
            return BadRequest(Problem($"Le paramètre 'pageSize' doit être compris entre 1 et {BusinessConstants.MaxPageSize}."));

        return Ok(await clientService.GetPagedAsync(page, pageSize, q, ct));
    }

    /// <summary>Returns a single client by id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var client = await clientService.GetByIdAsync(id, ct);
        return client is null ? NotFound() : Ok(client);
    }

    /// <summary>Creates a new client.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] ClientWriteDto input, CancellationToken ct)
    {
        try
        {
            var client = await clientService.CreateAsync(input, ct);
            return CreatedAtAction(nameof(GetById), new { id = client.Id }, client);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(Problem(ex.Message, "Client déjà existant"));
        }
    }

    /// <summary>Updates an existing client.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromBody] ClientWriteDto input, CancellationToken ct)
    {
        try
        {
            var client = await clientService.UpdateAsync(id, input, ct);
            return client is null ? NotFound() : Ok(client);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(Problem(ex.Message, "Conflit de données"));
        }
    }

    /// <summary>Deletes a client by id.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        return await clientService.DeleteAsync(id, ct) ? NoContent() : NotFound();
    }

    // Returns a ProblemDetails object with a detail message and optional title.
    private static ProblemDetails Problem(string detail, string title = "Paramètre invalide") =>
        new() { Title = title, Detail = detail };
}
