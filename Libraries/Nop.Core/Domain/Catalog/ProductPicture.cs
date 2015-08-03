
using Nop.Core.Domain.Media;
using System;

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product picture mapping
    /// </summary>
    [Serializable]
    public partial class ProductPicture : BaseEntity
    {
        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the picture identifier
        /// </summary>
        public int PictureId { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }
        
        /// <summary>
        /// Gets the picture
        /// </summary>
        public virtual Picture Picture { get; set; }

        /// <summary>
        /// Gets the product
        /// </summary>
        public virtual Product Product { get; set; }


        public static ProductPicture clone(ProductPicture cloneObject)
        {
            ProductPicture newObject = new ProductPicture();
            newObject.DisplayOrder = cloneObject.DisplayOrder;
            newObject.Id = cloneObject.Id;
            newObject.Picture = Picture.clone(cloneObject.Picture);
            newObject.ProductId = cloneObject.ProductId;
            //newObject.Product = cloneObject.Product; //todo 
            newObject.PictureId = cloneObject.PictureId;
            return newObject;
        }
    }



}
