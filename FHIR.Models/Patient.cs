using Hl7.Fhir.Model;

namespace FHIR.Models;

public class Patient
{
	public string? Id { get; set; }
	public string FirstName { get; set; }
	public string MiddleName { get; set; }
	public string LastName { get; set; }
	public DateOnly BirthDate { get; set; }
	public AdministrativeGender? Gender { get; set; }
	public string Email { get; set; }
	public Address? Address { get; set; }
}
