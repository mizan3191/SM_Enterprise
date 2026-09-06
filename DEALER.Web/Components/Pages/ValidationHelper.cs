using Radzen;

namespace DEALER.Web.Components.Pages
{
    public static class ValidationHelper
    {
        public static bool ValidateAndNotify<T>(
            IEnumerable<T> data,
            Message notificationComponent,
            string title = "Warning",
            string message = "Data is not available")
        {
            if (data == null || !data.Any())
            {
                notificationComponent.Show(title, message, NotificationSeverity.Warning);
                return false;
            }

            return true;
        }
    }
}
