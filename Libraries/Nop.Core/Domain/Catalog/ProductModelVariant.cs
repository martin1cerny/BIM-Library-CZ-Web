using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Media;

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product attribute
    /// </summary>
    public partial class ProductModelVariant : BaseEntity
    {
        /// <summary>
        /// Gets or sets the product identifier
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// Gets or sets the model variant identifier
        /// </summary>
        public int ModelVariantId { get; set; }

        /// <summary>
        /// Gets the picture
        /// </summary>
        public virtual ModelVariant ModelVariant { get; set; }

        /// <summary>
        /// Gets the product
        /// </summary>
        public virtual Product Product { get; set; }
    }
}
