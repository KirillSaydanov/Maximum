using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Maximum.Models;
using Maximum.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Maximum.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // GET: /Admin/Users
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users
                .Select(u => new UserViewModel
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    IsActive = u.IsActive,
                    RegistrationDate = u.RegistrationDate,
                    Roles = _userManager.GetRolesAsync(u).Result.ToList()
                })
                .ToListAsync();

            return View(users);
        }

        // GET: /Admin/CreateUser
        public IActionResult CreateUser()
        {
            ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View();
        }

        // POST: /Admin/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailConfirmed = true,
                    IsActive = true,
                    RegistrationDate = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                
                if (result.Succeeded)
                {
                    // Добавляем роли
                    if (model.Roles != null && model.Roles.Any())
                    {
                        await _userManager.AddToRolesAsync(user, model.Roles);
                    }

                    TempData["SuccessMessage"] = "Пользователь успешно создан!";
                    return RedirectToAction(nameof(Users));
                }
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ViewBag.Roles = _roleManager.Roles.Select(r => r.Name).ToList();
            return View(model);
        }

        // GET: /Admin/EditUser
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var allRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                Roles = userRoles.ToList(),
                AllRoles = allRoles
            };

            return View(model);
        }

        // POST: /Admin/EditUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                {
                    return NotFound();
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.IsActive = model.IsActive;

                var result = await _userManager.UpdateAsync(user);
                
                if (result.Succeeded)
                {
                    // Обновляем роли
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    var rolesToRemove = currentRoles.Except(model.Roles ?? new List<string>());
                    var rolesToAdd = (model.Roles ?? new List<string>()).Except(currentRoles);

                    if (rolesToRemove.Any())
                    {
                        await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                    }

                    if (rolesToAdd.Any())
                    {
                        await _userManager.AddToRolesAsync(user, rolesToAdd);
                    }

                    TempData["SuccessMessage"] = "Пользователь успешно обновлен!";
                    return RedirectToAction(nameof(Users));
                }
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        // POST: /Admin/DeleteUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Нельзя удалить самого себя
            if (user.Id == User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value)
            {
                TempData["ErrorMessage"] = "Нельзя удалить свой собственный аккаунт!";
                return RedirectToAction(nameof(Users));
            }

            var result = await _userManager.DeleteAsync(user);
            
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Пользователь успешно удален!";
            }
            else
            {
                TempData["ErrorMessage"] = "Ошибка при удалении пользователя!";
            }

            return RedirectToAction(nameof(Users));
        }

        // GET: /Admin/Roles
        public async Task<IActionResult> Roles()
        {
            var roles = await _roleManager.Roles
                .Select(r => new RoleViewModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    UserCount = _userManager.GetUsersInRoleAsync(r.Name).Result.Count
                })
                .ToListAsync();

            return View(roles);
        }

        // POST: /Admin/CreateRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            if (!string.IsNullOrWhiteSpace(roleName))
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
                    if (result.Succeeded)
                    {
                        TempData["SuccessMessage"] = "Роль успешно создана!";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Ошибка при создании роли!";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Роль с таким именем уже существует!";
                }
            }

            return RedirectToAction(nameof(Roles));
        }
    }

    // Модели представления
    public class UserViewModel
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsActive { get; set; }
        public DateTime RegistrationDate { get; set; }
        public List<string> Roles { get; set; } = new();
    }

    public class CreateUserViewModel
    {
        [Required(ErrorMessage = "Email обязателен")]
        [EmailAddress(ErrorMessage = "Некорректный формат email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Имя обязательно")]
        [Display(Name = "Имя")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Фамилия обязательна")]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Пароль обязателен")]
        [StringLength(100, ErrorMessage = "Пароль должен содержать минимум {2} символов", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        [Compare("Password", ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "Роли")]
        public List<string>? Roles { get; set; }
    }

    public class EditUserViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Имя обязательно")]
        [Display(Name = "Имя")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Фамилия обязательна")]
        [Display(Name = "Фамилия")]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Активен")]
        public bool IsActive { get; set; }

        [Display(Name = "Роли")]
        public List<string>? Roles { get; set; }

        public List<string> AllRoles { get; set; } = new();
    }

    public class RoleViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int UserCount { get; set; }
    }
}
