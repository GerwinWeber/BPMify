﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BPMify_Client.Helpers
{
    public static class SD
    {
        public const string Local_PkceData = "PkceData";
        public const string Local_RefreshToken = "RefreshToken";

        public const string AuthState_Initialized = "Initialized";
        public const string AuthState_ReceivedCode = "ReceivedCode";
        public const string AuthState_ReceivedAccessToken = "ReceivedAccessToken";
        public const string AuthState_RefreshTokenStored = "RefreshTokenStored";

        public const string PlayerState_FirstRender = "FirstRender";
        public const string PlayerState_ReceivedCode = "ReceivedCode";
        public const string PlayerState_ReceivedToken = "ReceivedToken";
        public const string PlayerState_NoTokenAvailable = "NoTokenAvailable";
        public const string PlayerState_InitializingLocalPlayer = "InitializingLocalPlayer";


        public const string PlayerState_TryToAuthenticate = "TryToAuthenticate";
        public const string PlayerState_PlayerNotInitialized = "PlayerNotInitialized";
        public const string PlayerState_PlayerReady = "PlayerReady";
        public const string PlayerState_PlayerActive = "PlayerActive";


        public const string HttpClient_SpotifyApiClient = "ApiClient";
        public const string HttpClient_SpotifyAuthenticationClient = "AuthenticationClient";
    }
}
