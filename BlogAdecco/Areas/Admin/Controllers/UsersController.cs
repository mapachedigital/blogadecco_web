// Copyright (c) 2021, Mapache Digital
// Version: 1.1
// Author: Samuel Kobelkowsky
// Email: samuel@mapachedigital.com

using BlogAdecco.Areas.Admin.Models.ApplicationUserViewModels;
using BlogAdecco.Data;
using BlogAdecco.Models;
using BlogAdecco.Utils;
using CsvHelper;
using CsvHelper.Configuration;
using MDWidgets;
using MDWidgets.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Text;

namespace BlogAdecco.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = Globals.RoleAdmin + "," + Globals.RoleSupervisor)]
    public partial class UsersController(ApplicationDbContext _context,
        UserManager<ApplicationUser> _userManager,
        IUserUtils _userUtils,
        IBlogAdeccoUtils _blogAdeccoUtils,
        ISiteUtils _siteUtils,
        IConfiguration _configuration,
        IUserStore<ApplicationUser> _userStore,
        IStringLocalizer<SharedResources> L) : Controller
    {

        // GET: UsersController
        public async Task<IActionResult> Index(string? sortOrder,
            bool? resetCookie,
            string? searchString,
            int? pageNumber,
            int? pageSize)
        {
            // Determine the sort order from the GET parameters and send to the view for each column the proper link to create
            // Basically it inverts from ascending to descending the current (or visceversa) columns and for the other columns it does notheing
            // TODO: Store in a cookie
            ViewData["FirstnameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "firstname_desc" : "";
            ViewData["LastnameSortParm"] = sortOrder == "lastname" ? "lastname_desc" : "lastname";
            ViewData["DisplayNameSortParm"] = sortOrder == "display_name" ? "display_name_desc" : "display_name";
            ViewData["CompanySortParm"] = sortOrder == "company" ? "company_desc" : "company";
            ViewData["ApprovedSortParam"] = sortOrder == "approved" ? "approved_desc" : "approved";
            ViewData["EmailSortParm"] = sortOrder == "email" ? "email_desc" : "email";
            ViewData["PhoneNumberSortParm"] = sortOrder == "phone_number" ? "phone_number_desc" : "phone_number";
            ViewData["EnabledSortParm"] = sortOrder == "enabled" ? "enabled_desc" : "enabled";
            ViewData["ApprovedSortParm"] = sortOrder == "approved" ? "approved_desc" : "approved";
            ViewData["LastAccessSortParm"] = sortOrder == "last_access" ? "last_access_desc" : "last_access";

            // Get a sensible page size depending on what we received from GET variable and what is stored in a cookie.
            pageSize = this.CookiePageSize(pageSize);

            // Reset the pagination when a new search is performed ("searchString" is only populated when 
            // the search form is filled, and "currentFilter" is in all the links, that is, it preserves the search)
            if (resetCookie == true) { pageNumber = 1; }

            // Send to the view the current sort ordre
            ViewData["CurrentSort"] = sortOrder;

            // Send to the view the current search string
            ViewData["CurrentFilter"] = searchString;

            // Get all the users
            var users = _context.ApplicationUser
                .Select(x => x);

            // Apply a filter with the search string if any
            searchString = searchString?.Trim();
            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(x =>
                    x.Company.Contains(searchString) ||
                    (x.Firstname + " " + x.Lastname).Contains(searchString) ||
                    (x.Lastname + " " + x.Firstname).Contains(searchString) ||
                    (x.DisplayName != null && x.DisplayName.Contains(searchString)) ||
                    (x.Email != null && x.Email.Contains(searchString)) ||
                    (x.PhoneNumber != null && x.PhoneNumber.Contains(searchString)));
            }

            // And apply the order chosen
            users = sortOrder switch
            {
                "firstname_desc" => users.OrderByDescending(x => x.Firstname),
                "lastname_desc" => users.OrderByDescending(x => x.Lastname),
                "lastname" => users.OrderBy(x => x.Lastname),
                "display_name_desc" => users.OrderByDescending(x => x.DisplayName),
                "display_name" => users.OrderBy(x => x.DisplayName),
                "company_desc" => users.OrderByDescending(x => x.Company),
                "company" => users.OrderBy(x => x.Company),
                "approved_desc" => users.OrderByDescending(x => x.Approved),
                "approved" => users.OrderBy(x => x.Approved),
                "email_desc" => users.OrderByDescending(x => x.Email),
                "email" => users.OrderBy(x => x.Email),
                "phone_number_desc" => users.OrderByDescending(x => x.PhoneNumber),
                "phone_number" => users.OrderBy(x => x.PhoneNumber),
                "enabled" => users.OrderBy(x => x.LockoutEnabled),
                "enabled_desc" => users.OrderByDescending(x => x.LockoutEnabled),
                "last_access" => users.OrderBy(x => x.LastAccess),
                "last_access_desc" => users.OrderByDescending(x => x.LastAccess),
                _ => users.OrderBy(x => x.Firstname),
            };

            var usersVM = users.Select(x => new ApplicationUserListViewModel
            {
                Id = x.Id,
                FullName = x.FullName,
                Company = x.Company,
                Email = x.Email,
                MobilePhone = x.PhoneNumber ?? string.Empty,
                AcceptTermsOfService = x.AcceptTermsOfService,
                Position = x.Position,
                Approved = x.Approved,
                ApplicationUser = x,
                LastAccess = x.LastAccess == default ? null : x.LastAccess,
                Role = string.Empty,
            }).ToList();

            foreach (var user in usersVM)
            {
                var roles = (await _userUtils.GetUserRolesAsync(user.Id)).Select(x => L[x]);
                user.Role = string.Join(", ", roles);
                user.Enabled = !await _userUtils.IsLockedOutAsync(user.ApplicationUser);
                user.CanBeEdited = await CanEditUserAsync(user.ApplicationUser);
            }

            return View(PaginatedList<ApplicationUserListViewModel>.Create(usersVM, pageNumber ?? 1, pageSize ?? MDGlobals.PageSizeDefault)); ;
        }

        // GET: UsersController/Create
        public async Task<ActionResult> Create()
        {
            ViewData["Roles"] = await GetAllowedRolesSelectList();

            return View();
        }

        // POST: UsersController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind("Id,Firstname,Lastname,DisplayName,Company,CompanyWebsite,LandlinePhone,MobilePhone,Position,AcceptTermsOfService,AcceptUsageOfLogo,Email,EmailConfirmed,Approved,UserName,Role,Password,ConfirmPassword")] ApplicationUserCreateViewModel userVM)
        {
            if (!await _userUtils.IsAdminAsync(onlySuperAdmin: true) && userVM.Role != Globals.RoleCompanyUser)
            {
                ModelState.AddModelError("Role", L["Only super admins can create admin users."]);
            }

            if (_context.ApplicationUser?.Any(x => (x.Firstname + x.Lastname).Equals(userVM.Firstname.Trim() + userVM.Lastname.Trim())) == true)
            {
                ModelState.AddModelError("", L["Name must be unique."]);
            }

            if (_context.ApplicationUser?.Any(x => x.Email == null || x.Email.Equals(userVM.Email.Trim())) == true)
            {
                ModelState.AddModelError("Email", L["Email must be unique."]);
            }

            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    Firstname = userVM.Firstname.Trim(),
                    Lastname = userVM.Lastname.Trim(),
                    DisplayName = userVM.DisplayName?.Trim(),
                    Company = userVM.Company.Trim(),
                    Position = userVM.Position?.Trim(),
                    AcceptTermsOfService = userVM.AcceptTermsOfService,
                    Language = _siteUtils.GetDefaultLanguage(),
                    Approved = userVM.Approved,
                    EmailConfirmed = true,
                    UserName = userVM.Email?.Trim(),
                };

                IdentityResult createResult;
                IdentityResult roleResult = new();

                await _userStore.SetUserNameAsync(user, userVM.Email, CancellationToken.None);
                await _userUtils.GetEmailStore().SetEmailAsync(user, userVM.Email, CancellationToken.None);

                createResult = await _userManager.CreateAsync(user, userVM.Password);

                if (createResult.Succeeded)
                {
                    roleResult = await _userManager.AddToRoleAsync(user, userVM.Role);

                    if (roleResult.Succeeded)
                    {
                        return RedirectToAction(nameof(Details), new { user.Id });
                    }
                }

                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ViewData["Roles"] = await GetAllowedRolesSelectList(userVM.Role);

            return View(userVM);
        }

        public async Task<ActionResult> Edit(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);

            // Super admin cannot be modified
            if (user == null)
            {
                return NotFound();
            }

            if (!await CanEditUserAsync(user))
            {
                return Unauthorized();
            }

            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

            var userVM = new ApplicationUserEditViewModel
            {
                Id = user.Id,
                Firstname = user.Firstname,
                Lastname = user.Lastname,
                DisplayName = user.DisplayName,
                Company = user.Company,
                AcceptTermsOfService = user.AcceptTermsOfService,
                Position = user.Position,
                Email = user.Email ?? string.Empty,
                // The default is the lowest role
                Role = role ?? Globals.RoleCompanyUser,
                Approved = user.Approved,
                MobilePhone = user.PhoneNumber,
                Enabled = !await _userUtils.IsLockedOutAsync(user),
            };

            ViewData["Roles"] = await GetAllowedRolesSelectList(role);

            return View(userVM);
        }

        // POST: UsersController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(string id, [Bind("Id,Firstname,Lastname,DisplayName,Company,CompanyWebsite,LandlinePhone,MobilePhone,Position,AcceptTermsOfService,AcceptUsageOfLogo,Email,EmailConfirmed,Approved,UserName,Role,Password,ConfirmPassword,Enabled")] ApplicationUserEditViewModel userVM)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            if (!await CanEditUserAsync(user))
            {
                return Unauthorized();
            }

            if (!await _userUtils.IsAdminAsync(onlySuperAdmin: true) && userVM.Role != Globals.RoleCompanyUser)
            {
                ModelState.AddModelError("Role", L["Only super admins can edit admin users."]);
            }

            if (_context.ApplicationUser?.Where(x => x.Id != userVM.Id).Any(x => (x.Firstname + x.Lastname).Equals(userVM.Firstname.Trim() + userVM.Lastname.Trim())) == true)
            {
                ModelState.AddModelError("Firstname", L["Name must be unique."]);
            }

            if (_context.ApplicationUser?.Where(x => x.Id != userVM.Id).Any(x => x.Email == null || userVM.Email == null || x.Email.Equals(userVM.Email.Trim())) == true)
            {
                ModelState.AddModelError("Email", L["Email must be unique."]);
            }

            if (ModelState.IsValid)
            {
                IdentityResult updateResult;
                IdentityResult roleResult = new();
                IdentityResult passwordResult = new();

                user.Firstname = userVM.Firstname.Trim();
                user.Lastname = userVM.Lastname.Trim();
                user.DisplayName = userVM.DisplayName?.Trim();
                user.Company = userVM.Company.Trim();
                user.AcceptTermsOfService = userVM.AcceptTermsOfService;
                user.Position = userVM.Position?.Trim();
                user.EmailConfirmed = true;
                user.UserName = userVM.Email?.Trim();
                user.Approved = userVM.Approved;

                await _userManager.SetLockoutEnabledAsync(user, true);
                await _userManager.SetLockoutEndDateAsync(user, userVM.Enabled ? DateTimeOffset.MinValue : DateTimeOffset.MaxValue);

                await _userUtils.GetEmailStore().SetEmailAsync(user, userVM.Email, CancellationToken.None);

                updateResult = await _userManager.UpdateAsync(user);
                if (updateResult.Succeeded)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Count > 0)
                    {
                        await _userManager.RemoveFromRolesAsync(user, roles);
                    }

                    roleResult = await _userManager.AddToRoleAsync(user, userVM.Role);
                    if (roleResult.Succeeded)
                    {
                        // If the user is admin, do special stuff
                        //if (userVM.Role == Globals.RoleSupervisor || userVM.Role == Globals.RoleAdmin)
                        //{
                        //    await _context.SaveChangesAsync();
                        //}

                        if (!string.IsNullOrWhiteSpace(userVM.Password))
                        {
                            await _userManager.RemovePasswordAsync(user);

                            passwordResult = await _userManager.AddPasswordAsync(user, userVM.Password.Trim());
                            if (passwordResult.Succeeded)
                            {
                                return RedirectToAction(nameof(Details), new { userVM.Id });
                            }
                        }
                        else
                        {
                            return RedirectToAction(nameof(Details), new { userVM.Id });
                        }
                    }
                }

                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                foreach (var error in passwordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ViewData["Roles"] = await GetAllowedRolesSelectList(userVM.Role);

            return View(userVM);
        }

        // GET: UsersController/Details/5
        public async Task<ActionResult> Details(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
            var userVM = new ApplicationUserDetailsViewModel
            {
                Id = user.Id,
                Firstname = user.Firstname,
                Lastname = user.Lastname,
                DisplayName = user.DisplayName,
                Company = user.Company,
                AcceptTermsOfService = user.AcceptTermsOfService,
                Position = user.Position,
                Email = user.Email ?? string.Empty,
                MobilePhone = user.PhoneNumber ?? string.Empty,
                Role = L[role ?? string.Empty],
                Enabled = !await _userUtils.IsLockedOutAsync(user),
                Approved = user.Approved,
            };

            ViewData["CanEdit"] = await CanEditUserAsync(user);
            return View(userVM);
        }

        // GET: UsersController/Delete/5
        public async Task<ActionResult> Delete(string? id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null || id == _userUtils.GetUserId())
            {
                return NotFound();
            }

            if (!await CanEditUserAsync(user))
            {
                return Unauthorized();
            }

            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
            var userVM = new ApplicationUserDeleteViewModel
            {
                Id = user.Id,
                Firstname = user.Firstname,
                Lastname = user.Lastname,
                DisplayName = user.DisplayName,
                Company = user.Company,
                AcceptTermsOfService = user.AcceptTermsOfService,
                Position = user.Position,
                Email = user.Email ?? string.Empty,
                MobilePhone = user.PhoneNumber ?? string.Empty,
                Role = L[role ?? string.Empty],
                NewId = null,
                Enabled = !await _userUtils.IsLockedOutAsync(user),
                Approved = user.Approved,
            };

            var applicationUsers = await _userUtils.GetAllUsersAsync(true);

            ViewData["NewId"] = new SelectList(applicationUsers, "Id", "FullName");
            ViewData["IsUsed"] =
                await _context.Attachment.AnyAsync(x => x.CreatedById == user.Id) ||
                await _context.Post.AnyAsync(x => x.CreatedById == user.Id);

            return View(userVM);
        }

        public async Task<ActionResult> Export()
        {
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, Encoding.UTF8);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Encoding = Encoding.UTF8,
            };

            var csv = new CsvWriter(writer, config);
            csv.Context.RegisterClassMap<ApplicationUserExportViewModelMap>();

            var usersVM = _context.ApplicationUser
                .OrderBy(x => x.Firstname)
                .ThenBy(x => x.Lastname)
                .Select(x => new ApplicationUserListViewModel
                {
                    Id = x.Id,
                    FullName = x.FullName,
                    Company = x.Company,
                    Email = x.Email,
                    MobilePhone = x.PhoneNumber ?? string.Empty,
                    AcceptTermsOfService = x.AcceptTermsOfService,
                    Position = x.Position,
                    Approved = x.Approved,
                    ApplicationUser = x,
                    LastAccess = x.LastAccess == default ? null : x.LastAccess,
                    Role = string.Empty,
                })
                .ToList();

            foreach (var user in usersVM)
            {
                var roles = (await _userUtils.GetUserRolesAsync(user.Id)).Select(x => L[x]);
                user.Role = string.Join(", ", roles);
                user.Enabled = !await _userUtils.IsLockedOutAsync(user.ApplicationUser);
                user.CanBeEdited = await CanEditUserAsync(user.ApplicationUser);
            }

            var values = usersVM.Select(x => new ApplicationUserExportViewModel
            {
                Firstname = x.ApplicationUser.Firstname,
                Lastname = x.ApplicationUser.Lastname,
                DisplayName = x.ApplicationUser.DisplayName,
                Company = x.ApplicationUser.Company,
                Position = x.ApplicationUser.Position,
                Email = x.ApplicationUser.Email ?? string.Empty,
                MobilePhone = x.ApplicationUser.PhoneNumber ?? string.Empty,
                AcceptTermsOfService = x.ApplicationUser.AcceptTermsOfService,
                Approved = x.ApplicationUser.Approved,
                LastAccess = x.LastAccess,
                Roles = x.Role,
                Enabled = x.Enabled,
            });

            await csv.WriteRecordsAsync(values);
            writer.Flush();
            stream.Position = 0;
            return File(stream.ToArray(), "text/csv", $"{_siteUtils.GetSiteName()} - {L["Users"]} - {DateTime.Now:yyyy-MM-dd}.csv");
        }

        // POST: UsersController/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id, [Bind("Id,Firstname,Lastname,DisplayName,Company,Email,PhoneNumber,Role,Enabled,NewId")] ApplicationUserDeleteViewModel userVM)
        {
            var user = await _userManager.FindByIdAsync(id);
            var currentUserId = _userUtils.GetUserId();

            if (user == null || currentUserId == null)
            {
                return NotFound();
            }

            var isUsed =
                await _context.Attachment.AnyAsync(x => x.CreatedById == user.Id) ||
                await _context.Post.AnyAsync(x => x.CreatedById == user.Id);

            if (isUsed && string.IsNullOrEmpty(userVM.NewId) || userVM.NewId == id)
            {
                ModelState.AddModelError("NewId", L["Please select a valid option."]);

                var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

                var applicationUsers = await _userUtils.GetAllUsersAsync(true);

                ViewData["NewId"] = new SelectList(applicationUsers, "Id", "FullName", userVM.Id);
                ViewData["IsUsed"] = isUsed;

                return View(nameof(Delete), userVM);
            }

            // Update entities
            await _context.Attachment.Where(x => x.CreatedById == id).ForEachAsync(x => x.CreatedById = null);
            await _context.Post.Where(x => x.CreatedById == id).ForEachAsync(x => x.CreatedById = null);

            await _context.SaveChangesAsync();

            var deleteResult = await _userManager.DeleteAsync(user);

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Determine if the current user can edit another user, depending on the role of both
        /// </summary>
        private async Task<bool> CanEditUserAsync(ApplicationUser user)
        {
            var myUserId = _userUtils.GetUserId();

            var iAmAdmin = await _userUtils.IsAdminAsync(onlySuperAdmin: false);

            var iAmSuperAdmin = await _userUtils.IsAdminAsync(onlySuperAdmin: true);

            var otherUserIsAdmin = await _userUtils.IsAdminAsync(user, false);

            // Self cannot be edited
            if (user.Id == myUserId) return false;

            // Only admin users can edit users.
            if (!iAmAdmin) return false;

            // The super super super admin cannot be edited
            if (user.Email?.ToLower() == _configuration[Globals.ConfigSuperAdminEmail]?.ToLower()) return false;

            // Super admins can edit everyone (except self, but that's checked above)
            if (iAmSuperAdmin) return true;

            // Admins can only be edited by super admins
            if (otherUserIsAdmin && !iAmSuperAdmin) return false;


            return true;
        }

        /// <summary>
        /// Determine the list of roles that the current user can give to another user
        /// </summary>
        private async Task<SelectList> GetAllowedRolesSelectList(string? selectedValue = null)
        {
            var allowedRoles = await _blogAdeccoUtils.MySubordinatedRolesAsync();
            if (await _userUtils.IsAdminAsync(onlySuperAdmin: true))
            {
                if (!allowedRoles.Contains(Globals.RoleAdmin)) allowedRoles.Add(Globals.RoleAdmin);
                if (!allowedRoles.Contains(Globals.RoleSupervisor)) allowedRoles.Add(Globals.RoleSupervisor);
            }

            var rolesSelectList = new SelectList(allowedRoles.OrderBy(x => L[x].ToString()).Select(x => new SelectListItem { Text = L[x], Value = x }), "Value", "Text", selectedValue);
            return rolesSelectList;
        }
    }
}