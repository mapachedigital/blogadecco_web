using BlogAdecco.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlogAdecco.Pages.Candidatos
{
    public class RecursosDigitalesCandidatosModel : PageModel
    {
        public void OnGet()
        {
            var headerData = new HeaderData
            {
                Template = "CandidatesCompanies",
                Subtitle = "Aspirantes",
                Email = "calidadmx@adecco.com",
                Phone = "800 832 9050",
                Breadcrumbs = ["Recursos digitales para candidatos"],
                Links1 = [
                new() { Title = "Consejos para el desarrollo profesional", Width = 220, Url = Url.Page("/Categoria", new { Category = "candidatos", SubCategory1 = "consejos-para-el-desarrollo-profesional" }) },
                new() { Title = "Derechos y prestaciones laborales", Width = 220, Url = Url.Page("/Categoria", new { Category = "candidatos", SubCategory1 = "derechos-prestaciones-laborales" }) },
                new() { Title = "Tips para entrevistas de trabajo", Width = 220, Url = Url.Page("/Categoria", new { Category = "candidatos", SubCategory1 = "tips-para-entrevistas-de-trabajo" }) },
            ],
                Links2 = [
                new() { Title = "Infografías", Width = 120, Url = Url.Page("/Categoria", new { Category = "candidatos", SubCategory1 = "infografias" }) },
                new() { Title = "Recursos digitales", Width = 120, Url = Url.Page("/Candidatos/RecursosDigitalesCandidatos"), Enabled = false },
                new() { Title = "Video Blog", Width = 100, Url = Url.Page("/Candidatos/VideoBlogBusquedaDeEmpleo") },
            ]
            };

            ViewData["Header"] = headerData;
        }
    }
}
