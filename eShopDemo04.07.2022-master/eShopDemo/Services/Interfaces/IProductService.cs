using eShopDemo.Data;
using eShopDemo.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eShopDemo.Services
{
    public interface IProductService
    {
        Task<List<Product>> GetProductsWithImages(int takeProducts, int skipProduct);
    }
}
