using Nop.Core.Domain.Media;

namespace Nop.Data.Mapping.Media
{
    public partial class ModelVariantMap : NopEntityTypeConfiguration<ModelVariant>
    {
        public ModelVariantMap()
        {
            this.ToTable("ModelVariant");
            this.HasKey(p => p.Id);
            this.Property(p => p.Name).HasMaxLength(40);
        }
    }
}