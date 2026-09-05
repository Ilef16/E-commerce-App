using CommercialManagement.API.DTOs;
using CommercialManagement.API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CommercialManagement.API.Controllers;

[ApiController]
[Route("api/products")]
public class ProduitsController(IProduitService produitService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProduitDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct) => Ok(await produitService.GetAllAsync(ct));

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
    public async Task<IActionResult> Create(ProduitWriteDto input, CancellationToken ct)
    {
        try
        {
            var produit = await produitService.CreateAsync(input, ct);
            return CreatedAtAction(nameof(GetById), new { id = produit.Id }, produit);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails { Title = "Produit déjà existant", Detail = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProduitDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, ProduitWriteDto input, CancellationToken ct)
    {
        try
        {
            var produit = await produitService.UpdateAsync(id, input, ct);
            return produit is null ? NotFound() : Ok(produit);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails { Title = "Référence déjà utilisée", Detail = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        try
        {
            return await produitService.DeleteAsync(id, ct) ? NoContent() : NotFound();
        }
        catch (DbUpdateException)
        {
            return Conflict(new ProblemDetails { Title = "Produit utilisé", Detail = "Ce produit est utilisé par une commande." });
        }
    }
}
