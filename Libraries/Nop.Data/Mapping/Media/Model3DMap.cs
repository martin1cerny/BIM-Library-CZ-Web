using Nop.Core.Domain.Media;

namespace Nop.Data.Mapping.Media
{
    public partial class Model3DMap : NopEntityTypeConfiguration<Model3D>
    {
        public Model3DMap()
        {
            this.ToTable("Model3D");
            this.HasKey(p => p.Id);
            this.Property(p => p.Model3DBinary).IsMaxLength();
            this.Property(p => p.MimeType).IsRequired().HasMaxLength(40);
            this.Property(p => p.SeoFilename).HasMaxLength(300);
        }
    }
}