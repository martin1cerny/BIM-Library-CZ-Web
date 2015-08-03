using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.Text;
using Ionic.Zip;
using System.Web;

namespace Nop.Web
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "BIMservice" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select BIMservice.svc or BIMservice.svc.cs at the Solution Explorer and start debugging.
    public class BIMservice : IBIMservice
    {

        #region Fields
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        #endregion

        #region Ctor
        public BIMservice()
        {
            this._productService =  EngineContext.Current.Resolve<IProductService>();
            this._categoryService = EngineContext.Current.Resolve<ICategoryService>();
        }
        #endregion


        public string Test()
        {
            return "It's work";
        }

        public IList<Product> GetProductByName(string name)
        {
            const int productNumber = 15;
            var vendorId = 0;

            IList<Product> products = (IList<Product>)_productService.SearchProducts(
                vendorId: vendorId,
                keywords: name,
                pageSize: productNumber,
                showHidden: true);
            IList<Product> resultProducts = new List<Product>();
            foreach (Product element in products)
            {
                resultProducts.Add(Product.clone(element));
            }
            return resultProducts;
        }

        public IList<Category> GetAllCategoriesWithLevelByParentCategoryId(int parentCategoryId,
            bool showHidden, int level)
        {
            IList<Category> categories = _categoryService.GetAllCategoriesByParentCategoryId(parentCategoryId,
            showHidden);
            IList<Category> resultCategories = new List<Category>();
            foreach (Category element in categories)
            {
                Category clone = Category.clone(element);
                if (level > 0)
                {
                    level = level - 1;
                    clone.setChildren(GetAllCategoriesWithLevelByParentCategoryId(clone.Id, showHidden, level));
                }
                resultCategories.Add(clone);
            }
            return resultCategories;
        }

        public IList<Category> GetAllCategoriesByParentCategoryId(int parentCategoryId,
           bool showHidden)
        {
            IList<Category> categories = _categoryService.GetAllCategoriesByParentCategoryId(parentCategoryId,
            showHidden);
            IList<Category> resultCategories = new List<Category>();
            foreach (Category element in categories)
            {
               Category clone = Category.clone(element);
               clone.setChildren(GetAllCategoriesByParentCategoryId(clone.Id, showHidden));
               resultCategories.Add(clone);
            }
            return resultCategories;
        }

        public Stream GetZip(int productId){
            HttpContext context = System.Web.HttpContext.Current;
            context.Response.AddHeader("Content-Disposition", "attachment; filename=" + productId + ".zip");
            context.Response.ContentType = "application/zip";
            string apPath = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath;
            using (ZipFile zip = new ZipFile("C:/VUT BIM/!no_remove!/data/"))
            {
                    if (!zip.ContainsEntry("download" + "_" + productId + "/"))
                    {
                        if (System.IO.Directory.Exists("C:/VUT BIM/!no_remove!/data/files/" + productId + "/forAll"))
                        {
                            zip.AddDirectory("C:/VUT BIM/!no_remove!/data/files/" + productId + "/forAll", "download" + "_" + productId);
                        }
                    }
                    /*if (item.FilesForAtributeId != -1)
                    {
                        if (System.IO.Directory.Exists("C:/VUT BIM/!no_remove!/data/files/" + item.ProductId + "/" + item.FilesForAtributeId))
                        {
                            zip.AddDirectory("C:/VUT BIM/!no_remove!/data/files/" + item.ProductId + "/" + item.FilesForAtributeId, item.ProductName + "_" + item.ProductId + "/" + item.AttributeInfo);
                        }
                    }*/
                //return zip;
                zip.Save(context.Response.OutputStream);
                return context.Response.OutputStream;
            }
        }

    }
}
