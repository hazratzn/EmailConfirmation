using eShopDemo.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace eShopDemo.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<List<Category>> GetCategories();
    }
}
