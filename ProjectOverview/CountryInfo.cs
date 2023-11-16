namespace ProjectOverview
{
    public class CountryInfo
    {
        public string CommonName { get; set; }=string.Empty;
        public string Capital { get; set; }=string.Empty;
        public List<string> Borders { get; set; }= new List<string>();
    }
}
