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
            var users = await _userManager.Users.ToListAsync().ConfigureAwait(false);
            var userViewModels = new List<UserViewModel>();

            // Получаем роли для всех пользователей одним запросом
            var userRoles = new Dictionary<string, List<string>>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
                userRoles[user.Id] = roles.ToList();
            }

            foreach (var user in users)
            {
                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    IsActive = user.IsActive,
                    RegistrationDate = user.RegistrationDate,
                    Roles = userRoles[user.Id]
                });
            }

            return View(userViewModels);
        }

        // GET: /Admin/CreateUser
        public async Task<IActionResult> CreateUser()
        {
            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync().ConfigureAwait(false);
            ViewBag.Roles = roles;
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

                var result = await _userManager.CreateAsync(user, model.Password).ConfigureAwait(false);
                
                if (result.Succeeded)
                {
                    // Добавляем роли
                    if (model.Roles != null && model.Roles.Any())
                    {
                        await _userManager.AddToRolesAsync(user, model.Roles).ConfigureAwait(false);
                    }
                    else
                    {
                        // Если роли не выбраны, назначаем роль "User" по умолчанию
                        await _userManager.AddToRoleAsync(user, "User").ConfigureAwait(false);
                    }

                    TempData["SuccessMessage"] = "Пользователь успешно создан!";
                    return RedirectToAction(nameof(Users));
                }
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync().ConfigureAwait(false);
            ViewBag.Roles = roles;
            return View(model);
        }

        // GET: /Admin/EditUser
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id).ConfigureAwait(false);
            if (user == null)
            {
                return NotFound();
            }

            // Получаем роли пользователя и все доступные роли параллельно
            var userRolesTask = _userManager.GetRolesAsync(user).ConfigureAwait(false);
            var allRolesTask = _roleManager.Roles.Select(r => r.Name).ToListAsync().ConfigureAwait(false);

            await Task.WhenAll(userRolesTask, allRolesTask);

            var userRoles = userRolesTask.Result;
            var allRoles = allRolesTask.Result;

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
                var user = await _userManager.FindByIdAsync(model.Id).ConfigureAwait(false);
                if (user == null)
                {
                    return NotFound();
                }

                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.IsActive = model.IsActive;

                var result = await _userManager.UpdateAsync(user).ConfigureAwait(false);
                
                if (result.Succeeded)
                {
                    // Обновляем роли
                    var currentRoles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
                    var rolesToRemove = currentRoles.Except(model.Roles ?? new List<string>());
                    var rolesToAdd = (model.Roles ?? new List<string>()).Except(currentRoles);

                    if (rolesToRemove.Any())
                    {
                        await _userManager.RemoveFromRolesAsync(user, rolesToRemove).ConfigureAwait(false);
                    }

                    if (rolesToAdd.Any())
                    {
                        await _userManager.AddToRolesAsync(user, rolesToAdd).ConfigureAwait(false);
                    }

                    TempData["SuccessMessage"] = "Пользователь успешно обновлен!";
                    return RedirectToAction(nameof(Users));
                }
                
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync().ConfigureAwait(false);
            ViewBag.Roles = roles;
            return View(model);
        }

        // POST: /Admin/DeleteUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id).ConfigureAwait(false);
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

            var result = await _userManager.DeleteAsync(user).ConfigureAwait(false);
            
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
            var roles = await _roleManager.Roles.ToListAsync().ConfigureAwait(false);
            var roleViewModels = new List<RoleViewModel>();

            // Получаем количество пользователей для всех ролей
            var roleUserCounts = new Dictionary<string, int>();
            foreach (var role in roles)
            {
                var users = await _userManager.GetUsersInRoleAsync(role.Name).ConfigureAwait(false);
                roleUserCounts[role.Name] = users.Count;
            }

            foreach (var role in roles)
            {
                roleViewModels.Add(new RoleViewModel
                {
                    Id = role.Id,
                    Name = role.Name,
                    UserCount = roleUserCounts[role.Name]
                });
            }

            return View(roleViewModels);
        }

        // POST: /Admin/CreateRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            if (!string.IsNullOrWhiteSpace(roleName))
            {
                if (!await _roleManager.RoleExistsAsync(roleName).ConfigureAwait(false))
                {
                    var result = await _roleManager.CreateAsync(new IdentityRole(roleName)).ConfigureAwait(false);
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

        // POST: /Admin/DeleteRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            if (!string.IsNullOrWhiteSpace(roleName))
            {
                // Защищаем системные роли
                if (roleName == "Admin" || roleName == "User")
                {
                    TempData["ErrorMessage"] = "Нельзя удалить системные роли!";
                    return RedirectToAction(nameof(Roles));
                }

                var role = await _roleManager.FindByNameAsync(roleName).ConfigureAwait(false);
                if (role != null)
                {
                    var usersInRole = await _userManager.GetUsersInRoleAsync(roleName).ConfigureAwait(false);
                    if (usersInRole.Count == 0)
                    {
                        var result = await _roleManager.DeleteAsync(role).ConfigureAwait(false);
                        if (result.Succeeded)
                        {
                            TempData["SuccessMessage"] = "Роль успешно удалена!";
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Ошибка при удалении роли!";
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Нельзя удалить роль, которая назначена пользователям!";
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Роль не найдена!";
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
