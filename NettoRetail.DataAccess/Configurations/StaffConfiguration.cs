using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NettoRetail.Domain.Entities;

namespace NettoRetail.DataAccess.Configurations;

    public class StaffConfiguration : IEntityTypeConfiguration<StaffEntity>
    {
        public void Configure(EntityTypeBuilder<StaffEntity> builder)
        {
        builder.ToTable("Staff");

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.Property(x => x.FirstName)
            .HasMaxLength(30);

        builder.Property(x => x.LastName)
            .HasMaxLength(30);

        builder.Property(x => x.Email)
            .HasMaxLength(256);

        builder.HasOne(x => x.Role)
            .WithMany(r => r.StaffMembers)
            .HasForeignKey(x => x.RoleId);
    }
}
