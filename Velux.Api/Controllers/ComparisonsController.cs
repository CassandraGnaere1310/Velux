using Microsoft.AspNetCore.Mvc;
using Velux.Api.Services;
using Velux.Shared.Models;

namespace Velux.Api.Controllers;

[ApiController]
[Route("api/comparisons")]
public class ComparisonsController(IComparisonService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<SupplierComparison>>> GetAll() => Ok(await service.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SupplierComparison>> Get(int id)
    {
        var comparison = await service.GetAsync(id);
        return comparison is null ? NotFound() : Ok(comparison);
    }

    // Calculate without saving
    [HttpPost("calculate")]
    public async Task<ActionResult<SupplierComparison>> Calculate(SupplierComparison comparison)
    {
        var errors = await service.ValidateAsync(comparison, forSaving: false);
        if (errors.Count > 0) return BadRequest(errors);

        return Ok(await service.CalculateAsync(comparison));
    }

    // Calculate and save
    [HttpPost]
    public async Task<ActionResult<SupplierComparison>> Save(SupplierComparison comparison)
    {
        var errors = await service.ValidateAsync(comparison, forSaving: true);
        if (errors.Count > 0) return BadRequest(errors);

        var saved = await service.SaveAsync(comparison);
        return CreatedAtAction(nameof(Get), new { id = saved.Id }, saved);
    }
}
