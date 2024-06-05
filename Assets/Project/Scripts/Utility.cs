using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Project.Scripts
{
    public static class Utility
    {
        public static void SetField<T>(ref T field, T value, object sender,
            Action<object, PropertyChangedEventArgs> eventAction, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            eventAction?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
        }
    }
}