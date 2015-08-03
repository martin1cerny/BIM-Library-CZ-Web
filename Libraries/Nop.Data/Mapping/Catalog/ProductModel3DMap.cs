using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Catalog
{
    public partial class ProductModel3DMap : NopEntityTypeConfiguration<ProductModel3D>
    {
        public ProductModel3DMap()
        {
            this.ToTable("Product_Model3D_Mapping");
            this.HasKey(pp => pp.Id);
            
            this.HasRequired(pp => pp.Model3D)
                .WithMany(p => p.ProductModel3Ds)
                .HasForeignKey(pp => pp.Model3DId);

            this.HasRequired(pp => pp.Product)
                .WithMany(p => p.ProductModel3Ds)
                .HasForeignKey(pp => pp.ProductId);
        }
    }
}