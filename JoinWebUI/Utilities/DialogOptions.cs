using Syncfusion.Blazor.Popups;

namespace JoinWebUI.Utilities
{
    public class DialogOptions
    {
        public static Syncfusion.Blazor.Popups.DialogOptions ConfirmOptions { get; } = new Syncfusion.Blazor.Popups.DialogOptions()
        {
            Width = "400px",
            CloseOnEscape = true
        };

        public static Syncfusion.Blazor.Popups.DialogOptions AlertOptions { get; } = new Syncfusion.Blazor.Popups.DialogOptions()
        {
            Width = "400px",
            ShowCloseIcon = true,
            CloseOnEscape = true
        };
    }
}
