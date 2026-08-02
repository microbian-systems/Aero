namespace Aero.Core;

        /// <summary>
    /// Represents a class for JobResponse.
    /// </summary>
public class JobResponse
    {
                /// <summary>
        /// Gets or sets the Job Id.
        /// </summary>
[JsonPropertyName("job_id")]
        public string JobId { get; } = Guid.NewGuid().ToString().Replace("-", "");
                /// <summary>
        /// Gets or sets the Message.
        /// </summary>
[JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
                /// <summary>
        /// Gets or sets the Info.
        /// </summary>
[JsonPropertyName("info")]
        public List<string> Info { get; } = new();
                /// <summary>
        /// Gets or sets the Errors.
        /// </summary>
[JsonPropertyName("errors")]
        public List<string> Errors { get; } = new();
                /// <summary>
        /// Gets or sets the Warnings.
        /// </summary>
[JsonPropertyName("warnings")]
        public List<string> Warnings { get; } = new();
                /// <summary>
        /// ToString method.
        /// </summary>
public override string ToString() => this.ToString(false);
    }