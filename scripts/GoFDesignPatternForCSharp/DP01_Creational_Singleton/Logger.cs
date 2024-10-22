public class Logger
{
    /// <summary>
    /// Logger クラスのシングルトンインスタンスを保持するための静的フィールド
    /// </summary>
    private static Logger? instance;

    /// <summary>
    /// ログファイルのパス
    /// </summary>
    private string logFilePath;

    /// <summary>
    /// コンストラクタを private に設定し、外部からインスタンス化できないようにする。
    /// </summary>
    private Logger()
    {
        logFilePath = "log.txt"; // ログファイルのデフォルトパスを設定
    }

    /// <summary>
    /// Logger クラスのシングルトンインスタンスを取得するプロパティ
    /// </summary>
    public static Logger Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new Logger();
            }
            return instance;
        }
    }

    /// <summary>
    /// ログメッセージをログファイルに記録するメソッド
    /// </summary>
    /// <param name="message">記録するメッセージ</param>
    public void Log(string message)
    {
        try
        {
            // ログメッセージにタイムスタンプを追加
            string logMessage = $"{DateTime.Now}: {message}";

            // ログメッセージをログファイルに書き込む
            File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
        }
        catch (Exception ex)
        {
            // ログの書き込み中にエラーが発生した場合の処理
            Console.WriteLine("ログ出力に失敗しました。: " + ex.Message);
        }
    }

    /// <summary>
    /// ログファイルのパスを設定するメソッド
    /// </summary>
    /// <param name="path">設定するログファイルのパス</param>
    public void SetLogFilePath(string path)
    {
        logFilePath = path;
    }
}
