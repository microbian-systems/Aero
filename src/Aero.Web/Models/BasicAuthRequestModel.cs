namespace Aero.Web.Models;

public record BasicAuthRequestModel(string Id, string Password) 
    : ApiAuthRequestModel(Id), IBasicAuthRequestModel;