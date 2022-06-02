using System;
using Microsoft.EntityFrameworkCore;
using ShopOnline.Api.Data;
using ShopOnline.Api.Entities;
using ShopOnline.Api.Repositories.Contracts;
using ShopOnline.Models.Dtos;

namespace ShopOnline.Api.Repositories
{
	public class ShoppingCartRepository : IShoppingCartRepository
	{
        private readonly ShopOnlineDbContext dbContext;

        public ShoppingCartRepository(ShopOnlineDbContext dbContext)
		{
            this.dbContext = dbContext;
        }

        private async Task<bool> CartItemExists(int cartId, int productId)
        {
            return await dbContext.CartItems.AnyAsync(c => c.CartId == cartId && c.ProductId == productId);
        }

        public async Task<CartItem> AddItem(CartItemToAddDto cartItemToAddDto)
        {
            if (await CartItemExists(cartItemToAddDto.CartId, cartItemToAddDto.ProductId))
            {
                return null;
            }

            var item = await (from product in dbContext.Products
                        where product.Id == cartItemToAddDto.ProductId
                        select new CartItem
                        {
                            CartId = cartItemToAddDto.CartId,
                            ProductId = product.Id,
                            Qty = cartItemToAddDto.Qty
                        }).SingleOrDefaultAsync();

            if(item == null)
            {
                return null;
            }

            var result = await dbContext.CartItems.AddAsync(item);
            await dbContext.SaveChangesAsync();
            return result.Entity;
        }

        public async Task<CartItem> DeleteItem(int id)
        {
            var item = await dbContext.CartItems.FindAsync(id);

            if (item != null)
            {
                dbContext.Remove(item);
                await dbContext.SaveChangesAsync();
            }

            return item;
        }

        public async Task<CartItem> GetItem(int id)
        {
            return await (from cart in dbContext.Carts
                         join cartItem in dbContext.CartItems
                         on cart.Id equals cartItem.CartId
                         where cartItem.Id == id
                         select new CartItem
                         {
                             Id = cartItem.Id,
                             ProductId = cartItem.ProductId,
                             Qty = cartItem.Qty,
                             CartId = cartItem.CartId
                         }).SingleAsync();
        }

        public async Task<IEnumerable<CartItem>> GetItems(int userId)
        {
            return await (from cart in dbContext.Carts
                          join cartItem in dbContext.CartItems
                          on cart.Id equals cartItem.CartId
                          where cart.UserId == userId
                          select new CartItem
                          {
                              Id = cartItem.Id,
                              ProductId = cartItem.ProductId,
                              Qty = cartItem.Qty,
                              CartId = cartItem.CartId
                          }).ToListAsync();
        }

        public async Task<CartItem> UpdateQty(int id, CartItemQtyUpdateDto cartItemQtyUpdateDto)
        {
            var item = await dbContext.CartItems.FindAsync(id);

            if(item == null)
            {
                return default(CartItem);
            }

            item.Qty = cartItemQtyUpdateDto.Qty;

            await dbContext.SaveChangesAsync();
            return item;
        }
    }
}

