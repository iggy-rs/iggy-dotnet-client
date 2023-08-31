using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Iggy_SDK.Contracts.Http;
using Iggy_SDK.Exceptions;
using Iggy_SDK.JsonConfiguration;

namespace Iggy_SDK.MessagesDispatcher;

internal sealed class HttpMessageInvoker : MessageInvoker
{
	private readonly HttpClient _client;
    private readonly JsonSerializerOptions _toSnakeCaseOptions;

	public HttpMessageInvoker(HttpClient client)
	{
		_client = client;
        _toSnakeCaseOptions = new();
        
        _toSnakeCaseOptions.PropertyNamingPolicy = new ToSnakeCaseNamingPolicy();
        _toSnakeCaseOptions.WriteIndented = true;

        //This code makes the source generated JsonSerializer work with JsonIgnore attribute for required properties
        _toSnakeCaseOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers =
            {
                ti =>
                {
                    if (ti.Kind == JsonTypeInfoKind.Object)
                    {
                        JsonPropertyInfo[] props = ti.Properties
                            .Where(prop => prop.AttributeProvider == null || prop.AttributeProvider
                                .GetCustomAttributes(typeof(JsonIgnoreAttribute), false).Length == 0)
                            .ToArray();

                        if (props.Length != ti.Properties.Count)
                        {
                            ti.Properties.Clear();
                            foreach (var prop in props)
                            {
                                ti.Properties.Add(prop);
                            }
                        }
                    }
                }
            }
        };
        
        _toSnakeCaseOptions.Converters.Add(new UInt128Converter());
        _toSnakeCaseOptions.Converters.Add(new JsonStringEnumConverter(new ToSnakeCaseNamingPolicy()));
	}
	internal override async Task SendMessagesAsync(MessageSendRequest request, CancellationToken token = default)
	{
        var json = JsonSerializer.Serialize(request, _toSnakeCaseOptions);
        var data = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _client.PostAsync($"/streams/{request.StreamId}/topics/{request.TopicId}/messages", data, token);
        if (!response.IsSuccessStatusCode)
        {
            await HandleResponseAsync(response);
            throw new Exception("Unknown error occurred.");
        }
	}
    private static async Task HandleResponseAsync(HttpResponseMessage response)
    {
        if ((int)response.StatusCode > 300 && (int)response.StatusCode < 500)
        {
            var err = await response.Content.ReadAsStringAsync();
            throw new InvalidResponseException(err);
        }
        if (response.StatusCode == HttpStatusCode.InternalServerError)
        {
            throw new Exception("HTTP Internal server error");
        }
    }
}