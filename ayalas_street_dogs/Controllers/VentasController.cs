using ayalas_street_dogs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ayalas_street_dogs.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ayalas_street_dogs.Controllers
{
    public class VentasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VentasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Ventas
        public async Task<IActionResult> Index()
        {
            var ventas = await _context.Venta
                .Include(v => v.UsuarioNavigation)
                .Include(v => v.Detalleventa)
                    .ThenInclude(d => d.IdPlatilloNavigation)
                .OrderByDescending(v => v.FechaV)
                .ToListAsync();

            return View(ventas);
            /*var applicationDbContext = _context.Venta.Include(v => v.UsuarioNavigation);
            return View(await applicationDbContext.ToListAsync());*/
        }

        // GET: Ventas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venta = await _context.Venta
                .Include(v => v.UsuarioNavigation)
                .Include(v => v.Detalleventa)
                    .ThenInclude(d => d.IdPlatilloNavigation)
                .FirstOrDefaultAsync(v => v.IdVenta == id);

            if (venta == null)
            {
                return NotFound();
            }

            return View(venta);
            /*if (id == null)
            {
                return NotFound();
            }

            var ventum = await _context.Venta
                .Include(v => v.UsuarioNavigation)
                .FirstOrDefaultAsync(m => m.IdVenta == id);
            if (ventum == null)
            {
                return NotFound();
            }

            return View(ventum);*/
        }

        // GET: Ventas/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Platillos = new SelectList(
                await _context.Platillos.OrderBy(p => p.NombrePlatillo).ToListAsync(),
                "IdPlatillo",
                "NombrePlatillo"
            );

            var viewModel = new VentaCreateViewModel
            {
                Usuario = User.Identity?.Name ?? "Desconocido"
            };
            return View(viewModel);
        }

        // POST: Ventas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VentaCreateViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Platillos = new SelectList(
                    await _context.Platillos.OrderBy(p => p.NombrePlatillo).ToListAsync(),
                    "IdPlatillo",
                    "NombrePlatillo"
                );
                return View(viewModel);
            }

            if (viewModel.Detalles == null || !viewModel.Detalles.Any())
            {
                ModelState.AddModelError("", "Debe agregar al menos un platillo a la venta");
                ViewBag.Platillos = new SelectList(
                    await _context.Platillos.OrderBy(p => p.NombrePlatillo).ToListAsync(),
                    "IdPlatillo",
                    "NombrePlatillo"
                );
                return View(viewModel);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                decimal totalCalculado = 0;
                foreach (var detalle in viewModel.Detalles)
                {
                    totalCalculado += detalle.Cantidad * detalle.PrecioUnitario;
                }

                // Crear la venta
                var venta = new Ventum
                {
                    FechaV = DateTime.Now,
                    TotalV = totalCalculado,
                    NombreCliente = viewModel.NombreCliente,
                    Usuario = User.Identity?.Name ?? "Desconocido",
                    Estado = "Pendiente"
                };

                _context.Venta.Add(venta);
                await _context.SaveChangesAsync();

                // Crear los detalles de venta
                foreach (var detalle in viewModel.Detalles)
                {
                    var detalleVenta = new Detalleventum
                    {
                        IdVenta = venta.IdVenta,
                        IdPlatillo = detalle.IdPlatillo,
                        CantidadDv = detalle.Cantidad,
                        CostoDv = detalle.PrecioUnitario
                    };

                    _context.Detalleventa.Add(detalleVenta);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "Venta creada exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", $"Error al crear la venta: {ex.Message}");

                ViewBag.Platillos = new SelectList(
                    await _context.Platillos.OrderBy(p => p.NombrePlatillo).ToListAsync(),
                    "IdPlatillo",
                    "NombrePlatillo"
                );
                return View(viewModel);
            }
        }

        // POST: Ventas/CambiarEstado
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

        // GET: Ventas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id.Value == 0)
            {
                return NotFound();
            }

            var ventum = await _context.Venta
                .Include(v => v.Detalleventa)
                    .ThenInclude(d => d.IdPlatilloNavigation)
                .FirstOrDefaultAsync(v => v.IdVenta == id.Value);

            if (ventum == null)
            {
                // Si sigue siendo null aquí, el ID no existe o el nombre de la tabla/columna es incorrecto.
                return NotFound();
            }

            // 2. Cargar Platillos para el SelectList
            ViewBag.Platillos = new SelectList(
                await _context.Platillos.OrderBy(p => p.NombrePlatillo).ToListAsync(),
                "IdPlatillo",
                "NombrePlatillo"
            );

            // 3. Mapear la entidad Ventum a tu VentaCreateViewModel
            var viewModel = new VentaCreateViewModel
            {
                IdVenta = ventum.IdVenta,
                NombreCliente = ventum.NombreCliente,
                Usuario = ventum.Usuario,
                TotalV = ventum.TotalV,
                Detalles = ventum.Detalleventa.Select(d => new DetalleVentaItem
                {
                    IdPlatillo = d.IdPlatillo,
                    Cantidad = d.CantidadDv,
                    PrecioUnitario = d.CostoDv,
                    NombrePlatillo = d.IdPlatilloNavigation?.NombrePlatillo
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // *** CAMBIO CLAVE: Recibir el ViewModel con los detalles ***
        public async Task<IActionResult> Edit(int id, VentaCreateViewModel viewModel)
        {
            if (id != viewModel.IdVenta)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                // Si falla la validación, recarga los datos necesarios para la vista
                ViewBag.Platillos = new SelectList(
                    await _context.Platillos.OrderBy(p => p.NombrePlatillo).ToListAsync(),
                    "IdPlatillo",
                    "NombrePlatillo"
                );
                return View(viewModel);
            }

            // --- LÓGICA TRANSACCIONAL PARA ACTUALIZAR VENTA Y DETALLES ---
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (viewModel.Detalles == null || !viewModel.Detalles.Any())
                {
                    // Lanzar excepción o manejar error de validación de negocio
                    throw new InvalidOperationException("Debe agregar al menos un platillo a la venta para guardar.");
                }

                // 1. Recalcular el Total
                decimal totalCalculado = 0;
                foreach (var detalle in viewModel.Detalles)
                {
                    totalCalculado += detalle.Cantidad * detalle.PrecioUnitario;
                }

                // 2. Buscar la Venta original (incluyendo los detalles para poder borrarlos)
                var ventaExistente = await _context.Venta
                    .Include(v => v.Detalleventa)
                    .FirstOrDefaultAsync(v => v.IdVenta == id);

                if (ventaExistente == null)
                {
                    await transaction.RollbackAsync();
                    return NotFound();
                }

                // 3. Actualizar campos principales
                ventaExistente.NombreCliente = viewModel.NombreCliente;
                ventaExistente.TotalV = totalCalculado;
                // El Usuario y FechaV normalmente se dejan intactos al editar

                // 4. Eliminar detalles antiguos
                _context.Detalleventa.RemoveRange(ventaExistente.Detalleventa);

                // 5. Crear e insertar nuevos detalles (reemplazo completo)
                foreach (var detalleItem in viewModel.Detalles)
                {
                    var detalleVenta = new Detalleventum
                    {
                        IdVenta = ventaExistente.IdVenta,
                        IdPlatillo = detalleItem.IdPlatillo,
                        CantidadDv = detalleItem.Cantidad,
                        CostoDv = detalleItem.PrecioUnitario
                    };
                    _context.Detalleventa.Add(detalleVenta);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "Venta actualizada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError("", $"Error al actualizar la venta: {ex.Message}");

                // Recargar ViewBag si falla
                ViewBag.Platillos = new SelectList(
                    await _context.Platillos.OrderBy(p => p.NombrePlatillo).ToListAsync(),
                    "IdPlatillo",
                    "NombrePlatillo"
                );
                return View(viewModel);
            }
        }

        // GET: Ventas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ventum = await _context.Venta
                .Include(v => v.UsuarioNavigation)
                .FirstOrDefaultAsync(m => m.IdVenta == id);
            if (ventum == null)
            {
                return NotFound();
            }

            return View(ventum);
        }

        // POST: Ventas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ventum = await _context.Venta.FindAsync(id);
            if (ventum != null)
            {
                _context.Venta.Remove(ventum);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetPrecioPlatillo(int id)
        {
            var platillo = await _context.Platillos.FindAsync(id);

            if (platillo == null)
            {
                return NotFound();
            }

            return Json(new
            {
                precio = platillo.Precio,
                nombre = platillo.NombrePlatillo
            });
        }

        private bool VentumExists(int id)
        {
            return _context.Venta.Any(e => e.IdVenta == id);
        }
    }
}
