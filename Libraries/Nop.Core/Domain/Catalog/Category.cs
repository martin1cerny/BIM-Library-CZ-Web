using System;
using System.Collections.Generic;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Stores;

namespace Nop.Core.Domain.Catalog
{
    /// <summary>
    /// Represents a category
    /// </summary>
    public partial class Category : BaseEntity, ILocalizedEntity, ISlugSupported, IAclSupported, IStoreMappingSupported
    {
        private ICollection<Discount> _appliedDiscounts;
        public ICollection<Category> Children;

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value of used category template identifier
        /// </summary>
        public int CategoryTemplateId { get; set; }

        /// <summary>
        /// Gets or sets the meta keywords
        /// </summary>
        public string MetaKeywords { get; set; }

        /// <summary>
        /// Gets or sets the meta description
        /// </summary>
        public string MetaDescription { get; set; }

        /// <summary>
        /// Gets or sets the meta title
        /// </summary>
        public string MetaTitle { get; set; }

        /// <summary>
        /// Gets or sets the parent category identifier
        /// </summary>
        public int ParentCategoryId { get; set; }

        /// <summary>
        /// Gets or sets the picture identifier
        /// </summary>
        public int PictureId { get; set; }

        /// <summary>
        /// Gets or sets the page size
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether customers can select the page size
        /// </summary>
        public bool AllowCustomersToSelectPageSize { get; set; }

        /// <summary>
        /// Gets or sets the available customer selectable page size options
        /// </summary>
        public string PageSizeOptions { get; set; }

        /// <summary>
        /// Gets or sets the available price ranges
        /// </summary>
        public string PriceRanges { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to show the category on home page
        /// </summary>
        public bool ShowOnHomePage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to include this category in the top menu
        /// </summary>
        public bool IncludeInTopMenu { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this category has discounts applied
        /// <remarks>The same as if we run category.AppliedDiscounts.Count > 0
        /// We use this property for performance optimization:
        /// if this property is set to false, then we do not need to load Applied Discounts navifation property
        /// </remarks>
        /// </summary>
        public bool HasDiscountsApplied { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is subject to ACL
        /// </summary>
        public bool SubjectToAcl { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is limited/restricted to certain stores
        /// </summary>
        public bool LimitedToStores { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is published
        /// </summary>
        public bool Published { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity has been deleted
        /// </summary>
        public bool Deleted { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance creation
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the date and time of instance update
        /// </summary>
        public DateTime UpdatedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets the collection of applied discounts
        /// </summary>
        public virtual ICollection<Discount> AppliedDiscounts
        {
            get { return _appliedDiscounts ?? (_appliedDiscounts = new List<Discount>()); }
            protected set { _appliedDiscounts = value; }
        }

        public void setChildren(ICollection<Category> newChildren){
            this.Children = newChildren;
        }

        public ICollection<Category> getChildren()
        {
            return Children;
        }

        public static Category clone(Category cloneObject)
        {
            Category newObject = new Category();
            newObject.AllowCustomersToSelectPageSize = cloneObject.AllowCustomersToSelectPageSize;
            //newObject.AppliedDiscounts
            newObject.CategoryTemplateId = cloneObject.CategoryTemplateId;
            newObject.CreatedOnUtc = cloneObject.CreatedOnUtc;
            newObject.Deleted = cloneObject.Deleted;
            newObject.Description = cloneObject.Description;
            newObject.DisplayOrder = cloneObject.DisplayOrder;
            newObject.HasDiscountsApplied = cloneObject.HasDiscountsApplied;
            newObject.Id = cloneObject.Id;
            newObject.IncludeInTopMenu = cloneObject.IncludeInTopMenu;
            newObject.LimitedToStores = cloneObject.LimitedToStores;
            newObject.MetaDescription = cloneObject.MetaDescription;
            newObject.MetaKeywords = cloneObject.MetaKeywords;
            newObject.MetaTitle = cloneObject.MetaTitle;
            newObject.Name = cloneObject.Name;
            newObject.PageSize = cloneObject.PageSize;
            newObject.PageSizeOptions = cloneObject.PageSizeOptions;
            newObject.ParentCategoryId = cloneObject.ParentCategoryId;
            newObject.PictureId = cloneObject.PictureId;
            newObject.PriceRanges = cloneObject.PriceRanges;
            newObject.Published = cloneObject.Published;
            newObject.ShowOnHomePage = cloneObject.ShowOnHomePage;
            newObject.SubjectToAcl = cloneObject.SubjectToAcl;
            newObject.UpdatedOnUtc = cloneObject.UpdatedOnUtc;
            return newObject;
        }
    }
}
