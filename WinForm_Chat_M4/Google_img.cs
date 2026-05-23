using Google.GenAI;
using Google.GenAI.Types;
using DotNetEnv;

public class GoogleImg
{
    private readonly Client _imageClient;
    private readonly string _imgFolder;
    private string Model;

    public GoogleImg(string model = "imagen-4.0-generate-001")
    {
        Env.TraversePath().Load();
        var apiKey = System.Environment.GetEnvironmentVariable("GeminiAPIKey");
        _imageClient = new Client(apiKey: apiKey);
        _imgFolder = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Img");
        Model = model;
    }
    
    public async Task<byte[]> GenerateImageAsync(string prompt, string fileName = "generated.png")
    {
        var options = new GenerateImagesConfig
        {
            NumberOfImages = 1,
            AspectRatio = "1:1",
            OutputMimeType = "image/png"
        };

        var result = await _imageClient.Models.GenerateImagesAsync(Model, prompt, options);
        byte[] imageBytes = result.GeneratedImages.First().Image.ImageBytes;

        string savePath = Path.Combine(_imgFolder, fileName);
        await System.IO.File.WriteAllBytesAsync(savePath, imageBytes);
        Console.WriteLine($"Generated: {Path.GetFullPath(savePath)}");
        return imageBytes;
    }
}
