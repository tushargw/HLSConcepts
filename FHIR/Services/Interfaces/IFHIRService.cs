using FHIR.Models;

namespace FHIR.Services.Interfaces;

public interface IFHIRService
{
	Task<string> AddPatient(Patient patient);
	Task DeletePatient(string id);
	Task<Patient?> GetPatient(string id);
	Task<IEnumerable<Patient>> GetPatients(string nameFilter, int maxCount);
	Task<string> UpdatePatient(Patient patient);
}
