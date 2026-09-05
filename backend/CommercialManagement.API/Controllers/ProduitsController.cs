using CommercialManagement.API.Constants;
using CommercialManagement.API.DTOs;
using CommercialManagement.API.Helpers;
using CommercialManagement.API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CommercialManagement.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProduitsController(IProduitService produitService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProduitDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = BusinessConstants.DefaultPageSize,
        CancellationToken ct = default)
    {
        var paginationError = Pagination.Validate(page, pageSize);
        if (paginationError is not null)
            return BadRequest(ApiProblem.Create(paginationError, "Paramètres invalides", StatusCodes.Status400BadRequest));

        return Ok(await produitService.GetAllAsync(page, pageSize, ct));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProduitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var produit = await produitService.GetByIdAsync(id, ct);
        return produit is null ? NotFound() : Ok(produit);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProduitDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromForm] ProduitWriteDto input, CancellationToken ct)
    {
        try
        {
            var produit = await produitService.CreateAsync(input, ct);
            return CreatedAtAction(nameof(GetById), new { id = produit.Id }, produit);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiProblem.Create(ex.Message, "Produit invalide", StatusCodes.Status409Conflict));
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProduitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(int id, [FromForm] ProduitWriteDto input, CancellationToken ct)
    {
        try
        {
            var produit = await produitService.UpdateAsync(id, input, ct);
            return produit is null ? NotFound() : Ok(produit);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiProblem.Create(ex.Message, "Produit invalide", StatusCodes.Status409Conflict));
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
            return await produitService.DeleteAsync(id, ct) ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ApiProblem.Create(ex.Message, "Produit utilisé", StatusCodes.Status409Conflict));
        }
    }
}
