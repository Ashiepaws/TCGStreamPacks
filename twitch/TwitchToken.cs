using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace TCGStreamPacks.Twitch;

public class TwitchToken
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }
    [JsonProperty("expires_in")]
    public int ExpiresIn { get; set; }
    [JsonProperty("refresh_token")]
    public string RefreshToken { get; set; }
    [JsonProperty("scope")]
    public List<string> Scopes { get; set; }
    [JsonProperty("token_type")]
    public string TokenType { get; set; }

    private readonly Task _refreshTask;

    private TwitchToken(string jsonString)
    {
        Plugin.Logger.LogDebug("Parsing Twitch token response...");
        JsonConvert.PopulateObject(jsonString, this);
        Plugin.Logger.LogDebug("Twitch token parsed successfully.");
        _refreshTask = Task.Run(async () =>
        {
            Plugin.Logger.LogDebug("Starting Twitch token refresh task...");
            while (true)
            {
                if (string.IsNullOrEmpty(RefreshToken))
                    throw new Exception("No refresh token available; cannot refresh Twitch token.");
                Plugin.Logger.LogDebug($"Waiting {ExpiresIn - 60} seconds before refreshing Twitch token...");
                await Task.Delay((ExpiresIn - 60) * 1000); // Refresh 1 minute before expiry
                await RefreshTokenAsync();
            }
        });

        Plugin.Logger.LogDebug($"Twitch token: {AccessToken}, expires in {ExpiresIn} seconds, scopes: {string.Join(", ", Scopes)}");
    }

    /// <summary>
    /// Refreshes the Twitch OAuth token using the refresh token.
    /// </summary>
    private async Task RefreshTokenAsync()
    {
        var response = await TwitchAPI.RefreshToken(RefreshToken);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        JsonConvert.PopulateObject(responseBody, this);
        Plugin.Logger.LogInfo("Twitch token refreshed successfully.");
    }

    /// <summary>
    /// Creates a TwitchToken by initiating the device authorization flow.
    /// </summary>
    /// <param name="clientId">Twitch Client ID</param>
    /// <param name="scopes">Space-delimited list of requested scopes</param>
    public static async Task<TwitchToken> Create()
    {
        Plugin.Logger.LogInfo("Creating Twitch token via device authorization flow...");
        DeviceAuthResponse deviceAuth = await StartDeviceAuth();
        Plugin.Logger.LogDebug($"Device Auth request successful. User Code: {deviceAuth.UserCode}");
        if (Plugin.OpenBrowserOnAuth)
            Process.Start(deviceAuth.VerificationUri);
        else
            Plugin.Logger.LogInfo($"Please open the following URL in your browser to authorize the application: {deviceAuth.VerificationUri}");

        return await PollForToken(deviceAuth);
    }

    /// <summary>
    /// Starts the device authorization flow to get a device code.
    /// </summary>
    /// <param name="clientId">Twitch Client ID</param>
    /// <param name="scopes">Space-delimited list of requested scopes</param>
    private static async Task<DeviceAuthResponse> StartDeviceAuth()
    {
        Plugin.Logger.LogDebug("Starting device authorization...");
        var response = await TwitchAPI.StartDeviceAuth();
        Plugin.Logger.LogDebug($"Device authorization response status: {response.StatusCode}");
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<DeviceAuthResponse>(responseBody);
    }

    /// <summary>
    /// Polls the Twitch OAuth token endpoint for an access token using the provided device authorization response.
    /// </summary>
    /// <param name="clientId">Twitch Client ID</param>
    /// <param name="deviceAuth">Device authorization response</param>
    private static async Task<TwitchToken> PollForToken(DeviceAuthResponse deviceAuth)
    {
        while (true)
        {
            await Task.Delay(deviceAuth.Interval * 1000);
            var response = await TwitchAPI.PollDeviceAuth(deviceAuth.DeviceCode);
            var responseBody = await response.Content.ReadAsStringAsync();
            Plugin.Logger.LogDebug($"Token polling response status: {response.StatusCode}");

            if (response.IsSuccessStatusCode) 
                return new TwitchToken(responseBody);
            else
            {
                var errorResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseBody);
                if (errorResponse.TryGetValue("message", out string error))
                {
                    if (error == "authorization_pending")
                        continue;
                    else
                        throw new Exception($"Error during token polling: {errorResponse["message"]}");
                }
                else
                    throw new Exception("Unexpected response during token polling: " + responseBody);
            }
        }
    }

    /// <summary>
    /// Response from Twitch device authorization endpoint
    /// </summary>
    internal class DeviceAuthResponse
    {
        [JsonProperty("device_code")]
        public string DeviceCode { get; set; }
        [JsonProperty("user_code")]
        public string UserCode { get; set; }
        [JsonProperty("verification_uri")]
        public string VerificationUri { get; set; }
        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }
        [JsonProperty("interval")]
        public int Interval { get; set; }
    }
}