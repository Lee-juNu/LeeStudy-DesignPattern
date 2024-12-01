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

新しく出た HybridCache API addresses は stampede problem のよう`IDistributedCache` と `IMemoryCache` に前から存在していた問題をいくつかの問題(差？GAP?)を解決します。HybridCache は新しい機能と能力、柔軟で向上された性能を持つ.NET のキャッシュ作成が入ります。HybridCache はいろんな状況で手軽に IDistributedChache と IMemoryChache を入れ替えられるように設計されています。

```cs
public record Post(int UserId, int Id, string Title, string Body);

public class PostsService(
    IHttpClientFactory httpClientFactory,
    IMemoryCache memoryCache,
    IDistributedCache distributedCache,
    HybridCache hybridCache)
{
    public async Task<List<Post>?> GetUserPostsAsync(string userId)
    {
        var cacheKey = $"posts_{userId}";

        // Before (Memory Chache)
        var posts = await memoryCache.GetOrCreateAsync(cacheKey,
            async _ => await GetPostsAsync(userId));

        // Before (Distributed Chache)
        var postsJson = await distributedCache.GetStringAsync(cacheKey);
        if(postsJson is null)
        {
            posts = await GetPostsAsync(userId);
            await distributedCache.SetStringAsync(cacheKey, JsonSerializer.Serialize(posts));
        } else {
            posts = JsonSerializer.Deserialize<List<Post>>(postsJson);
        }

        // .NET 9 Hybrid Cache
        posts = await hybridCache.GetOrCreateAsync(cacheKey,
            async _ => await GetPostsAsync(userId), new HybridCacheEntryOptions() {
                Flags = HybridCacheEntryFlags.DisableLocalCache | // Act as distributed cache
                        HybridCacheEntryFlags.DisableDistributedCache // Act as local cache
            });

        return posts;
    }

    private async Task<List<Post>?> GetPostsAsync(string userId)
    {
        Console.WriteLine("===========Fetching posts from API");
        var url = $"https://jsonplaceholder.typicode.com/posts?userId={userId}";
        var client = httpClientFactory.CreateClient();
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<List<Post>>();
    }
}
```

1. `IDistributedCache` と `IMemoryCache` すべての長所がある。HybridCahceは二つのAPIを簡潔に統合してデータメモリまたは分散キャッシュを保存する柔軟性を提供してくれます。
この設定を通じてよく使うデータあｈローカルでアクセスす(L1)を通じて速いスピードでアクセスすることができて、よく使われていないかもっと大きいデータは拡張可能な外部キャッシュ(L2)を通じてアクセス可能です。この動作は `HybridChcheEntryFlags`を通じて制御できます。

2. **Stampede から安全。** IMemoryCache と IDistributeCache は Stamepede の問題ががあります。HybridCache は特定のキーに対して１つのcaller のみを再生成することを保証し、他の caller は結果を待つことで問題を解決しています。結果過剰な
キャッシュの再生成を防止できます。

3. **他の特徴。** `HybridCahce` は次のような機能を提供します。Tagging、.WithSerializer および .WithSerializerFactory メソッドを通じた設定可能な直列化、[ImmutableObject(true)]アノテーションを使ったキャッシュインスタンスの再利用。

最初には少し難しく聞こえるかも知りませんが、 `HybridCache`がプロセス外部(L2)にデータを保存しようとすると相変わらず IDistributedCacheを構成するべきで、HybridCacheはこれを基盤に作動します。しかし IDistributedCache が無くっても HybridCache サービスはプロセス内部のキャッシングを提供します。

```cs
// Distributed Cache
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = "SampleInstance";
});

builder.Services.AddMemoryCache(); // Only for demo purposes as HybridCache can handle both
builder.Services.AddHybridCache();
builder.Services.AddSingleton<PostsService>();
```


6. Built-in OpenAPI Document Generation（内蔵できる OpenAPI 文書自動生成）

.NET5 からは Web API テンプレート Swashbuckle.AspNetCore パッケージを通じて OpenAPI を支援していました。.NET9 からは Microsoft が開発したパッケージである Microsoft.AspNetCore.OpenApi を通じて OpenAPI を支援して、 Swashbuckle.AspNetCore から replacement される予定です。

```cs
// Before
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// .NET 9
builder.Services.AddOpenApi();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
```

アップ実行後、`/openapi/v1.json`に OpenAPI 文書が生成されていることが確認できます。

**Swagger UI。**(使ったことないのでまだ理解不可能)
文法はもっと完結になって、初人証ではもっと自然に見えることです。しかし、相互作用するAPIドキュメントをなしでも OpenAPI文書のみ生成されます。もしも Swagger UI とおなじ相互作用することができる API 文書が必須的なら Scalarのようなサードパティツールを統合する必要があります。詳細なガイドですScalar .NET API Reference Integration.

**Build-Time Generation。**
OpenAPI 文書は Microsoft.Extensions.ApiDescription.Server パッケージを使用してビルドするときにも生成することが可能です。

### SearchValues Improvements

SerachValues は .NET 8 から導入されていて、ICollection.Contains にい比べて効率的な検索のために最適されている readonlyの値セットを提供します。元は文字やバイト集合だけで限定されていたが.NET9からは **SearchValues<T>**のおかげで文字列も支援するように拡張されました。


```cs
var text = "Exploring new capabilities of SearchValues!".AsSpan();

// Before
var vowelSearch = SearchValues.Create([ 'n', 'e', 'w' ]);
Console.WriteLine(text.ContainsAny(vowelSearch));

// .NET 9
var keywordSearch = SearchValues.Create(["new", "of"], StringComparison.OrdinalIgnoreCase);
Console.WriteLine(text.ContainsAny(keywordSearch));
```

ここから `StringComparison` パラメータを使って比較するタイプを指定することができます。
未来には、この機能は文書のパーシング、入力フィルタリング、スパム探知、データ修正、検索など広範囲なテキスト処理が必要なアプリケーションで必ず使用する選択肢になります。


### NEW LINQ Methods

三つの新しいメソッドが追加されました（CountBy, AggregateBy, Index）
この三つのメソッドは一般的なデータを操作するタスクを簡潔さと性能を高めるように設計されています。以下の例で実際の使用例を見てください

### Built-in UUID v7 Generation (UUIDって何？)

.NET は初期から Guid.NewGuid() を使用して UUID を生成してきました。このメソッドは UUID v4 を生成します。今後 UUID 使用は大きく発展して現在は安定されていて現在使えるのはv7です。
v7 の特上としてはタイムスタンプがあるということです。

```bash
+------------------+---------------+----------------------+
| 48-bit timestamp | 12-bit random |    62-bit random     |
+------------------+---------------+----------------------+
```




---

reduce 減らす
dedicated **～専用の・特定の目的のための**、献身的な
Enhanced 強化された、高める（性能などを）
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
scalable 拡張性がある(sclae + able)
integrate 統合する
specification 仕様書(spec + ification)
internally 内部・内部的に
definitely 明確に、もちろん（certainly）[definite]
Since the early days of .NET：初期から
significantly：
advanced : 進む、進捗させる、出世する
significant : 重要な、大切な、～を意味する、表す
facilitates : 容易にする、進行する、すすむ



Stampede Problem キャッシュ内で特定のデータが満了されたり削除されたとき、多数のクライアント（もしくはスレッド）が同時にこのデータにリクエストを送り、処理するためにバックエンドのデータソースに**同時に接近**する

「下記」は下ではなく、For My Case Above 上と書くところ


