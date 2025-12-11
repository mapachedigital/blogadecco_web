// Copyright (c) 2025, Mapache Digital
// Version: 1
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using BlogAdecco.Api.Responses;
using BlogAdecco.Pages.Candidatos;
using BlogAdecco.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlogAdecco.Pages.Empresas;

public class VideoBlogRecursosHumanosModel(IConfiguration _configuration, IHttpClientFactory _httpClientFactory, ILogger<VideoBlogBusquedaDeEmpleoModel> _logger) : PageModel
{
    public List<Video> Videos { get; set; } = [];

    public bool FailedToLoadVideos { get; set; } = false;

    public async Task<IActionResult> OnGet()
    {
        var headerData = new HeaderData
        {
            Template = "CandidatesCompanies",
            Subtitle = "Empresas",
            Email = "atencionaclientes@adecco.com",
            Phone = "800-890-0173",
            Breadcrumbs = ["Video blog para recursos humanos"],
            Links1 = [
                new() { Title = "Maquila de nómina", Width = 130, Url = Url.Page("/Categoria", new { Category = "empresas", SubCategory1 = "maquila-de-nomina"}) },
                new() { Title = "Reclutamiento de personal", Width = 160, Url = Url.Page("/Categoria", new { Category = "empresas", SubCategory1 = "reclutamiento-outsourcing-de-personal"}) },
                new() { Title = "Capacitación de personal", Width =  180, Url = Url.Page("/Categoria", new { Category = "empresas", SubCategory1 = "capacitacion-de-personal"}) },
            ],
            Links2 = [
                new() { Title = "Recursos digitales", Width = 106, Url = Url.Page("/Empresas/RecursosDigitalesEmpresas") },
                new() { Title = "Video Blog", Width =  80, Url = Url.Page("/Empresas/VideoBlogRecursosHumanos"), Enabled = false},
            ]
        };

        ViewData["Header"] = headerData;

        var maxResults = 48;
        var apiKey = _configuration[Globals.ConfigGoogleApiKey];
        var url = $"https://youtube.googleapis.com/youtube/v3/playlistItems?part=snippet&maxResults={maxResults}&playlistId={Globals.YoutubePlaylistCompanies}&key={apiKey}";
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
