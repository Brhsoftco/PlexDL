﻿using PlexDL.Common.Globals.Providers;
using System;

namespace PlexDL.Common.Globals
{
    public static class Strings
    {
        public static string CurrentApiUri { get; set; } = "";
        public static string RepoUrl { get; } = "https://github.com/brh-media/plexdl";
        public static string CurrentSessionId { get; } = Methods.GenerateRandomNumber(Integers.SessionIdLength);

        public static string UserAppData { get; } =
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        public static string PlexDlAppData { get; } = $@"{UserAppData}\.plexdl";

        public static string PlexDlDefault { get; } = $@"{PlexDlAppData}\.default";

        public static string GetToken()
        {
            return ObjectProvider.Svr != null ? ObjectProvider.Svr.accessToken : "";
        }

        public static string GetBaseUri(bool incToken)
        {
            if (incToken)
                return "http://" + ObjectProvider.Settings.ConnectionInfo.PlexAddress + ":" +
                       ObjectProvider.Settings.ConnectionInfo.PlexPort +
                       "/?X-Plex-Token=";
            return "http://" + ObjectProvider.Settings.ConnectionInfo.PlexAddress + ":" +
                   ObjectProvider.Settings.ConnectionInfo.PlexPort + "/";
        }
    }
}