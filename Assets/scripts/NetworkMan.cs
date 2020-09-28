using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net.Sockets;
using System.Net;

public class NetworkMan : MonoBehaviour
{
    public GameObject playerObj;
    UdpClient udp;
    
    bool spawnNeeded;
    void Start()
    {
        spawnNeeded = true;
        udp = new UdpClient();
        udp.Connect("3.130.163.55", 12345);
        //udp.Connect("192.168.1.101", 12345);
        
        Byte[] sendBytes = Encoding.ASCII.GetBytes("connect");

        udp.Send(sendBytes, sendBytes.Length);

        udp.BeginReceive(new AsyncCallback(OnReceived), udp);

        InvokeRepeating("HeartBeat", 1, 1);
    }

    void OnDestroy()
    {
        udp.Dispose();
    }
    
    public enum commands{
        NEW_CLIENT,
        UPDATE
    };
   
    [Serializable]
    public class Message
    {
        public commands cmd;
    }
    
    [Serializable]
    public class Player
    {
        [Serializable]
        public struct receivedColor{
            public float R;
            public float G;
            public float B;
        }
        public string id;
        public receivedColor color;

    }

    [Serializable]
    public class GameState
    {
        public List<Player> players;
    }

    public Message latestMessage;
    public GameState latestGameState;
    
    void OnReceived(IAsyncResult result)
    {
        // this is what had been passed into BeginReceive as the second parameter:
        UdpClient socket = result.AsyncState as UdpClient;
        // points towards whoever had sent the message:
        IPEndPoint source = new IPEndPoint(0, 0);
        // get the actual message and fill out the source:
        byte[] serverMsg = socket.EndReceive(result, ref source);
        // do what you'd like with `message` here:
        string serverData = Encoding.ASCII.GetString(serverMsg);
        Debug.Log("Got this: " + serverData);
        latestMessage = JsonUtility.FromJson<Message>(serverData);
        try
        {
            switch(latestMessage.cmd)
            {
                case commands.NEW_CLIENT:
                    latestGameState.players = JsonUtility.FromJson<List<Player>>(serverData);
                    break;
                case commands.UPDATE:
                    latestGameState = JsonUtility.FromJson<GameState>(serverData);
                    break;
                default:
                    Debug.Log("Error");
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
        // schedule the next receive operation once reading is done:
        socket.BeginReceive(new AsyncCallback(OnReceived), socket);
    }

    void SpawnPlayers()
    {
        if (spawnNeeded)
        {
            Player newP = new Player();
            latestGameState.players.Add(newP);
            Instantiate(playerObj);
        }
        spawnNeeded = false;
    }
    
    void UpdatePlayers()
    {
        
    }

    void DestroyPlayers()
    {

    }
    
    void HeartBeat()
    {
        Byte[] sendBytes = Encoding.ASCII.GetBytes("heartbeat");
        udp.Send(sendBytes, sendBytes.Length);
    }

    void Update()
    {
        SpawnPlayers();
        UpdatePlayers();
        DestroyPlayers();
    }
}
