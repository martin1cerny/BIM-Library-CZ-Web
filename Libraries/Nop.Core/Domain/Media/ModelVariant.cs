using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;

namespace Nop.Core.Domain.Media
{
    /// <summary>
    /// Represents a download
    /// </summary>
    public partial class ModelVariant : BaseEntity
    {
        private ICollection<ProductModelVariant> _productModelVariants;
        /// <summary>
        /// Gets a sets a model variant name
        /// </summary>
        public string Name { get; set; }


        /// <summary>
        /// Gets or sets the product variants
        /// </summary>
        public virtual ICollection<ProductModelVariant> ProductModelVariants
        {
            get { return _productModelVariants ?? (_productModelVariants = new List<ProductModelVariant>()); }
            protected set { _productModelVariants = value; }
        }
    }
}
