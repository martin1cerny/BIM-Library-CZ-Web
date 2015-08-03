
using Nop.Core.Domain.Media;
using System;

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product picture mapping
    /// </summary>
    [Serializable]
    public partial class ProductModel3D : BaseEntity
    {
        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the picture identifier
        /// </summary>
        public int Model3DId { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }
        
        /// <summary>
        /// Gets the picture
        /// </summary>
        public virtual Model3D Model3D { get; set; }

        /// <summary>
        /// Gets the product
        /// </summary>
        public virtual Product Product { get; set; }


        public static ProductModel3D clone(ProductModel3D cloneObject)
        {
            ProductModel3D newObject = new ProductModel3D();
            newObject.DisplayOrder = cloneObject.DisplayOrder;
            newObject.Id = cloneObject.Id;
            newObject.Model3D = Model3D.clone(cloneObject.Model3D);
            newObject.ProductId = cloneObject.ProductId;
            //newObject.Product = cloneObject.Product; //todo 
            newObject.Model3DId = cloneObject.Model3DId;
            return newObject;
        }
    }

}
