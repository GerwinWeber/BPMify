using BPMify_Client.Helpers;
using BPMify_Client.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BPMify_Client.Services
{
    public class PlayerStateManager : IPlayerStateManager
    {
        private bool _localStorageHasPkceData;

        public bool LocalStorageHasPkceData
        {
            get { return _localStorageHasPkceData; }
            set 
            {
                _localStorageHasPkceData = value;
                UpdatePlayerState();
                //PlayerStateHasChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool UrlContainsCode { get; set; }
        public bool ValidTokenAvailable { get; set; }
        public bool LocalPlayerIsReady { get; set; }
        public bool LocalPlayerIsAcitve { get; set; }

        public event EventHandler<NewStateEventArgs> PlayerStateHasChanged;//Call a method in component which call StateHasChangedMethod

        public string UpdatePlayerState()
        {
            NewStateEventArgs newState;
            if (!LocalStorageHasPkceData && UrlContainsCode && !ValidTokenAvailable)
            {
                newState = new NewStateEventArgs(SD.PlayerState_ReceivedCode);
            }
            else if (!LocalStorageHasPkceData && !UrlContainsCode && ValidTokenAvailable)
            {
                newState = new NewStateEventArgs(SD.PlayerState_ReceivedToken);
            }
            else
            {
                newState = new NewStateEventArgs(SD.PlayerState_FirstReneder);
            }
            PlayerStateHasChanged?.Invoke(this, newState);
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
