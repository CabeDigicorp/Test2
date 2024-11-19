using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace JoinWebUI.Extensions
{
    public class ObservableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler PropertyChanged;

        // Override l'indicizzatore per generare l'evento CollectionChanged quando un elemento viene modificato
        public new TValue this[TKey key]
        {
            get => base[key];
            set
            {
                if (this.ContainsKey(key))
                {
                    var oldValue = base[key];
                    base[key] = value;
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldValue));
                }
                else
                {
                    Add(key, value);
                }
            }
        }

        // Sovrascrivi il metodo Add per inviare notifiche
        public new void Add(TKey key, TValue value)
        {
            base.Add(key, value);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, value));
            OnPropertyChanged(nameof(Count)); // Notifica quando cambia la dimensione della collezione
        }

        // Sovrascrivi il metodo Remove per inviare notifiche
        public new bool Remove(TKey key)
        {
            if (TryGetValue(key, out TValue value) && base.Remove(key))
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, value));
                OnPropertyChanged(nameof(Count)); // Notifica il cambiamento di dimensione
                return true;
            }
            return false;
        }

        // Sovrascrivi il metodo Clear per inviare notifiche
        public new void Clear()
        {
            base.Clear();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            OnPropertyChanged(nameof(Count)); // Notifica il cambiamento di dimensione
        }

        // Metodo per notificare cambiamenti nella collezione
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e) =>
            CollectionChanged?.Invoke(this, e);

        // Metodo per notificare cambiamenti nelle proprietà
        protected virtual void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}