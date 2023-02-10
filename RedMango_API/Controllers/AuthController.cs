using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RedMango_API.Data;
using RedMango_API.DTOs.Authentication;
using RedMango_API.Models;
using RedMango_API.Utility;
using System.Net;

namespace RedMango_API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;    
        private ApiResponse _response;
        private string secretKey;

        public AuthController(ApplicationDbContext context,IConfiguration configuration, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            secretKey = configuration.GetValue<string>("ApiSettings:Secret");
            _response = new ApiResponse();
            _userManager = userManager;
            _roleManager = roleManager;
        }


        //ENDPOINT TO REGISTER
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDTO model)
        {  
            //Access All ApplicationUser and check if it already exists
            ApplicationUser userFromDb = _context.ApplicationUsers.
                FirstOrDefault(u=>u.UserName.ToLower() == model.UserName.ToLower());

            if (userFromDb != null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess= false;
                _response.ErrorMessages.Add("Username already exists");
                return BadRequest(_response);
            }

            //Else Create a new User
            ApplicationUser newUser = new()
            {
                UserName = model.UserName,
                Email = model.UserName,
                NormalizedEmail = model.UserName.ToUpper(),
                Name = model.Name
            };
            try
            {
                var result = await _userManager.CreateAsync(newUser, model.Password);
                if (result.Succeeded)
                {
                    if (!_roleManager.RoleExistsAsync(SD.Role_Admin).GetAwaiter().GetResult())
                    {
                        //Create roles in Database
                        await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
                        await _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer));
                    }
                    //Assign role to a new User
                    if (model.Role.ToLower() == SD.Role_Admin)
                    {
                        await _userManager.AddToRoleAsync(newUser, SD.Role_Admin);
                    }
                    else
                    {
                        await _userManager.AddToRoleAsync(newUser, SD.Role_Customer);
                    }

                    _response.StatusCode = HttpStatusCode.OK;
                    _response.IsSuccess = true;
                    return Ok(_response);
                }
            }
            catch (Exception) 
            { 
            }
            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.IsSuccess = false;
            _response.ErrorMessages.Add("Error while registering");
            return BadRequest(_response);
        }



        //ENDPOINT TO LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO model)
        {
            //Access All ApplicationUser and check if it already exists
            ApplicationUser userFromDb = _context.ApplicationUsers.
                FirstOrDefault(u => u.UserName.ToLower() == model.UserName.ToLower());

            //Check Password
            bool isValid = await _userManager.CheckPasswordAsync(userFromDb, model.Password);

            if (!isValid == false)
            {
                _response.Result = new LoginResponseDTO();
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Username or password is incorrect");
                return BadRequest(_response);
            }


            //Generate JWT token
            LoginResponseDTO loginResponse = new()
            {
                Email = userFromDb.Email,
                Token = "test"
            };
            if (loginResponse.Email == null || string.IsNullOrEmpty(loginResponse.Token))
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                _response.ErrorMessages.Add("Username or password is incorrect");
                return BadRequest(_response);
            }
            _response.StatusCode = HttpStatusCode.OK;
            _response.IsSuccess = true;
            _response.Result = loginResponse;
            return Ok(_response);
        }
    }
}
