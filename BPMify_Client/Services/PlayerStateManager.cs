﻿using BPMify_Client.Helpers;
using BPMify_Client.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BPMify_Client.Services
{
    public class PlayerStateManager : IPlayerStateManager
    {
        private bool _localStorageHasPkceData = false;

        public bool LocalStorageHasPkceData
        {
            get { return _localStorageHasPkceData; }
            set 
            {
                Console.WriteLine("SetPkceData");
                _localStorageHasPkceData = value;
                UpdatePlayerState();
            }
        } 

        public bool UrlContainsCode { get; set; } = false;

        private bool _validTokenAvaiblable = false;

        public bool ValidTokenAvailable
        {
            get { return _validTokenAvaiblable; }
            set {
                _validTokenAvaiblable = value;
                UpdatePlayerState();
            }
        }

        private bool _localPlayerIsReady = false;

        public bool LocalPlayerIsReady
        {
            get { return _localPlayerIsReady; }
            set { 
                _localPlayerIsReady = value;
                UpdatePlayerState();
            }
        }


        public bool LocalPlayerIsAcitve { get; set; } = false;

        public string PlayerState { get; set; } = SD.PlayerState_FirstRender;

        public event EventHandler<NewStateEventArgs> PlayerStateHasChanged;//Call a method in component which call StateHasChangedMethod

        public string GetPlayerState()
        {
            return PlayerState;
        }

        public string UpdatePlayerState()
        {
            if (!_localStorageHasPkceData && UrlContainsCode && !_validTokenAvaiblable)
            {
                PlayerState = SD.PlayerState_ReceivedCode;
            }
            else if (!_localStorageHasPkceData && !UrlContainsCode && _validTokenAvaiblable)
            {
                PlayerState = SD.PlayerState_ReceivedToken;
            }
            else
            {
                PlayerState = SD.PlayerState_FirstRender;
            }
            PlayerStateHasChanged?.Invoke(this, new NewStateEventArgs(PlayerState));
            //Console.WriteLine("Update player state to: " +PlayerState);
            return "";
        }


    }

    public class NewStateEventArgs : EventArgs
    {
        private readonly string newState;

        public NewStateEventArgs(string test)
        {
            this.newState = test;
        }

        public string NewState
        {
            get { return this.newState; }
        }
    }
}
