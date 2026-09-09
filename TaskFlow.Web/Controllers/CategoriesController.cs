using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Web.Models;
using TaskFlow.Web.Services;
using TaskFlow.Web.ViewModels.Categories;

namespace TaskFlow.Web.Controllers
{
    [Authorize]
    public class CategoriesController : Controller
        {
            private readonly ICategoryService _categoryService;

            public CategoriesController(ICategoryService Categorieservice)
            {
                _categoryService = Categorieservice;
            }

            public async Task<IActionResult> Index()
            {
                List<Category> Categories =
                    await _categoryService.GetAllAsync();

                return View(Categories);
            }

            [HttpGet]
            public IActionResult Create()
            {
                return View();
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Create(CreateCategoryViewModel model)
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                Category Category = new Category
                {
                    Name = model.Name,
                };

                await _categoryService.CreateAsync(Category);

                return RedirectToAction(nameof(Index));
            }

            [HttpGet]
            public async Task<IActionResult> Edit(int id)
            {
                Category? Category =
                    await _categoryService.GetByIdAsync(id);

                if (Category == null)
                {
                    return NotFound();
                }

                EditCategoryViewModel model = new EditCategoryViewModel
                {
                    Id = id,
                    Name = Category.Name,
                };

                return View(model);
            }

            [HttpPost]
            [ValidateAntiForgeryToken]
            public async Task<IActionResult> Edit(EditCategoryViewModel model)
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                Category Category = new Category
                {
                    Id = model.Id,
                    Name = model.Name,
                };

                bool updated =
                    await _categoryService.UpdateAsync(Category);

                if (!updated)
                {
                    return NotFound();
                }

                return RedirectToAction(nameof(Index));
            }

            [HttpGet]
            public async Task<IActionResult> Delete(int id)
            {
                Category? Category =
                    await _categoryService.GetByIdAsync(id);

                if (Category == null)
                {
                    return NotFound();
                }

                DeleteCategoryViewModel model = new DeleteCategoryViewModel
                {
                    Id = id,
                    Name = Category.Name,
                };

                return View(model);
            }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
        DeleteCategoryViewModel model)
        {
            bool hasTasks =
                await _categoryService.HasTasksAsync(model.Id);

            if (hasTasks)
            {
                Category? category =
                    await _categoryService.GetByIdAsync(model.Id);

                if (category == null)
                {
                    return NotFound();
                }

                model.Name = category.Name;

                ModelState.AddModelError(
                    "",
                    "This category cannot be deleted because it is assigned to one or more tasks.");

                return View("Delete", model);
            }

            bool deleted =
                await _categoryService.DeleteAsync(model.Id);

            if (!deleted)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
