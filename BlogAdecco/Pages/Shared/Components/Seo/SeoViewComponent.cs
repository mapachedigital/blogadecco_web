using MDWidgets.Utils;
using Microsoft.AspNetCore.Mvc;

namespace BlogAdecco.Pages.Shared.Components.Seo;

public class SeoViewComponent(ISiteUtils _siteUtils) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(SeoInfo? seoInfo)
    {
        seoInfo ??= new SeoInfo
        {
            Description = _siteUtils.GetSiteDescription(),
            OgInfo = new OgInfo
            {
                SiteName = _siteUtils.GetSiteName(),
                Description = _siteUtils.GetSiteDescription(),
                Type = "website",
                Locale = _siteUtils.GetDefaultLanguage(),
            },
        };

        if (seoInfo.OgInfo?.Locale!=null) 
        {
            // Open Graph protocol requires locale to use underscore instead of hyphen
            seoInfo.OgInfo.Locale = seoInfo.OgInfo.Locale.Replace("-", "_");
        }

        return View(seoInfo);
    }
}
