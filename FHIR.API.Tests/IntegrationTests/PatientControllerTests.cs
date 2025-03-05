using Hl7.Fhir.Model;
using System.Diagnostics;
using System.Net;

namespace FHIR.API.Tests.IntegrationTests;

/*
	Refer https://code-maze.com/dotnet-test-rest-api-xunit/ for examples
*/
public class PatientControllerTests: BaseTestsController
{
	[Fact]
	public async System.Threading.Tasks.Task GivenARequest_WhenCallingAddPatient_ThenSuccessful()
	{
		#region INSERT
		// Arrange.
		var expectedStatusCode = HttpStatusCode.Created;
		var patient = new Models.Patient
		{
			FirstName = "Tushar" + DateTime.Now.ToString(),
			MiddleName = "G",
			LastName = "Doe",
			BirthDate = DateOnly.Parse("1980-06-25"),
			Gender = AdministrativeGender.Male,
			Email = "tushar.doe@exampled.com"
		};
		var stopwatch = Stopwatch.StartNew();

		// Act.
		var api = "api/patient";
		var response = await _httpClient.PostAsync(api, GetJsonStringContent(patient));
		var time = stopwatch.ElapsedMilliseconds;
		var id = await GetContent(response);

		// Assert.
		AssertTextResponseAsync(stopwatch, response, expectedStatusCode, () => id! != default);
		Console.WriteLine($"Post {api} - {time} ms");
		#endregion

		#region READ
		// Arrange.
		expectedStatusCode = HttpStatusCode.OK;
		stopwatch.Restart();

		// Act.
		var getAPI = $"{api}/{id}";
		response = await _httpClient.GetAsync(getAPI);
		time = stopwatch.ElapsedMilliseconds;
		var newPatient = await GetContent<Models.Patient>(response);

		// Assert.
		AssertJsonResponseAsync(stopwatch, response, expectedStatusCode, () => newPatient!.FirstName.StartsWith("Tushar"));
		Console.WriteLine($"Get {getAPI} - {time} ms");
		#endregion

		#region UPDATE
		// Arrange.
		expectedStatusCode = HttpStatusCode.OK;
		patient.Id = id;

		var address = new Models.Address();
		address.Line1 = "123 Someplace Dr.";
		address.City = "Georgetown";
		address.PostalCode = "QAZ234";
		patient.Address = address;

		stopwatch.Restart();

		// Act.
		var updated = await _httpClient.PutAsync(api, GetJsonStringContent(patient));
		time = stopwatch.ElapsedMilliseconds;
		var versionId = await GetContent(updated);

		// Assert.
		AssertTextResponseAsync(stopwatch, updated, expectedStatusCode, () => versionId! != default);
		Console.WriteLine($"Put {api} - {time} ms");
		#endregion

		#region LIST
		// Arrange.
		expectedStatusCode = HttpStatusCode.OK;
		stopwatch.Restart();

		// Act.
		response = await _httpClient.GetAsync(api);
		time = stopwatch.ElapsedMilliseconds;
		var list = await GetContent<List<Models.Patient>>(response);

		// Assert.
		AssertJsonResponseAsync(stopwatch, response, expectedStatusCode, () => list!.Any());
		Console.WriteLine($"Get {getAPI} - {time} ms");
		#endregion

		#region LIST_WITH_MAXCOUNT
		// Arrange.
		expectedStatusCode = HttpStatusCode.OK;
		stopwatch.Restart();

		// Act.
		var maxCountApi = $"{api}?maxCount=30";
		response = await _httpClient.GetAsync(maxCountApi);
		time = stopwatch.ElapsedMilliseconds;
		list = await GetContent<List<Models.Patient>>(response);

		// Assert.
		AssertJsonResponseAsync(stopwatch, response, expectedStatusCode, () => list!.Count() == 30);
		Console.WriteLine($"Get {maxCountApi} - {time} ms");
		#endregion

		#region LIST_WITH_FILTER
		// Arrange.
		expectedStatusCode = HttpStatusCode.OK;
		stopwatch.Restart();

		// Act.
		var filterAPI = $"{api}?namefilter=tus";
		response = await _httpClient.GetAsync(filterAPI);
		time = stopwatch.ElapsedMilliseconds;
		list = await GetContent<List<Models.Patient>>(response);

		// Assert.
		AssertJsonResponseAsync(stopwatch, response, expectedStatusCode, () => list!.Count() >= 1);
		Console.WriteLine($"Get {filterAPI} - {time} ms");
		#endregion

		#region LIST WITH FILTER AND 0 RESULTS
		// Arrange.
		expectedStatusCode = HttpStatusCode.NoContent;
		stopwatch.Restart();

		// Act.
		filterAPI = $"{api}?namefilter=ldy";
		response = await _httpClient.GetAsync(filterAPI);
		time = stopwatch.ElapsedMilliseconds;

		// Assert.
		AssertJsonResponseAsync(stopwatch, response, expectedStatusCode);
		Console.WriteLine($"Get {filterAPI} - {time} ms");
		#endregion

		#region DELETE
		// Arrange.
		expectedStatusCode = HttpStatusCode.OK;
		stopwatch.Restart();

		// Act.
		response = await _httpClient.DeleteAsync(getAPI);
		time = stopwatch.ElapsedMilliseconds;

		// Assert.
		AssertJsonResponseAsync(stopwatch, response, expectedStatusCode);
		Console.WriteLine($"Delete {getAPI} - {time} ms");
		#endregion

		#region LIST AFTER DELETE
		// Arrange.
		expectedStatusCode = HttpStatusCode.NoContent;
		stopwatch.Restart();

		// Act.
		response = await _httpClient.GetAsync(getAPI);
		time = stopwatch.ElapsedMilliseconds;
		
		// Assert.
		AssertJsonResponseAsync(stopwatch, response, expectedStatusCode);
		Console.WriteLine($"Get after Delete {getAPI} - {time} ms");
		#endregion
	}
}