using System;
using static System.Net.Mime.MediaTypeNames;
using System.IO;

using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

List<MyImage> images = new List<MyImage>();

String fileWithImages = "data/images.txt";

app.Run(async (context) =>
{    
    var response = context.Response;
    var request = context.Request;
    var path = request.Path;
   
    if (path == "/api/images" && request.Method == "GET")
    {
        string jsonString = getFileInformation(fileWithImages);        
       
        await response.WriteAsync(jsonString);
    }
    else if (path == "/api/images" && request.Method == "POST")
    {
        await CreateImage(response, request); 
    }    
    else if (request.Method == "PUT")
    {
        await UpdateImage(response, request);
    }
    else if (request.Method == "DELETE")
    {
        await DeleteImage(response, request);
    }
    else
    {        
        await response.WriteAsync("Server");
    }
});

app.Run();

async Task CreateImage(HttpResponse response, HttpRequest request)
{
    try
    {        
        MyImage? newImage = await request.ReadFromJsonAsync<MyImage>();        

        if (newImage != null)
        {            
            string jsonString = getFileInformation(fileWithImages);

            String id = Guid.NewGuid().ToString();
            newImage.Id = id;

            if (jsonString != null)
            {
                images = JsonSerializer.Deserialize<List<MyImage>>(jsonString)!;
            }
            else
            {
                images = new List<MyImage>();
            }                        

            images.Add(newImage);

            updateFileInformation(images, fileWithImages);

            await response.WriteAsync(JsonSerializer.Serialize(newImage));
        }
        else
        {
            throw new Exception("Wrong data!");
        }
    }
    catch (Exception ex)
    {
        response.StatusCode = 400;
        await response.WriteAsync(ex.Message);
    }
}

async Task UpdateImage(HttpResponse response, HttpRequest request)
{
    try
    {        
        MyImage? imageToUpdate = await request.ReadFromJsonAsync<MyImage>();

        if (imageToUpdate != null)
        {
            string jsonString = getFileInformation(fileWithImages);

            images = JsonSerializer.Deserialize<List<MyImage>>(jsonString)!;

            bool wasImageUpdated = false;

            for (int i = 0; i < images.Count; i++)
            {
                MyImage item = images[i];

                if (item.Id == imageToUpdate.Id)
                {
                    item.Name = imageToUpdate.Name;
                    item.Content = imageToUpdate.Content;

                    updateFileInformation(images, fileWithImages);

                    wasImageUpdated = true;
                    await response.WriteAsync(JsonSerializer.Serialize(imageToUpdate));
                    break;
                }
            }            

            if (!wasImageUpdated)
            {
                response.StatusCode = 404;
                await response.WriteAsync("Item was not found!");
            }
        }
        else
        {
            throw new Exception("Wrong data!");
        }
    }
    catch (Exception ex)
    {
        response.StatusCode = 400;
        await response.WriteAsync(ex.Message);
    }
}

async Task DeleteImage(HttpResponse response, HttpRequest request)
{
    try
    {
        var path = request.Path;

        string? id = path.Value?.Split("/")[3];

        string jsonString = getFileInformation(fileWithImages);

        if (jsonString != null)
        {
            images = JsonSerializer.Deserialize<List<MyImage>>(jsonString)!;
            
            MyImage? imageToDelete = images.FirstOrDefault((item) => item.Id == id);

            if (imageToDelete != null)
            {
                images.Remove(imageToDelete);                

                updateFileInformation(images, fileWithImages);

                await response.WriteAsync(imageToDelete.Id);
            }        
            else
            {
                response.StatusCode = 404;
                await response.WriteAsync("Image was not found!");
            }
        }
    }
    catch (Exception ex)
    {
        response.StatusCode = 400;
        await response.WriteAsync(ex.Message);
    }
}

string getFileInformation(string path)
{    
    StreamReader sr = new StreamReader(path);
    string jsonString = sr.ReadLine();
    sr.Close();
    return jsonString;
}

void updateFileInformation(List<MyImage> images, string path)
{
    string jsonString = JsonSerializer.Serialize(images);

    StreamWriter sw = new StreamWriter(path, false, Encoding.ASCII);
    sw.Write(jsonString);
    sw.Close();
}

public class MyImage
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Content { get; set; } = "";
}