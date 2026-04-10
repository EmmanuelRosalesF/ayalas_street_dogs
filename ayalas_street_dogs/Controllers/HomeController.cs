using ayalas_street_dogs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using PuppeteerSharp;
using PuppeteerSharp.Media;
using System.Diagnostics;
using System.IO;
using static ayalas_street_dogs.Controllers.HomeController;

namespace ayalas_street_dogs.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;
        private readonly ICompositeViewEngine _viewEngine;

        public HomeController(ILogger<HomeController> logger, 
            ApplicationDbContext context, IWebHostEnvironment env,
            ICompositeViewEngine viewEngine, 
    ITempDataDictionaryFactory tempDataDictionaryFactory)
        {
            _logger = logger;
            _context = context;
            _env = env;
            _viewEngine = viewEngine;
            _tempDataDictionaryFactory = tempDataDictionaryFactory;
        }

        public ITempDataDictionary TempData => _tempDataDictionaryFactory.GetTempData(HttpContext);

        private async Task<DashboardViewModel> ObtenerDatosDashboardAsync()
        {
            var viewModel = new DashboardViewModel
            {
                VentasDelDia = await _context.Venta
                    .Where(v => v.FechaV.HasValue && v.FechaV.Value.Date == DateTime.Today)
                    .SumAsync(v => v.TotalV),

                TotalProductos = await _context.Platillos.CountAsync(),

                PedidosHoy = await _context.Venta
                    .Where(v => v.FechaV.HasValue && v.FechaV.Value.Date == DateTime.Today)
                    .CountAsync(),

                StockBajo = await _context.Platillos
                    .Where(p => !_context.Detalleventa.Any(d => d.IdPlatillo == p.IdPlatillo))
                    .CountAsync(),

                VentasSemana = await _context.Venta
                    .Where(v => v.FechaV.HasValue && v.FechaV.Value >= DateTime.Today.AddDays(-6))
                    .GroupBy(v => v.FechaV.Value.Date)
                    .Select(g => new VentaDiariaDto
                    {
                        Fecha = g.Key,
                        Total = g.Sum(v => v.TotalV)
                    })
                    .OrderBy(v => v.Fecha)
                    .ToListAsync(),

                TopProductos = await _context.Detalleventa
                    .Include(d => d.IdPlatilloNavigation) 
                    .GroupBy(d => new { d.IdPlatillo, d.IdPlatilloNavigation.NombrePlatillo })
                    .Select(g => new ProductoVendidoDto
                    {
                        IdPlatillo = g.Key.IdPlatillo,
                        NombrePlatillo = g.Key.NombrePlatillo ?? "Sin nombre",
                        CantidadVendida = g.Sum(d => d.CantidadDv)
                    })
                    .OrderByDescending(p => p.CantidadVendida)
                    .Take(5)
                    .ToListAsync(),

                VentasRecientes = await _context.Venta
                .Where(v => v.Estado == "Completada")
                    .Include(v => v.UsuarioNavigation)
                    .Include(v => v.Detalleventa)
                        .ThenInclude(d => d.IdPlatilloNavigation)
                    .OrderByDescending(v => v.FechaV)
                    .Take(10)
                    .Select(v => new VentaRecenteDto
                    {
                        IdVenta = v.IdVenta,
                        NombreCliente = v.NombreCliente,
                        FechaV = v.FechaV,
                        TotalV = v.TotalV,
                        CantidadProductos = v.Detalleventa.Sum(d => d.CantidadDv),
                        PrimerProducto = v.Detalleventa.FirstOrDefault().IdPlatilloNavigation.NombrePlatillo ?? "N/A"
                    })
                    .ToListAsync(),

                PorcentajeCambioVentas = await CalcularPorcentajeCambioVentas()
            };

            viewModel.VentasSemana = CompletarVentasSemana(viewModel.VentasSemana);

            return viewModel;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = await ObtenerDatosDashboardAsync();
            return View(viewModel);
        }

        private async Task<decimal> CalcularPorcentajeCambioVentas()
        {
            var ventasHoy = await _context.Venta
                .Where(v => v.FechaV.HasValue && v.FechaV.Value.Date == DateTime.Today)
                .SumAsync(v => v.TotalV);

            var ventasAyer = await _context.Venta
                .Where(v => v.FechaV.HasValue && v.FechaV.Value.Date == DateTime.Today.AddDays(-1))
                .SumAsync(v => v.TotalV);

            if (ventasAyer == 0)
                return ventasHoy > 0 ? 100 : 0;

            return ((ventasHoy - ventasAyer) / ventasAyer) * 100;
        }

        private List<VentaDiariaDto> CompletarVentasSemana(List<VentaDiariaDto> ventas)
        {
            var resultado = new List<VentaDiariaDto>();

            for (int i = 6; i >= 0; i--)
            {
                var fecha = DateTime.Today.AddDays(-i);
                var ventaDia = ventas.FirstOrDefault(v => v.Fecha.Date == fecha.Date);

                resultado.Add(new VentaDiariaDto
                {
                    Fecha = fecha,
                    Total = ventaDia?.Total ?? 0
                });
            }

            return resultado;
        }

        public async Task<IActionResult> GenerarReporte()
        {
            var model = await ObtenerDatosDashboardAsync();

            var logoFileName = "logoCirculo.png";
            var logoPath = Path.Combine(_env.WebRootPath, "img", logoFileName);

            string base64Image = "";
            if (System.IO.File.Exists(logoPath))
            {
                byte[] imageBytes = await System.IO.File.ReadAllBytesAsync(logoPath);

                string base64String = Convert.ToBase64String(imageBytes);

                base64Image = $"data:image/png;base64,{base64String}";
            }

            var htmlContent = await RenderViewToStringAsync("PdfReport", model);

            await new BrowserFetcher().DownloadAsync();
            await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
            {
                Headless = true
            });
            await using var page = await browser.NewPageAsync();

            await page.SetContentAsync(htmlContent);
            await page.WaitForSelectorAsync("body");

            var pdfOptions = new PdfOptions
            {
                Format = PaperFormat.Letter,
                PrintBackground = true,
                MarginOptions = new MarginOptions { Top = "25mm", Bottom = "25mm", Left = "30mm", Right = "30mm" },
                DisplayHeaderFooter = true,

                HeaderTemplate = $@"
            <div style='width: 100%; border-bottom: 1px solid #ccc; padding-bottom: 1px; margin-left: 10mm; margin-right: 10mm; display: flex; justify-content: space-between; align-items: center;'>
                <img src='{base64Image}' style='height: 60px; margin-left: 5mm;' />
                <span style='font-size: 10pt; font-weight: bold; margin-right: 5mm; color: #333;'>REPORTE DE GESTIÓN</span>
            </div>
        ",
                FooterTemplate = "<div style='font-size: 8pt; text-align: right; margin-right: 10mm;'>Página <span class='pageNumber'></span> de <span class='totalPages'></span></div>"
            };

            var pdfData = await page.PdfDataAsync(pdfOptions);

            string fileName = $"Reporte_Dashboard_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            return File(pdfData, "application/pdf", fileName);
        }

        // Helper para renderizar la vista a string
        private async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = ControllerContext.ActionDescriptor.ActionName;
            }

            ViewData.Model = model;

            using (var writer = new StringWriter())
            {
                var viewEngineResult = _viewEngine.FindView(ControllerContext, viewName, false);

                if (!viewEngineResult.Success)
                {
                    throw new InvalidOperationException($"No se pudo encontrar la vista '{viewName}'.");
                }

                var viewContext = new ViewContext(
                    ControllerContext,
                    viewEngineResult.View,
                    ViewData,
                    TempData,
                    writer,
                    new HtmlHelperOptions()
                );

                await viewEngineResult.View.RenderAsync(viewContext);

                return writer.GetStringBuilder().ToString();
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public class DashboardViewModel
        {
            public decimal VentasDelDia { get; set; }
            public int TotalProductos { get; set; }
            public int PedidosHoy { get; set; }
            public int StockBajo { get; set; }
            public decimal PorcentajeCambioVentas { get; set; }
            public List<VentaDiariaDto> VentasSemana { get; set; } = new();
            public List<ProductoVendidoDto> TopProductos { get; set; } = new();
            public List<VentaRecenteDto> VentasRecientes { get; set; } = new();
            public List<VentaRecenteDto> VentasPendientes { get; set; } = new();

        }

        public class VentaDiariaDto
        {
            public DateTime Fecha { get; set; }
            public decimal Total { get; set; }
        }

        public class ProductoVendidoDto
        {
            public int IdPlatillo { get; set; }
            public string NombrePlatillo { get; set; } = null!;
            public int CantidadVendida { get; set; }
        }

        public class VentaRecenteDto
        {
            public int IdVenta { get; set; }
            public string NombreCliente { get; set; } = null!;
            public DateTime? FechaV { get; set; }
            public decimal TotalV { get; set; }
            public int CantidadProductos { get; set; }
            public string PrimerProducto { get; set; } = null!;
        }
    }
}