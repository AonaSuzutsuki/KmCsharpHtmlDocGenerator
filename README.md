# C# XMLDocument To Html
このプログラムではC# Compilerから出力できるXML Documentをユーザー定義のHTMLドキュメントへ変換する手法を提供します。  

# 開発経緯とゴール
C# XML Documentはソースコード上で記述したDocコメントをXMLで出力を行うものの、それだけでは人が読むには適しません。  
JavaなどのようにHTML出力してどのような端末でもWebブラウザ(標準インストールされているアプリケーション)を用いて見られることが理想ですが、残念ながらC# XML Documentには搭載されていません。手書きでHTMLを実装しても良いですが、毎回行うには不便で、理想は少ない操作でHTMLが生成されることです。  
そこでXML Documentが出力するXMLを解析してHTMLフォーマットに変換し、誰もが読めるように整えるのがこのプログラムおよび、プロジェクトのゴールです。  

## 開発環境
**Windows**
1. Visual Studio 2017
2. .Net Framework 4.7.2

**Mac**
1. Visual Studio for Mac

## 最低限動作に必要な環境
1. .Net Framework 4.7.2

## 使用方法
**CUI**
```
XMLDocumentToHtmlCUI.exe -o {OutputDir} {XmlDocuments}
example: XMLDocumentToHtmlCUI.exe -o Out XmlDocument1.xml XmlDocument2.xml
```

**オプション**  

| オプション | 説明 |
|-----|:----|
|-o   |出力先のディレクトリパスを指定します。|

**引数**  
C# XML Documentが出力するXMLを一つまたは複数指定します。

## ビルド
1. XMLDocumentToHtmlCUI.slnを開く
2. メニュー上のビルドボタンをクリック
