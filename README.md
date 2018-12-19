# 前提
### 開発環境（関係ありそうなヤツだけ抜粋）
Microsoft Visual Studio Enterprise 2017  
Version 15.9.4  
VisualStudio.15.Release/15.9.4+28307.222  
Microsoft .NET Framework  
Version 4.7.03056  
インストールされているバージョン:Enterprise  

ASP.NET and Web Tools 2017   15.9.04012.0  
ASP.NET Core Razor Language Services   15.8.31590  
ASP.NET Web Frameworks and Tools 2017   5.2.60913.0  
Azure App Service Tools v3.0.0   15.9.03024.0  
Azure Functions と Web ジョブ ツール   15.9.02046.0  
Common Azure Tools   1.10  
Microsoft Azure Tools   2.9  
Microsoft Azure Tools for Microsoft Visual Studio 2017 - v2.9.10730.2  
NuGet パッケージ マネージャー   4.6.0  

### DI
下記を参考にDIしようとしている。  
https://blog.wille-zone.de/post/dependency-injection-for-azure-functions/  
Nuget : [Willezone.Azure.WebJobs.Extensions.DependencyInjection](https://www.nuget.org/packages/Willezone.Azure.WebJobs.Extensions.DependencyInjection)  

# 問題
### 現象
下記のようなスタートアップ処理があるが .netcore v2.1 の Function を実行しても処理が走らない
```C#
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using netcore_v2_1_TestFunctions;
using netcore_v2_1_TestFunctions.Services;
using System.IO;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

[assembly: WebJobsStartup(typeof(Startup))]
namespace netcore_v2_1_TestFunctions
{
    public class Startup : IWebJobsStartup
    {
        public void Configure(IWebJobsBuilder builder) =>
           builder.AddDependencyInjection(ConfigureServices);

        private void ConfigureServices(IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            services.AddSingleton(configuration);
            services.AddSingleton<IMyService, MyService>();
        }
    }
}

```

- .netcore v2.0 の場合  
https://github.com/shunsuke-kawai/netcore_TestFunctions/tree/master/netcore_v2_0_TestFunctions
    - Function 起動時に Startup が走る
![image](https://user-images.githubusercontent.com/6369070/50200864-1fcf3c00-039b-11e9-9e13-27e305d3aaad.png)

    - extensions.json には Startup が登録されている   
        \netcore_TestFunctions\netcore_v2_0_TestFunctions\bin\Debug\netcoreapp2.0\bin\extensions.json
        ```json
        {
        "extensions":[
            { "name": "Startup", "typeName":"netcore_v2_0_TestFunctions.Startup, netcore_v2_0_TestFunctions, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"}
        ]
        }
        ```

- .netcore v2.1 の場合  
https://github.com/shunsuke-kawai/netcore_TestFunctions/tree/master/netcore_v2_1_TestFunctions
    - Function 起動時に Startup が走らない
![image](https://user-images.githubusercontent.com/6369070/50201116-29a56f00-039c-11e9-850c-89757868e0d9.png)

    - extensions.json は空   
        \netcore_TestFunctions\netcore_v2_1_TestFunctions\bin\Debug\netcoreapp2.1\bin\extensions.json
        ```json
        {
        "extensions":[
        ]
        }
        ```

# 回避策
- 下記を参考に Directory.Build.targets を v2.1 のプロジェクト直下に配置  
https://github.com/Azure/azure-functions-host/issues/3386#issuecomment-419565714

- v2.0 で生成されている extensions.json をプロジェクトに追加⇒常にコピーする を設定  
![image](https://user-images.githubusercontent.com/6369070/50201679-5c506700-039e-11e9-9cbc-6f26be04d388.png)

- 🎉v2.1 でも Startup が走るようになった🎉
![image](https://user-images.githubusercontent.com/6369070/50202147-58bddf80-03a0-11e9-98b0-80a9f3280c2e.png)

- 対応後プロジェクト  
https://github.com/shunsuke-kawai/netcore_TestFunctions/tree/master/netcore_v2_1_TestFunctions_after


# 疑問
- こんなことをやってやらないといけないのか？  
- extensions.json にはバージョンも持ってるみたい（これが食い違っても実行はできたが気持ち悪い  
- この他の extension が増えた場合、逐次 v2.0 でビルド ⇒ extensions.json をコピー ⇒ v2.1 に戻す みたいな手順をやらないといけない？  

# その他参考リンク
Startup は別にして .NET Standard 2.0 で作れってこと？  
https://github.com/Azure/Azure-Functions/issues/1016#issuecomment-436092526
> This is a known issue we're addressing. In the meantime, one of the common ways to workaround the issue is to implement the startup in a separate project (targeting .NET Standard 2.0).



