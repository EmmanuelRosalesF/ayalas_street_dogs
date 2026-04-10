using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ayalas_street_dogs.Models;

namespace ayalas_street_dogs.Controllers
{
    public class PlatillosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PlatillosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Platillos
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Platillos.Include(p => p.IdCategoriaMenuNavigation);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Platillos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var platillo = await _context.Platillos
                .Include(p => p.IdCategoriaMenuNavigation)
                .FirstOrDefaultAsync(m => m.IdPlatillo == id);
            if (platillo == null)
            {
                return NotFound();
            }

            return View(platillo);
        }

        // GET: Platillos/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.CategoriaMenu = new SelectList(
                await _context.CategoriaMenus.OrderBy(c => c.NombreCategoriaMenu).ToListAsync(),
                "IdCategoriaMenu",
                "NombreCategoriaMenu"
            );

            return View();
        }

        // POST: Platillos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdPlatillo,NombrePlatillo,Precio,Descripcion,IdCategoriaMenu")] Platillo platillo)
        {
            if (ModelState.IsValid)
            {
                _context.Add(platillo);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCategoriaMenu"] = new SelectList(_context.CategoriaMenus, "IdCategoriaMenu", "NombreCategoriaMenu", platillo.IdCategoriaMenu);
            return View(platillo);
        }

        // GET: Platillos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var platillo = await _context.Platillos.FindAsync(id);
            if (platillo == null)
            {
                return NotFound();
            }
            ViewBag.CategoriaMenu = new SelectList(
                            await _context.CategoriaMenus.OrderBy(c => c.NombreCategoriaMenu).ToListAsync(),
                            "IdCategoriaMenu",
                            "NombreCategoriaMenu"
                        );
            return View(platillo);
        }

        // POST: Platillos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdPlatillo,NombrePlatillo,Precio,Descripcion,IdCategoriaMenu")] Platillo platillo)
        {
            if (id != platillo.IdPlatillo)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(platillo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlatilloExists(platillo.IdPlatillo))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdCategoriaMenu"] = new SelectList(_context.Categoria, "IdCategoriaMenu", "NombreCategoriaMenu", platillo.IdCategoriaMenu);
            return View(platillo);
        }

        // GET: Platillos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var platillo = await _context.Platillos
                .Include(p => p.IdCategoriaMenuNavigation)
                .FirstOrDefaultAsync(m => m.IdPlatillo == id);
            if (platillo == null)
            {
                return NotFound();
            }

            return View(platillo);
        }

        // POST: Platillos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var platillo = await _context.Platillos.FindAsync(id);
            if (platillo != null)
            {
                _context.Platillos.Remove(platillo);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PlatilloExists(int id)
        {
            return _context.Platillos.Any(e => e.IdPlatillo == id);
        }
    }
}
