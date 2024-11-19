using Microsoft.AspNetCore.Components;

namespace JoinWebUI.Shared
{
    public class NavigationComponentInfo
    {
        public string Title { get; } = "[Titolo]";
        public string? Description { get; } = null;
        public string? Image { get; } = null;
        public string? LinkUrl { get; } = null;
        public EventCallback? Callback { get; }

        public NavigationComponentInfo(string title,
                                       string? description = null,
                                       string? image = null,
                                       string? linkUrl = null,
                                       EventCallback? callback = null)
        {
            this.Title = title;
            this.Description = string.IsNullOrWhiteSpace(description) ? null : description;
            this.Image = image;
            this.LinkUrl = string.IsNullOrWhiteSpace(linkUrl) ? null : linkUrl;
            this.Callback = callback;
        }


        public static string ComputeWidthAsync(int minWidth, int maxWidth, int totalWidth, int totalElements)
        {
            int cardsPerRow = 0;
            int maxCardsPerRow = Math.Min(totalElements, 5);
            int pixel = minWidth;
            do
            {
                cardsPerRow++;
                pixel += minWidth;
            }
            while (pixel < totalWidth && cardsPerRow < maxCardsPerRow);

            int rows = (int)Math.Ceiling((double)totalElements / cardsPerRow);
            cardsPerRow = (int)Math.Ceiling((double)totalElements / rows);

            minWidth = totalWidth / cardsPerRow;
            minWidth = Math.Min(minWidth, maxWidth);

            return minWidth.ToString() + "px";

        }

    }
}
