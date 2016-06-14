using System;

namespace Dukebox.Library.Model
{
    public class GenericEventArgs<T> : EventArgs
    {
        public T Data { get; set; }

        public GenericEventArgs() : base()
        {
        }
    }
}
