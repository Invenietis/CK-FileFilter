using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CK.FileFilter
{
    public class NotifierBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged( [CallerMemberName]string propertyName = null )
        {
            var h = PropertyChanged;
            if( h != null ) h( this, new PropertyChangedEventArgs( propertyName ) );
        }

    }

}
