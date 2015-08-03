using System.Collections.Generic;
using Nop.Core.Domain.Localization;
using System;

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a product tag
    /// </summary>
    [Serializable]
    public partial class ProductTag : BaseEntity, ILocalizedEntity
    {
        private ICollection<Product> _products;

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the products
        /// </summary>
        public virtual ICollection<Product> Products
        {
            get { return _products ?? (_products = new List<Product>()); }
            protected set { _products = value; }
        }
    }
}
