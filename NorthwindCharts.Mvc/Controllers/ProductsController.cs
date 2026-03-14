using Microsoft.AspNetCore.Mvc;
using NorthwindCharts.Mvc.Services.Abstractions;
using NorthwindCharts.Mvc.Services.Common;
using NorthwindCharts.Mvc.ViewModels.Products;
using NorthwindCharts.Mvc.ViewModels.Shared;

namespace NorthwindCharts.Mvc.Controllers;

public sealed class ProductsController : Controller
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? search = null)
    {
        var result = await _service.SearchAsync(new PagedQuery { Page = page, PageSize = pageSize, Search = search });
        return View(new ProductIndexVm
        {
            Items = result.Items,
            Pagination = new PaginationVm { Page = result.Page, PageSize = result.PageSize, TotalCount = result.TotalCount, Search = search }
        });
    }

    public async Task<IActionResult> Details(int id)
    {
        var data = await _service.GetDetailsAsync(id);
        return data is null ? NotFound() : View(data);
    }

    public async Task<IActionResult> Create()
    {
        return View(await BuildFormPageVmAsync(new ProductFormVm()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormVm form)
    {
        if (!ModelState.IsValid)
        {
            return View(await BuildFormPageVmAsync(form));
        }

        var result = await _service.CreateAsync(form);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(await BuildFormPageVmAsync(form));
        }

        TempData["StatusMessage"] = "Product created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var form = await _service.GetFormAsync(id);
        if (form is null)
        {
            return NotFound();
        }

        return View(await BuildFormPageVmAsync(form, id));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductFormVm form)
    {
        if (!ModelState.IsValid)
        {
            return View(await BuildFormPageVmAsync(form, id));
        }

        var result = await _service.UpdateAsync(id, form);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(await BuildFormPageVmAsync(form, id));
        }

        TempData["StatusMessage"] = "Product updated.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.GetDeleteAsync(id);
        if (!result.Success)
        {
            return NotFound();
        }

        return View(result.Value);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _service.DeleteAsync(id);
        TempData[result.Success ? "StatusMessage" : "ErrorMessage"] = result.Success ? "Product archived (discontinued)." : result.Error;
        return RedirectToAction(nameof(Index));
    }

    private async Task<ProductFormPageVm> BuildFormPageVmAsync(ProductFormVm form, int? productId = null)
    {
        return new ProductFormPageVm
        {
            ProductId = productId,
            Form = form,
            CategoryOptions = await _service.GetCategoryOptionsAsync(),
            SupplierOptions = await _service.GetSupplierOptionsAsync()
        };
    }
}
