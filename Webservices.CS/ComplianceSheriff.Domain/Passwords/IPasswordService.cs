namespace ComplianceSheriff.Passwords
{
    public interface IPasswordService
    {
        string GenerateRandomPassword(PasswordOptions opts = null);

        HashResult GenerateHash(string valueToHash);

        string EncryptPassword(string password);
    }
}
