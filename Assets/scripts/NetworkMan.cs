using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net.Sockets;
using System.Net;

public class NetworkMan : MonoBehaviour
{
    public GameObject playerPrefab;
    public List<GameObject> p_objs;
    UdpClient udp;

    void Start()
    {
        udp = new UdpClient();
        p_objs = new List<GameObject>();

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
    public enum commands
    {
        NEW_CLIENT,
        UPDATE,
        DISCONNECT_CLIENT,
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
        public struct receivedColor
        {
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
        public List<Player>players;
    }

    public Message latestMessage;
    public GameState latestGameState;
    public GameState prevGameState;
    void OnReceived(IAsyncResult result)
    {
        // this is what had been passed into BeginReceive as the second parameter:
        UdpClient socket = result.AsyncState as UdpClient;

        // points towards whoever had sent the message:
        IPEndPoint msgSrc = new IPEndPoint(0, 0);
        // get the actual message and fill out the source:
        byte[] message = socket.EndReceive(result, ref msgSrc);
        // do what you'd like with `message` here:
        string returnData = Encoding.ASCII.GetString(message);

        Debug.Log("Got this: " + returnData);
        latestMessage = JsonUtility.FromJson<Message>(returnData);
        try
        {
            switch (latestMessage.cmd)
            {
                case commands.NEW_CLIENT:
                    break;
                case commands.UPDATE:
                    latestGameState = JsonUtility.FromJson<GameState>(returnData);
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
        if(latestGameState.players.Count > p_objs.Count)
        {
            p_objs.Add(Instantiate(playerPrefab));
        }
    }
    
    void UpdatePlayers()
    {
        for(int p = 0; p< p_objs.Count; p++)
        {
            p_objs[p].GetComponent<PlayerScript>().id = latestGameState.players[p].id;
            
            p_objs[p].GetComponent<PlayerScript>().r = latestGameState.players[p].color.R;
            p_objs[p].GetComponent<PlayerScript>().g = latestGameState.players[p].color.G;
            p_objs[p].GetComponent<PlayerScript>().b = latestGameState.players[p].color.B;

            float r = p_objs[p].GetComponent<PlayerScript>().r;
            float g = p_objs[p].GetComponent<PlayerScript>().g;
            float b = p_objs[p].GetComponent<PlayerScript>().b;
            Color c = new Color(r,g,b);
            p_objs[p].GetComponent<PlayerScript>().GetComponent<MeshRenderer>().material.color = c;
        }
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
