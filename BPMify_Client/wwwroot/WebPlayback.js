﻿var player;//declare as global variable
var indexComponentRef; //needs to be stored to call a non-static DotNet-Method
var playlistsComponentRef;

function SetIndexComponentRef(objRef) {
    indexComponentRef = objRef; //passes the BlazorComponent as object reference
}

function SetPlaylistsComponentRef(objRef) {
    playlistsComponentRef = objRef; //passes the BlazorComponent as object reference
}

function RefreshToken(token) {
    player.getOAuthToken = token;
}


function InitializePlayer(token) {
    player = new Spotify.Player({
        name: 'BPMify',
        getOAuthToken: cb => { cb(token); }
    });

    // Error handling
    player.addListener('initialization_error', ({ message }) => { console.error(message); });
    player.addListener('authentication_error', ({ message }) => { console.error(message); });
    player.addListener('account_error', ({ message }) => { console.error(message); });
    player.addListener('playback_error', ({ message }) => { console.error(message); });

    // Playback status updates
    player.addListener('player_state_changed', state => { console.log(state + 'SDK.js player state changed'); });

    //Ready
    player.addListener('ready',
        ({ device_id }) => {
            console.log('SDK.js The Web Playback SDK is ready to play music with Device ID: ', device_id);
            //After player is ready, this C# method for taking playback control can be called.

            //DotNet.invokeMethodAsync('BPMify_Client', 'SetDeviceId'); //The function GetPlaybackControlAsyncInvokeable needs to be a static funtion.Otherwise it is not found
            
            indexComponentRef.invokeMethodAsync('TransferPlayback', device_id);

            //DotNet.invokeMethod('BPMify_Client', 'GetPlaybackControlAsyncInvokeable', device_id);
        }
    )

    // Not Ready
    player.addListener('not_ready', ({ device_id }) => {
        console.log('SDK.js Device ID has gone offline', device_id);
    })
 
    // Connect to the player!
    player.connect().then(success => {
        if (success) {
            console.log('The Web Playback SDK successfully connected to Spotify!');
            //Resume();//Resume playing before WEB Playback SDK took contol over audio ouput
        }
    })
}

window.onSpotifyWebPlaybackSDKReady = () => { }; //Muss Definiert sein, weil sonst eine Fehlermeldung ausgegeben wird

async function GetMetadataAsync() {
    return JSON.stringify(await player.getCurrentState());//liefert ein Promise 
    //https://developer.spotify.com/documentation/web-playback-sdk/reference/#api-spotify-player-getcurrentstate
}

function GetCurrentState() {
    player.getCurrentState().then(state => {
        if (!state) {
            console.error('User is not playing music through the Web Playback SDK');
            return;
        }

        let {
            current_track,
            next_tracks: [next_track]
        } = state.track_window;

        console.log('Currently Playing', current_track);
        console.log('Playing Next', next_track);
    });
}


/*
 https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Promise
 An action can be assigned to an already "settled" promise.
 In that case the action (if appropriate) will be performed at the first asynchronous opportunity.
 Note that promises are guaranteed to be asynchronous.
 Therefore, an action for an already "settled" promise will occur only after the stack has cleared and a clock-tick has passed.
 The effect is much like that of setTimeout(action,10).


const promiseA = new Promise( (resolutionFunc,rejectionFunc) => {
    resolutionFunc(777);
});
// At this point, "promiseA" is already settled.
promiseA.then( (val) => console.log("asynchronous logging has val:",val) );
console.log("immediate logging");

// produces output in this order:
// immediate logging
// asynchronous logging has val: 777

 */

function Resume() {
    //Resume playing music
    player.resume().then(() => {
        console.log('SDK.js Resumed!');
    });
}

function Pause() {
    //Pause Playing music
    player.pause().then(() => {
        console.log('Paused!');
    });
}

function SkipToNextTrack() {
    player.nextTrack().then(() => {
        console.log('SDK.js Skipped to next track!');
    });
}

