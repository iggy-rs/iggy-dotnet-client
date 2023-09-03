using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Iggy_SDK_Tests.Utils.Errors;
using Iggy_SDK_Tests.Utils.Messages;
using Iggy_SDK.Exceptions;
using Iggy_SDK.JsonConfiguration;
using RichardSzalay.MockHttp;

namespace Iggy_SDK_Tests.InvokerTests;

//TODO - legacy remove when E2E tests are done.
public sealed class HttpMessageInvoker
{
	private readonly Iggy_SDK.MessagesDispatcher.HttpMessageInvoker _sut;
	private readonly MockHttpMessageHandler _httpHandler;
	private readonly JsonSerializerOptions _toSnakeCaseOptions;
	
	private const string URL = "http://localhost:3000";

	public HttpMessageInvoker()
	{

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
		_httpHandler = new MockHttpMessageHandler();
		var client = _httpHandler.ToHttpClient();
		client.BaseAddress = new Uri(URL);
		_sut = new(client);
	}
	[Fact]
	public async Task SendMessageAsync_ThrowsErrorResponseException_OnFailure()
	{
		var request = MessageFactory.CreateMessageSendRequest();		
		var json = JsonSerializer.Serialize(request, _toSnakeCaseOptions); 
		var error = JsonSerializer.Serialize(ErrorModelFactory.CreateErrorModelBadRequest(), _toSnakeCaseOptions);

		
		_httpHandler.When(HttpMethod.Post, $"/streams/{request.StreamId}/topics/{request.TopicId}/messages")
			.With(message =>
			{
				message.Content = new StringContent(json, Encoding.UTF8, "application/json");
				return true;
			})
			.Respond(HttpStatusCode.BadRequest, "application/json", error);
		
		await Assert.ThrowsAsync<InvalidResponseException>( async () => await _sut.SendMessagesAsync(request));
		_httpHandler.Flush();
	} 
	
}