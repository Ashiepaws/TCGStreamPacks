using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace TCGStreamPacks.Twitch;

#nullable enable

/// <summary>
/// Static wrapper for Twitch API calls
/// </summary>
public static class TwitchAPI
{
    private static readonly HttpClient HttpClient = new();

    public static async Task<HttpResponseMessage> StartDeviceAuth()
    {
        var formData = new Dictionary<string, string>
        {
            { "client_id", Plugin.TwitchClientID },
            { "scope", Plugin.TwitchTokenScopes }
        };
        var response = await HttpClient.PostAsync("https://id.twitch.tv/oauth2/device", new FormUrlEncodedContent(formData));
        return response;
    }

    public static async Task<HttpResponseMessage> PollDeviceAuth(string deviceCode)
    {
        var formData = new Dictionary<string, string>
        {
            { "client_id", Plugin.TwitchClientID },
            { "device_code", deviceCode },
            { "grant_type", "urn:ietf:params:oauth:grant-type:device_code" }
        };
        var response = await HttpClient.PostAsync("https://id.twitch.tv/oauth2/token", new FormUrlEncodedContent(formData));
        return response;
    }

    public static async Task<HttpResponseMessage> RefreshToken(string refreshToken)
    {
        var formData = new Dictionary<string, string>
        {
            { "client_id", Plugin.TwitchClientID },
            { "grant_type", "refresh_token" },
            { "refresh_token", refreshToken }
        };
        var response = await HttpClient.PostAsync("https://id.twitch.tv/oauth2/token", new FormUrlEncodedContent(formData));
        return response;
    }

    public static async Task<HttpResponseMessage> GetUserInfo(TwitchToken token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.twitch.tv/helix/users");
        request.Headers.Add("Client-ID", Plugin.TwitchClientID);
        request.Headers.Add("Authorization", $"Bearer {token.AccessToken}");
        var response = await HttpClient.SendAsync(request);
        return response;
    }

    public static async Task<string> GetUserId(TwitchToken token)
    {
        var response = await GetUserInfo(token);
        response.EnsureSuccessStatusCode();
        var responseBody = await response.Content.ReadAsStringAsync();
        JToken userInfo = JToken.Parse(responseBody);
        return userInfo["data"]?[0]?["id"]?.ToString() ?? throw new Exception("Failed to get user ID from Twitch API response.");
    }
    
    public static async Task<HttpResponseMessage> RegisterEventSub(TwitchToken token, object? body)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, Plugin.TwitchEventSubUrl);
        request.Headers.Add("Client-ID", Plugin.TwitchClientID);
        request.Headers.Add("Authorization", $"Bearer {token.AccessToken}");
        request.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(body), System.Text.Encoding.UTF8, "application/json");
        var response = await HttpClient.SendAsync(request);
        return response;
    }
}