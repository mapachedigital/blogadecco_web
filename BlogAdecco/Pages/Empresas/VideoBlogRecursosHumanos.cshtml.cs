// Copyright (c) 2025, Mapache Digital
// Version: 1
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using BlogAdecco.Api.Responses;
using BlogAdecco.Pages.Candidatos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlogAdecco.Pages.Empresas
{
    public class VideoBlogRecursosHumanosModel(IConfiguration _configuration, IHttpClientFactory _httpClientFactory, ILogger<VideoBlogBusquedaDeEmpleoModel> _logger) : PageModel
    {
        public List<Video> Videos { get; set; } = [];

        public bool FailedToLoadVideos { get; set; } = false;

        public async Task<IActionResult> OnGet()
        {
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
}
