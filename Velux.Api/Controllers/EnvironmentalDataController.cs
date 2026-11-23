using Microsoft.AspNetCore.Mvc;
using Velux.Api.Services;
using Velux.Shared.Models;

namespace Velux.Api.Controllers;

[ApiController]
[Route("api/environmentaldata")]
public class EnvironmentalDataController(ISupplierService service, IWebHostEnvironment environment) : ControllerBase
{
    // POST api/environmentaldata/5/documents  (multipart form: file, title, documentType)
    [HttpPost("{id:int}/documents")]
    [RequestSizeLimit(20_000_000)]
    public async Task<ActionResult<EnvironmentalDocument>> UploadDocument(
        int id,
        IFormFile file,
        [FromForm] string? title,
        [FromForm] DocumentType documentType = DocumentType.EPD)
    {
        if (file is null || file.Length == 0) return BadRequest(new[] { "No file was uploaded." });

        var folder = Path.Combine(environment.ContentRootPath, "uploads");
        Directory.CreateDirectory(folder);

        var storedName = $"{Guid.NewGuid():N}_{Path.GetFileName(file.FileName)}";
        await using (var stream = System.IO.File.Create(Path.Combine(folder, storedName)))
        {
            await file.CopyToAsync(stream);
        }

        var document = await service.AddDocumentAsync(id, title ?? file.FileName, documentType, Path.Combine("uploads", storedName));
        return document is null ? NotFound() : Ok(document);
    }
}
