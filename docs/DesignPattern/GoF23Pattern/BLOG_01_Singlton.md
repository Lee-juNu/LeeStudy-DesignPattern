1. 私の経験
2. 私ならどう使うか
3. シングルトンの対案
4. 直接実験してコード、結果を一緒に上げること

## 挨拶

こんにちはリリです。いずれかブログを書くことになったは一度は書いてみた語った話題であります。シングルトンパターンはアンチパターンなのかに関して話していこうかと思います。

偶然にこの記事を見てシングルトンとは何？と思われる方のため書きますので事前知識がある方はスキップしても大丈夫です。

### シングルトンとは？

シングルトンパターンは、複数のインスタンスが生成されるとリソースを無駄にするクラスに対して、インスタンスを 1 回だけ生成し、グローバルにアクセスできるようにするために使用されます。

### C#で見るシングルトン

### シングルトンクラス

シングルトンクラスになるためには下記の三つの条件を満たす必要があります。

1. コンストラクターを使用できないように private を指定する。
2. インスタンスを呼び出す Static のメソッドまたはプロパティが必要です。
3. インスタンスを参照できる Static の変数が必要です。

```cs
public class Logger
{
    // 3. インスタンスを参照できる Static の変数が必要です。
    private static Logger? instance;

    // 1. コンストラクターを使用できないように private を指定する。
    private Logger()
    {
    }

    // 1. コンストラクターを使用できないように private を指定する。
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
}
```

```csharp
using System;

class Program
{
    static void Main(string[] args)
    {
        // Logger のインスタンスを取得
        // Instance は初めて呼ばれたため インスタンスを生成し返します。
        Logger logger1 = Logger.Instance;

        // 今回は Logger がすでに生成されているためコンストラクターなしで 参照変数を呼び出します。
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
    }
}
```

### シングルトンパターン

シングルトン（ Singleton ）パターンはインスタンスを一つのみを持つパターンです。シングルトンを使う理由と注意点は少し後にしてどんな形意をしているか見てみます。

### 人生初めてのデザインパターン「シングルトン」！
