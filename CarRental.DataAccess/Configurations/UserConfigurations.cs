using CarRental.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRental.DataAccess.Configurations
{
    // DataAccess/Configurations/UserConfiguration.cs
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(u => u.Id);

            builder.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            builder.Property(u => u.LastName).HasMaxLength(100).IsRequired();
            builder.Property(u => u.Email).HasMaxLength(200).IsRequired();
            builder.Property(u => u.PasswordHash).IsRequired();
            builder.Property(u => u.PhoneNumber).HasMaxLength(20);
            builder.Property(u => u.Role).HasConversion<string>();

            builder.HasIndex(u => u.Email).IsUnique();

            builder.HasMany(u => u.Cars)
                   .WithOne(c => c.Owner)
                   .HasForeignKey(c => c.OwnerId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(u => u.Bookings)
                   .WithOne(b => b.Renter)
                   .HasForeignKey(b => b.RenterId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(u => u.IsActive);
        }
    }
}
