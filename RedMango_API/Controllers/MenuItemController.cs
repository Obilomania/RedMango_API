using Microsoft.AspNetCore.Mvc;
using RedMango_API.Data;
using RedMango_API.DTOs.MenuItem;
using RedMango_API.Models;
using RedMango_API.Services.IServices;
using RedMango_API.Utility;
using System.Net;

namespace RedMango_API.Controllers
{
    [Route("api/MenuItem")]
    [ApiController]
    public class MenuItemController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ApiResponse _response;
        private readonly IBlobservice _blobservice;

        public MenuItemController(ApplicationDbContext context, IBlobservice blobservice)
        {
            _context = context;
            _response = new ApiResponse();
            _blobservice = blobservice;
        }


        //END POINT OT GET ONE MENU ITEM
        [HttpGet]
        public async Task<IActionResult> GetMenuItems()
        {
            _response.Result = _context.MenutItems;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }


        //END POINT OT GET ONE MENU ITEM
        [HttpGet("{id:int}", Name = "GetMenuItem" )]
        public async Task<IActionResult> GetMenuItem(int id)
        {
            if (id == 0)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
            MenuItem menuItem = _context.MenutItems.FirstOrDefault(m => m.Id == id);
            if (id == 0)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }
            _response.Result = menuItem;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }


        //EnDPOINT TO CREATE A MENU ITEM
        [HttpPost]
        public async Task<ActionResult<ApiResponse>> CreateMenuItem([FromForm]MenuItemCreateDTO menuItemCreateDTO)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (menuItemCreateDTO.File == null || menuItemCreateDTO.File.Length == 0)
                    {
                        return BadRequest();
                    }
                    string filename = $"{Guid.NewGuid()}{Path.GetExtension(menuItemCreateDTO.File.FileName)}";
                    MenuItem menuItemToCreate = new()
                    {
                        Name = menuItemCreateDTO.Name,
                        Price= menuItemCreateDTO.Price,
                        Category= menuItemCreateDTO.Category,
                        SpecialTag= menuItemCreateDTO.SpecialTag,
                        Description= menuItemCreateDTO.Description,
                        Image = await _blobservice.UploadBlob(filename, SD.SD_Storage_Conttainer, menuItemCreateDTO.File)
                    };
                    _context.MenutItems.Add(menuItemToCreate);
                    _context.SaveChanges();
                    _response.Result = menuItemToCreate;
                    _response.StatusCode = HttpStatusCode.Created;
                    return CreatedAtRoute("GetMenuItem", new {id=menuItemToCreate.Id}, _response);
                }
                else
                {
                    _response.IsSuccess = false;
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }
    }
}
