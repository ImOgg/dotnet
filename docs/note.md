dotnet new sln 
dotnet new webapi -controllers
dotnet new webapi -h
dotnet new list 
dotnet run

目前學到
dotnet沒有像是laravel有個集中式管理route的地方 通常會寫在controller上
再來是WeatherForecastController 
只要把 controller去掉 貼到url上 就可以看到api嚕

給自己憑證
dotnet dev-certs https --trust