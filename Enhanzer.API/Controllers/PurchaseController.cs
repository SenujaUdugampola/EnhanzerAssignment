using System.Security.Claims;
using Enhanzer.API.Data;
using Enhanzer.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Enhanzer.API.Controllers;

[ApiController]
[Route("api/purchases")]
[Authorize]
public class PurchaseController(AppDbContext dbContext) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        CreatePurchaseBillRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Items is null || request.Items.Count == 0)
        {
            return BadRequest(new { message = "At least one purchase item is required." });
        }

        if (request.Items.Any(item =>
                string.IsNullOrWhiteSpace(item.ItemName) ||
                string.IsNullOrWhiteSpace(item.BatchCode) ||
                item.StandardCost < 0 ||
                item.StandardPrice < 0 ||
                item.MarginPercent < 0 ||
                item.Quantity <= 0 ||
                item.FreeQuantity < 0 ||
                item.DiscountPercent is < 0 or > 100))
        {
            return BadRequest(new { message = "One or more purchase items are invalid." });
        }

        var bill = new PurchaseBill
        {
            BillNo = $"PB-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1, 1_000_000):D6}",
            CreatedBy = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name,
            Items = request.Items.Select(item => new PurchaseBillItem
            {
                ItemName = item.ItemName.Trim(),
                BatchCode = item.BatchCode.Trim(),
                StandardCost = item.StandardCost,
                StandardPrice = item.StandardPrice,
                MarginPercent = item.MarginPercent,
                Quantity = item.Quantity,
                FreeQuantity = item.FreeQuantity,
                DiscountPercent = item.DiscountPercent
            }).ToList()
        };

        dbContext.PurchaseBills.Add(bill);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(Get), new { id = bill.Id }, new
        {
            bill.Id,
            bill.BillNo,
            bill.PurchaseDate,
            bill.CreatedBy,
            TotalQuantity = bill.Items.Sum(item => item.Quantity),
            TotalFreeQuantity = bill.Items.Sum(item => item.FreeQuantity),
            TotalCost = bill.Items.Sum(item =>
                item.StandardCost * item.Quantity * (1 - item.DiscountPercent / 100)),
            TotalSelling = bill.Items.Sum(item => item.StandardPrice * item.Quantity)
        });
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id, CancellationToken cancellationToken)
    {
        var bill = await dbContext.PurchaseBills
            .AsNoTracking()
            .Include(item => item.Items)
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

        return bill is null ? NotFound() : Ok(bill);
    }
}
