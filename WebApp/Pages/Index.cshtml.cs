using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace WebApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly DAL.ApplicationDbContext _context;
        
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(DAL.ApplicationDbContext context, ILogger<IndexModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IList<Game> Game { get; set; } = default!;

        public async Task OnGetAsync()
        {
            Game = await _context.Games
                .OrderByDescending(g => g.GameId)
                .Include(g => g.GameOption)
                .Include(g => g.PlayerA)
                .Include(g => g.PlayerB).ToListAsync();
        }
    }
}