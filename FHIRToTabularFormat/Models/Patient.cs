namespace FHIRToTabularFormat.Models
{
    public class Patient
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Prefix { get; set; }

        // There may be more than one contact option, but there will always be at least one. 
        public List<Dictionary<string, string>> Telecom { get; set; } = new List<Dictionary<string, string>>()
        {
            new Dictionary<string, string>()
            {
                { "system", null},
                { "value", null},
                { "use", null}
            }
        };

        public string Gender { get; set; }
        // year, month, day
        public DateTime DOB { get; set; } 
        // year, month, day, hour, min, seconds, UTC timezone
        public DateTime DeceasedDateTime { get; set; }

        public List<Dictionary<string, string>> Address { get; set; } = new List<Dictionary<string, string>>()
        {
            new Dictionary<string, string>()
            {
                { "line", null}, // can be more than one, hence need to be addressed properly
                { "city", null},
                { "state", null},
                { "country", null}
            }
        };

        public char MaritalStatus { get; set; }

        public bool MultipleBirth { get; set; }

        // List of languages 
        public List<string> Communication { get; set; } = new List<string>();

    }
}
