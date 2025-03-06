using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace FHIR.API.Tests.IntegrationTests;

public class BaseTestsController
{
	protected readonly HttpClient _httpClient = new() { BaseAddress = new Uri("http://localhost:5157") };

	private const string _jsonMediaType = "application/json";
	private const string _textMediaType = "text/plain";
	private const int _expectedMaxElapsedMilliseconds = 3000;
	private readonly JsonSerializerOptions _jsonSerializerOptions = new() { 
		PropertyNameCaseInsensitive = true,
		// WriteIndented = true,
	};

	protected void AssertJsonResponseAsync(Stopwatch stopwatch, HttpResponseMessage response, System.Net.HttpStatusCode expectedStatusCode, Func<bool>? verify = default)
	{
		AssertCommonResponseParts(stopwatch, response, expectedStatusCode, _jsonMediaType, verify);
	}

	protected void AssertTextResponseAsync(Stopwatch stopwatch, HttpResponseMessage response, System.Net.HttpStatusCode expectedStatusCode, Func<bool>? verify = default) {
		AssertCommonResponseParts(stopwatch, response, expectedStatusCode, _textMediaType, verify);
	}

	private static void AssertCommonResponseParts(Stopwatch stopwatch, HttpResponseMessage response, System.Net.HttpStatusCode expectedStatusCode, string mediaType, Func<bool>? verify)
	{
		Assert.Equal(expectedStatusCode, response.StatusCode);
		Assert.True(stopwatch.ElapsedMilliseconds < _expectedMaxElapsedMilliseconds);
		if (response.Content.Headers.ContentType != null)	Assert.Equal(mediaType, response.Content.Headers.ContentType.MediaType);
		if (verify != default) {
			Assert.True(verify());
		}
	}

	protected StringContent GetJsonStringContent<T>(T model) where T: class => new(JsonSerializer.Serialize(model), Encoding.UTF8, _jsonMediaType);

	protected async Task<T?> GetContent<T>(HttpResponseMessage response)
	{
		return await JsonSerializer.DeserializeAsync<T?>(await response.Content.ReadAsStreamAsync().ConfigureAwait(false), _jsonSerializerOptions).ConfigureAwait(false);
	}

	protected async Task<string> GetContent(HttpResponseMessage response) {
		return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
	}
}
