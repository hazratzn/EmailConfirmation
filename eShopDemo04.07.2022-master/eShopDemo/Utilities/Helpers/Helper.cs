using System;
using System.IO;

namespace eShopDemo.Utilities.Helpers
{
    public static class Helper
    {
        public static string GetFilePath(string root, string folder, string fileName)
        {
            return Path.Combine(root, folder, fileName);
        }

        public static void DeleteFile(string path)
        {
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }

        internal static string GetFilePath(object webRootPath, string v, string filename)
        {
            throw new NotImplementedException();
        }
        //int priceBelow = Int32.Parse(numbers[0]);
        //int priceAbove = Int32.Parse(numbers[1]);
        //        if (priceBelow != 0 && priceAbove == 0) products = products
        //         .Where(p => p.DiscountPrice <= priceAbove && p.DiscountPrice >= priceBelow)
        //         .ToList();

    }
}
