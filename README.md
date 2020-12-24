## TsNode
github  
https://github.com/p4j4dyxcry/TsNode

![.NET Core Version: >= 3.1](https://img.shields.io/badge/.NET%20Core-%3E%3D%203.1-brightgreen) 
![.NET Framework version: >= 4.62](https://img.shields.io/badge/.NET%20Framework-%3E%3D%204.62-brightgreen) [![MIT License](http://img.shields.io/badge/license-MIT-lightgray)](LICENSE)  
グラフエディターを作成するためのWPFのカスタムコントロールライブラリです。
他のオープンソースライブラリ等に依存しない軽量でカスタマイズ性の高いパッケージとなっています。

![ts-node-anime](https://user-images.githubusercontent.com/11988607/56496933-6e9e0580-6536-11e9-8a80-967e5dcdc8a6.gif)

## TsNodeの特徴
MVVMと相性の良い設計になっており、ViewModelやModelデータのクラスを制限しません。  
interfaceを使った疎結合な実装で独自のViewModel、Modelを利用することができます。  
もちろん、データを可視化したいだけのユースケースであればプリセットのデータモデルを利用することもできます。
プリセットについては[こちら](https://github.com/p4j4dyxcry/TsNode-Examples)のNodeEngine Sampleをご覧ください。

TsNodeではグラフオブジェクトを Node , Plug , Connection という名前で識別します。
更にこれらのNode,Plug,ConnectionはCustomControlを利用して実装しているのでDataTemplateを用いて自由な見た目に変更することができます。

## 便利な機能
- <b>グリッドスナップ機能</b> ノードをドラッグするときに自動的にグリッドに吸着させることができます。  
- <b>表示領域の自動拡張機能</b> ノードの配置座標からスクロールバー領域を自動的に計算します。利用者はキャンバスサイズを意識しません。
- <b>範囲選択機能</b> 標準で複数のノードを矩形を使った範囲選択する機能です。
- <b>画面フィット機能</b> Fキーで選択ノードにフォーカスすることができます。ノードが選択されていない状態ですと全ノードが画面に収まるように自動フィットします。
- <b>拡縮・平行移動機能</b> Ctrl + 中ホイールで表示領域の拡縮　、中ボタンの押し込みドラッグで画面の平行移動ができます。
- <b>自動レイアウト(α版)</b> プリセット機能を使う場合はノードの自動レイアウトが行えます。

## 今後の展望
- ミニマップ機能　  
今後のアップデートでノードグラフをミニマップで凡その領域把握と移動をできるようにサポートします。
- ルーラー表示　  
大きさなどが大まかにわかるようにルーラーを実装予定です。
- パフォーマンス改善   
現在のコントロールでは数千を超えるノード表示をする場合はどうしてもパフォーマンスが落ちてしまいます。今後のアップデートで改善予定です。
- Node,Connectionの描画順管理　  
現在のノードエディターではノードの表示順等を柔軟にカスタイマイズすることができません。ユーザーがレガシーな手段で実現する必要があります。これをライブラリとしてサポートします。
- グループ化・コメント等   
ノードをグループやコメント機能、折り畳み機能などはユーザー側で実現する必要があります。これをライブラリでサポートします。
- 自動レイアウトの拡張    
現在の自動レイアウト機能は多くの制限があります。
1. 自動レイアウトを利用する場合は自由なModelやViewModelが利用できず、大きな制約があります。
2. レイアウトアルゴリズムをカスタマイズできません。現在は独自のアルゴリズムに基づき整列されます。  

## nuget
https://www.nuget.org/packages/TsNode/

VisualStudioを使った導入方法をステップバイステップで紹介しています。  
[nugetTutorial](https://github.com/p4j4dyxcry/TsNode/tree/master/nuget-SampleProjects)

## 利用例
利用例については別リポジトリに用意しています。こちらを参考に実装していただくことをお勧めします。  
https://github.com/p4j4dyxcry/TsNode-Examples


## ライセンス
[MIT](https://github.com/p4j4dyxcry/TsNode/blob/master/LICENSE)
