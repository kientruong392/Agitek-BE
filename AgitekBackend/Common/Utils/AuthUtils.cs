namespace AgitekBackend.Common.Utils
{
    public static class AuthUtils
    {
        public static string HashPassword(string password)
            => BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);

        public static bool VerifyPassword(string password, string hash)
            => BCrypt.Net.BCrypt.Verify(password, hash);

        public static string GenerateUuid() => Guid.NewGuid().ToString();
    }
}
