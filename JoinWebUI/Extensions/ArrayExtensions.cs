namespace JoinWebUI.Extensions
{
    public static class ArrayExtensions
    {
        public static T? ElementAtOrDefaultNullable<T>(this T?[] source, int? index) where T : struct
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (!index.HasValue || index < 0 || index >= source.Length)
            {
                return null;
            }

            return source[index.Value];
        }
    }
}
