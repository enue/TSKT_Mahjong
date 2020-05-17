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

## 半荘の開始

　`Game`を`Create`してルールを定める

```cs
var beforeRoundStart = Game.Create(Random.Range(0, 4),
    new RuleSetting()
    {
        end = new Mahjongs.Rules.EndRule()
        {
            extraRoundScoreThreshold = 30000,
            lengthType = Mahjongs.Rules.LengthType.東風戦,
            suddenDeathInExtraRound = true,
        },
        payment = new Mahjongs.Rules.PaymentRule(30000, 20, 10, -10, -20),
        initialPoint = 25000,
        redTile = Mahjongs.Rules.RedTile.赤ドラ3
    }
);
```

## 局の開始

```cs
var afterDraw = beforeRoundStart.StartRound();
```

## 局を進める
 APIを呼ぶと戻り値として`IController`が返ってくるので、これを使ってゲームを進行していく。

```cs
// 牌を選択して捨てる
var afterDiscard = afterDraw.Discard(tile, riichi: false);

// 捨てられた牌を鳴く
if (afterDiscard.CanChi(player))
{
    var afterDraw = afterDiscard.Chi(player, tile);
}
else
{
    // 鳴かないなら下家へ手番を移す
    // このとき流局判定も行われる
    var afterDraw = afterDiscard.AdvanceTurn(out var roundResult, out var finishRoundStates);
}
```

## 局の終わり

　流局や上がりを処理するAPIは、`RoundResult`を返す。

```cs
// ツモ
if (afterDraw.CanTsumo())
{
    var roundResult = afterDraw.Tsumo(out tsumoResult);
}
```

```cs
// ロン
if (afterDiscard.CanRon(player))
{
    var roundResult = afterDiscard.Ron(out ronResult, player);
}
```

```cs
// 次の局を開始する
var afterDraw = roundResult.beforeRoundStart.StartRound();
```

## ゲームの終わり

　`RoundResult`が`gameResult`を持っている場合はゲーム終了。

```cs
if (roundResult.gameResult != null)
{
    // 終了処理
}
```


# できていないこと

　ローカルルールは設定で切り替えられたらいいなあ

+ 国士無双の暗槓の槍槓
+ 花牌、季節牌（赤牌は実装済み）
+ トリロン時の頭ハネ設定
+ 食いタン禁止（アリ固定）
+ 喰い替え禁止（あり固定）
+ 焼き鳥（なし固定）
+ 八連荘（何も起こりません）
+ 二翻縛り（常に一翻縛り固定）
+ 青天井ルール

