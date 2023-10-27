using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using SearchEngine;

// TODO: переименуй с учетом, что речь идёт не о "песнях", а о "текстах" или "заметках"
// 1.    [+] нейминг в программе
// 2.    [+] async-await в JS

// ==== Рефакторинг в отпуск 8-21-2023 ============
// 3.   [+] menu router в JS: ветка [react-router]:
//      [+] разобраться и добавить react-router вместо компонента Menu
//      [+] добавить передачу параметров через path из catalog и create вместо "прокидывания через компонент"
//      [+] net6 > net7, sdk:7
//      [+] добавить дамп на каждый новый текст в зависимости от json-настроек

// ==== Не успел ======================================================================================
//      [ ] разобраться с проектом Rsse.Front: нужен ли .NET-проект, если полезная нагрузка в нём только JS
//      [ ] переименовать папки и проекты: почему до сих пор Rsse если проект TagIT

// ===== План продолжения рефакторинга =======================================================
// TODO: почистить фронт от закомментированного кода (рядом с редиректом), также почистить бэк
// 4.    [ ] нейминг в DTO и сущностях бд (миграция?)
// 5.    [~] тесты
// 6.    [ ] дополни dockerfile чтобы фронт самому не билдить. [-] по ходу FROM: node.js - разберись в целесообразности
// 7.    [~] добавь готовый editorconfig

// 8.    [ ] EPIC: перейди на pg, интегрируйся с jira и ci/cd (реши, где запускать раннеры и что делать с артефактами)
// 9.    [ ] во фронте удалить: menu.tsx - program/startup - поправить index.tsx

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseStartup<Startup>();
        webBuilder.UseWebRoot("ClientApp/build");
        webBuilder.UseKestrel(options =>
        {
            // defaults:
            // MinRequestBodyDataRate = 240b/s grace 00:00:05
            // MinResponseDataRate = 240b/s grace 00:00:05
            // MaxConcurrentConnections = null

            var kestrelLimits = options.Limits;

            kestrelLimits.MinResponseDataRate = new MinDataRate(100, TimeSpan.FromSeconds(5));

            kestrelLimits.MinRequestBodyDataRate = new MinDataRate(100, TimeSpan.FromSeconds(5));

            // options.Listen(new IPEndPoint(new IPAddress(new byte[]{127,0,0,1}), 5000));
        });
    });

var app = builder.Build();

try
{
    app.Run();
}
catch (InvalidOperationException ex)
{
    throw new Exception("[DB FAILURE] turn on database server please", ex);
}
