using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using DAL;
using Domain;

namespace WebApp.Pages_Games
{
    public class CreateModel : PageModel
    {
        private readonly DAL.ApplicationDbContext _context;

        public CreateModel(DAL.ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
        ViewData["GameOptionId"] = new SelectList(_context.GameOptions, "GameOptionId", "GameOptionId");
        ViewData["PlayerAId"] = new SelectList(_context.Players, "PlayerId", "PlayerId");
        ViewData["PlayerBId"] = new SelectList(_context.Players, "PlayerId", "PlayerId");
            return Page();
        }

        [BindProperty]
        public Game? Game { get; set; }

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Games.Add(Game!);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
