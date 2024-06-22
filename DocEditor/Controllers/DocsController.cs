using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DocEditor.Data;
using DocEditor.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace DocEditor.Controllers
{
    [Authorize]
    public class DocsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DocsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Docs
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = from c in _context.Docs
                                       select c;
            applicationDbContext = _context.Docs.Where(a => a.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier));

            return View(await applicationDbContext.Include(d => d.User).ToListAsync());
        }

        // GET: Docs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Docs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Content,UserId")] Doc doc)
        {
            if (ModelState.IsValid)
            {
                var exists = await _context.Docs.FirstOrDefaultAsync(x => x.Title == doc.Title && x.UserId == doc.UserId);
                if(exists != null)
                {
                    return RedirectToAction("Error", new
                    {
                        StatusCode = 500,
                        Message = "File with this name already exists!"
                    });
                }
                _context.Add(doc);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", doc.UserId);
            return View(doc);
        }

        // GET: Docs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Error", new
                {
                    StatusCode = 404,
                    Message = "Not Found!"
                });
            }

            var doc = await _context.Docs.FindAsync(id);
            if (doc == null)
            {
                return RedirectToAction("Error", new
                {
                    StatusCode = 404,
                    Message = "Not Found!"
                });
            }

            if (doc.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return RedirectToAction("Error", new
                {
                    StatusCode = 404,
                    Message = "Not Found!"
                });
            }

            return View(doc);
        }

        // POST: Docs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,UserId")] Doc doc)
        {
            if (id != doc.Id)
            {
                return RedirectToAction("Error", new
                {
                    StatusCode = 404,
                    Message = "Not Found!"
                });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var exists = await _context.Docs.FirstOrDefaultAsync(x => x.Title == doc.Title && x.UserId == doc.UserId && x.Id != doc.Id);

                    if (exists != null)
                    {
                        return RedirectToAction("Error", new { StatusCode = 500, 
                            Message = "File with this name already exists!" });
                    }

                    _context.Update(doc);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DocExists(doc.Id))
                    {
                        return RedirectToAction("Error", new
                        {
                            StatusCode = 404,
                            Message = "Not Found!"
                        });
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", doc.UserId);
            return View(doc);
        }

        // GET: Docs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Error", new
                {
                    StatusCode = 404,
                    Message = "Not Found!"
                });
            }

            var doc = await _context.Docs
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (doc == null)
            {
                return RedirectToAction("Error", new
                {
                    StatusCode = 404,
                    Message = "Not Found!"
                });
            }

            if (doc.UserId != User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                return RedirectToAction("Error", new
                {
                    StatusCode = 404,
                    Message = "Not Found!"
                });
            }

            return View(doc);
        }

        // POST: Docs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var doc = await _context.Docs.FindAsync(id);
            if (doc != null && doc.UserId == User.FindFirstValue(ClaimTypes.NameIdentifier))
            {
                _context.Docs.Remove(doc);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Error(int status, string message)
        {
            var errorViewModel = new ErrorViewModel
            {
                StatusCode = status,
                Message = message
            };

            return View(errorViewModel);
        }

        private bool DocExists(int id)
        {
            return _context.Docs.Any(e => e.Id == id);
        }
    }
}
