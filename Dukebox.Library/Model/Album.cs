using Dukebox.Library.Helper;

namespace Dukebox.Library.Model
{
    public class Album
    {
        public string Name { get; private set; }
        public string Id
        {
            get
            {
                return Md5HashGenerator.GenerateMd5Hash(Name);
            }
        }

        public Album (string name)
        {
            Name = string.IsNullOrEmpty(name) ? "Unknown Album" : name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
