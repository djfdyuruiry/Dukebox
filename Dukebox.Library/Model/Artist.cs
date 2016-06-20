namespace Dukebox.Library.Model
{
    public class Artist
    {
        public string Name { get; private set; }

        public Artist(string name)
        {
            Name = string.IsNullOrEmpty(name) ? "Unknown Artist" : name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
