class Program
{
    static void Main(string[] args)
    {
        // Logger のインスタンスを取得
        // Instance は初めて呼ばれたため インスタンスを生成し返します。
        Logger logger1 = Logger.Instance;

        // ログファイルのパスを設定
        logger1.SetLogFilePath("singletonTestLog.txt");

        // ログにメッセージを記録
        logger1.Log("アプリケーションが開始されました。");

        // 別の Logger インスタンスを取得
        // 今回は Logger がすでに生成されているため
        Logger logger2 = Logger.Instance;

        // 2つのインスタンスが同一か確認
        if (ReferenceEquals(logger1, logger2))
        {
            // 同じインスタンスであることを確認
            Console.WriteLine("logger1 と logger2 は同じインスタンスです。");
        }
        else
        {
            // 異なるインスタンスである場合（通常はこのケースは発生しません）
            Console.WriteLine("logger1 と logger2 は異なるインスタンスです。");
        }

        // 追加のログメッセージを記録
        logger2.Log("アプリケーションが終了しました。");

        // コンソールにメッセージを表示
        Console.WriteLine("ログの記録が完了しました。");
    }
}
