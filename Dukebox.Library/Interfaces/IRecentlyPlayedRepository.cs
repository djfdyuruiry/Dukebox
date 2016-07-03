using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Dukebox.Library.Interfaces
{
    public interface IRecentlyPlayedRepository
    {
        event EventHandler<NotifyCollectionChangedEventArgs> RecentlyPlayedListModified;
        ObservableCollection<ITrack> RecentlyPlayed { get; }
        List<ITrack> RecentlyPlayedAsList { get; }
    }
}
