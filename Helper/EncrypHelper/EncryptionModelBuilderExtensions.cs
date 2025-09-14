// Dùng API chính quy EF Core: ModelBuilder, Property, HasConversion, ValueConverter
using Dashboard.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Reflection;

public static class EncryptionModelBuilderExtensions
{
    public static void UseStringEncryptionConverters(this ModelBuilder modelBuilder)
    {
        // Ghi: luôn mã hoá + thêm prefix "gcm:" để đánh dấu
        // Đọc: thử giải mã; nếu không phải bản mã hợp lệ -> trả nguyên giá trị cũ
        var stringEncryptionConverter = new ValueConverter<string?, string?>(
            v => v == null ? null : AesGcmCrypto.Prefix + AesGcmCrypto.EncryptToBase64(v),
            v => v == null ? null : AesGcmCrypto.DecryptFromBase64OrPassthrough(v)
        );

        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var prop in entity.ClrType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.PropertyType == typeof(string) &&
                    prop.GetCustomAttribute<EncryptedAttribute>() != null)
                {
                    var efProp = modelBuilder.Entity(entity.ClrType).Property(prop.Name);
                    efProp.HasConversion(stringEncryptionConverter);
                    // Khuyến nghị: đặt cột NVARCHAR/TEXT tùy DB. Với PostgreSQL: TEXT là ổn.
                }
            }
        }
    }
}
