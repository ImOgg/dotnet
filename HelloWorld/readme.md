建立第一支 C# Project
建立新專案
dotnet new console -n HelloWorld
進入專案資料夾
cd HelloWorld
執行程式
dotnet run
執行完畢後就可以看到結果如下

Hello, World!
利用 VScode 打開Program.cs可以看到程式碼如下
// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
加入一段程式後再執行
加入的程式碼如下：
Console.WriteLine("The current time is " + DateTime.Now);
儲存後執行，結果如下：

Hello, World!