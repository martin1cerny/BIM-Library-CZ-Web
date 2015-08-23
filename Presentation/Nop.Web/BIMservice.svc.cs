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
using System.ServiceModel.Web;
using Nop.Services.Media;
using Nop.Core.Domain.Media;

namespace Nop.Web
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "BIMservice" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select BIMservice.svc or BIMservice.svc.cs at the Solution Explorer and start debugging.
    public class BIMservice : IBIMservice
    {

        #region Fields
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IPictureService _pictureService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IModelVariantService _modelVariantService;

        #endregion

        #region Ctor
        public BIMservice()
        {
            this._productService =  EngineContext.Current.Resolve<IProductService>();
            this._categoryService = EngineContext.Current.Resolve<ICategoryService>();
            this._pictureService = EngineContext.Current.Resolve<IPictureService>();
            this._productAttributeService = EngineContext.Current.Resolve<IProductAttributeService>();
            this._modelVariantService = EngineContext.Current.Resolve<IModelVariantService>();
        }
        #endregion


        public string Test()
        {
            ModelVariant test = _modelVariantService.GetModelVariantById(1);
            IList<ModelVariant> tests = _modelVariantService.GetModelVariants();
            IList<ModelVariant> tests2 = _modelVariantService.GetModelVariantsByProductId(1);
            return _modelVariantService.Test();
        }

        public IList<Product> GetProductByName(string name, bool withPictures)
        {
            IList<Product> products = searchProducts(name);
            return cloneProducts(withPictures, products);
        }

        public IList<Product> GetProductByNameWithModelVariant(string name, bool withPictures, string modelVariantName)
        {
            //XXX: STRASNE NEEFEKTIVNE !!!! -> vytvorit v DB proceduru
            //_modelVariantService.GetModelVariantsByProductId
            IList<Product> products = searchProducts(name);
            IList<Product> productsWithVariant = new List<Product>();
            foreach (Product product in products)
            {
                IList<ModelVariant> modelVariants = _modelVariantService.GetModelVariantsByProductId(product.Id);
                foreach (ModelVariant modelVariant in modelVariants)
                {
                    if (modelVariant.Name.Equals(modelVariantName)){
                        productsWithVariant.Add(product);
                        break;
                    }
                }
            }
            return cloneProducts(withPictures, productsWithVariant);
        }


        private IList<Product> searchProducts(string name)
        {
            const int productNumber = 15;
            var vendorId = 0;
            
            IList<Product> products = (IList<Product>)_productService.SearchProducts(
                vendorId: vendorId,
                keywords: name,
                pageSize: productNumber,
                showHidden: true);
            return products;
        }


        private IList<Product> cloneProducts(bool withPictures, IList<Product> products)
        {
            IList<Product> resultProducts = new List<Product>();
            if (withPictures)
            {
                foreach (Product element in products)
                {
                    foreach (ProductPicture productPicture in element.ProductPictures)
                    {
                        productPicture.Picture.PictureBinary = _pictureService.LoadPictureBinary(productPicture.Picture);
                    }
                    resultProducts.Add(Product.clone(element));
                }
            }
            else
            {
                foreach (Product element in products)
                {
                    resultProducts.Add(Product.clone(element));
                }
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

        //public Stream GetZipOld(int productId)
        //{
        //   return File.OpenRead("C:/VUT BIM/!no_remove!/data/files/ja.jpg");
        //}

        public Stream GetZipById(int productId, string modelVariantName){
            string apPath = System.Web.Hosting.HostingEnvironment.ApplicationPhysicalPath;
            Stream stream = new MemoryStream();
            
            string path = "";
            //int filesForAtributeId = -1;
            //Vratim vsetko
            if (modelVariantName == null)
            {
                path = "C:/VUT BIM/!no_remove!/data/files/" + productId;
            }
            else
            {
                path = "C:/VUT BIM/!no_remove!/data/files/" + productId + "/forAll";
                //IList<ProductAttributeMapping> productAttributeMappings =_productAttributeService.GetProductAttributeMappingsByProductId(productId);
                //ProductAttributeMapping productAttributeMapping = productAttributeMappings.FirstOrDefault(c => c.TextPrompt.Equals("model"));
                //ProductAttributeValue productAttributeValue =  productAttributeMapping.ProductAttributeValues.FirstOrDefault(c => c.Name.Equals(variant));
                //filesForAtributeId = productAttributeValue.Id;
                //Vytvorit enum
                //foreach (ProductAttributeValues productAttributeMapping in productAttributeMapping.ProductAttributeValues)
                //{
                //    if (productAttributeMapping.TextPrompt.Equals(variant))
                //    {
                //        filesForAtributeId = productAttributeMapping.ProductAttributeId;
                //        break;
                //    }
                //}
            }
            
            using (ZipFile zip = new ZipFile("C:/VUT BIM/!no_remove!/data/"))
            {
                if (!zip.ContainsEntry("download" + "_" + productId + "/"))
                {

                    if (System.IO.Directory.Exists(path))
                    {
                        zip.AddDirectory(path, "download" + "_" + productId+"/forAll");
                    }
                    if (modelVariantName != null)
                    {
                        if (System.IO.Directory.Exists("C:/VUT BIM/!no_remove!/data/files/" + productId + "/" + modelVariantName))
                        {
                            zip.AddDirectory("C:/VUT BIM/!no_remove!/data/files/" + productId + "/" + modelVariantName, "download_" + productId + "/" + modelVariantName);
                        }
                    }
                 }
                //item.ProductName + "_" + item.ProductId + "/" + item.AttributeInfo

               
                //return zip;
                    
                zip.Save(stream); 
                stream.Position = 0L;
                return stream;
            }
        }

        public IList<ModelVariant> GetModelVariantsForProduct(int productId)
        {
            return _modelVariantService.GetModelVariantsByProductId(productId);
        }

        public IList<ModelVariant> GetAllModelVariants()
        {
            return _modelVariantService.GetModelVariants();
        }

    }
}
