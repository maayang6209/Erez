using EntitiesManager.DM;
using EntitiesManager;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        EntitiesManager.EntitiesManager manager = InitEntitiesManager();

        var app = builder.Build();

        app.MapPost("merege/{entityType}", async context =>
        {
            try
            {
                string entityType = context.Request.RouteValues["entityType"].ToString();

                var reader = new StreamReader(context.Request.Body);
                var json = JObject.Parse(await reader.ReadToEndAsync());
                Dictionary<string, JObject> formatBody = json.ToObject<Dictionary<string, JObject>>();

                var result = manager.MergeResults(entityType, formatBody);

                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(result.ToString());
            }
            catch (InvalidOperationException ex)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync(ex.Message);
            }
            catch (InvalidDataException ex)
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync(ex.Message);
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
            }
        });
    }

    private static EntitiesManager.EntitiesManager InitEntitiesManager()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        var templetsFolder = configuration.GetSection("EntitiesTemplets:Folder").Value;
        var templets = Directory.GetFiles(templetsFolder).Select((path) => EntityTemplate.FromJsonFile(path));

        return new EntitiesManager.EntitiesManager(templets);
    }
}