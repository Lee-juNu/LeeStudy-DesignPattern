### Must-Know C# 13 and .NET 9 Features

排他制御のため新しい `System.Threading.Lock` タイプが C# 13 で紹介されました。

lock をするために昔は object タイプを使用していましたが。しかし今からは未来のスタンダードになっていく　 Lock タイプが出ました。

```cs

// Before

public class LockExample

{

    private readonly object _lock = new();



    public void DoStuff()

    {

        lock (_lock)

        {

           Console.WriteLine("We're inside old lock");

        }          

    }

}



// .NET 9

public class LockExample

{

    private readonly Lock _lock = new();



    public void DoStuff()

    {

        lock (_lock)

        {

            Console.WriteLine("We're inside .NET 9 lock");

        }          

    }

}

```

1. **完結で安全なコード。** このタイプは読みやすくそしてどんなオブジェクトが予想しやすくしてくれます。
2. **パフォーマンス。** Microsoft によれば任意の object インスタンスより効率的です。
3. **新しいロッキングメカニズム。** EnterScope が Monitor クラスの代わりに登場しました。コードはこの[リンク](https://gist.github.com/dkorolov1/307588775724e2a920f995998785fb70)から確認可能です.。enterScope は Dispose を含む ref struct を返します。そして using を使うことでもリリースすることが可能です。
4. **非同期の限界。** lock ブロックから aync を呼び出すことは相変わらず不可能です。 昔からの SemaphoresSlim が有効でしょう

```cs

public class LockExample

{

   private readonly Lock _lock = new();

   private readonly SemaphoreSlim _semaphore = new(1, 1);



   public async Task DoStuff(int val)

   {

      lock(_lock)

      {

         await Task.Delay(1000); // Compiler Error: Cannot 'await' in the body of a 'lock' statement

      }



      using(_lock.EnterScope())

      {

         await Task.Delay(1000); // Runtime Error: Instance of type 'System.Threading.Lock.Scope' cannot be preserved across 'await' or 'yield' boundary.

      }



      await _semaphore.WaitAsync();

      try

      {

         await Task.Delay(10); // eえらが出ない

      }

      finally

      {

         _semaphore.Release();

      }

   }

}

```

### Task.WhenEach

別の間隔で終了する task list を想像してみてください。あなたは個別に処理されることを望むでしょう。`WaitAll()`はすべてのタスクが終わるまで待ちます。タスクが完了されるたびに次のタスクを選択するために Task.WaitAny を使うことを方便として使っていました。C# 13 からはもっと優雅で効率的なアプローチで Task.WhenEach が出ました。

```cs

// List of 5 tasks that finish at random intervals

var tasks = Enumerable.Range(1, 5)

   .Select(async i =>

   {

     await Task.Delay(new Random().Next(1000, 5000));

     return $"Task {i} done";

   })

   .ToList();



// Before

while(tasks.Count > 0)

{

   var completedTask = await Task.WhenAny(tasks);

   tasks.Remove(completedTask);

   Console.WriteLine(await completedTask);

}



// .NET 9

await foreach (var completedTask in Task.WhenEach(tasks))

   Console.WriteLine(await completedTask);

```

Task WhenEach は `IAsyncEnumerable<Task<TResult>>` タイプを返します、これを用いて await foreach を使用してタスクが完了される順番で iterator を巡回することが可能になります。

### 3. params Collections

c#13 からは、params の引数がコレクション式で使えるすべてのタイプを使えるようになりました。

```cs
// Before

static void WriteNumbersCount(params int[] numbers)

   => Console.WriteLine(numbers.Length);



// .NET 9

static void WriteNumbersCount(params ReadOnlySpan<int> numbers) =>

    Console.WriteLine(numbers.Length);



static void WriteNumbersCount(params IEnumerable<int> numbers) =>

    Console.WriteLine(numbers.Count());



static void WriteNumbersCount(params HashSet<int> numbers) =>

    Console.WriteLine(numbers.Count);

```

1. **簡潔なコード**。 無数な ToArray() と ToList() の呼び出しを減らせます。

2. **性能向上**。 `.ToArray()` と `ToList()` はすでに追加されたことだけでリソースオバーヘッドを起こします。また、 `span<>` と `IEnumerable<>` もパラメータとして使えるためもっと効率的なメモリー使用と遅延実行が可能になります。結果、いろんな状況で柔軟で効率の良い性能を引きだます。

### 4. 半自動 プロパティ

C# で自動実装プロパティを定義するときに `public int Number { get; set; }`, コンパイラーは BackField(e.g., `_number`)と getter/setter メソッドを生成します。 しかし検証、基本値、 計算または遅延ローディングをプロパティの getter もしくは setter で作りたい場合 C# 13 で実装した `field` によれば backing field に直接アクセスすることが可能になります。

```cs
// Before
public class MagicNumber
{
    private int _number;

    public int Number
    {
        get => _number * 10;
        set {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Value must be greater than 0");
            _number = value;
        }
    }
}

// .NET 9
public class MagicNumber
{
    public int Number
    {
        get => field;
        set {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(value), "Value must be greater than 0");
            field = value;
        }
    }
}
```

1. **繰り返されるコードを減らせる.** 直接作った private backing fields を削除することによりもっと簡潔なコードを作成することが可能です。
2. **可読性が向上する。** カスタム backing fields 名を管理しなくなるため、 fields キーワードを標準として使うことでコードが簡潔になる。
3. **Property-Scoped Field**
   private backing field は自分自身を閉じ込めたプロパティーで意図せずにクラス外で使用することを事前に防いでくれます。
4. **変更による故障の可能性**。　既にプロパティー名に `field` を使っている場合新しい field キーワードが優先され意図してない行動をする可能性があります。C# チームからこの問題をどう対処するべきかはこの[リンク](https://github.com/dotnet/csharplang/blob/main/proposals/field-keyword.md#breaking-changes)を参照してください。 この機能は 2016 年 初めて提案されて延期になってきたのですが、延期されてきた理由の一つかもしれません

### Hybrid Cache

---

reduce 減らす
dedicated **～専用の・特定の目的のための**、献身的な
Enhanced 強化された
confined 閉じ込められた
preventing 未然に防ぐ
unintended 意図しない
encapsulation カプセル化
address ～を解決する
Potential Breaking Change 潜在的な互換性破壊変更
precedence 　優先順位
proposal 提案する
predictable 予測可能な
arbitrary 任意
unified 統一された

Stampede Problem キャッシュ内で特定のデータが満了されたり削除されたとき、多数のクライアント（もしくはスレッド）が同時にこのデータにリクエストを送り、処理するためにバックエンドのデータソースに**同時に接近**する
