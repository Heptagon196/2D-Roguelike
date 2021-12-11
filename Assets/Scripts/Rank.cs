using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine.UI;
using Task = System.Threading.Tasks.Task;

public struct Received
{
    public IPEndPoint Sender;
    public string Message;
}

abstract class UdpBase {
    protected UdpClient Client;

    protected UdpBase()
    {
        Client = new UdpClient();
    }

    public async Task<Received> Receive()
    {
        var result = await Client.ReceiveAsync();
        return new Received()
        {
            Message = Encoding.UTF8.GetString(result.Buffer, 0, result.Buffer.Length),
            Sender = result.RemoteEndPoint
        };
    }
}

//Client
class UdpUser : UdpBase
{
    private UdpUser(){}

    public static UdpUser ConnectTo(string hostname, int port)
    {
        var connection = new UdpUser();
        connection.Client.Connect(hostname, port);
        return connection;
    }

    public void Send(string message)
    {
        var datagram = Encoding.UTF8.GetBytes(message);
        Client.Send(datagram, datagram.Length);
    }

}

public class Pair {
    public string name;
    public int score;

    public Pair(string Name, int Score) {
        name = Name;
        score = Score;
    }
}

public class Rank : MonoBehaviour {
    public static Rank instance = null;
    public List<GameObject> textBox;
    public int playerScore = -1;
    [HideInInspector] public bool isRefreshing = false;
    private UdpUser client;
    private string Received = "";
    
    private void Awake() {
        if (instance == null) {
            instance = this;
        } else if (instance != this) {
            Destroy(this.gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        client = UdpUser.ConnectTo("45.32.138.103", 32123);
        Task.Factory.StartNew(async () => {
            while (true) {
                try {
                    var received = await client.Receive();
                    Received = received.Message;
                } catch (Exception ex) {
                    Debug.Log(ex);
                }
            }
        });
    }
    
    private IEnumerator _Send(Pair message) {
        client.Send(message.name + ":" + Convert.ToString(message.score));
        yield break;
    }

    public void Upload(Pair message) {
        StartCoroutine(_Send(message));
    }

    private IEnumerator _ShowMessage() {
        client.Send("0:-1");
        float startTime = Time.time;
        while (Received == "") {
            if (Time.time - startTime > 3f) {
                textBox[0].GetComponent<Text>().text = "Error: Connection Timeout. Please retry.";
                isRefreshing = false;
                yield break;
            }
            yield return new WaitForSeconds(0.1f);
        }
        List<string> recMenu = new List<string>();
        int l = 0, r = -2;
        for (int i = 0; i < Received.Length; i++) {
            if (Received[i] == '\n') {
                l = r + 2;
                r = i - 1;
                //Debug.Log("|" + Received.Substring(l, r - l + 1) + "|");
                recMenu.Add(Received.Substring(l, r - l + 1));
            }
        }
        for (int i = 0; i < Math.Min(recMenu.Count, textBox.Count); i++) {
            textBox[i].GetComponent<Text>().text = recMenu[i];
        }
        isRefreshing = false;
    }

    public void ShowMessage() {
        for (int i = 0; i < textBox.Count; i++) {
            textBox[i].GetComponent<Text>().text = "";
        }
        if (isRefreshing) {
            return;
        }
        isRefreshing = true;
        StartCoroutine(_ShowMessage());
    }
}
