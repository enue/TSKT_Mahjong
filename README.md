# install

UPM Package

add package from git url

+ `https://github.com/enue/Unity_TSKT_Container.git?path=Assets/Package`
+ `https://github.com/enue/Unity_TSKT_Math.git?path=Assets/Package`
+ `https://github.com/enue/TSKT_Mahjong.git?path=Assets/Package`

# 何をするライブラリなのか

　麻雀のルール部分を実装したライブラリです。

　卓上にある牌を管理して、鳴いたりロンしたりといったAPIを提供します。

　あるのはルール部分の実装のみなので、「牌を画面に表示する」「ユーザー操作を受け付ける」「AIが打つ」といった部分は別途開発する必要があります。

# 使い方

## ゲームの進め方

### 1. 半荘の開始

　`Game`を`Create`してルールを定める。

```cs
var afterDraw = Game.Create((PlayerIndex)Random.Range(0, 4),
    new RuleSetting()
    {
        end = new Mahjongs.Rules.EndRule()
        {
            extraRoundScoreThreshold = 30000,
            lengthType = Mahjongs.Rules.LengthType.東風戦,
            suddenDeathInExtraRound = true,
        },
        payment = new Mahjongs.Rules.PaymentRule()
        {
            返し = 30000,
            ウマ = new[] { 20, 10, -10, -20 },
        },
        initialScore = 25000,
        redTile = Mahjongs.Rules.RedTile.赤ドラ3});
```

### 2. IController取得
　APIを呼ぶと戻り値として`IController`が返ってくるので、これを使ってゲームを進行していく。

### 3. Command生成
　プレイヤーがとれる行動は`Command`クラスで表現されている。

　プレイヤー操作なりAIなりで希望する行動を決め、`Command`を生成する。

```cs
// 例えば牌を切る行動はDiscardクラスで表現される。
if (afterDraw.CanDiscard(tile, out var discardCommand))
{
}

// 実行可能な全行動はExecutableCommandsで取得できる
var allExecutableCommands = afterDraw.ExecutableCommands;
```

### 4. Command実行

　`IController.ExecuteCommands`で実行する。

```cs
// 切った牌に対してポン
var commandResult = afterDiscard.ExecuteCommands(out var executeds, pon);
```

```cs
// ポンとチーが両方かかる場合。優先順位によりポンのみが実行される。
// executedsには{pon}が返される
var commandResult = afterDiscard.ExecuteCommands(out var executeds, pon, chi);
```

```cs
// ロンが複数希望されている場合は複数同時に処理されダブロンになる。
// executedsには{ron1, ron2}が返される
var commandResult = afterDiscard.ExecuteCommands(out var executeds, ron1, ron2);
```

```cs
// 特にすることがない（切ったあと誰も鳴かないなど）場合はcommandを渡さずに実行する。
var commandResult = afterDiscard.ExecuteCommands(out _);
```

### 5. 表示処理

+ 上りや流局などで局が終わった場合、`commandResult.roundResult`にデータが入っているのでそれに応じた画面を表示する
+ ゲームが終了した場合は`CommandResult.roundResult.gameResult`が入っている。

### 6. 繰り返し

　`commandResult.nextController`を取得して2へ戻る。

## シリアライズ

　状態をjson文字列にシリアライズできる。

```cs
var jsonString = controller.SerializeSession().ToJson();
var controller = Serializable.Session.FromJson(jsonString);
```


# できていないこと

　ローカルルールは設定で切り替えられたらいいなあ

+ 花牌、季節牌（赤牌は実装済み）
+ トリロン時の頭ハネ設定
+ 食いタン禁止（アリ固定）
+ 焼き鳥（なし固定）
+ 八連荘（何も起こりません）
+ 二翻縛り（常に一翻縛り固定）
+ 青天井ルール

