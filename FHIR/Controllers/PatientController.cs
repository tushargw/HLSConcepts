using FHIR.Models;
using FHIR.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace FHIR.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PatientController : ControllerBase
{
	private readonly IFHIRService _fhirService;

	public PatientController(IFHIRService fhirService)
	{
		_fhirService = fhirService;
	}

	[HttpGet]
	public async Task<IEnumerable<Patient>> GetPatients(string nameFilter = null, int maxCount = 20)
	{
		var patients = await _fhirService.GetPatients(nameFilter, maxCount).ConfigureAwait(false);
		if (!patients.Any()) return null;

		return patients;
	}

	[HttpGet("{id}")]
	public async Task<Patient?> GetPatient(string id)
	{
		var patient = await _fhirService.GetPatient(id).ConfigureAwait(false);
		return patient;
	}

	[HttpPost]
	public async Task<string> AddPatient(Patient patient)
	{
		var id = await _fhirService.AddPatient(patient).ConfigureAwait(false);
		HttpContext.Response.StatusCode = (int)HttpStatusCode.Created;
		return id;
	}

	[HttpPut]
	public async Task<string> UpdatePatient(Patient patient)
	{
		var versionId = await _fhirService.UpdatePatient(patient).ConfigureAwait(false);
		return versionId;
	}

	[HttpDelete("{id}")]
	public async Task DeletePatient(string id)
	{
		await _fhirService.DeletePatient(id).ConfigureAwait(false);
	}
}
