// Copyright (c) 2021, Mapache Digital
// Version: 1.2
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using BlogAdecco.Data;
using BlogAdecco.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlogAdecco.Utils;

public interface IBlogAdeccoUtils
{     /// <summary>
      /// Obtain all the roles that are "inferior" to the current logged in user
      /// </summary>
    Task<List<string>> MySubordinatedRolesAsync();

    /// <summary>
    /// Obtain all the roles that are "inferior" to the given one
    /// </summary>
    /// <param name="myRole">The reference role</param>
    List<string> GetSubordinatedRoles(string myRole);
}

public partial class BlogAdeccoUtils(UserManager<ApplicationUser> _userManager,
        IUserUtils _userUtils, ApplicationDbContext _context) : IBlogAdeccoUtils
{
    /// <summary>
    /// Obtain all the roles that are "inferior" to the current logged in user
    /// </summary>
    public async Task<List<string>> MySubordinatedRolesAsync()
    {
        var myRole = (await _userManager.GetRolesAsync(await _userUtils.GetUserAsync())).FirstOrDefault();

        if (myRole == null) return [];

        return GetSubordinatedRoles(myRole);
    }

    /// <summary>
    /// Obtain all the roles that are "inferior" to the given one
    /// </summary>
    /// <param name="myRole">The reference role</param>
    public List<string> GetSubordinatedRoles(string myRole)
    {
        // Obtain all the roles
        var allRoles = Globals.Roles.ToList();

        // Admin is not subordinated to anyone
        allRoles.Remove(Globals.RoleAdmin);
        if (myRole == Globals.RoleAdmin) return allRoles;

        // Now the supervisor is not subordinated to anyone remaining in the roles list
        allRoles.Remove(Globals.RoleSupervisor);
        if (myRole == Globals.RoleSupervisor) return allRoles;

        // and so on...
        allRoles.Remove(Globals.RoleCompanyUser);
        return allRoles;
    }
}