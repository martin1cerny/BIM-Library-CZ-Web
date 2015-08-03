using System.Collections.Generic;
using Nop.Core.Domain.Catalog;

namespace Nop.Core.Domain.Media
{
    /// <summary>
    /// Represents a picture
    /// </summary>
    public partial class Model3D : BaseEntity
    {
        private ICollection<ProductModel3D> _productModels;
        /// <summary>
        /// Gets or sets the picture binary
        /// </summary>
        public byte[] Model3DBinary { get; set; }

        /// <summary>
        /// Gets or sets the picture mime type
        /// </summary>
        public string MimeType { get; set; }

        /// <summary>
        /// Gets or sets the SEO friednly filename of the picture
        /// </summary>
        public string SeoFilename { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the picture is new
        /// </summary>
        public bool IsNew { get; set; }

        /// <summary>
        /// Gets or sets the product pictures
        /// </summary>
        public virtual ICollection<ProductModel3D> ProductModel3Ds
        {
            get { return _productModels ?? (_productModels = new List<ProductModel3D>()); }
            protected set { _productModels = value; }
        }

        public static Model3D clone(Model3D cloneObject)
        {
            Model3D newObject = new Model3D();
            newObject.Id = cloneObject.Id;
            newObject.IsNew = cloneObject.IsNew;
            newObject.MimeType = cloneObject.MimeType;
            newObject.Model3DBinary = cloneObject.Model3DBinary;
            //newObject.ProductModel3Ds = cloneObject.ProductModel3Ds; @JSK TODO:
            newObject.SeoFilename = cloneObject.SeoFilename;
            return newObject;
        }

    }
}
