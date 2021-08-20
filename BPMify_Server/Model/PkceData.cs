namespace BPMify_Server.Model
{
    public class PkceData
    {
        public string Verifier { get; set; }
        public string Challenge { get; set; }
        public string RefreshToken { get; set; }

    }
}
