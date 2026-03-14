using Microsoft.AspNetCore.Mvc;
using NorthwindCharts.Mvc.Services.Abstractions;
using NorthwindCharts.Mvc.Services.Common;
using NorthwindCharts.Mvc.ViewModels.Shared;
using NorthwindCharts.Mvc.ViewModels.Suppliers;

namespace NorthwindCharts.Mvc.Controllers;

public sealed class SuppliersController : Controller
{
    private readonly ISupplierService _service;

    public SuppliersController(ISupplierService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? search = null)
    {
        var result = await _service.SearchAsync(new PagedQuery { Page = page, PageSize = pageSize, Search = search });
        return View(new SupplierIndexVm
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

    public IActionResult Create() => View(new SupplierFormVm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SupplierFormVm form)
    {
        if (!ModelState.IsValid)
        {
            return View(form);
        }

        var result = await _service.CreateAsync(form);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(form);
        }

        TempData["StatusMessage"] = "Supplier created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var form = await _service.GetFormAsync(id);
        return form is null ? NotFound() : View(form);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SupplierFormVm form)
    {
        if (!ModelState.IsValid)
        {
            return View(form);
        }

        var result = await _service.UpdateAsync(id, form);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(form);
        }

        TempData["StatusMessage"] = "Supplier updated.";
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
        TempData[result.Success ? "StatusMessage" : "ErrorMessage"] = result.Success ? "Supplier deleted." : result.Error;
        return RedirectToAction(nameof(Index));
    }
}
