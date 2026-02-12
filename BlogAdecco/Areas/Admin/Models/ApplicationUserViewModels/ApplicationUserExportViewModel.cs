// Copyright (c) 2021, Mapache Digital
// Version: 1.1
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using CsvHelper.Configuration;

namespace BlogAdecco.Areas.Admin.Models.ApplicationUserViewModels;

/// <summary>
/// Model for export of users
/// </summary>
public class ApplicationUserExportViewModel
{
    public string Firstname { get; set; } = default!;
    public string Lastname { get; set; } = default!;
    public string? DisplayName { get; set; }
    public string Company { get; set; } = default!;
    public string? Position { get; set; }
    public string? Email { get; set; }
    public string? MobilePhone { get; set; }
    public bool AcceptTermsOfService { get; set; } = default!;
    public bool Approved { get; set; }
    public DateTime? LastAccess { get; set; }
    public string Roles { get; set; } = default!;
    public bool Enabled { get; set; }
}

/// <summary>
/// CSV mapping for the export of users
/// </summary>
public sealed class ApplicationUserExportViewModelMap : ClassMap<ApplicationUserExportViewModel>
{
    public ApplicationUserExportViewModelMap()
    {
        Map(m => m.Firstname).Name("Nombres");
        Map(m => m.Lastname).Name("Apellidos");
        Map(m => m.DisplayName).Name("Nombre para mostrar");
        Map(m => m.Company).Name("Empresa");
        Map(m => m.Position).Name("Puesto");
        Map(m => m.Email).Name("Correo electrónico");
        Map(m => m.MobilePhone).Name("Teléfono móvil");
        Map(m => m.AcceptTermsOfService).Name("Acepta términos y condiciones");
        Map(m => m.Approved).Name("Aprobado");
        Map(m => m.LastAccess).Name("Último acceso").TypeConverterOption.Format("yyyy/MM/dd HH:mm:ss");
        Map(m => m.Roles).Name("Roles");
        Map(m => m.Enabled).Name("Activo");
    }
}