using BPMify_Client.Helpers;
using BPMify_Client.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BPMify_Client.Services
{
    public class PlayerStateManager 
    {
        //private bool _localStorageHasPkceData = false;

        //public bool LocalStorageHasPkceData
        //{
        //    get { return _localStorageHasPkceData; }
        //    set 
        //    {
        //        Console.WriteLine("SetPkceData");
        //        _localStorageHasPkceData = value;
        //        UpdatePlayerState();
        //    }
        //} 

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
        private bool _localPlayerIsActive = false;

        public bool LocalPlayerIsAcitve
        {
            get { return _localPlayerIsActive; }
            set { _localPlayerIsActive = value; }
        }


        public string PlayerState { get; set; } = SD.PlayerState_FirstRender;

        public event EventHandler<NewStateEventArgs> PlayerStateHasChanged;//Call a method in component which call StateHasChangedMethod

        public string UpdatePlayerState()
        {
            if (_validTokenAvaiblable && !_localPlayerIsReady && !_localPlayerIsActive)
            {
                PlayerState = SD.PlayerState_ReceivedCode;
            }
            else if (!_validTokenAvaiblable)
            {
                PlayerState = SD.PlayerState_NoTokenAvailable;
            }
            else if (_localPlayerIsReady)
            {
                PlayerState = SD.PlayerState_PlayerReady;
            }
            //else if (_localPlayerIsActive)
            //{
            //    PlayerState = SD.PlayerState_PlayerActive;
            //}
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
