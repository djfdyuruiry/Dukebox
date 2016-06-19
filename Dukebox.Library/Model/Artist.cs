using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;

namespace Dukebox.Library.Model
{
    [Table("artists")]
    public class Artist
    {
        private string _name;

        public event EventHandler NameUpdated;

        public Artist()
        {
            Songs = new HashSet<Song>();
        }

        [Column("id")]
        public long Id { get; set; }

        [Required]
        [StringLength(2147483647)]
        [Column("name")]
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                UpdateName(value);
            }
        }

        private void UpdateName(string value)
        {
            var newTitle = _name == null || !_name.Equals(value, StringComparison.Ordinal);

            if (newTitle)
            {
                _name = value;

                NameUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        public virtual ICollection<Song> Songs { get; set; }
        public override string ToString()
        {
            return Name;
        }        
    }
}
