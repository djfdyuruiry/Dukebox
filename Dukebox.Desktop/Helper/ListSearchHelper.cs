using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dukebox.Desktop.Helper
{
    public class ListSearchHelper <T>
    {
        public const string MatchAllString = "*";

        public string SearchFilter { get; set; }
        public Func<T, string, bool> FilterLambda { get; set; }
        public bool SortResults { get; set; }
        public Func<T, string> SortLambda { get; set; }
        public List<T> Items { get; set; }
        public List<T> FilteredItems
        {
            get
            {
                if (string.IsNullOrEmpty(SearchFilter) ||
                    SearchFilter.Equals(MatchAllString) ||
                    FilterLambda == null || 
                    Items == null)
                {
                    return Items;
                }

                var results = Items.Where(i => FilterLambda(i, SearchFilter));

                return SortResults && SortLambda != null ? 
                    results.OrderBy(SortLambda).ToList() : 
                    results.ToList();
            } 
        }
    }
}
