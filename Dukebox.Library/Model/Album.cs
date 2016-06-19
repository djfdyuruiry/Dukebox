using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;

namespace Dukebox.Library.Model
{
    [Table("albums")]
    public class Album
    {
        private string _name;

        public event EventHandler NameUpdated;

        public Album()
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

        [Column("hasAlbumArt")]
        public int HasAlbumArtBit { get; set; }

        public virtual ICollection<Song> Songs { get; set; }
        public bool HasAlbumArt
        {
            get
            {
                return HasAlbumArtBit == 0 ? false : true;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
