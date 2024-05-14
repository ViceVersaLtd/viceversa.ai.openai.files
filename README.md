# viceversa.ai.openai.files
Wrapper for the OpenAI file endpoints

## Usage
To upload a file specify the file path. it returns a File response object as defined in the [Microsoft Learn documentation](https://learn.microsoft.com/en-us/rest/api/azureopenai/files/upload?view=rest-azureopenai-2024-03-01-preview&tabs=HTTP#file) 
```
public async Task<string> FileUpload(string filePath)
{
    string resVal = "";
    Files localFile = new Files(AzureEndpointDefault, AzureKeyDefault);
    var uploadResponse = await localFile.UploadAsync(Files.Purpose.assistants, filePath);
    if (uploadResponse is Files.FileResponse)
    {
        Files.FileResponse fileResponse = (Files.FileResponse)uploadResponse;
        resVal = fileResponse.id;
    }

    if (!string.IsNullOrWhiteSpace(filePath) && System.IO.File.Exists(filePath)) { System.IO.File.Delete(filePath); }
    return resVal;
}
```

The below deletes a file.
```
public async Task<object> FileDelete(string fileID)
{
	bool resVal = false;
	Files localFile = new Files(AzureEndpointDefault, AzureKeyDefault);
	var deleteResponse = await localFile.DeleteAsync(fileID);
	if (deleteResponse is bool) { resVal = true; }
	return resVal;
}
```