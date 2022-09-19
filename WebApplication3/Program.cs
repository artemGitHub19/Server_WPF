using System;
using static System.Net.Mime.MediaTypeNames;
using System.IO;

using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

List<MyImage> images = new List<MyImage>();

app.Run(async (context) =>
{    
    var response = context.Response;
    var request = context.Request;
    var path = request.Path;
   
    if (path == "/api/images" && request.Method == "GET")
    {
        StreamReader sr = new StreamReader("data/images.txt");       
        string jsonString = sr.ReadLine();
        sr.Close();

        if (jsonString != null)
        {
            images = JsonSerializer.Deserialize<List<MyImage>>(jsonString)!;

            images.Sort((x, y) => x.Id.CompareTo(y.Id));

            for (int i = 0; i < images.Count; i++)
            {
                var image = images[i];
                image.Id = i + 1;
            }

            jsonString = JsonSerializer.Serialize(images);

            StreamWriter sw = new StreamWriter("data/images.txt", false, Encoding.ASCII);
            sw.Write(jsonString);
            sw.Close();
        }        
       
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
        response.StatusCode = 400;
        await response.WriteAsync("Wrong data!");
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
            string fileName = "data/images.txt";
            StreamReader sr = new StreamReader(fileName);
            string? jsonString = sr.ReadLine();
            sr.Close();

            if (jsonString != null)
            {
                images = JsonSerializer.Deserialize<List<MyImage>>(jsonString)!;

                images.Sort((x, y) => x.Id.CompareTo(y.Id));

                newImage.Id = images[images.Count - 1].Id + 1;
            }
            else
            {
                images = new List<MyImage>();
                newImage.Id = 1;
            }                        

            images.Add(newImage);

            jsonString = JsonSerializer.Serialize(images);

            StreamWriter sw = new StreamWriter(fileName, false, Encoding.ASCII);
            sw.Write(jsonString);
            sw.Close();

            await response.WriteAsJsonAsync(newImage.Id);
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
            string fileName = "data/images.txt";
            StreamReader sr = new StreamReader(fileName);
            string jsonString = sr.ReadLine();
            sr.Close();

            images = JsonSerializer.Deserialize<List<MyImage>>(jsonString)!;

            bool wasImageUpdated = false;

            for (int i = 0; i < images.Count; i++)
            {
                MyImage item = images[i];

                if (item.Id == imageToUpdate.Id)
                {
                    item.Name = imageToUpdate.Name;
                    item.Content = imageToUpdate.Content;

                    jsonString = JsonSerializer.Serialize(images);

                    StreamWriter sw = new StreamWriter(fileName, false, Encoding.ASCII);
                    sw.Write(jsonString);
                    sw.Close();

                    wasImageUpdated = true;
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

        string? stringId = path.Value?.Split("/")[3];

        int id = int.Parse(stringId);

        string fileName = "data/images.txt";
        StreamReader sr = new StreamReader(fileName);
        string jsonString = sr.ReadLine();
        sr.Close();

        if (jsonString != null)
        {
            images = JsonSerializer.Deserialize<List<MyImage>>(jsonString)!;
            
            MyImage? imageToDelete = images.FirstOrDefault((item) => item.Id == id);

            if (imageToDelete != null)
            {
                images.Remove(imageToDelete);

                if (images.Count == 0)
                {
                    jsonString = "";
                }
                else
                {
                    jsonString = JsonSerializer.Serialize(images);
                }            

                StreamWriter sw = new StreamWriter(fileName, false, Encoding.ASCII);
                sw.Write(jsonString);
                sw.Close();
            
                await response.WriteAsync(imageToDelete.Name);
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

public class MyImage
{
    public int Id { get; set; } = 0;
    public string Name { get; set; } = "";
    public string Content { get; set; } = "";
}