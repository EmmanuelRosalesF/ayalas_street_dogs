using ayalas_street_dogs.Models;
using ayalas_street_dogs.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace ayalas_street_dogs.Controllers
{
    [Authorize(Roles = "admin")]
    public class InventarioController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<InventarioController> _logger;
        private readonly IWebHostEnvironment _env;
        private readonly ICompositeViewEngine _viewEngine;
        private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;

        public InventarioController(ApplicationDbContext context,
            IWebHostEnvironment env,
            ICompositeViewEngine viewEngine,
            ITempDataDictionaryFactory tempDataDictionaryFactory,
            ILogger<InventarioController> logger)
        {
            _context = context;
            _env = env;
            _viewEngine = viewEngine;
            _tempDataDictionaryFactory = tempDataDictionaryFactory;
            _logger = logger;
        }

        public ITempDataDictionary TempDataDict => _tempDataDictionaryFactory.GetTempData(HttpContext);

        public async Task<IActionResult> Index(bool mostrarInactivos = false)
        {
            var query = _context.Ingrediente.AsQueryable();

            if (!mostrarInactivos)
            {
                query = query.Where(i => i.Activo);
            }

            var viewModel = new InventarioViewModel
            {
                Ingredientes = await query
                    .OrderBy(i => i.Nombre)
                    .ToListAsync(),

                IngredientesBajoStock = await _context.Ingrediente
                    .Where(i => i.Activo && i.StockActual <= i.StockMinimo)
                    .OrderBy(i => i.StockActual)
                    .ToListAsync(),

                ValorTotalInventario = await _context.Ingrediente
                    .Where(i => i.Activo)
                    .SumAsync(i => i.StockActual * i.CostoUnitarioPromedio),

                MostrarInactivos = mostrarInactivos
            };

            return View(viewModel);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Ingrediente ingrediente)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ingrediente.Activo = true;

                    _context.Ingrediente.Add(ingrediente);
                    await _context.SaveChangesAsync();

                    if (ingrediente.StockActual > 0)
                    {
                        var ajuste = new AjusteInventario
                        {
                            IdIngrediente = ingrediente.IdIngrediente,
                            FechaAjuste = DateTime.Now,
                            TipoMovimiento = "INICIAL",
                            Cantidad = ingrediente.StockActual,
                            Motivo = "Inventario inicial",
                            Usuario = User.Identity?.Name ?? "admin"
                        };
                        _context.AjusteInventario.Add(ajuste);
                        await _context.SaveChangesAsync();
                    }

                    TempData["Success"] = $"Ingrediente '{ingrediente.Nombre}' agregado correctamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al crear ingrediente");
                    ModelState.AddModelError("", $"Error al crear ingrediente: {ex.Message}");
                }
            }
            return View(ingrediente);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ingrediente = await _context.Ingrediente.FindAsync(id);
            if (ingrediente == null)
            {
                return NotFound();
            }

            return View(ingrediente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Ingrediente ingrediente)
        {
            if (id != ingrediente.IdIngrediente)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ingrediente);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Ingrediente actualizado correctamente";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!IngredienteExists(ingrediente.IdIngrediente))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return View(ingrediente);
        }
        public async Task<IActionResult> Ajustar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ingrediente = await _context.Ingrediente.FindAsync(id);
            if (ingrediente == null)
            {
                return NotFound();
            }

            var viewModel = new AjusteInventarioViewModel
            {
                IdIngrediente = ingrediente.IdIngrediente,
                NombreIngrediente = ingrediente.Nombre,
                StockActual = ingrediente.StockActual,
                UnidadMedida = ingrediente.UnidadMedida
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ajustar(AjusteInventarioViewModel model)
        {
            _logger.LogDebug("Ajustar POST -> Id:{Id} Tipo:{Tipo} Cantidad:{Cantidad}",
                model.IdIngrediente, model.TipoMovimiento, model.Cantidad);

            if (model.Cantidad <= 0)
            {
                ModelState.AddModelError("Cantidad", "La cantidad debe ser mayor a cero");
            }

            var tipoMovimientoValido = new[] { "ENTRADA", "SALIDA", "INICIAL", "AJUSTE" };
            if (!string.IsNullOrWhiteSpace(model.TipoMovimiento))
            {
                model.TipoMovimiento = model.TipoMovimiento.Trim().ToUpper();
                if (!tipoMovimientoValido.Contains(model.TipoMovimiento))
                {
                    ModelState.AddModelError("TipoMovimiento",
                        "Tipo de movimiento no válido. Use: ENTRADA, SALIDA, INICIAL o AJUSTE");
                }
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Ajustar POST: ModelState inválido. Errores: {Errors}",
                    string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

                var ingredienteForInvalid = await _context.Ingrediente.FindAsync(model.IdIngrediente);
                if (ingredienteForInvalid != null)
                {
                    model.NombreIngrediente = ingredienteForInvalid.Nombre;
                    model.StockActual = ingredienteForInvalid.StockActual;
                    model.UnidadMedida = ingredienteForInvalid.UnidadMedida;
                }
                return View(model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var ingrediente = await _context.Ingrediente.FindAsync(model.IdIngrediente);
                if (ingrediente == null)
                {
                    _logger.LogWarning("Ingrediente no encontrado: {Id}", model.IdIngrediente);
                    return NotFound();
                }

                decimal cambio;
                if (model.TipoMovimiento == "SALIDA")
                {
                    cambio = -model.Cantidad;
                }
                else
                {
                    cambio = model.Cantidad;
                }

                decimal nuevoStock = ingrediente.StockActual + cambio;

                _logger.LogDebug("Calculando ajuste -> StockActual:{StockActual} + Cambio:{Cambio} = NuevoStock:{NuevoStock}",
                    ingrediente.StockActual, cambio, nuevoStock);

                if (nuevoStock < 0)
                {
                    _logger.LogWarning("Stock negativo detectado: {NuevoStock}", nuevoStock);
                    ModelState.AddModelError("Cantidad",
                        $"Stock insuficiente. Stock actual: {ingrediente.StockActual} {ingrediente.UnidadMedida}. No se puede retirar {model.Cantidad} {ingrediente.UnidadMedida}");

                    model.NombreIngrediente = ingrediente.Nombre;
                    model.StockActual = ingrediente.StockActual;
                    model.UnidadMedida = ingrediente.UnidadMedida;
                    await transaction.RollbackAsync();
                    return View(model);
                }

                var ajuste = new AjusteInventario
                {
                    IdIngrediente = model.IdIngrediente,
                    FechaAjuste = DateTime.Now,
                    TipoMovimiento = model.TipoMovimiento,
                    Cantidad = cambio,
                    Motivo = model.Motivo ?? $"Ajuste de tipo {model.TipoMovimiento}",
                    Usuario = User.Identity?.Name ?? "admin"
                };
                _context.AjusteInventario.Add(ajuste);

                ingrediente.StockActual = nuevoStock;
                _context.Update(ingrediente);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Ajuste registrado exitosamente. Ingrediente:{Nombre} NuevoStock:{Stock}",
                    ingrediente.Nombre, nuevoStock);

                TempData["Success"] = $"Ajuste registrado correctamente. Nuevo stock: {nuevoStock:F2} {ingrediente.UnidadMedida}";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al registrar ajuste de inventario para IdIngrediente:{Id}", model.IdIngrediente);

                ModelState.AddModelError("", $"Error al registrar ajuste: {ex.Message}");

                var ingredienteForError = await _context.Ingrediente.FindAsync(model.IdIngrediente);
                if (ingredienteForError != null)
                {
                    model.NombreIngrediente = ingredienteForError.Nombre;
                    model.StockActual = ingredienteForError.StockActual;
                    model.UnidadMedida = ingredienteForError.UnidadMedida;
                }
                return View(model);
            }
        }

        public async Task<IActionResult> Historial(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ingrediente = await _context.Ingrediente.FindAsync(id);
            if (ingrediente == null)
            {
                return NotFound();
            }

            var historial = await _context.AjusteInventario
                .Where(a => a.IdIngrediente == id)
                .OrderByDescending(a => a.FechaAjuste)
                .Take(50)
                .ToListAsync();

            ViewBag.Ingrediente = ingrediente;
            return View(historial);
        }

        public async Task<IActionResult> Desactivar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ingrediente = await _context.Ingrediente.FindAsync(id);
            if (ingrediente == null)
            {
                return NotFound();
            }

            if (!ingrediente.Activo)
            {
                TempData["Warning"] = "Este ingrediente ya está desactivado";
                return RedirectToAction(nameof(Index));
            }

            var viewModel = new DesactivarIngredienteViewModel
            {
                IdIngrediente = ingrediente.IdIngrediente,
                Nombre = ingrediente.Nombre,
                StockActual = ingrediente.StockActual,
                UnidadMedida = ingrediente.UnidadMedida,

                TotalAjustes = await _context.AjusteInventario
                    .CountAsync(a => a.IdIngrediente == id),
                TotalPlatillos = await _context.DetallePlatillo
                    .CountAsync(d => d.IdIngrediente == id),
                TotalCompras = await _context.Detallecompras
                    .CountAsync(d => d.idIngrediente == id)
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesactivarConfirmed(DesactivarIngredienteViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var ing = await _context.Ingrediente.FindAsync(model.IdIngrediente);
                if (ing != null)
                {
                    model.Nombre = ing.Nombre;
                    model.StockActual = ing.StockActual;
                    model.UnidadMedida = ing.UnidadMedida;
                    model.TotalAjustes = await _context.AjusteInventario.CountAsync(a => a.IdIngrediente == model.IdIngrediente);
                    model.TotalPlatillos = await _context.DetallePlatillo.CountAsync(d => d.IdIngrediente == model.IdIngrediente);
                    model.TotalCompras = await _context.Detallecompras.CountAsync(d => d.idIngrediente == model.IdIngrediente);
                }
                return View("Desactivar", model);
            }

            try
            {
                var ingrediente = await _context.Ingrediente.FindAsync(model.IdIngrediente);
                if (ingrediente == null)
                {
                    return NotFound();
                }

                ingrediente.Activo = false;
                ingrediente.FechaDesactivacion = DateTime.Now;
                ingrediente.MotivoDesactivacion = model.Motivo;

                _context.Update(ingrediente);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Ingrediente desactivado: {Nombre} (ID: {Id}). Motivo: {Motivo}",
                    ingrediente.Nombre, ingrediente.IdIngrediente, model.Motivo);

                TempData["Success"] = $"Ingrediente '{ingrediente.Nombre}' desactivado correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al desactivar ingrediente");
                ModelState.AddModelError("", $"Error: {ex.Message}");
                return View("Desactivar", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivar(int id)
        {
            try
            {
                var ingrediente = await _context.Ingrediente.FindAsync(id);
                if (ingrediente == null)
                {
                    return NotFound();
                }

                if (ingrediente.Activo)
                {
                    TempData["Warning"] = "Este ingrediente ya está activo";
                    return RedirectToAction(nameof(Index), new { mostrarInactivos = true });
                }

                ingrediente.Activo = true;
                ingrediente.FechaDesactivacion = null;
                ingrediente.MotivoDesactivacion = null;

                _context.Update(ingrediente);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Ingrediente reactivado: {Nombre} (ID: {Id})",
                    ingrediente.Nombre, ingrediente.IdIngrediente);

                TempData["Success"] = $"Ingrediente '{ingrediente.Nombre}' reactivado correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reactivar ingrediente");
                TempData["Error"] = $"Error: {ex.Message}";
                return RedirectToAction(nameof(Index), new { mostrarInactivos = true });
            }
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ingrediente = await _context.Ingrediente
                .FirstOrDefaultAsync(m => m.IdIngrediente == id);

            if (ingrediente == null)
            {
                return NotFound();
            }

            return View(ingrediente);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var tieneAjustes = await _context.AjusteInventario.AnyAsync(a => a.IdIngrediente == id);
                var tieneDetallePlatillo = await _context.DetallePlatillo.AnyAsync(d => d.IdIngrediente == id);
                var tieneDetalleCompra = await _context.Detallecompras.AnyAsync(d => d.idIngrediente == id);

                if (tieneAjustes || tieneDetallePlatillo || tieneDetalleCompra)
                {
                    TempData["Error"] = "No se puede eliminar el ingrediente porque tiene registros relacionados. Use 'Desactivar' en su lugar.";
                    return RedirectToAction(nameof(Index));
                }

                var ingrediente = await _context.Ingrediente.FindAsync(id);
                if (ingrediente != null)
                {
                    _context.Ingrediente.Remove(ingrediente);
                    await _context.SaveChangesAsync();

                    _logger.LogWarning("Ingrediente ELIMINADO: {Nombre} (ID: {Id})",
                        ingrediente.Nombre, ingrediente.IdIngrediente);

                    TempData["Success"] = "Ingrediente eliminado correctamente";
                }
            }
            catch (DbUpdateException dbEx)
            {
                _logger.LogError(dbEx, "Error al eliminar ingrediente id={Id}", id);
                TempData["Error"] = $"No se puede eliminar: {dbEx.InnerException?.Message ?? dbEx.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al eliminar ingrediente id={Id}", id);
                TempData["Error"] = $"No se puede eliminar: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> GenerarReporteInventario()
        {
            var ingredientes = await _context.Ingrediente
                .OrderBy(i => i.Nombre)
                .ToListAsync();

            var valorTotal = ingredientes.Sum(i => i.StockActual * i.CostoUnitarioPromedio);

            var model = new ReporteInventarioViewModel
            {
                Fecha = DateTime.Now,
                Ingredientes = ingredientes,
                ValorTotalInventario = valorTotal
            };

            var logoFileName = "logoCirculo.png";
            var logoPath = Path.Combine(_env.WebRootPath ?? string.Empty, "img", logoFileName);
            string base64Image = "";
            if (System.IO.File.Exists(logoPath))
            {
                var imageBytes = await System.IO.File.ReadAllBytesAsync(logoPath);
                var base64String = Convert.ToBase64String(imageBytes);
                base64Image = $"data:image/png;base64,{base64String}";
            }

            var htmlContent = await RenderViewToStringAsync("ReporteInventario", model, base64Image);

            await new BrowserFetcher().DownloadAsync();
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            await using var page = await browser.NewPageAsync();

            await page.SetContentAsync(htmlContent);
            await page.WaitForSelectorAsync("body");

            var pdfOptions = new PdfOptions
            {
                Format = PaperFormat.Letter,
                PrintBackground = true,
                MarginOptions = new MarginOptions { Top = "20mm", Bottom = "20mm", Left = "15mm", Right = "15mm" },
                DisplayHeaderFooter = true,
                HeaderTemplate = $@"
                    <div style='font-size:10pt; width:100%; padding:6px 12px; border-bottom:1px solid #ddd; display:flex; justify-content:space-between; align-items:center;'>
                        <div style='display:flex; align-items:center; gap:8px;'>
                            {(string.IsNullOrEmpty(base64Image) ? "" : $"<img src='{base64Image}' style='height:36px;'/>")}
                            <strong>Reporte de Inventario</strong>
                        </div>
                        <div style='font-size:9pt; color:#444;'>{DateTime.Now.ToString("g", CultureInfo.CurrentCulture)}</div>
                    </div>",
                FooterTemplate = "<div style='font-size:8pt; text-align:right; width:100%; padding-right:10mm;'>Página <span class='pageNumber'></span> de <span class='totalPages'></span></div>"
            };

            var pdfData = await page.PdfDataAsync(pdfOptions);

            var fileName = $"Reporte_Inventario_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            return File(pdfData, "application/pdf", fileName);
        }

        private async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model, string base64Image = "")
        {
            ViewData.Model = model;
            // inyectar la imagen en ViewData si la vista la usa
            ViewData["LogoBase64"] = base64Image;

            using var writer = new StringWriter();
            var viewResult = _viewEngine.FindView(ControllerContext, viewName, false);
            if (!viewResult.Success)
            {
                throw new InvalidOperationException($"No se pudo encontrar la vista '{viewName}'.");
            }

            var viewContext = new ViewContext(
                ControllerContext,
                viewResult.View,
                ViewData,
                TempDataDict,
                writer,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);
            return writer.GetStringBuilder().ToString();
        }

        private bool IngredienteExists(int id)
        {
            return _context.Ingrediente.Any(e => e.IdIngrediente == id);
        }
    }

    // ============================================
    // VIEWMODELS
    // ============================================

    public class InventarioViewModel
    {
        public List<Ingrediente> Ingredientes { get; set; } = new();
        public List<Ingrediente> IngredientesBajoStock { get; set; } = new();
        public decimal ValorTotalInventario { get; set; }
        public bool MostrarInactivos { get; set; }
    }

    public class AjusteInventarioViewModel
    {
        public int IdIngrediente { get; set; }
        public string? NombreIngrediente { get; set; }
        public decimal? StockActual { get; set; }
        public string? UnidadMedida { get; set; }

        [Required(ErrorMessage = "El tipo de movimiento es obligatorio")]
        [RegularExpression("^(ENTRADA|SALIDA|INICIAL|AJUSTE)$",
            ErrorMessage = "Tipo de movimiento no válido")]
        public string TipoMovimiento { get; set; } = null!;

        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(0.001, 999999.999, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public decimal Cantidad { get; set; }

        [MaxLength(200, ErrorMessage = "El motivo no puede exceder 200 caracteres")]
        public string? Motivo { get; set; }
    }

    public class DesactivarIngredienteViewModel
    {
        public int IdIngrediente { get; set; }
        public string Nombre { get; set; } = null!;
        public decimal StockActual { get; set; }
        public string UnidadMedida { get; set; } = null!;

        [Required(ErrorMessage = "Debe especificar el motivo de desactivación")]
        [MaxLength(200, ErrorMessage = "El motivo no puede exceder 200 caracteres")]
        [MinLength(10, ErrorMessage = "El motivo debe tener al menos 10 caracteres")]
        public string Motivo { get; set; } = null!;

        public int TotalAjustes { get; set; }
        public int TotalPlatillos { get; set; }
        public int TotalCompras { get; set; }
    }
}