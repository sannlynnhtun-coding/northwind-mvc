using Microsoft.AspNetCore.Mvc;
using NorthwindCharts.Mvc.Services.Abstractions;
using NorthwindCharts.Mvc.Services.Common;
using NorthwindCharts.Mvc.ViewModels.Orders;
using NorthwindCharts.Mvc.ViewModels.Shared;

namespace NorthwindCharts.Mvc.Controllers;

[Route("Orders")]
public sealed class OrdersController : Controller
{
    private readonly IOrderService _orderService;
    private readonly ICustomerService _customerService;
    private readonly IShipperService _shipperService;

    public OrdersController(IOrderService orderService, ICustomerService customerService, IShipperService shipperService)
    {
        _orderService = orderService;
        _customerService = customerService;
        _shipperService = shipperService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? search = null)
    {
        var result = await _orderService.SearchAsync(new PagedQuery { Page = page, PageSize = pageSize, Search = search, Descending = true });
        return View(new OrderIndexVm
        {
            Items = result.Items,
            Pagination = new PaginationVm { Page = result.Page, PageSize = result.PageSize, TotalCount = result.TotalCount, Search = search }
        });
    }

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        return View(await BuildCreatePageVmAsync(new OrderCreateVm()));
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrderCreateVm form)
    {
        if (!ModelState.IsValid)
        {
            return View(await BuildCreatePageVmAsync(form));
        }

        var result = await _orderService.CreateAsync(form);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            return View(await BuildCreatePageVmAsync(form));
        }

        TempData["StatusMessage"] = "Order created. Add order lines and ship when ready.";
        return RedirectToAction(nameof(Edit), new { id = result.Value });
    }

    [HttpGet("Details/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var data = await _orderService.GetDetailsAsync(id);
        return data is null ? NotFound() : View(data);
    }

    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var page = await BuildEditPageVmAsync(id);
        return page is null ? NotFound() : View(page);
    }

    [HttpPost("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, OrderCreateVm form)
    {
        if (!ModelState.IsValid)
        {
            var pageVm = await BuildEditPageVmAsync(id, form);
            return pageVm is null ? NotFound() : View(pageVm);
        }

        var result = await _orderService.UpdateHeaderAsync(id, form);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error!);
            var pageVm = await BuildEditPageVmAsync(id, form);
            return pageVm is null ? NotFound() : View(pageVm);
        }

        TempData["StatusMessage"] = "Order header updated.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost("{id:int}/Lines/Add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddLine(int id, AddOrderLineVm addLine)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Invalid order line input.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        var result = await _orderService.AddLineAsync(id, addLine);
        TempData[result.Success ? "StatusMessage" : "ErrorMessage"] = result.Success ? "Order line added." : result.Error;
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost("{id:int}/Lines/{productId:int}/Update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateLine(int id, int productId, OrderLineInputVm line)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Invalid line update.";
            return RedirectToAction(nameof(Edit), new { id });
        }

        var result = await _orderService.UpdateLineAsync(id, productId, line);
        TempData[result.Success ? "StatusMessage" : "ErrorMessage"] = result.Success ? "Order line updated." : result.Error;
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost("{id:int}/Lines/{productId:int}/Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLine(int id, int productId)
    {
        var result = await _orderService.RemoveLineAsync(id, productId);
        TempData[result.Success ? "StatusMessage" : "ErrorMessage"] = result.Success ? "Order line removed." : result.Error;
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost("{id:int}/Ship")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ship(int id)
    {
        var result = await _orderService.ShipAsync(id);
        TempData[result.Success ? "StatusMessage" : "ErrorMessage"] = result.Success ? "Order shipped successfully." : result.Error;
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpGet("Delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _orderService.GetDeleteAsync(id);
        if (!result.Success)
        {
            return NotFound();
        }

        return View(result.Value);
    }

    [HttpPost("Delete/{id:int}")]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var result = await _orderService.DeleteAsync(id);
        TempData[result.Success ? "StatusMessage" : "ErrorMessage"] = result.Success ? "Order deleted." : result.Error;
        return RedirectToAction(nameof(Index));
    }

    private async Task<OrderFormPageVm> BuildCreatePageVmAsync(OrderCreateVm form)
    {
        return new OrderFormPageVm
        {
            Form = form,
            Customers = await _customerService.GetCustomerOptionsAsync(),
            Shippers = await _shipperService.GetShipperOptionsAsync()
        };
    }

    private async Task<OrderEditPageVm?> BuildEditPageVmAsync(int id, OrderCreateVm? overrideHeader = null)
    {
        var details = await _orderService.GetDetailsAsync(id);
        if (details is null)
        {
            return null;
        }

        if (overrideHeader is not null)
        {
            details = new OrderEditVm
            {
                OrderId = details.OrderId,
                Header = overrideHeader,
                Lines = details.Lines,
                Subtotal = details.Subtotal,
                GrandTotal = details.GrandTotal,
                IsShipped = details.IsShipped
            };
        }

        return new OrderEditPageVm
        {
            Order = details,
            ProductOptions = await _orderService.GetProductOptionsAsync(),
            Customers = await _customerService.GetCustomerOptionsAsync(),
            Shippers = await _shipperService.GetShipperOptionsAsync(),
            AddLine = new AddOrderLineVm { Quantity = 1, Discount = 0f }
        };
    }
}
