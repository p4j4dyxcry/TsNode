
## チュートリアル(visual studio & nuget)
1.新しいWPFプロジェクト(csproj)の作成
![](https://cdn-ak.f.st-hatena.com/images/fotolife/a/at12k313/20200312/20200312015427.png)

2.nuget のPackageManagerから TsNodeをインストール
![](https://cdn-ak.f.st-hatena.com/images/fotolife/a/at12k313/20200312/20200312015547.png)

3.App.xamlのを`Application.Resources`を編集
```xml
    <Application.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <!--NetworkViewのテンプレートをインポート-->
                <ResourceDictionary Source="pack://application:,,,/TsNode;component/Template.xaml" />                
                <!--サンプルをインポート-->
                <ResourceDictionary Source="pack://application:,,,/TsNode;component/Preset/Presettemplate.xaml" />
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </Application.Resources>
```

4.MainWindow.xamlに参照を追加

```xml
xmlns:controls="clr-namespace:TsNode.Controls;assembly=TsNode"
```

5.MainWindow.xamlのGridに下記を追加

```xml
    <Grid>
        <controls:NetworkView
            Background="LightGray"
            Connections="{Binding Connections}"
            Nodes="{Binding Nodes}"/>
    </Grid>
```

6.MainWindowDataContextクラスの作成

```cs
    public class MainWindowDataContext
    {
        public ObservableCollection<INodeDataContext> Nodes { get; set; }
        public ObservableCollection<IConnectionDataContext> Connections { get; set; }

        public MainWindowDataContext()
        {
            Nodes = new ObservableCollection<INodeDataContext>();
            Connections = new ObservableCollection<IConnectionDataContext>();

            var node1 = new PresetNodeViewModel()
            {
                OutputPlugs = new ObservableCollection<IPlugDataContext>
                {
                    new PresentPlugViewModel(),
                }
            };
            
            var node2 = new PresetNodeViewModel()
            {
                X = 150,
                InputPlugs = new ObservableCollection<IPlugDataContext>
                {
                    new PresentPlugViewModel(),
                },
            };

            Nodes.Add(node1);
            Nodes.Add(node2);

            var connection = new PresetConnectionViewModel()
            {
                SourcePlug = node1.OutputPlugs[0],
                DestPlug = node2.InputPlugs[0],
            };

            Connections.Add(connection);
        }
    }
```

7.MainWindow.xaml.csのコンストラクタを編集
```cs
        public MainWindow()
        {
            DataContext = new MainWindowDataContext();
            InitializeComponent();
        }
```
8.実行すると下記の画面が表示されます。

![](https://cdn-ak.f.st-hatena.com/images/fotolife/a/at12k313/20200312/20200312014138.png)

※完成プロジェクトはこちらから取得できます。
[.NET Core用](https://github.com/p4j4dyxcry/TsNode/tree/master/nuget-SampleProjects/TsNodeApp)
[.NET Framework用](https://github.com/p4j4dyxcry/TsNode/tree/master/nuget-SampleProjects/TsNodeAppBy.NetFramework)
