// Đây là cú pháp được custom dựa theo tài liệu (prefix + nhận diện an toàn)
using System.Security.Cryptography;
using System.Text;


/// <summary>
/// Tiện ích mã hoá/giải mã bằng AES-GCM (Authenticated Encryption).
/// - Nonce 12 bytes (chuẩn AEAD).
/// - Tag 16 bytes (128-bit, mức bảo mật tối đa của AES-GCM).
/// - Dữ liệu xuất/nhập dạng Base64 để tiện lưu DB.
/// 
/// Thiết kế "an toàn khi đọc":
/// - <see cref="DecryptFromBase64OrPassthrough"/> KHÔNG ném exception; 
///   nếu chuỗi không đúng định dạng, trả lại nguyên văn (passthrough).
/// 
/// Chú ý:
/// - <see cref="EncryptToBase64"/> hiện CHƯA gắn prefix "gcm:" vào kết quả.
///   <see cref="DecryptFromBase64OrPassthrough"/> sẽ ưu tiên nhận diện bằng prefix nếu có,
///   nếu không có prefix thì vẫn thử giải mã (best-effort).
/// </summary>
public static class AesGcmCrypto
{
    /// <summary>
    /// Prefix dùng để đánh dấu giá trị đã mã hoá theo chuẩn nội bộ.
    /// Ví dụ khi lưu DB: "gcm:&lt;base64&gt;"
    /// </summary>
    public const string Prefix = "gcm:"; // đánh dấu giá trị đã mã hoá

    /// <summary>
    /// Khóa bí mật (dạng Base64 trong biến môi trường "base64Key").
    /// Khuyến nghị độ dài 16/24/32 bytes cho AES-128/192/256.
    /// </summary>
    /// 
    private const int NonceSize = 12; // GCM khuyến nghị 12 byte
    private const int TagSize = 16; // 128-bit tag

    private static readonly byte[] Key = Convert.FromBase64String(Environment.GetEnvironmentVariable("base64Key")!);

    /// <summary>
    /// Mã hoá chuỗi UTF-8 bằng AES-GCM và trả về chuỗi Base64 (nonce|ciphertext|tag).
    /// 
    /// Lưu ý: Hàm này hiện <b>không gắn</b> prefix "gcm:" vào kết quả.
    /// Nếu team muốn nhận diện tuyệt đối (an toàn hơn),
    /// có thể chuẩn hoá việc gắn prefix khi lưu/ghi (xem NOTE bên dưới).
    /// </summary>
    /// <param name="plaintext">Chuỗi cần mã hoá. Null/empty sẽ trả về nguyên giá trị đầu vào.</param>
    /// <returns>Base64 chứa (nonce + ciphertext + tag).</returns>
    public static string EncryptToBase64(string? plaintext)
    {
        // Null/empty: giữ nguyên semantics — không mã hoá để tránh mất dữ liệu “rỗng”.
        if (string.IsNullOrEmpty(plaintext)) return plaintext!;

        // Nonce 12 bytes: kích thước khuyến nghị cho AES-GCM.
        var nonce = RandomNumberGenerator.GetBytes(12);

        // Chuẩn hoá về bytes.
        var pt = Encoding.UTF8.GetBytes(plaintext);

        // Cấp phát bộ đệm: ct bằng kích thước pt; tag 16 bytes (128-bit).
        var ct = new byte[pt.Length];
        var tag = new byte[16];

        // AesGcm với tagSize=16 (hỗ trợ 12..16, 16 là khuyến nghị).
        using var aes = new AesGcm(Key, 16);

        // Thực hiện mã hoá: đầu vào (nonce, pt) -> đầu ra (ct, tag).
        aes.Encrypt(nonce, pt, ct, tag);

        // Ghép nonce | ciphertext | tag để tiện lưu trữ và truyền đi.
        var combined = new byte[nonce.Length + ct.Length + tag.Length];
        Buffer.BlockCopy(nonce, 0, combined, 0, nonce.Length);
        Buffer.BlockCopy(ct, 0, combined, nonce.Length, ct.Length);
        Buffer.BlockCopy(tag, 0, combined, nonce.Length + ct.Length, tag.Length);

        // NOTE (tùy chọn – chuẩn hoá nhận diện):
        // Nếu muốn luôn có prefix để nhận diện an toàn tuyệt đối ở mọi nơi,
        // có thể gắn: return Prefix + Convert.ToBase64String(combined);
        // Đây là cú pháp được custom dựa theo tài liệu (quy ước nội bộ về prefix).
        return Convert.ToBase64String(combined);
    }

