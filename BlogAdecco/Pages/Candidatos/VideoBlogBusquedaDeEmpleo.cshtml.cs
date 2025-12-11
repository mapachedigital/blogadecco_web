// Copyright (c) 2025, Mapache Digital
// Version: 1
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using BlogAdecco.Api.Responses;
using BlogAdecco.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlogAdecco.Pages.Candidatos;

public class VideoBlogBusquedaDeEmpleoModel(IConfiguration _configuration, IHttpClientFactory _httpClientFactory, ILogger<VideoBlogBusquedaDeEmpleoModel> _logger) : PageModel
{
    public List<Video> Videos { get; set; } = [];

    public bool FailedToLoadVideos { get; set; } = false;

    public async Task<IActionResult> OnGet()
    {
        var headerData = new HeaderData
        {
            Template = "CandidatesCompanies",
            Subtitle = "Aspirantes",
            Email = "calidadmx@adecco.com",
            Phone = "800 832 9050",
            Breadcrumbs = ["Video blog para la búsqueda de empleo"],
            Links1 = [
                new() { Title = "Consejos para el desarrollo profesional", Width = 220, Url = Url.Page("/Categoria", new { Category = "candidatos", SubCategory1 = "consejos-para-el-desarrollo-profesional" }) },
                new() { Title = "Derechos y prestaciones laborales", Width = 220, Url = Url.Page("/Categoria", new { Category = "candidatos", SubCategory1 = "derechos-prestaciones-laborales" }) },
                new() { Title = "Tips para entrevistas de trabajo", Width = 220, Url = Url.Page("/Categoria", new { Category = "candidatos", SubCategory1 = "tips-para-entrevistas-de-trabajo" }) },
            ],
            Links2 = [
                new() { Title = "Infografías", Width = 120, Url = Url.Page("/Categoria", new { Category = "candidatos", SubCategory1 = "infografias" }) },
                new() { Title = "Recursos digitales", Width = 120, Url = Url.Page("/Candidatos/RecursosDigitalesCandidatos") },
                new() { Title = "Video Blog", Width = 100, Url = Url.Page("/Candidatos/VideoBlogBusquedaDeEmpleo"), Enabled = false },
            ]
        };

        ViewData["Header"] = headerData;
        var maxResults = 48;
        var apiKey = _configuration[Globals.ConfigGoogleApiKey];
        var url = $"https://youtube.googleapis.com/youtube/v3/playlistItems?part=snippet&maxResults={maxResults}&playlistId={Globals.YoutubePlaylistCandidates}&key={apiKey}";
        var client = _httpClientFactory.CreateClient();
        GoogleApiResponse? result;

        try
        {
            result = await client.GetFromJsonAsync<GoogleApiResponse>(url);
            Videos = result?.Videos ?? [];
        }
        catch (HttpRequestException ex)
        {
            // Log error
            if (_logger.IsEnabled(LogLevel.Critical))
            {
                _logger.LogCritical(ex, "Error fetching YouTube playlist: {message}. Check the API Key and playlist Id.", ex.Message);
            }

            FailedToLoadVideos = true;
        }

        return Page();
    }
}
