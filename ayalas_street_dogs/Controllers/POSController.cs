using ayalas_street_dogs.Models;
using ayalas_street_dogs.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static ayalas_street_dogs.Controllers.HomeController;

namespace ayalas_street_dogs.Controllers
{
    [Authorize(Roles = "empleado,admin")]
    public class POSController : Controller
    {
        private readonly ApplicationDbContext _context;

        public POSController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: POS
        public async Task<IActionResult> Index()
        {
            var viewModel = new POSViewModel
            {
                Categorias = await _context.CategoriaMenus
                    .Include(c => c.Platillos)
                    .OrderBy(c => c.OrdenVisualizacion)
                    .ToListAsync(),

                Platillos = await _context.Platillos
                    .Include(p => p.IdCategoriaMenuNavigation)
                    .OrderBy(p => p.IdCategoriaMenu)
                    .ThenBy(p => p.NombrePlatillo)
                    .ToListAsync(),

                VentasPendientes = await _context.Venta
                .Where(v => v.Estado == "Pendiente")
                    .Include(v => v.UsuarioNavigation)
                    .Include(v => v.Detalleventa)
                        .ThenInclude(d => d.IdPlatilloNavigation)
                    .OrderByDescending(v => v.FechaV)
                    .Take(10)
                    .Select(v => new VentaRecienteDto
                    {
                        IdVenta = v.IdVenta,
                        NombreCliente = v.NombreCliente,
                        FechaV = v.FechaV,
                        TotalV = v.TotalV,
                        CantidadProductos = v.Detalleventa.Sum(d => d.CantidadDv),
                        PrimerProducto = v.Detalleventa.FirstOrDefault().IdPlatilloNavigation.NombrePlatillo ?? "N/A",
                        Estado = v.Estado
                    })
                    .ToListAsync()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id, string nuevoEstado)
        {
            var venta = await _context.Venta.FindAsync(id);

            if (venta == null)
            {
                return NotFound();
            }

            // Validar transiciones de estado
            if (venta.Estado == "Completada" || venta.Estado == "Cancelada")
            {
                TempData["Error"] = "No se puede modificar una venta completada o cancelada.";
                return RedirectToAction(nameof(Index));
            }

            // Validar estado válido
            if (nuevoEstado != "Pendiente" && nuevoEstado != "Completada" && nuevoEstado != "Cancelada")
            {
                TempData["Error"] = "Estado no válido.";
                return RedirectToAction(nameof(Index));
            }

            venta.Estado = nuevoEstado;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Estado de la venta #{id} actualizado a '{nuevoEstado}'.";
            return RedirectToAction(nameof(Index));
        }

        // POST: POS/CrearVenta
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearVenta([FromBody] POSVentaRequest request)
        {
            if (request.Items == null || !request.Items.Any())
            {
                return Json(new { success = false, message = "Debe agregar al menos un producto" });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                decimal total = 0;
                foreach (var item in request.Items)
                {
                    var platillo = await _context.Platillos.FindAsync(item.IdPlatillo);
                    if (platillo != null)
                    {
                        total += platillo.Precio * item.Cantidad;
                    }
                }

                var venta = new Ventum
                {
                    FechaV = DateTime.Now,
                    TotalV = total,
                    NombreCliente = request.NombreCliente ?? "Cliente",
                    Usuario = User.Identity?.Name ?? "empleado",
                    Estado = "Pendiente"
                };

                _context.Venta.Add(venta);
                await _context.SaveChangesAsync();

                // Crear detalles
                foreach (var item in request.Items)
                {
                    var platillo = await _context.Platillos.FindAsync(item.IdPlatillo);
                    if (platillo != null)
                    {
                        var detalle = new Detalleventum
                        {
                            IdVenta = venta.IdVenta,
                            IdPlatillo = item.IdPlatillo,
                            CantidadDv = item.Cantidad,
                            CostoDv = platillo.Precio
                        };
                        _context.Detalleventa.Add(detalle);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Json(new
                {
                    success = true,
                    message = "Venta realizada exitosamente",
                    ventaId = venta.IdVenta,
                    total = total
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // GET: POS/VentasHoy
        public async Task<IActionResult> VentasHoy()
        {
            var usuario = User.Identity?.Name;
            var hoy = DateTime.Today;

            var ventas = await _context.Venta
                .Include(v => v.Detalleventa)
                    .ThenInclude(d => d.IdPlatilloNavigation)
                .Where(v => v.FechaV.HasValue
                        && v.FechaV.Value.Date == hoy
                        && v.Usuario == usuario)
                .OrderByDescending(v => v.FechaV)
                .ToListAsync();

            return View(ventas);
        }
    }
}
