#if WINDOWS
using Microsoft.Toolkit.Uwp.Notifications;
using NativeNotification.Interface;

namespace NativeNotification.Windows
{
    static class ToastContentBuilderExtend
    {
        public static ToastContentBuilder AddButton(
            this ToastContentBuilder content, ActionButton button)
        {
            return content.AddButton(
                new ToastButton(button.Text, button.ActionId)
                {
                    ActivationType = ToastActivationType.Background
                }
            );
        }
    }
}
#endif