using Microsoft.AspNetCore.Mvc;
using Velux.Api.Services;
using Velux.Shared.Models;

namespace Velux.Api.Controllers;

[ApiController]
[Route("api/suppliers")]
public class SuppliersController(ISupplierService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Supplier>>> GetAll() => Ok(await service.GetSuppliersAsync());
}
