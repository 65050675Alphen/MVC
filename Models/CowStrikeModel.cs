namespace CowStrike2_MVC_65050675.Models
{
    public class Cow
    {
        public string id { get; set; }
        public string breed { get; set; }
        public Age age { get; set; }
        public int? BottleCount { get; set; }
        public bool HasEatenLemon { get; set; } // Track if cow has eaten lemon
        public bool IsBSOD { get; set; }        // วัว BSOD
        public string Color { get; set; }       // สีของวัว (White, Brown, หรือ Blue เมื่อเป็น BSOD)
    }

    // โมเดลสำหรับอายุวัว
    public class Age
    {
        public int years { get; set; }
        public int months { get; set; }
    }

    public class Request
    {
        public string CowId { get; set; }
    }
}
