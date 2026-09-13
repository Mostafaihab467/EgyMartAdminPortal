using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Json;

public class ImageService
{
    private readonly HttpClient _httpClient;

    public ImageService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> UploadImageAsync(IBrowserFile file)
    {
        if (file == null)
            throw new ArgumentNullException(nameof(file));

        string[] allowedExtensions = { ".png", ".jpg", ".jpeg" };
        string fileExtension = Path.GetExtension(file.Name).ToLower();

        if (!allowedExtensions.Contains(fileExtension))
            throw new InvalidOperationException("Invalid file type. Please upload a .png, .jpg, or .jpeg file.");

        using var content = new MultipartFormDataContent();
        using var stream = file.OpenReadStream(maxAllowedSize: 25 * 1024 * 1024); // 25MB limit
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);

        content.Add(new StreamContent(memoryStream), "file", file.Name);

        var response = await _httpClient.PostAsync("api/FileUpload/upload", content);

        if (!response.IsSuccessStatusCode)
            throw new Exception("Failed to upload image.");

        var result = await response.Content.ReadFromJsonAsync<string>();

        return result ?? string.Empty;
    }
}
