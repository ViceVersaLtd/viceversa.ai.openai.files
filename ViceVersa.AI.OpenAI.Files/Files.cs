using System.Text.Json.Serialization;
using System.Text.Json;
using RestSharp;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System;


namespace ViceVersa.AI.OpenAI
{
    /// <summary>
    /// Wrapper for the Azure OpenAI Files upload. See https://learn.microsoft.com/en-us/rest/api/azureopenai/files
    /// </summary>
    public class Files
    {
        /// <summary>
        /// Supported Cognitive Services endpoints (protocol and hostname, for example: https://aoairesource.openai.azure.com. Replace "aoairesource" with your Azure OpenAI account name).
        /// </summary>
        public static string EndPoint { get; set; }

        /// <summary>
        /// Provide your Cognitive Services Azure OpenAI account key here.
        /// </summary>
        public static string Key { get; set; }

        /// <summary>
        /// The requested API version. Default is 2024-03-01-preview
        /// </summary>
        public static string ApiVersion { get; set; } = "2024-03-01-preview";

        /// <summary>
        /// Default constructer
        /// </summary>
        /// <param name="endPoint">Supported Cognitive Services endpoints (protocol and hostname, for example: https://aoairesource.openai.azure.com. Replace "aoairesource" with your Azure OpenAI account name).</param>
        /// <param name="key">Provide your Cognitive Services Azure OpenAI account key here.</param>
        public Files(string endPoint, string key)
        {
            EndPoint = endPoint;
            Key = key;
        }
        /// <summary>
        /// Default constructer
        /// </summary>
        /// <param name="endPoint">Supported Cognitive Services endpoints (protocol and hostname, for example: https://aoairesource.openai.azure.com. Replace "aoairesource" with your Azure OpenAI account name).</param>
        /// <param name="key">Provide your Cognitive Services Azure OpenAI account key here.</param>
        /// <param name="apiVersion">The requested API version.</param>
        public Files(string endPoint, string key, string apiVersion)
        {
            EndPoint = endPoint;
            Key = key;
            ApiVersion = apiVersion;
        }

        private static readonly int MaxRetries = 1;
        private static readonly JsonSerializerOptions _Jsonoptions =
            new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = { new JsonStringEnumConverter() }
            };

        /// <summary>
        /// The RestSharp client.
        /// </summary>
        private RestClient _client;

        /// <summary>
        /// Creates a new file entity by uploading data from a local machine. Uploaded files can, for example, be used for training or evaluating fine-tuned models.
        /// </summary>
        /// <param name="purpose">The intended purpose of the uploaded documents. Use "fine-tune" for fine-tuning. This allows us to validate the format of the uploaded file.</param>
        /// <param name="file">The file to upload into Azure OpenAI.</param>
        /// <returns>FileResponse on Success, ErrorResponse on Fault</returns>
        public async Task<object> UploadAsync(Purpose purpose, string filePath)
        {
            string _purpose = "fine-tune";
            switch (purpose)
            {
                case Purpose.assistants:
                    _purpose = "assistants";
                    break;
                case Purpose.assistants_output:
                    _purpose = "assistants_output";
                    break;
                case Purpose.fine_tune_results:
                    _purpose = "fine-tune-results";
                    break;
                default:
                    _purpose = "fine-tune";
                    break;
            }

            var options = new RestClientOptions(EndPoint.TrimEnd('/'));

            _client = new RestClient(options);

            _client.AddDefaultHeader("Api-Key", Key);

            var request = new RestRequest("/openai/files", Method.Post)
                .AddQueryParameter("api-version", ApiVersion)
                .AddFile("file", filePath)
                .AddParameter("purpose", _purpose, ParameterType.RequestBody);

            RestResponse restResponse = await _client.PostAsync(request);

            if (restResponse.IsSuccessStatusCode)
            {
                string restContent = restResponse.Content;
                FileResponse result = JsonSerializer.Deserialize<Files.FileResponse>(restContent, _Jsonoptions);

                return result;
            }
            else
            {
                string restContent = restResponse?.Content;
                ErrorResponse errorResponse = JsonSerializer.Deserialize<ErrorResponse>(restContent, _Jsonoptions);
                return errorResponse;
            }

        }


