using BlogAdecco.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlogAdecco.Pages.Empresas
{
    public class RecursosDigitalesParaRecursosHumanosModel : PageModel
    {
        public void OnGet()
        {
            var headerData = new HeaderData
            {
                Template = "CandidatesCompanies",
                Subtitle = "Empresas",
                Email = "atencionaclientes@adecco.com",
                Phone = "800-890-0173",
                Breadcrumbs = ["Recursos digitales para recursos humanos"],
                Links1 = [
                new() { Title = "Maquila de nómina", Width = 130, Url = Url.Page("/Categoria", new { Category = "empresas", SubCategory1 = "maquila-de-nomina"}) },
                new() { Title = "Reclutamiento de personal", Width = 160, Url = Url.Page("/Categoria", new { Category = "empresas", SubCategory1 = "reclutamiento-outsourcing-de-personal"}) },
                new() { Title = "Capacitación de personal", Width =  180, Url = Url.Page("/Categoria", new { Category = "empresas", SubCategory1 = "capacitacion-de-personal"}) },
            ],
                Links2 = [
                new() { Title = "Recursos digitales", Width = 106, Url = Url.Page("/Empresas/RecursosDigitalesEmpresas"), Enabled = false },
                new() { Title = "Video Blog", Width =  80, Url = Url.Page("/Empresas/VideoBlogRecursosHumanos")},
            ]
            };

            ViewData["Header"] = headerData;
        }
    }
}
