namespace Aero.Web.Models;

/// <summary>
/// Represents a record for BasicAuthRequestModel.
/// </summary>
public record BasicAuthRequestModel(string Id, string Password) 
    : ApiAuthRequestModel(Id), IBasicAuthRequestModel;