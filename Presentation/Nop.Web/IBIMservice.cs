using Ionic.Zip;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace Nop.Web
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IBIMservice" in both code and config file together.
    [ServiceContract]
    public interface IBIMservice
    {
        [OperationContract]
        string Test();
        [OperationContract]
        IList<Product> GetProductByName(string name, bool withPictures);
        [OperationContract]
        IList<Category> GetAllCategoriesWithLevelByParentCategoryId(int parentCategoryId,
            bool showHidden, int level);
        [OperationContract]
        IList<Category> GetAllCategoriesByParentCategoryId(int parentCategoryId,
           bool showHidden);
        [OperationContract]
        Stream GetZipById(int productId, string modelVariantName);
        [OperationContract]
        IList<ModelVariant> GetModelVariantsForProduct(int productId);
        [OperationContract]
        IList<ModelVariant> GetAllModelVariants();
        [OperationContract]
        IList<Product> GetProductByNameWithModelVariant(string name, bool withPictures, string modelVariantName);
    }



}
