QuadtreeContour
===============
半透明ビルボードの不透明部分だけメッシュ化して、無駄なオーバードローを減らすツールです。

例
[画像にそったメッシュ生成](Doc/Mesh.png)

使い方
QuadtreeContourGenerator (Monobehaviour) を半透明テクスチャを持つGameObjectに追加して、(再帰的)分割数と、アルファのしきい値を指定して、Updateするとメッシュを生成します。
