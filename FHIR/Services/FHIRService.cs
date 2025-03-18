using FHIR.Services.Interfaces;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Utility;

// Sample code from: https://github.com/FirelyTeam/fhirstarters/tree/master/dotnet

namespace FHIR.Services;

public class FHIRService : IFHIRService
{
	private FhirClient _client;

	const string _identifierSystem = "http://app.winwire.example/tushar";

	public FHIRService()
	{
		// Choose your preferred FHIR server or add your own
		// More at https://confluence.hl7.org/display/FHIR/Public+Test+Servers

		FhirClientSettings settings = new FhirClientSettings();
		settings.PreferredParameterHandling = SearchParameterHandling.Strict;
		settings.PreferredFormat = ResourceFormat.Json;

		_client = new FhirClient("https://server.fire.ly/R5", settings);
		// _client = new FhirClient("http://hapi.fhir.org/baseR5", settings);
	}

	public async Task<string> AddPatient(Models.Patient patient)
	{
		var pat = ToFHIRPatient(patient);
		var createdPat = await _client.CreateAsync(pat);
		if (createdPat == default) throw new NullReferenceException(nameof(createdPat));

		return createdPat.Id;
	}

	public async Task<string> UpdatePatient(Models.Patient patient)
	{
		var pat = ToFHIRPatient(patient);
		var updatedPatient = await _client.UpdateAsync(pat);
		if (updatedPatient == default) throw new NullReferenceException(nameof(updatedPatient));

		return updatedPatient.Meta.VersionId;
	}

	private Patient ToFHIRPatient(Models.Patient patient)
	{
		if (patient == null) throw new ArgumentNullException(nameof(patient));

		Patient pat = new Patient();
		pat.Id = patient.Id;
		pat.Identifier.Add(new Identifier(_identifierSystem, DateTime.Now.Ticks.ToString()));
		pat.Gender = patient.Gender;

		// pat.Name.Add(new HumanName().WithGiven(patient.FirstName).WithGiven(patient.MiddleName).AndFamily(patient.LastName));
		if (patient.FirstName != null)
		{
			pat.Name.Add(new HumanName().WithGiven(patient.FirstName));
		}
		if (patient.MiddleName != null)
		{
			pat.Name.Add(new HumanName().WithGiven(patient.MiddleName));
		}
		if (patient.LastName != null)
		{
			pat.Name.Add(new HumanName().AndFamily(patient.LastName));
		}

		if (patient.BirthDate != DateOnly.MinValue)
		{
			pat.BirthDate = patient.BirthDate.ToString("yyyy-MM-dd");
		}

		if (patient.Address != default)
		{
			var lines = new List<string>();
			lines.Add(patient.Address.Line1);
			if (!string.IsNullOrEmpty(patient.Address.Line2)) lines.Add(patient.Address.Line2);
			if (!string.IsNullOrEmpty(patient.Address.Line3)) lines.Add(patient.Address.Line3);

			var address = new Address();
			address.Line = lines.ToArray();
			address.City = patient.Address.City;
			address.State = patient.Address.State;
			address.Country = patient.Address.Country;
			address.PostalCode = patient.Address.PostalCode;

			pat.Address.Add(address);
		}

		return pat;
	}

	private Models.Patient ToModelsPatient(Patient fihrPatient)
	{
		var patient = new Models.Patient()
		{
			Id = fihrPatient.Id,
			Gender = fihrPatient.Gender,
		};

		if (DateOnly.TryParse(fihrPatient.BirthDate, out DateOnly birthDate)) {
			patient.BirthDate = birthDate;
		}

		var name = fihrPatient.Name.FirstOrDefault(n => n.Use == HumanName.NameUse.Usual) ?? fihrPatient.Name.FirstOrDefault(n => n.Use == HumanName.NameUse.Official) ?? fihrPatient.Name.FirstOrDefault();
		if (name != null)
		{
			patient.FirstName = name.Given.First();
			if (name.Given.Count() > 1) patient.MiddleName = name.Given.ElementAt(1);
			patient.LastName = name.Family;
		}

		var address = fihrPatient.Address.FirstOrDefault(a => a.Use == Address.AddressUse.Billing) ?? fihrPatient.Address.FirstOrDefault(a => a.Use == Address.AddressUse.Home) ?? fihrPatient.Address.FirstOrDefault(a => a.Use == Address.AddressUse.Work) ?? fihrPatient.Address.FirstOrDefault();
		if (address != null)
		{
			patient.Address = new Models.Address()
			{
				City = address.City,
				State = address.State,
				Country = address.Country,
				PostalCode = address.PostalCode
			};

			var lineCount = address.Line.Count();
			if (lineCount > 0)	patient.Address.Line1 = address.Line.ElementAt(0);
			if (lineCount > 1)	patient.Address.Line2 = address.Line.ElementAt(1);
			if (lineCount > 2)	patient.Address.Line3 = address.Line.ElementAt(2);
		}

		return patient;
	}

	public async System.Threading.Tasks.Task DeletePatient(string id)
	{
		var searchParams = new SearchParams().Where($"_id={id}");
		await _client.ConditionalDeleteSingleAsync(searchParams, resourceType: ResourceType.Patient.GetLiteral());
	}

	public async Task<IEnumerable<Models.Patient>> GetPatients(string nameFilter, int maxCount) {
		var searchParams = new SearchParams().LimitTo(10);
		if(!string.IsNullOrEmpty(nameFilter)) searchParams.Where($"name:contains={nameFilter}");
		return await GetPatients(searchParams, maxCount);
	}

	private async Task<IEnumerable<Models.Patient>> GetPatients(SearchParams searchParams, int maxCount) {
		var results = await _client.SearchAsync<Patient>(searchParams);

		var patients = new List<Models.Patient>();

		// This time continue asking for the next bundle while there are more results on the server
		while (results != null) {
			if (results.Entry == null) return patients;

			foreach (var entry in results.Entry) {
				if (entry.Resource is Patient)
					patients.Add(ToModelsPatient((Patient)entry.Resource));
				else
					Console.WriteLine($"Found unexpected fihrPatient type: {entry.Resource.TypeName}'");
			}

			// get the next page of results
			if(patients.Count >= maxCount) break;

			results = await _client.ContinueAsync(results);
		}

		return patients;
	}

	public async Task<Models.Patient?> GetPatient(string id) {
		var searchParams = new SearchParams().Where($"_id={id}");
		var patients = await GetPatients(searchParams, 1);
		var patient = patients.FirstOrDefault();
		return patient;
	}
}
