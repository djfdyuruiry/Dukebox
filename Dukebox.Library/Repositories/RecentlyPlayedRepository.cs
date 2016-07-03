using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Dukebox.Library.Interfaces;

namespace Dukebox.Library.Repositories
{
    public class RecentlyPlayedRepository : IRecentlyPlayedRepository
    {
        public event EventHandler<NotifyCollectionChangedEventArgs> RecentlyPlayedListModified;

        public ObservableCollection<ITrack> RecentlyPlayed { get; private set; }

        public List<ITrack> RecentlyPlayedAsList
        {
            get
            {
                return RecentlyPlayed.ToList();
            }
        }

        public RecentlyPlayedRepository()
        {
            RecentlyPlayed = new ObservableCollection<ITrack>();
            RecentlyPlayed.CollectionChanged += (o, e) => Task.Run(() => RecentlyPlayedListModified?.Invoke(this, e));
        }
    }
}
