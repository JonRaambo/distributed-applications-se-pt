using System.Net.Http.Headers;

namespace CarService.Mvc.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _ctx;

    public ApiClient(HttpClient http, IHttpContextAccessor ctx)
    {
        _http = http;
        _ctx = ctx;
    }

    private void AttachToken()
    {
        var token = _ctx.HttpContext?.Session.GetString("jwt");
        _http.DefaultRequestHeaders.Authorization = null;
        if (!string.IsNullOrWhiteSpace(token))
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<T?> GetAsync<T>(string url)
    {
        AttachToken();
        return await _http.GetFromJsonAsync<T>(url);
    }

    public async Task<HttpResponseMessage> PostAsync<T>(string url, T body)
    {
        AttachToken();
        return await _http.PostAsJsonAsync(url, body);
    }

    public async Task<HttpResponseMessage> PutAsync<T>(string url, T body)
    {
        AttachToken();
        return await _http.PutAsJsonAsync(url, body);
    }

    public async Task<HttpResponseMessage> DeleteAsync(string url)
    {
        AttachToken();
        return await _http.DeleteAsync(url);
    }

    public async Task<HttpResponseMessage> PostMultipartAsync(string url, MultipartFormDataContent content)
    {
        AttachToken();
        return await _http.PostAsync(url, content);
    }

    public async Task<HttpResponseMessage> GetRawAsync(string url)
    {
        AttachToken();
        return await _http.GetAsync(url);
    }
}
