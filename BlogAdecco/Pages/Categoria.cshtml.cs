using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlogAdecco.Pages
{
    public class CategoriesModel : PageModel
    {
        [FromRoute]
        public string Category { get; set; } = default!;

        [FromRoute]
        public string? SubCategory1 { get; set; }

        [FromRoute]
        public string? SubCategory2 { get; set; }

        [FromRoute]
        public string? SubCategory3 { get; set; }

        public void OnGet()
        {
        }
    }
}
