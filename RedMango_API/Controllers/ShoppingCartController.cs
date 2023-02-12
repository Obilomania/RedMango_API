using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedMango_API.Data;
using RedMango_API.Models;
using System.Net;

namespace RedMango_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShoppingCartController : ControllerBase
    {
        protected ApiResponse _response;
        private readonly ApplicationDbContext _context;

        public ShoppingCartController(ApplicationDbContext context)
        {
            _context = context;
            _response = new();
        }




        //ENDPOINT TO RETRIEVE SHOPPING CART
        [HttpGet]
        public async Task<ActionResult<ApiResponse>> GetShoppingCart(string userId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    _response.IsSuccess = false;
                    _response.StatusCode= HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                ShoppingCart shoppingCart= _context.ShoppingCarts
                    .Include(u => u.CartItems)
                    .ThenInclude(u => u.MenuItem)
                    .FirstOrDefault(u => u.UserId == userId);

                if (shoppingCart != null && shoppingCart.CartItems.Count > 0)
                {
                    shoppingCart.CartTotal = shoppingCart.CartItems.Sum(u => u.Quantity * u.MenuItem.Price);
                }
                _response.Result = shoppingCart;
                _response.StatusCode= HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {

                _response.IsSuccess= false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
                _response.StatusCode = HttpStatusCode.BadRequest;
            }
            return _response;
        }




        //ENDPOINT TO CREATE AND UPDATE A SHOPPING CART
        [HttpPost]
        public async Task<ActionResult<ApiResponse>> AddOrUpdateItemInCart(string userId, int menuItemId, int updateQuantityBy)
        {
            //Retrieve a shoppingCart based on a the userId
            ShoppingCart shoppingCart = _context.ShoppingCarts.Include(u => u.CartItems).FirstOrDefault(u => u.UserId == userId);
            //Retreive menuItem based on menuItemsId
            MenuItem menuItem = _context.MenuItems.FirstOrDefault(u => u.Id == menuItemId);
            if (menuItem == null)
            {
                _response.StatusCode = HttpStatusCode.BadRequest;
                _response.IsSuccess = false;
                return BadRequest(_response);
            }
            if (shoppingCart == null && updateQuantityBy > 0)
            {
                //create a shopping Cart and add cart item
                ShoppingCart newCart = new()
                {
                    UserId = userId,
                };
                _context.ShoppingCarts.Add(newCart);
                _context.SaveChanges();

                CartItem newCartItem = new()
                {
                    MenuItemId = menuItemId,
                    Quantity = updateQuantityBy,
                    ShoppingCartId = newCart.Id,
                    MenuItem = null
                };
                _context.CartItems.Add(newCartItem);
                _context.SaveChanges();
            }
            else
            {
                //If shopping cart exist
                //First check if the cart item exists
                CartItem cartItemInCart = shoppingCart.CartItems.FirstOrDefault(c => c.MenuItemId == menuItemId);
                if (cartItemInCart == null)
                {
                    //Item does not exist in current cart
                    CartItem newCartItem = new()
                    {
                        MenuItemId = menuItemId,
                        Quantity = updateQuantityBy,
                        ShoppingCartId = shoppingCart.Id,
                        MenuItem = null
                    };
                    _context.CartItems.Add(newCartItem);
                    _context.SaveChanges();
                }
                else
                {
                    //item already exist in cart and the quantity needs to be updated
                    int newQuantity = cartItemInCart.Quantity + updateQuantityBy;
                    if (updateQuantityBy == 0 || newQuantity <= 0)
                    {
                        //Remove cat item from cart and if its the only item then remove cart
                        _context.CartItems.Remove(cartItemInCart);
                        if (shoppingCart.CartItems.Count() == 1)
                        {
                            _context.ShoppingCarts.Remove(shoppingCart);
                        }
                        _context.SaveChanges();
                    }
                    else
                    {
                        cartItemInCart.Quantity = newQuantity;
                        _context.SaveChanges();
                    }
                }
            }
            return _response;
        }
    }
}
