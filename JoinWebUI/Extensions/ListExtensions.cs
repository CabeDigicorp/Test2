using Syncfusion.Blazor.Schedule;
using System.Collections.Generic;

namespace JoinWebUI.Extensions
{
    public static class ListExtensions
    {
        public static bool RemoveRangeSafely<T>(this List<T> list, int? startIndexNullable, int? countNullable)
        {
            if (!startIndexNullable.HasValue || !countNullable.HasValue)
            {
                return false;
            }

            int startIndex = startIndexNullable.Value;
            int count = countNullable.Value;

            if (startIndex < 0 || startIndex >= list.Count || count <= 0 || startIndex + count > list.Count)
            {
                return false;
            }

            list.RemoveRange(startIndex, count);
            return true;
        }

        public static IEnumerable<T> SafeSkip<T>(this IEnumerable<T> source, int? count)
        {
            if (source == null)
                return Enumerable.Empty<T>();

            if (count == null || count <= 0)
                return source;

            return source.Skip((int)count);
        }

        public static List<T> SaveSnapshot<T>(this IEnumerable<T> source) where T : ICloneable
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source), "La lista di input non può essere null.");

            var clonedList = new List<T>();
            foreach (var item in source)
            {
                var clonedItem = (T)item.Clone();
                clonedList.Add(clonedItem);
            }

            return clonedList;
        }

        public static List<Guid> CopyTo<Guid>(this IEnumerable<Guid> source) where Guid : class, ICloneable
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source), "La lista di input non può essere null.");
            }

            var newList = new List<Guid>();

            foreach (var item in source)
            {
                var clonedItem = (Guid)item.Clone();
                newList.Add(clonedItem);
            }

            return newList;
        }

        public static bool CompareLists<T>(this List<T> firstList, List<T> secondList)
        {
            try
            {
                if (firstList == null && secondList == null)
                    return true;
                if ((firstList == null && secondList != null) || (firstList != null && secondList == null))
                    return false;
                if (firstList?.Count != secondList?.Count)
                    return false;

                for (int i = 0; i < firstList?.Count; i++)
                {
                    if (!CompareObjects(firstList[i], secondList![i]))
                        return false;
                }
            }
            catch (Exception)
            {
                return false;
                throw new ArgumentNullException(nameof(firstList), "La lista di input non può essere null.");
            }
            return true;
        }

        private static bool CompareObjects<T>(T obj1, T obj2)
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
                    // Escludi le proprietà di tipo List<Guid>
                    //if (property.PropertyType == typeof(List<Guid>))
                    //    continue;

                    var value1 = property.GetValue(obj1);
                    var value2 = property.GetValue(obj2);
                    if (!CompareObjects(value1, value2))
                        return false;
                }
                return true;
            }

            return false;
        }
    }
}