using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Catalog
{
    public partial class ProductModelVariantMap : NopEntityTypeConfiguration<ProductModelVariant>
    {
        public ProductModelVariantMap()
        {
            this.ToTable("Product_ModelVariant_Mapping");
            this.HasKey(pp => pp.Id);
            
            this.HasRequired(pp => pp.ModelVariant)
                .WithMany(p => p.ProductModelVariants)
                .HasForeignKey(pp => pp.ModelVariantId);

            this.HasRequired(pp => pp.Product)
                .WithMany(p => p.ProductModelVariants)
                .HasForeignKey(pp => pp.ProductId);
        }
    }
}