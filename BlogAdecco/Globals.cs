// Copyright (c) 2025, Mapache Digital
// Version: 1
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

namespace BlogAdecco;

public class Globals
{
    // Define here the roles for the app
    public const string RoleAdmin = "Administrator"; // The super user. Can do anything
    public const string RoleSupervisor = "Supervisor"; // The supervisor can do mostly anything, except managing the super user
    public const string RoleCompanyUser = "Company";
    public static readonly string[] Roles = [RoleAdmin, RoleSupervisor, RoleCompanyUser];

    public const string ConfigSuperAdminEmail = "SuperAdmin:Email";
    public const string ConfigSuperAdminFirstname = "SuperAdmin:Firstname";
    public const string ConfigSuperAdminLastname = "SuperAdmin:Lastname";
    public const string ConfigSuperAdminCompany = "SuperAdmin:Company";
    public const string ConfigSuperAdminPhoneNumber = "SuperAdmin:PhoneNumber";
    public const string ConfigSuperAdminPassword = "SuperAdmin:Password";
    public const string ConfigGoogleApiKey = "Google:ApiKey";

    public const string GoogleTagManagerCode = "GTM-PWLRVCKL";
    public const string YoutubePlaylistCandidates = "PLnQX-jgAF5pQS2GUFCsatSyZkSH7e8UM8";
    public const string YoutubePlaylistCompanies =  "PLz58QJ68R9CSw5YUr1oAjD9dXDj8qbkRw";

    public const string StorageContainerNameAttachments = "attachments";
}