        /// <summary>
        /// Deletes the file with the given file-id. Deletion is also allowed if a file was used, e.g., as training file in a fine-tune job.
        /// </summary>
        /// <param name="fileID">The identifier of the file.</param>
        /// <returns>True on Success, ErrorResponse on Fault</returns>
        public async Task<object> DeleteAsync(string fileID)
        {

            var options = new RestClientOptions(EndPoint.TrimEnd('/'));

            _client = new RestClient(options);

            _client.AddDefaultHeader("Api-Key", Key);

            var request = new RestRequest($"/openai/files/{fileID}", Method.Delete)
                .AddQueryParameter("api-version", ApiVersion);
            //.AddUrlSegment("file-id", fileID);

            //var response = await _client.PostAsync(request);

            RestResponse restResponse = await _client.DeleteAsync(request);

            if (restResponse.IsSuccessStatusCode)
            {
                return true;
            }
            else
            {
                string restContent = restResponse?.Content;
                ErrorResponse errorResponse = JsonSerializer.Deserialize<ErrorResponse>(restContent, _Jsonoptions);
                return errorResponse;
            }

        }

        /// <summary>
        /// assistants - This file contains data to be used in assistants.
        /// assistants_output - This file contains the results of an assistant.
        /// fine-tune - This file contains training data for a fine tune job.
        /// fine-tune-results - This file contains the results of a fine tune job.
        /// </summary>
        public enum Purpose
        {
            assistants,
            assistants_output,
            fine_tune,
            fine_tune_results
        }

        private class Payload
        {
            [JsonPropertyName("file")]
            public Byte[] File { get; set; }
            [JsonPropertyName("purpose")]
            public string Purpose { get; set; }
            public Payload(Byte[] file, string purpose)
            {
                File = file;
                Purpose = purpose;
            }
        }

        public class ErrorResponse
        {
            public ErrorDetail error { get; set; }
        }

        /// <summary>
        /// Error content as defined in the Microsoft REST guidelines (https://github.com/microsoft/api-guidelines/blob/vNext/Guidelines.md#7102-error-condition-responses).
        /// </summary>
        public class ErrorDetail
        {
            public string code { get; set; }
            public List<ErrorDetail> details { get; set; }
            public string message { get; set; }
            public string target { get; set; }
            public InnerError innererror { get; set; }
        }

        public class InnerError
        {
            public string code { get; set; }
            public InnerError innererror { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        public class FileResponse
        {
            /// <summary>
            /// The size of this file when available (can be null). File sizes larger than 2^53-1 are not supported to ensure compatibility with JavaScript integers.
            /// </summary>
            public int bytes { get; set; }
            /// <summary>
            /// A timestamp when this job or item was created (in unix epochs).
            /// </summary>
            public int created_at { get; set; }
            /// <summary>
            /// The name of the file.
            /// </summary>
            public string filename { get; set; }
            /// <summary>
            /// The identity of this item.
            /// </summary>
            public string id { get; set; }
            /// <summary>
            /// TypeDiscriminator. Defines the type of an object.
            /// </summary>
            [JsonPropertyName("object")]
            public string _object { get; set; }
            /// <summary>
            /// The intended purpose of the uploaded documents. Use "fine-tune" for fine-tuning. This allows us to validate the format of the uploaded file.
            /// </summary>
            public string purpose { get; set; }
            /// <summary>
            /// The state of a file.
            /// </summary>
            public string status { get; set; }
            /// <summary>
            /// The error message with details in case processing of this file failed.
            /// </summary>
            public string status_details { get; set; }
        }
    }

}
