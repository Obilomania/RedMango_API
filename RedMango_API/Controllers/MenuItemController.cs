using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedMango_API.Data;
using RedMango_API.DTOs.MenuItem;
using RedMango_API.Models;
using RedMango_API.Services.IServices;
using RedMango_API.Utility;
using System.Net;
using static System.Net.Mime.MediaTypeNames;

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
            _response.Result = _context.MenuItems;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }




        //END POINT TO GET ONE MENU ITEM
        [HttpGet("{id:int}", Name = "GetMenuItem")]
        public async Task<IActionResult> GetMenuItem(int id)
        {
            if (id == 0)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }
            MenuItem menuItem = _context.MenuItems.FirstOrDefault(m => m.Id == id);
            if (id == 0)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                _response.IsSuccess = false;
                return NotFound(_response);
            }
            _response.Result = menuItem;
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }



        //EnDPOINT TO CREATE A MENU ITEM
        [HttpPost]
        public async Task<ActionResult<ApiResponse>> CreateMenuItem([FromForm] MenuItemCreateDTO menuItemCreateDTO)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (menuItemCreateDTO.File == null || menuItemCreateDTO.File.Length == 0)
                    {
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.IsSuccess = false;
                        return BadRequest();
                    }
                    string fileName = $"{Guid.NewGuid()}{Path.GetExtension(menuItemCreateDTO.File.FileName)}";
                    MenuItem menuItemToCreate = new()
                    {
                        Name = menuItemCreateDTO.Name,
                        Price = menuItemCreateDTO.Price,
                        Category = menuItemCreateDTO.Category,
                        SpecialTag = menuItemCreateDTO.SpecialTag,
                        Description = menuItemCreateDTO.Description,
                        Image = await _blobservice.UploadBlob(fileName, SD.SD_Storage_Conttainer, menuItemCreateDTO.File)
                    };
                    _context.MenuItems.Add(menuItemToCreate);
                    _context.SaveChanges();
                    _response.Result = menuItemToCreate;
                    _response.StatusCode = HttpStatusCode.Created;
                    return CreatedAtRoute("GetMenuItem", new { id = menuItemToCreate.Id }, _response);
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



        //ENDPOINT TO UPDATE MENUITEM
        [HttpPut("{id:int}")]
        public async Task<ActionResult<ApiResponse>> UpdateMenuItem(int id, [FromForm] MenuItemUpdateDTO menuItemUpdateDTO)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (menuItemUpdateDTO == null || id != menuItemUpdateDTO.Id)
                    {
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.IsSuccess = false;
                        return BadRequest();
                    }
                    //Get Menu Item From Db
                    MenuItem menuItemFromDb = await _context.MenuItems.FindAsync(id);
                    if (menuItemFromDb == null)
                    {
                        _response.StatusCode = HttpStatusCode.BadRequest;
                        _response.IsSuccess = false;
                        return BadRequest();
                    }
                    menuItemFromDb.Name = menuItemUpdateDTO.Name;
                    menuItemFromDb.Price = menuItemUpdateDTO.Price;
                    menuItemFromDb.Description = menuItemUpdateDTO.Description;
                    menuItemFromDb.Category = menuItemUpdateDTO.Category;
                    menuItemFromDb.SpecialTag = menuItemUpdateDTO.SpecialTag;

                    //Check if Image was uploaded
                    if (menuItemUpdateDTO.File != null && menuItemUpdateDTO.File.Length > 0)
                    {
                        string fileName = $"{Guid.NewGuid()}{Path.GetExtension(menuItemUpdateDTO.File.FileName)}";
                        //if image is populated and need updating, delete image
                        await _blobservice.DeleteBlob(menuItemFromDb.Image.Split('/').Last(), SD.SD_Storage_Conttainer);
                        //After delete, then Upload
                        menuItemFromDb.Image = await _blobservice.UploadBlob(fileName, SD.SD_Storage_Conttainer, menuItemUpdateDTO.File);

                    }


                    _context.MenuItems.Update(menuItemFromDb);
                    _context.SaveChanges();
                    _response.StatusCode = HttpStatusCode.NoContent;
                    return Ok(_response);
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




        //ENDPOINT TO DELETE MENUITEM
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<ApiResponse>> DeleteMenuItem(int id)
        {
            try
            {
                if (id == 0)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    return BadRequest();
                }
                //Ge Menu Item From Db
                MenuItem menuItemFromDb = await _context.MenuItems.FindAsync(id);
                if (menuItemFromDb == null)
                {
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    return BadRequest();
                }
                await _blobservice.DeleteBlob(menuItemFromDb.Image.Split('/').Last(), SD.SD_Storage_Conttainer);
                int milliseconds = 2000;
                Thread.Sleep(milliseconds);
                _context.MenuItems.Remove(menuItemFromDb);
                _context.SaveChanges();
                _response.StatusCode = HttpStatusCode.NoContent;
                return Ok(_response);
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
