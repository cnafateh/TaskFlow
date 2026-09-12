using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Web.Services;
using TaskFlow.Web.ViewModels.Admin;

namespace TaskFlow.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }


    public async Task<IActionResult> Index()
    {
        AdminDashboardViewModel model =
            await _adminService.GetDashboardAsync();

        return View(model);
    }


    public async Task<IActionResult> Users()
    {
        List<AdminUserViewModel> users =
            await _adminService.GetUsersAsync();

        return View(users);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeRole(
        string userId,
        string role)
    {
        string? currentAdminId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (currentAdminId == null)
        {
            return Unauthorized();
        }


        string? error =
            await _adminService.UpdateUserRoleAsync(
                userId,
                role,
                currentAdminId);


        if (error != null)
        {
            TempData["ErrorMessage"] = error;

            return RedirectToAction(
                nameof(Users));
        }


        TempData["SuccessMessage"] =
            "User role updated successfully.";

        return RedirectToAction(
            nameof(Users));
    }
}