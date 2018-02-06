using System;

public class Server
{
    public static void Main()
    {
        //送信してくるIPアドレス//とりあえず
        string ipString = "127.0.0.1"; //ローカルは”127.0.0.1”
        System.Net.IPAddress ipAdd = System.Net.IPAddress.Parse(ipString);


        //受け取るポート番号
        int port = 9999;

        //TcpListener（TCP ネットワーク クライアントからの接続をリッスン奴）
        System.Net.Sockets.TcpListener listener =
            new System.Net.Sockets.TcpListener(ipAdd, port);

        //受信開始
        listener.Start();
        Console.WriteLine("Listenを開始しました({0}:{1})。",
            ((System.Net.IPEndPoint)listener.LocalEndpoint).Address,
            ((System.Net.IPEndPoint)listener.LocalEndpoint).Port);

        //接続植え付け
        System.Net.Sockets.TcpClient client = listener.AcceptTcpClient();
        Console.WriteLine("クライアント({0}:{1})と接続しました。",
            ((System.Net.IPEndPoint)client.Client.RemoteEndPoint).Address,
            ((System.Net.IPEndPoint)client.Client.RemoteEndPoint).Port);


        //NetworkStream（ネットワークを読み書きの対象とする時のストリーム）うまくかけん)を取得
        System.Net.Sockets.NetworkStream ns = client.GetStream();

        //タイムアウト//とりあえず10秒
       // ns.ReadTimeout = 10000;
        //ns.WriteTimeout = 10000;

        //クライアントから送られたデータを受信する
        System.Text.Encoding enc = System.Text.Encoding.UTF8;
        bool disconnected = false;
        System.IO.MemoryStream ms = new System.IO.MemoryStream();
        byte[] resBytes = new byte[256];
        int resSize = 0;
        do
        {
            resSize = ns.Read(resBytes, 0, resBytes.Length);
           
            //Readが0を返す→通信が切断したと判断
            if (resSize == 0)
            {
                disconnected = true;
                Console.WriteLine("通信が切断されました。");
                break;
            }


            //受信したデータを蓄積する
            ms.Write(resBytes, 0, resSize);
        
            //まだ読み取れるデータがあるか、データの最後が\nでない時は受信を続ける
        } while (ns.DataAvailable || resBytes[resSize - 1] != '\n');
        
        //受信したデータを文字列に変換
        string resMsg = enc.GetString(ms.GetBuffer(), 0, (int)ms.Length);
        ms.Close();


        //末尾の\nを削除
        resMsg = resMsg.TrimEnd('\n');
        Console.WriteLine(resMsg);


        

        if (!disconnected)
        {
            //クライアントに送る文字列を作成してデータを送信する
            string sendMsg = resMsg.Length.ToString();
            //文字列をバイト型配列に変換
            byte[] sendBytes = enc.GetBytes(sendMsg + '\n');
            //データを送信する
            ns.Write(sendBytes, 0, sendBytes.Length);
            Console.WriteLine(sendMsg);
        }

        //閉じる
        ns.Close();
        client.Close();
        Console.WriteLine("クライアントとの接続終了");

        listener.Stop();
        Console.WriteLine("Listener終了");

        Console.ReadLine();
    }
}