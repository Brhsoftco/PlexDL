﻿// <auto-generated />
//
// To parse this JSON data, add NuGet 'Newtonsoft.Json' then do:
//
//    using PlexDL.PlexAPI.Auth;
//
//    var plexPins = PlexPins.FromJson(jsonString);

using System;
using Newtonsoft.Json;
using PlexDL.PlexAPI.LoginHandler.Auth.JSON.Helpers;
using PlexDL.PlexAPI.LoginHandler.Globals;

namespace PlexDL.PlexAPI.LoginHandler.Auth.JSON
{
    public class PlexAuth
    {
        public string PinEndpointUrl => $"https://plex.tv/api/v2/pins/{Id}";
        public string LoginInterfaceUrl =>
            $"https://app.plex.tv/auth/#!?clientID={PlexDefinitions.ClientId}&context[device][version]=Plex OAuth&context[device][model]=Plex OAuth&code={Code}&context[device][product]=Plex Web";

        [JsonProperty("id")] public long Id { get; set; }

        [JsonProperty("code")] public string Code { get; set; }

        [JsonProperty("product")] public string Product { get; set; }

        [JsonProperty("trusted")] public bool Trusted { get; set; }

        [JsonProperty("clientIdentifier")] public string ClientIdentifier { get; set; }

        [JsonProperty("location")] public Location Location { get; set; }

        [JsonProperty("expiresIn")] public long ExpiresIn { get; set; }

        [JsonProperty("createdAt")] public DateTimeOffset CreatedAt { get; set; }

        [JsonProperty("expiresAt")] public DateTimeOffset ExpiresAt { get; set; }

        [JsonProperty("authToken")] public string AuthToken { get; set; }

        [JsonProperty("newRegistration")] public object NewRegistration { get; set; }

        public static PlexAuth FromJson(string json) =>
            JsonConvert.DeserializeObject<PlexAuth>(json, Converter.Settings);
    }
}