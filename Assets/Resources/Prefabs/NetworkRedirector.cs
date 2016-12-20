using UnityEngine;
using System.Collections.Generic;
using System;

// Instrument controllers should implement this, where T is some way to identify notes
public interface NetworkSupport<T,P>
{
    void SetParameters(P parameters);
    // This will play a note
    void NoteEvent(bool isDown, T note, bool isSpectate);
    // This will assign a Photon user ID to the instrument, before this is assigned, the instrument
    // should be unresponsive. It will respond to local events only if the user ID is the local user.
    // Otherwise, it will respond only to network events.
    void SetupNetwork(int ownerID);
}

// Instrument controllers should have this as a class object
public class NetworkRedirector<T, P>
{
    NetworkSupport<T,P> controller; // A reference to the parent controller
    int ownerID; // The networked Photon player ID which owns this instrument

    public bool isLocal { get; private set; }
    public int? specID = null; // Who to spectate

    // This makes a local instrument
    public NetworkRedirector(NetworkSupport<T,P> controller) 
        : this(controller, PhotonNetwork.player.ID)
    {
    } 

    // If ownerID is the local player's ID, then this will be a local instrument, otherwise it is a network instrument
    public NetworkRedirector(NetworkSupport<T,P> controller, int ownerID)
    {
        this.ownerID = ownerID;
        // If not local, receive network notes
        isLocal = ownerID == PhotonNetwork.player.ID;
        PhotonNetwork.OnEventCall += NetworkEventReceive;
        this.controller = controller;
    }

    // This only gets called over the network if this is a network instrument
    void NetworkEventReceive(byte eventCode, object content, int senderID)
    {
        // See if the message is intended for this instrument
        if (senderID != ownerID) // Not us
        {
            if (specID == null || specID != senderID) return; // Not spectating this person either
            // Is spectating
            T noteID = (T)content;
            controller.NoteEvent(eventCode == 1, noteID, true);
        }
        else if (!isLocal) // Don't want to send ourselves messages
        {
            if (eventCode == 2) // If is a change of instrument params
            {
                controller.SetParameters((P)content);
            }
            else // Otherwise, playing a note
            {
                // Get the note ID and tell the controller to play that note
                T noteID = (T)content;
                controller.NoteEvent(eventCode == 1, noteID, false);
            }
        }
    }

    // This should only be called if it is a local instrument
    public void Play(bool isKeyDown, T note)
    {
        if (ownerID != PhotonNetwork.player.ID) return;
        controller.NoteEvent(isKeyDown, note, false); // Get the controller to play the note
        PhotonNetwork.RaiseEvent((byte)(isKeyDown ? 1 : 0), note, true, null); // Then broadcast it elsewhere
    }

    // This should only be called if it is a local instrument
    public void SetParameters(P parameters)
    {
        if (ownerID != PhotonNetwork.player.ID) return;
        controller.SetParameters(parameters);
        PhotonNetwork.RaiseEvent(2, parameters, true, null);
    }
}