    /// <summary>
    /// Giải mã "an toàn không ném exception".
    /// - Nếu có prefix "gcm:" → chắc chắn là dữ liệu của chúng ta → giải mã.
    /// - Nếu không có prefix:
    ///     * Thử nhận diện base64 và giải mã (best-effort).
    ///     * Nếu thất bại, trả lại nguyên văn (coi như plaintext legacy).
    /// </summary>
    /// <param name="input">Chuỗi đầu vào (có thể là plaintext, có thể là bản mã).</param>
    /// <returns>
    /// Plaintext nếu giải mã thành công; ngược lại trả nguyên văn <paramref name="input"/>.
    /// </returns>

    public static string DecryptFromBase64OrPassthrough(string? input)
    {
        // Rỗng/white-space: trả nguyên cho tiện xử lý upstream.
        if (string.IsNullOrWhiteSpace(input)) return input ?? string.Empty;

        var stringValue = input.Trim();

        // Nhánh 1: Có prefix -> chắc chắn là dữ liệu đã mã hoá theo chuẩn.
        if (stringValue.StartsWith(Prefix, StringComparison.Ordinal))
        {
            var b64 = stringValue.Substring(Prefix.Length);
            if (string.IsNullOrWhiteSpace(b64))
                return string.Empty; // Chỉ có prefix, không có dữ liệu → trả về string empty.
            return TryDecryptCore(b64, out var plain) ? plain : input;
        }

        // Nhánh 2: Không prefix -> có thể là plaintext legacy hoặc bản mã kiểu cũ.
        // Thử decode Base64 an toàn (lọc nhanh), sau đó giải mã.
        // Điều kiện tối thiểu: 12 (nonce) + 16 (tag) = 28 bytes.
        if (LooksLikeBase64(stringValue) && TryDecryptCore(stringValue, out var plaintext))
            return plaintext;

        // Nhánh 3: Không giống bản mã → giữ nguyên (passthrough).
        return input;
    }

    /// <summary>
    /// Core giải mã từ chuỗi base64 (đã ghép nonce|ciphertext|tag).
    /// Không ném exception ra ngoài; trả về true/false để caller quyết định.
    /// </summary>
    /// <param name="base64">Chuỗi base64 chứa (nonce|ciphertext|tag).</param>
    /// <param name="plaintext">Kết quả plaintext (nếu thành công).</param>
    /// <returns>true nếu giải mã thành công; ngược lại false.</returns>
    private static bool TryDecryptCore(string base64, out string plaintext)
    {
        plaintext = string.Empty;

        // .NET có Convert.TryFromBase64String (API chính quy) — dùng để lọc nhanh
        byte[] combined;
        try
        {
            // Dùng API chính quy Convert.FromBase64String để kiểm tra/giải mã nhanh.
            combined = Convert.FromBase64String(base64);
        }
        catch
        {
            return false; // Không phải base64 hợp lệ.
        }

        // Tối thiểu phải có 28 bytes (nonce 12 + tag 16), chưa tính ciphertext.
        if (combined.Length < 28) return false;

        // Tách nonce, tag, và ciphertext theo đúng kích thước đã quy ước.
        var nonce = new byte[12];
        var tag = new byte[16];
        var ct = new byte[combined.Length - nonce.Length - tag.Length];

        Buffer.BlockCopy(combined, 0, nonce, 0, nonce.Length);
        Buffer.BlockCopy(combined, nonce.Length, ct, 0, ct.Length);
        Buffer.BlockCopy(combined, nonce.Length + ct.Length, tag, 0, tag.Length);

        try
        {
            // Thực thi giải mã: nếu sai khoá/nonce/tag → AesGcm sẽ ném lỗi.
            var pt = new byte[ct.Length];
            using var aes = new AesGcm(Key, 16);
            aes.Decrypt(nonce, ct, tag, pt);

            // Chuẩn hoá về UTF-8 text.
            plaintext = Encoding.UTF8.GetString(pt);
            return true;
        }
        catch
        {
            // Sai khoá/nonce/tag hoặc dữ liệu bị sửa đổi → coi như không hợp lệ.
            return false;
        }
    }

    /// <summary>
    /// Kiểm tra nhanh một chuỗi có “trông giống” base64 không (length % 4 == 0, ký tự trong bảng base64).
    /// Mục đích: tránh Convert.FromBase64String ném exception trong luồng chính.
    /// </summary>
    private static bool LooksLikeBase64(string s)
    {
        if (s.Length % 4 != 0) return false;
        foreach (var ch in s)
        {
            // Cho phép khoảng trắng (nếu có) vì nhiều nơi có thể thêm line-break/space khi copy.
            if (!(ch >= 'A' && ch <= 'Z') &&
                !(ch >= 'a' && ch <= 'z') &&
                !(ch >= '0' && ch <= '9') &&
                ch != '+' && ch != '/' && ch != '=' && !char.IsWhiteSpace(ch))
                return false;
        }
        return true;
    }
}
