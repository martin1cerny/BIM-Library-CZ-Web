using Ionic.Zip;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
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
        IList<Product> GetProductByName(string name);
        [OperationContract]
        IList<Category> GetAllCategoriesWithLevelByParentCategoryId(int parentCategoryId,
            bool showHidden, int level);
        [OperationContract]
        IList<Category> GetAllCategoriesByParentCategoryId(int parentCategoryId,
           bool showHidden);
        [OperationContract]
        Stream GetZip(int productId);
    }

}
