using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Runtime;

namespace CK.FileFilter
{

    /// <summary>
    /// A <see cref="FilteredObservableCollection{T}"/> with another name: in a list, the index matters.
    /// </summary>
    /// <typeparam name="T">The type of elements in the collection.</typeparam>
    public class FilteredObservableCollection<T> : ObservableCollection<T>
    {
        Func<T,T> _filter;

        /// <summary>
        /// Initializes a new <see cref="FilteredObservableCollection{T}"/> with a predicate.
        /// </summary>
        /// <param name="filter">Filter to use. Can be null (it is then ignored).</param>
        public FilteredObservableCollection( Func<T, T> filter )
        {
            _filter = filter;
        }

        /// <summary>
        /// Initializes a new <see cref="FilteredObservableCollection{T}"/> from a list of items. 
        /// Existing items are not filtered.
        /// </summary>
        /// <param name="list">Original list of items to copy.</param>
        /// <param name="filter">Function to use for new elements. Can be null (it is then ignored).</param>
        public FilteredObservableCollection( List<T> list, Func<T, T> filter )
            : base( list )
        {
            _filter = filter;
        }

        /// <summary>
        /// Initializes a new <see cref="FilteredObservableCollection{T}"/> from a list of items. 
        /// Existing items are not filtered.
        /// </summary>
        /// <param name="collection">Original collection of items to copy.</param>
        /// <param name="filter">Function to use for new elements. Can be null (it is then ignored).</param>
        public FilteredObservableCollection( IEnumerable<T> collection, Func<T, T> filter )
            : base( collection )
        {
            _filter = filter;
        }

        /// <summary>
        /// Applies the filter function before calling <see cref="ObservableCollection{T}.InsertItem"/>.
        /// </summary>
        protected override void InsertItem( int index, T item )
        {
            if( _filter != null ) item = _filter( item );
            base.InsertItem( index, item );
        }

        /// <summary>
        /// Applies the filter function before calling <see cref="ObservableCollection{T}.SetItem"/>.
        /// </summary>
        protected override void SetItem( int index, T item )
        {
            if( _filter != null ) item = _filter( item );
            base.SetItem( index, item );
        }

    }

}
