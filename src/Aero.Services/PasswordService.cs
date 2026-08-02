using PasswordGenerator;

namespace Aero.Services;

/// <summary>
/// Defines an interface for IPasswordService.
/// </summary>
public interface IPasswordService
{
        /// <summary>
    /// GeneratePassword method.
    /// </summary>
string GeneratePassword(int length = 12);
        /// <summary>
    /// GenerateOneTimePass method.
    /// </summary>
string GenerateOneTimePass(int length = 5);
}

/// <summary>
/// Represents a class for PasswordService.
/// </summary>
public class PasswordService : IPasswordService
{
        /// <summary>
    /// GeneratePassword method.
    /// </summary>
public string GeneratePassword(int length = 12)
    {
        var password = new Password(length)
            .IncludeNumeric()
            .IncludeLowercase()
            .IncludeUppercase()
            .IncludeSpecial()
            .LengthRequired(length);

        return password.Next();
    }

        /// <summary>
    /// GenerateOneTimePass method.
    /// </summary>
public string GenerateOneTimePass(int length = 5)
    {
        var password = new Password(length).IncludeNumeric();
        return password.Next();
    }
}