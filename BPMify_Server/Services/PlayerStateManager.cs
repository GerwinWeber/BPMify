using BPMify_Server.Helpers;
using System;

namespace BPMify_Server.Services
{
    public class PlayerStateManager 
    {
        private bool _tryToAuthenticate = false;

        public bool TryToAuthenticate
        {
            get { return _tryToAuthenticate; }
            set
            {
                _tryToAuthenticate = value;
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
        //private bool _localPlayerIsActive = false;

        //public bool LocalPlayerIsAcitve
        //{
        //    get { return _localPlayerIsActive; }
        //    set { _localPlayerIsActive = value; }
        //}


        public string PlayerState { get; set; } = SD.PlayerState_FirstRender;

        public event EventHandler<NewStateEventArgs> PlayerStateHasChanged;//Call a method in component which call StateHasChangedMethod

        public string UpdatePlayerState()
        {
            if (_validTokenAvaiblable && !_localPlayerIsReady)
            {
                PlayerState = SD.PlayerState_ReceivedCode;
            }
            else if (_tryToAuthenticate && !_validTokenAvaiblable && !_localPlayerIsReady)
            {
                PlayerState = SD.PlayerState_NoTokenAvailable;
            }
            else if (!_tryToAuthenticate && !_validTokenAvaiblable && !_localPlayerIsReady)
            {
                PlayerState = SD.PlayerState_AuthenticatioFailed;
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
            Console.WriteLine("Update player state to: " +PlayerState);
            return "";
        }

        public void UpdateDom()
        {
            PlayerStateHasChanged?.Invoke(this, new NewStateEventArgs(""));
        }


    }

    public class NewStateEventArgs : EventArgs
    {
        private readonly string _newState;

        public NewStateEventArgs(string newState)
        {
            this._newState = newState;
        }

        public string NewState
        {
            get { return this._newState; }
        }
    }
}
