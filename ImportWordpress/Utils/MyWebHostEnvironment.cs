// Copyright (c) 2021, Mapache Digital
// Version: 1.2.4
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace ImportWordpress.Utils;

/// <summary>
/// Mock class for IWebHostEnvironment, which is used by StorageUtils to get the wwwroot path.
/// This is necessary because we don't have a real web host environment in this console application, but we need to provide the wwwroot path to StorageUtils to save the imported images in the correct location.
/// </summary>
class MyWebHostEnvironment : IWebHostEnvironment
{
    public string WebRootPath { get => Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(AppContext.BaseDirectory))) ?? throw new InvalidOperationException("Cannot determine the base directory")/*@"C:\Users\kobel\source\repos\BlogAdecco\BlogAdecco\wwwroot"*/; set => throw new NotImplementedException(); }
    public IFileProvider WebRootFileProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string ApplicationName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public IFileProvider ContentRootFileProvider { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string ContentRootPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string EnvironmentName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}