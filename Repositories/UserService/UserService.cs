using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dashboard.Common;
using Microsoft.AspNetCore.Identity;
using P2P.DbContextFolder;
using P2P.Helper.Contanst;
using P2P.Model.User;
using P2P.Repositories.UserService;
using P2P.ViewModel.User;

namespace P2P.Controllers.UserService
{
    public class UserService : IUserService
    {
        private readonly AdminDbContext _context;

        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly RoleManager<ApplicationRole> _roleManager;

        private readonly ILogger _logger;

        private readonly string? _currentUserName;

        public UserService(AdminDbContext context, IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationRole> roleManager,
            ILogger<UserService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _logger = logger;
            _currentUserName = _httpContextAccessor.HttpContext?.User?.Identity?.Name;

        }
        #region CRUD
        //Create user
        public async Task<DataResult<bool>> CreateUserAsync(CreateUserDTO createUserDTO)
        {
            var result = new DataResult<bool>();
            try
            {
                var existingUser = await _userManager.FindByNameAsync(createUserDTO.EmpId);
                if (existingUser != null)
                {
                    result.Data = false;
                    result.Message = "Tên đăng nhập đã tồn tại";
                    return result;
                }

                var user = new ApplicationUser
                {
                    UserName = createUserDTO.EmpId,
                    EmpId = createUserDTO.EmpId,
                    FullName = createUserDTO.FullName,
                    Address = createUserDTO.Address,
                    Email = createUserDTO.Email,
                    LoginEnabled = createUserDTO.LoginEnabled,
                    CreatedAt = DateTime.Now,
                    CreateBy = _currentUserName,
                    UpdatedAt = DateTime.Now,
                    UpdateBy = _currentUserName
                };

                var identityResult = await _userManager.CreateAsync(user, createUserDTO.Password ?? UserContants.DefaultPassword);

                if (identityResult.Succeeded)
                {
                    result.Data = true;
                    result.Message = "Tạo người dùng thành công";
                }
                else
                {
                    result.Data = false;
                    result.Message = string.Join("; ", identityResult.Errors.Select(e => e.Description));
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Có lỗi khi tạo người dùng: {Message}", ex.Message);
                result.Data = false;
            }
            return result;

        }
        //Update user

        //Delete user

        //Get user by id

        #endregion
    }
}