using UnityEngine;
using System.Collections;
using Photon;
using UnityEngine.UI;

// Test - Sets up networking using Photon
public class PhotonInitSetup : PunBehaviour
{
    public static string roomName = "herro114477";

    void Awake()
    {
        PhotonNetwork.logLevel = PhotonLogLevel.Full;
        PhotonNetwork.autoJoinLobby = false;
        PhotonNetwork.automaticallySyncScene = true;
    }

    void Start()
    {
        PhotonNetwork.ConnectUsingSettings("5.4");
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinOrCreateRoom(roomName, new RoomOptions() { IsVisible = false, MaxPlayers = 3 }, null);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Made a room");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined a room");
    }

    [PunRPC]
    public void TestRPC(byte param)
    {
        Debug.Log("RPC received " + param);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1) && PhotonNetwork.room != null)
        {
            byte param = (byte)Random.Range(0.0f, 255.0f);
            photonView.RPC("TestRPC", PhotonTargets.All, param);
            Debug.Log("RPC sent " + param);
        }
    }
}
