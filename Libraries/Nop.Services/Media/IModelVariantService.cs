using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Catalog;

namespace Nop.Services.Media
{
    /// <summary>
    /// Picture service interface
    /// </summary>
    public partial interface IModelVariantService
    {

        /// <summary>
        /// Gets a modelVariant
        /// </summary>
        /// <param name="pictureId">Picture identifier</param>
        /// <returns>Picture</returns>
        ModelVariant GetModelVariantById(int modelVariantId);

        /// <summary>
        /// Gets a collection of model variants
        /// </summary>
        /// <returns>List of model variants</returns>
        IList<ModelVariant> GetModelVariants();


        /// <summary>
        /// Inserts a product model variant
        /// </summary>
        /// <param name="productAttribute">Product model variant</param>
        void InsertProductModelVariant(ProductModelVariant productModelVariant);

        /// <summary>
        /// Deletes a product model variant
        /// </summary>
        /// <param name="picture">Product model variant</param>
        void DeleteProductModelVariant(ProductModelVariant productModelVariant);

        string Test();

        /// <summary>
        /// Gets model variants by product identifier
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <returns>Model variants</returns>
        IList<ModelVariant> GetModelVariantsByProductId(int productId);

        /// <summary>
        /// Gets peoduct model variants by product identifier
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <returns>Model variants</returns>
        IList<ProductModelVariant> GetProductModelVariantsByProductId(int productId);


        /// <summary>
        /// Gets product model variant
        /// </summary>
        //// <param name="pictureId">Product model variant identifier</param>
        /// <returns>Model variants</returns>
        ProductModelVariant GetProductModelVariantById(int productModelVariantId);

    }
}
