// Copyright (c) 2021, Mapache Digital
// Version: 1.0.0
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using Azure.Core.Extensions;
using Azure.Storage.Blobs;
using BlogAdecco;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Localization;
using System.Reflection;

namespace BlogAdecco;

/// <summary>
/// Empty class used to determine the name of this assembly
/// </summary>
public class SharedResources { };


/// <summary>
/// Empty class used to determine the name of this assembly
/// </summary>
public class ModelResources { };

/// <summary>
/// Extensions for the setup of this application
/// </summary>
internal static class StartupExtensions
{
    /// <summary>
    /// Start the Azure emulator for development.
    /// </summary>
    /// <remarks>
    /// You need to add the following to serviceDependencies.json:
    /// <code>
    /// "storage1": {
    ///   "type": "storage",
    ///   "connectionId": "StorageConnection",
    ///   "dynamicId": null
    /// }
    /// </code>
    /// And the following to serviceDependencies.local.json:
    /// <code>
    /// "storage1": {
    ///   "secretStore": "LocalSecretsFile",
    ///   "type": "storage.emulator",
    ///   "connectionId": "StorageConnection",
    ///   "dynamicId": null
    /// }
    /// </code>
    /// Then in secrets.json:
    /// <code>
    /// "Storage:ConnectionString": "UseDevelopmentStorage=true",
    /// </code>
    /// </remarks>
    public static IAzureClientBuilder<BlobServiceClient, BlobClientOptions> AddBlobServiceClient(this AzureClientFactoryBuilder builder, string serviceUriOrConnectionString, bool preferMsi = true)
    {
        if (preferMsi && Uri.TryCreate(serviceUriOrConnectionString, UriKind.Absolute, out Uri? serviceUri))
        {
            return builder.AddBlobServiceClient(serviceUri);
        }
        else
        {
            return BlobClientBuilderExtensions.AddBlobServiceClient(builder, serviceUriOrConnectionString);
        }
    }

    /// <summary>
    /// Create a localizer for the MVC binding error messages
    /// </summary>
    public static IMvcBuilder AddModelBindingMessagesLocalizer(this IMvcBuilder mvc, IServiceCollection services)
    {
        return mvc.AddMvcOptions(options =>
        {
            // Translate the Model Binding Strings.  For this, we need an empty class named ModelResources.
            // The translated strings must be in Resources/ModelResources.es.resx 
            // These strings might (and will) change with future changes in the Asp.Net Core version.  You need to check
            // the source code after any version migration.
            var assemblyName = new AssemblyName(typeof(ModelResources).GetTypeInfo().Assembly.FullName!);
            var localizerFactory = services.BuildServiceProvider().GetService<IStringLocalizerFactory>();
            var L = localizerFactory!.Create(nameof(ModelResources), assemblyName.Name!);

            options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(x => L["The value '{0}' is invalid.", x]);
            options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(x => L["The value '{0}' is invalid.", x]);
            options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(x => L["The '{0}' field must be a number.", x]);
            options.ModelBindingMessageProvider.SetMissingBindRequiredValueAccessor(x => L["A value for the '{0}' property was not provided.", x]);
            options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((x, y) => L["The value '{0}' is not valid for {1}.", x, y]);
            options.ModelBindingMessageProvider.SetMissingKeyOrValueAccessor(() => L["A value is required."]);
            options.ModelBindingMessageProvider.SetUnknownValueIsInvalidAccessor(x => L["The supplied value is invalid for {0}.", x]);
            options.ModelBindingMessageProvider.SetMissingRequestBodyRequiredValueAccessor(() => L["A non-empty request body is required."]);
            options.ModelBindingMessageProvider.SetNonPropertyAttemptedValueIsInvalidAccessor(x => L["The value '{0}' is not valid.", x]);
            options.ModelBindingMessageProvider.SetNonPropertyUnknownValueIsInvalidAccessor(() => L["The supplied value is invalid."]);
            options.ModelBindingMessageProvider.SetNonPropertyValueMustBeANumberAccessor(() => L["NonPropertyValueMustBeNumber"]);

        });
    }
}