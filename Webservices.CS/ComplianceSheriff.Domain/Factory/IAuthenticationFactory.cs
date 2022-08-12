using ComplianceSheriff.Authentication;

namespace ComplianceSheriff.Factory
{
    public interface IAuthenticationFactory
    {
        IAuthenticator Create(string authenticatorName);
    }
}
