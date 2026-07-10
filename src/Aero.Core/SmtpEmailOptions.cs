namespace Aero.Core;

/// <summary>
/// Represents a class for SmtpEmailOptions.
/// </summary>
public class SmtpEmailOptions : BaseOptions
{
        /// <summary>
    /// Initializes a new instance of the <see cref="SmtpEmailOptions"/> class.
    /// </summary>
public SmtpEmailOptions()
    {
        SectionName = "SmtpEmailOptions";
    }
        /// <summary>
    /// Gets or sets the Host.
    /// </summary>
public string Host {get; set;}
        /// <summary>
    /// Gets or sets the Port.
    /// </summary>
public int Port {get; set;}
        /// <summary>
    /// Gets or sets the Enable SSL.
    /// </summary>
public bool EnableSSL {get; set;}
        /// <summary>
    /// Gets or sets the Username.
    /// </summary>
public string Username {get; set;}
        /// <summary>
    /// Gets or sets the Password.
    /// </summary>
public string Password {get; set;}
        /// <summary>
    /// Gets or sets the Sender Email.
    /// </summary>
public string SenderEmail { get; set; }
}