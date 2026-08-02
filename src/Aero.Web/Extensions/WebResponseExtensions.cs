using Aero.Models;

namespace Aero.Web.Extensions;

/// <summary>
/// Represents a class for WebResponseExtensions.
/// </summary>
public static class WebResponseExtensions
{
        /// <summary>
    /// As method.
    /// </summary>
public static WebResponseModel<T> As<T>(this IWebResponseModel model)
        where T : class => (WebResponseModel<T>)model;

        /// <summary>
    /// ToWebResponseModel method.
    /// </summary>
public static Task<WebResponseModel> ToWebResponseModel(this HttpResponseMessage response)
    {
        var webResponse = new WebResponseModel
        {
            StatusCode = response.StatusCode,
            ReasonPhrase = response.ReasonPhrase,
            IsSuccessStatusCode = response.IsSuccessStatusCode,
        };

        return Task.FromResult(webResponse);
    }

        /// <summary>
    /// ToWebResponseModel method.
    /// </summary>
public static async Task<IWebResponseModel<T>> ToWebResponseModel<T>(this HttpResponseMessage response)
        where T : class
    {
        return new WebResponseModel<T>
        {
            StatusCode = response.StatusCode,
            ReasonPhrase = response.ReasonPhrase,
            IsSuccessStatusCode = response.IsSuccessStatusCode,
            Data = (await response.DeserializeContent<T>())!,
        };
    }


        /// <summary>
    /// ToWebResponseCollectionModel method.
    /// </summary>
public static async Task<IWebResponseCollectionModel<TType>> ToWebResponseCollectionModel<TType>
        (this HttpResponseMessage response)
    {
        return new WebResponseCollectionModel<TType>
        {
            StatusCode = response.StatusCode,
            ReasonPhrase = response.ReasonPhrase,
            IsSuccessStatusCode = response.IsSuccessStatusCode,
            Data = (await response.DeserializeContent<List<TType>>())!,
        };
    }

    private static async Task<T?> DeserializeContent<T>(this HttpResponseMessage response)
        where T : class
    {
        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrEmpty(json))
        {
            return default;
        }

        var model = JsonSerializer.Deserialize<T>(json);

        return model ?? null;
    }
}