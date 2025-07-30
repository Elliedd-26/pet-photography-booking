using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetPhotographyApp.Data;
using PetPhotographyApp.Models;

[Route("OwnersPage")]
public class OwnerPageController : Controller
{
    private readonly ApplicationDbContext _context;

    public OwnerPageController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /OwnersPage
    [HttpGet("")]
    public async Task<IActionResult> Index()
    {
        var owners = await _context.Owners.ToListAsync();
        return View(owners);
    }

    // GET: /OwnersPage/Details/5
    [HttpGet("Details/{id}")]
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var owner = await _context.Owners
            .Include(o => o.Pets)
            .Include(o => o.Bookings)
            .Include(o => o.Notifications)
            .FirstOrDefaultAsync(m => m.OwnerId == id);

        if (owner == null) return NotFound();

        return View(owner);
    }

    // GET: /OwnersPage/Create
    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: /OwnersPage/Create
    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("OwnerId,Name,Email,PhoneNumber,Address")] Owner owner)
    {
        if (ModelState.IsValid)
        {
            _context.Add(owner);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(owner);
    }

    // GET: /OwnersPage/Edit/5
    [HttpGet("Edit/{id}")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var owner = await _context.Owners.FindAsync(id);
        if (owner == null) return NotFound();

        return View(owner);
    }

    // POST: /OwnersPage/Edit/5
    [HttpPost("Edit/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("OwnerId,Name,Email,PhoneNumber,Address")] Owner owner)
    {
        if (id != owner.OwnerId) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(owner);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OwnerExists(owner.OwnerId)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(owner);
    }

    // GET: /OwnersPage/Delete/5
    [HttpGet("Delete/{id}")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var owner = await _context.Owners
            .FirstOrDefaultAsync(m => m.OwnerId == id);
        if (owner == null) return NotFound();

        return View(owner);
    }

    // POST: /OwnersPage/Delete/5
    [HttpPost("Delete/{id}"), ActionName("DeleteConfirmed")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var owner = await _context.Owners.FindAsync(id);
        if (owner != null)
        {
            _context.Owners.Remove(owner);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private bool OwnerExists(int id)
    {
        return _context.Owners.Any(e => e.OwnerId == id);
    }
}
