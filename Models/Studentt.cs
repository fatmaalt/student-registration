namespace student_registration.Models
{
    public class Studentt
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DoB { get; set; }
        public EducationalLevel EducationLevel { get; set; }
        public string Specialization { get; set; }
        public string UniversityName { get; set; }
        public User? User { get; set; }
    }
}