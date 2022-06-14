// в папке ClientApp - npm и tsx, всё остальное можно удалить
// билд из папок public + src + конфиги в корне => в папку build (при деплое с помощью Dockerfile):
// npm install && npm run build && npm start
// node -v v16.14.2
// npm -v 8.5.0
// на старте использовался шаблон: MS Template .NET Core 3.1 SPA using: Microsoft.AspNetCore.SpaServices.Extensions 3.1.16

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();