namespace Dukebox.Library
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class Library : DbContext
    {
        public Library()
            : base("name=Library")
        {
        }

        public virtual DbSet<album> albums { get; set; }
        public virtual DbSet<artist> artists { get; set; }
        public virtual DbSet<playlist> playlists { get; set; }
        public virtual DbSet<song> songs { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
