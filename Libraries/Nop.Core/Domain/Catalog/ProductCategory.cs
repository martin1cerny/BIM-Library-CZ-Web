using System;
namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product category mapping
    /// </summary>
    [Serializable]
    public partial class ProductCategory : BaseEntity
    {
        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the category identifier
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the product is featured
        /// </summary>
        public bool IsFeaturedProduct { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }
        
        /// <summary>
        /// Gets the category
        /// </summary>
        public virtual Category Category { get; set; }

        /// <summary>
        /// Gets the product
        /// </summary>
        public virtual Product Product { get; set; }

        public static ProductCategory clone(ProductCategory cloneObject)
        {
            ProductCategory newObject = new ProductCategory();
            newObject.Category = Category.clone(cloneObject.Category);
            newObject.CategoryId = cloneObject.CategoryId;
            newObject.DisplayOrder = cloneObject.DisplayOrder;
            newObject.Id = cloneObject.Id;
            newObject.IsFeaturedProduct = cloneObject.IsFeaturedProduct;
            //newObject.Product
            newObject.ProductId = cloneObject.ProductId;
            return newObject;
        }


    }

}
