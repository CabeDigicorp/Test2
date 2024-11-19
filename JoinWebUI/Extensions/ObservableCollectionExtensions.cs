using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace JoinWebUI.Extensions
{
    public static class ObservableCollectionExtensions
    {
        public static void ObservableForEach<T>(this ObservableCollection<T> collection, Action<T> action)
        {
            foreach (var item in collection)
            {
                action(item);
            }
        }

        public static bool ObservableRemoveRangeSafely<T>(this ObservableCollection<T> collection, int? startIndexNullable, int? countNullable)
        {
            if (!startIndexNullable.HasValue || !countNullable.HasValue)
            {
                return false;
            }

            int startIndex = startIndexNullable.Value;
            int count = countNullable.Value;

            if (startIndex < 0 || startIndex >= collection.Count || count <= 0 || startIndex + count > collection.Count)
            {
                return false;
            }

            for (int i = 0; i < count; i++)
            {
                collection.RemoveAt(startIndex);
            }
            return true;
        }

        public static IEnumerable<T> ObservableSafeSkip<T>(this IEnumerable<T> source, int? count)
        {
            if (source == null)
                return Enumerable.Empty<T>();

            if (count == null || count <= 0)
                return source;

            return source.Skip((int)count);
        }

        public static ObservableCollection<T> ObservableSaveSnapshot<T>(this IEnumerable<T> source) where T : ICloneable
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), "La lista di input non può essere null.");

            var clonedCollection = new ObservableCollection<T>();
            foreach (var item in source)
            {
                var clonedItem = (T)item.Clone();
                clonedCollection.Add(clonedItem);
            }

            return clonedCollection;
        }

        public static ObservableCollection<T> ObservableCopyTo<T>(this IEnumerable<T> source) where T : class, ICloneable
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source), "La lista di input non può essere null.");
            }

            var newCollection = new ObservableCollection<T>();

            foreach (var item in source)
            {
                var clonedItem = (T)item.Clone();
                newCollection.Add(clonedItem);
            }

            return newCollection;
        }

        public static bool ObservableCompareCollections<T>(this ObservableCollection<T> firstCollection, ObservableCollection<T> secondCollection)
        {
            try
            {
                if (firstCollection == null && secondCollection == null)
                    return true;
                if ((firstCollection == null && secondCollection != null) || (firstCollection != null && secondCollection == null))
                    return false;
                if (firstCollection?.Count != secondCollection?.Count)
                    return false;

                for (int i = 0; i < firstCollection?.Count; i++)
                {
                    if (!ObservableCompareObjects(firstCollection[i], secondCollection![i]))
                        return false;
                }
            }
            catch (Exception)
            {
                return false;
                throw new ArgumentNullException(nameof(firstCollection), "La lista di input non può essere null.");
            }
            return true;
        }

        private static bool ObservableCompareObjects<T>(T obj1, T obj2)
        {
            if (obj1 == null && obj2 == null)
                return true;
            if ((obj1 == null && obj2 != null) || (obj1 != null && obj2 == null))
                return false;

            var type = typeof(T);
            if (type.IsPrimitive || type == typeof(string) || type == typeof(Guid))
            {
                return obj1!.Equals(obj2);
            }

            else if (type.IsClass)
            {
                var properties = type.GetProperties();
                foreach (var property in properties)
                {
                    var value1 = property.GetValue(obj1);
                    var value2 = property.GetValue(obj2);
                    if (!ObservableCompareObjects(value1, value2))
                        return false;
                }
                return true;
            }

            return false;
        }

        public static int ObservableFindIndex<T>(this ObservableCollection<T> collection, Predicate<T> match)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            if (match == null)
                throw new ArgumentNullException(nameof(match));

            for (int i = 0; i < collection.Count; i++)
            {
                if (match(collection[i]))
                {
                    return i;
                }
            }

            return -1; // Not found
        }
    }
}