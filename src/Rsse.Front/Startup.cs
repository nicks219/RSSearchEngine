using System.Diagnostics;
using JavaScriptEngineSwitcher.ChakraCore;
using JavaScriptEngineSwitcher.Extensions.MsDependencyInjection;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using React.AspNet;

namespace RandomSongSearchEngine.Front;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMemoryCache(); // ?
        
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        
        services.AddReact();
        
        services.AddJsEngineSwitcher(options => options.DefaultEngineName = ChakraCoreJsEngine.EngineName).AddChakraCore();
        
        services.AddSpaStaticFiles(configuration =>
        {
            configuration.RootPath = "ClientApp/build";
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory logger)
    {
        // переключение между indexMin, indexJsx (и razor pages если будут)
        // var options = new DefaultFilesOptions();
        // options.DefaultFileNames.Clear();
        // options.DefaultFileNames.Add(Configuration.GetValue<string>("StartFile"));
        // app.UseDefaultFiles(options);
        // app.UseReact(config => { }); // еще такое есть, но тут не нужно
        
        app.UseSpaStaticFiles();
        
        // app.UseRouting();
        
        app.UseSpa(spa =>
        {
            spa.Options.SourcePath = "ClientApp";

            if (env.IsDevelopment())
            {
                spa.UseReactDevelopmentServer(npmScript: "start");
            }
            else
            {
                // скрипт из package.json (npm run build) построил билд, но не смог запустить kestrel, свалился
                // if (Directory.Exists("")) ;
                // spa.UseReactDevelopmentServer(npmScript: "build");
            }
        });
    }
    
    private void YarnRunBuild()
    {
        if (!Directory.Exists("./ClientApp/node_modules"))
        {
            // ноду надо ставить до старта - во время не получится, студия не скомпилирует typescript
            // перенесу в скрипт BuildEvents
            // yarn add --dev @types/react
            // string strCmdText = "/C cd ./ClientApp && npm install";
            // Process cmd = Process.Start("CMD.exe", strCmdText);
            // cmd.WaitForExit();
        }

        if (Directory.Exists("./ClientApp/build"))
        {
            return;
        }
        
        // yarn add --dev @types/react
            
        const string strCmdText = "/C cd ./ClientApp && npm run build";
            
        // using {Process}
            
        var cmd = Process.Start("CMD.exe", strCmdText);
            
        cmd.WaitForExit();
            
        // ProcessStartInfo cmdsi = new ProcessStartInfo("cmd.exe");
        // String command = @"/k java -jar myJava.jar";
        // cmdsi.Arguments = command;
        // Process cmd = Process.Start(cmdsi);
        // cmd.WaitForExit();
    }
}