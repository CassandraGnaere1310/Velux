using Microsoft.AspNetCore.Mvc;
using Velux.Api.Services;
using Velux.Shared.Models;

namespace Velux.Api.Controllers;

[ApiController]
[Route("api/alternatives")]
public class AlternativesController(ISupplierService service) : ControllerBase
{
    // GET api/alternatives?categoryId=1
    [HttpGet]
    public async Task<ActionResult<List<MaterialAlternative>>> GetByCategory([FromQuery] int categoryId) =>
        Ok(await service.GetAlternativesAsync(categoryId));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MaterialAlternative>> Get(int id)
    {
        var alternative = await service.GetAlternativeAsync(id);
        return alternative is null ? NotFound() : Ok(alternative);
    }

    [HttpPost]
    public async Task<ActionResult<MaterialAlternative>> Create(MaterialAlternative alternative)
    {
        var errors = await service.ValidateAsync(alternative);
        if (errors.Count > 0) return BadRequest(errors);

        var created = await service.CreateAlternativeAsync(alternative);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<MaterialAlternative>> Update(int id, MaterialAlternative alternative)
    {
        var errors = await service.ValidateAsync(alternative);
        if (errors.Count > 0) return BadRequest(errors);

        var updated = await service.UpdateAlternativeAsync(id, alternative);
        return updated is null ? NotFound() : Ok(updated);
    }
}
