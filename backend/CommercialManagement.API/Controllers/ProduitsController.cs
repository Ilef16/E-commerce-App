using CommercialManagement.API.Constants;
using CommercialManagement.API.DTOs;
using CommercialManagement.API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommercialManagement.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProduitsController(IProduitService produitService) : ControllerBase
{
    /// <summary>Returns a paginated list of products.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProduitDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = BusinessConstants.DefaultPageSize,
        CancellationToken ct = default)
    {
        if (page < 1 || pageSize < 1 || pageSize > BusinessConstants.MaxPageSize)
            return BadRequest(Problem($"page doit être >= 1 et pageSize compris entre 1 et {BusinessConstants.MaxPageSize}."));

        return Ok(await produitService.GetAllAsync(page, pageSize, ct));
    }

    /// <summary>Returns a single product by id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProduitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var produit = await produitService.GetByIdAsync(id, ct);
        return produit is null ? NotFound() : Ok(produit);
    }

    /// <summary>Creates a new product.</summary>
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
            return Conflict(Problem(ex.Message, "Produit déjà existant"));
        }
    }

    /// <summary>Updates an existing product.</summary>
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
            return Conflict(Problem(ex.Message, "Référence déjà utilisée"));
        }
    }

    /// <summary>Deletes a product by id.</summary>
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
        catch (DbUpdateException)
        {
            return Conflict(Problem("Ce produit est utilisé par une commande et ne peut pas être supprimé.", "Produit utilisé"));
        }
    }

    private static ProblemDetails Problem(string detail, string title = "Paramètre invalide") =>
        new() { Title = title, Detail = detail };
}
