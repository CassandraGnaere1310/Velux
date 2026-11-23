using Microsoft.AspNetCore.Mvc;
using Velux.Api.Services;
using Velux.Shared.Models;

namespace Velux.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController(ISupplierService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<MaterialCategory>>> GetAll() => Ok(await service.GetCategoriesAsync());
}
