//PCをクライアントまたはサーバにして、データの送受信をするプログラム
//written 2018/02/15

﻿using System;

//----------//コネクション処理と送受信処理//----------//
class Conection
{
    public const string IpString = "127.0.0.1";
    private string resMsg;

    private System.Net.IPAddress ipAdd;
    private System.Net.Sockets.TcpListener listener;
    private System.Net.Sockets.NetworkStream ns;
    private System.IO.MemoryStream ms;
    private System.Net.Sockets.TcpClient client;
    private System.Text.Encoding enc;
    private System.Net.Sockets.TcpClient tcp;

    //-----//サーバ側の処理//-----//
    public Conection(int port)
    {
        ipAdd = System.Net.IPAddress.Parse(IpString);
        listener = new System.Net.Sockets.TcpListener(ipAdd, port);
        //受信開始
        listener.Start();
        Console.WriteLine("Listenを開始しました({0}:{1})。",
            ((System.Net.IPEndPoint)listener.LocalEndpoint).Address,
            ((System.Net.IPEndPoint)listener.LocalEndpoint).Port);
        //接続植え付け
        client = listener.AcceptTcpClient();
        Console.WriteLine("クライアント({0}:{1})と接続しました。",
            ((System.Net.IPEndPoint)client.Client.RemoteEndPoint).Address,
            ((System.Net.IPEndPoint)client.Client.RemoteEndPoint).Port);
        //NetworkStream（ネットワークを読み書きの対象とする時のストリーム）うまくかけん)を取得
        ns = client.GetStream();
    }

    //-----//クライアント側の処理//-----//
    public Conection(string ipOrHost, int port)
    {
        //サーバーと接続する
        tcp = new System.Net.Sockets.TcpClient(ipOrHost, port);
        Console.WriteLine("サーバー({0}:{1})と接続しました({2}:{3})。", ((System.Net.IPEndPoint)tcp.Client.RemoteEndPoint).Address, ((System.Net.IPEndPoint)tcp.Client.RemoteEndPoint).Port, ((System.Net.IPEndPoint)tcp.Client.LocalEndPoint).Address, ((System.Net.IPEndPoint)tcp.Client.LocalEndPoint).Port);
        //NetworkStreamを取得する
        ns = tcp.GetStream();
    }

    //-----//送信相手からデータを受け取る//-----//
    public void recvData()
    {
        //タイムアウト//とりあえず10秒
        // ns.ReadTimeout = 10000;
        //ns.WriteTimeout = 10000;

        //クライアントから送られたデータを受信する
        enc = System.Text.Encoding.UTF8;
        ms = new System.IO.MemoryStream();
        byte[] resBytes = new byte[256];
        int resSize = 0;
        do
        {
            resSize = ns.Read(resBytes, 0, resBytes.Length);
            //Readが0を返す→通信が切断したと判断
            if (resSize == 0)
            {
                Console.WriteLine("通信が切断されました。");
                break;
            }
            //受信したデータを蓄積する
            ms.Write(resBytes, 0, resSize);
            //まだ読み取れるデータがあるか、データの最後が\nでない時は受信を続ける
        } while (ns.DataAvailable || resBytes[resSize - 1] != '\n');
        //受信したデータを文字列に変換
        resMsg = enc.GetString(ms.GetBuffer(), 0, (int)ms.Length);
        ms.Close();
        //末尾の\nを削除
        resMsg = resMsg.TrimEnd('\n');
        Console.WriteLine(resMsg);
    }

    //-----//接続相手にデータを送信する//-----//
    public void sendData()
    {
        enc = System.Text.Encoding.UTF8;
        //クライアントに送る文字列を作成してデータを送信する
        string sendMsg = "Hello, World";
        //文字列をバイト型配列に変換
        byte[] sendBytes = enc.GetBytes(sendMsg + '\n');
        //データを送信する
        ns.Write(sendBytes, 0, sendBytes.Length);
        Console.WriteLine(sendMsg);
    }

    //-----//サーバ側のソケットを閉じる//-----//
    public void serverClose()
    {
        ns.Close();
        client.Close();
        Console.WriteLine("クライアントとの接続終了");
        listener.Stop();
        Console.WriteLine("Listener終了");
        Console.ReadLine();
    }

    //-----//クライアント側のソケットを閉じる//-----//
    public void clientClose()
    {
        //閉じる
        ns.Close();
        tcp.Close();
        Console.WriteLine("切断しました。");
        Console.ReadLine();
    }
}

//----------//メイン//---------//
public class TcpIp
{
    public const string IpOrHost = "127.0.0.1";  //送信元のIPアンドレス
    public const int Port = 9999;                //受け取るポート番号

    public static void Main()
    {
        Conection client = new Conection(IpOrHost ,Port);
        while (true)
        {
            client.sendData();
            System.Threading.Thread.Sleep(2000);
        }
        client.clientClose();
    }
}
