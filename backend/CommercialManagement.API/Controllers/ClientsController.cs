using CommercialManagement.API.Constants;
using CommercialManagement.API.DTOs;
using CommercialManagement.API.Helpers;
using CommercialManagement.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CommercialManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientsController(IClientService clientService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ClientDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = BusinessConstants.DefaultPageSize,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var paginationError = Pagination.Validate(page, pageSize);
        if (paginationError is not null)
            return BadRequest(ApiProblem.Create(paginationError, "Paramètres invalides", StatusCodes.Status400BadRequest));

        return Ok(await clientService.GetPagedAsync(page, pageSize, search, ct));
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
            return Conflict(ApiProblem.Create(ex.Message, "Client déjà existant", StatusCodes.Status409Conflict));
        }
    }

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
            return Conflict(ApiProblem.Create(ex.Message, "Conflit de données", StatusCodes.Status409Conflict));
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        try
        {
            return await clientService.DeleteAsync(id, ct) ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiProblem.Create(ex.Message, "Suppression impossible", StatusCodes.Status409Conflict));
        }
    }
}
