using System;
using Microsoft.AspNetCore.Mvc;
using ShopOnline.Api.Extensions;
using ShopOnline.Api.Repositories.Contracts;
using ShopOnline.Models.Dtos;

namespace ShopOnline.Api.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ShoppingCartController : Controller
	{
		private readonly IShoppingCartRepository shoppingCartRepository;
		private readonly IProductRepository productRepository;

		public ShoppingCartController(IShoppingCartRepository shoppingCartRepository, IProductRepository productRepository)
		{
			this.shoppingCartRepository = shoppingCartRepository;
			this.productRepository = productRepository;
		}

		[HttpGet("{userId}/GetItems")]
		public async Task<ActionResult<IEnumerable<CartItemDto>>> GetItems(int userId)
        {
            try
            {
				var cartItems = await shoppingCartRepository.GetItems(userId);

                if (cartItems == null)
                {
					return NoContent();
                }

				var products = await productRepository.GetItems();

				if(products == null)
                {
					throw new Exception("No product exist in the system");
                }

				return Ok(cartItems.ConvertToDto(products));
            }
            catch (Exception ex)
            {
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

		[HttpGet("{id:int}")]
		public async Task<ActionResult<CartItemDto>> GetItem(int id)
        {
            try
            {
				var cartItem = await shoppingCartRepository.GetItem(id);

				if (cartItem == null)
                {
					return NotFound();
                }

				var product = await productRepository.GetItem(cartItem.ProductId);

				if(product == null)
                {
					return NotFound();
                }

				return Ok(cartItem.ConvertToDto(product));
            }
            catch (Exception ex)
            {
				return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

		[HttpPost]
		public async Task<ActionResult<CartItemDto>> PostItem([FromBody] CartItemToAddDto cartItemToAddDto)
        {
            try
            {
                var newCartItem = await shoppingCartRepository.AddItem(cartItemToAddDto);

                if(newCartItem == null)
                {
                    return NoContent();
                }

                var product = await productRepository.GetItem(newCartItem.ProductId);

                if(product == null)
                {
                    throw new Exception($"Something went wrong when attempting to retrieve product (product: {cartItemToAddDto.ProductId})");
                }

                var newCartItemDto = newCartItem.ConvertToDto(product);

                return CreatedAtAction(nameof(PostItem), new { id = newCartItemDto.Id }, newCartItemDto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult<CartItemDto>> DeleteItem(int id)
        {
            try
            {
                var cartItem = await shoppingCartRepository.DeleteItem(id);

                if (cartItem == null)
                {
                    return NotFound();
                }

                var product = await productRepository.GetItem(id);

                if (product == null)
                {
                    return NotFound();
                }

                return Ok(cartItem.ConvertToDto(product));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPatch("{id:int}")]
        public async Task<ActionResult<CartItemDto>> UpdateQty(int id, CartItemQtyUpdateDto cartItemQtyUpdateDto)
        {
            try
            {
                var cartItem = await shoppingCartRepository.UpdateQty(id, cartItemQtyUpdateDto);

                if(cartItem == null)
                {
                    return NotFound();
                }

                var product = await productRepository.GetItem(cartItem.ProductId);

                return Ok(cartItem.ConvertToDto(product));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
	}
}

