using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Web.Models;
using TaskFlow.Web.Services;
using TaskFlow.Web.ViewModels.Categories;

namespace TaskFlow.Web.Controllers;

[Authorize]
public class CategoriesController : Controller
{
    private readonly ICategoryService _categoryService;

    private readonly UserManager<ApplicationUser> _userManager;

    public CategoriesController(
        ICategoryService categoryService,
        UserManager<ApplicationUser> userManager)
    {
        _categoryService = categoryService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        string? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        List<Category> categories =
            await _categoryService.GetAllAsync(userId);

        return View(categories);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateCategoryViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        Category category = new Category
        {
            Name = model.Name,
            UserId = userId
        };

        await _categoryService.CreateAsync(category);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        string? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        Category? category =
            await _categoryService.GetByIdAsync(
                id,
                userId);

        if (category == null)
        {
            return NotFound();
        }

        EditCategoryViewModel model =
            new EditCategoryViewModel
            {
                Id = category.Id,
                Name = category.Name
            };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        EditCategoryViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        Category category = new Category
        {
            Id = model.Id,
            Name = model.Name
        };

        bool updated =
            await _categoryService.UpdateAsync(
                category,
                userId);

        if (!updated)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        string? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        Category? category =
            await _categoryService.GetByIdAsync(
                id,
                userId);

        if (category == null)
        {
            return NotFound();
        }

        DeleteCategoryViewModel model =
            new DeleteCategoryViewModel
            {
                Id = category.Id,
                Name = category.Name
            };

        return View(model);
    }

    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(
        DeleteCategoryViewModel model)
    {
        string? userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        Category? category =
            await _categoryService.GetByIdAsync(
                model.Id,
                userId);

        if (category == null)
        {
            return NotFound();
        }

        bool hasTasks =
            await _categoryService.HasTasksAsync(
                model.Id,
                userId);

        if (hasTasks)
        {
            model.Name = category.Name;

            ModelState.AddModelError(
                "",
                "This category cannot be deleted because it is assigned to one or more tasks.");

            return View("Delete", model);
        }

        bool deleted =
            await _categoryService.DeleteAsync(
                model.Id,
                userId);

        if (!deleted)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Index));
    }

    private string? GetCurrentUserId()
    {
        return _userManager.GetUserId(User);
    }
}