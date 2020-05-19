# install

UPM Package

add package from git url

+ `https://github.com/enue/Unity_TSKT_Container.git`
+ `https://github.com/enue/Unity_TSKT_Math.git`
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
var afterDraw = Game.Create(Random.Range(0, 4),
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
if (afterDraw.CanDiscard(tile))
{
    var discardCommand = new Discard(afterDraw, tile);
}

// 実行可能な全行動はExecutableCommandsで取得できる
var allExecutableCommands = afterDraw.ExecutableCommands;
```

### 4. CommandSelectorで実行

　`Command`を`CommandSelector`に放り込んで`Execute`する。

+ このときポンとチーを両方希望されている場合は優先順位によりポンのみが実行される。
+ またロンが複数希望されている場合は複数同時に処理されダブロンになる。

```cs
var selector = new CommandSelector(afterDraw);
selector.commands.Add(hoge);
selector.commands.Add(fuga);
selector.commands.Add(piyo);
var commandResult = selector.Execute(out var executedCommands);
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

