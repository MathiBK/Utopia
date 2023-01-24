using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using utopia.Controllers;
using utopia.Data;
using utopia.Helper;
using utopia.Models;

namespace utopia.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;

        public RegisterModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender, 
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(16, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 3)]
            [Display(Name = "Username")]
            public string Username { get; set; }
            
            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
            
            [Display(Name = "Select a Tribe")]
            public int ChoosenTribe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ViewData.Add("TribeList", new SelectList(_context.Tribes.ToList(), "Id", "Name"));
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = Input.Username, Email = Input.Email };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var p = new Player(Input.Username);
                    var tl = _context.Tribes
                        .Include(t => t.Village)
                        .ThenInclude(v => v.Tile).ToList();
                    p.Tribe = tl.Single(t => t.Id == Input.ChoosenTribe);
                    p.Tribe.TribePlayers.Add(p);
                    p.Tile = p.Tribe.Village.Tile;
                    p.Tile.TilePlayers.Add(p);

                    var resourceList = _context.Resources.ToList();
                    foreach (var r in resourceList)
                    {
                        var pr = new PlayerResource(r,0);
                        if (r.ResourceName == "Food" || r.ResourceName == "Water")
                        {
                            pr.Amount = 200;
                        }
                        pr.Player = p;
                        p.PlayerResources.Add(pr);
                        _context.PlayerResources.Add(pr);
                    }
                    List<Tile> AllTiles = _context.Tiles.ToList();
                    foreach (var tile in AllTiles)
                    {
                        if (Hex.Distance(p.Tile.HexCordX, p.Tile.HexCordY, tile.HexCordX, tile.HexCordY) <= 1)
                        {
                            PlayerSeenTile playerSeenTile = new PlayerSeenTile(p, tile);
                            p.PlayerSeenTiles.Add(playerSeenTile);
                            tile.PlayerSeenTiles.Add(playerSeenTile);
                            _context.PlayerSeenTiles.Add(playerSeenTile);
                        }
                    }
                    _context.Tiles.Update(p.Tile);
                    _context.Tribes.Update(p.Tribe);
                    _context.Players.Add(p);
                    await _context.SaveChangesAsync();
                    
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code },
                        protocol: Request.Scheme);

                    await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
