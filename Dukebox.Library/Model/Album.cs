using Dukebox.Library.Helper;

namespace Dukebox.Library.Model
{
    public class Album
    {
        public string Name { get; private set; }
        public string FileNameSafeName
        {
            get
            {
                return StringToFilenameConverter.ConvertStringToValidFileName(Name);
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
