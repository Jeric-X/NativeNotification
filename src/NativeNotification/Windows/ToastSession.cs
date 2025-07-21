#if WINDOWS

using Microsoft.Toolkit.Uwp.Notifications;
using NativeNotification.Common;
using NativeNotification.Interface;
using Windows.UI.Notifications;

namespace NativeNotification.Windows
{
    public class ToastSession(ToastNotifier _notifier, ActionManager<string> _actionManager)
        : INotification, INotificationInternal
    {
        public string? Title { get; set; }
        public string? Message { get; set; }
        public Uri? Image { get; set; }
        public List<ActionButton> Buttons { get; set; } = [];
        public bool IsAlive { get; set; } = false;
        public void SetIsAlive(bool IsAlive) => this.IsAlive = IsAlive;

        private ExpirationHelper? _expirationHelper;
        private const string Group = "DEFAULT_GROUP";
        private readonly string _tag = Guid.NewGuid().ToString();
        private string Tag => _tag.Length <= 64 ? _tag : _tag[..63];
        private string? Text1 => Message;
        private string? Text2 { get; set; }

        private const string TOAST_BINDING_TITLE = "TOAST_BINDING_TITLE";
        private const string TOAST_BINDING_TEXT1 = "TOAST_BINDING_TEXT1";
        private const string TOAST_BINDING_TEXT2 = "TOAST_BINDING_TEXT2";

        protected virtual ToastContentBuilder GetBuilder()
        {
            var builder = new ToastContentBuilder();
            builder.AddVisualChild(new AdaptiveText() { Text = new BindableString(TOAST_BINDING_TITLE) });
            builder.AddVisualChild(new AdaptiveText() { Text = new BindableString(TOAST_BINDING_TEXT1) });
            builder.AddVisualChild(new AdaptiveText() { Text = new BindableString(TOAST_BINDING_TEXT2) });
            if (Image is not null)
            {
                builder.AddInlineImage(Image);
            }
            foreach (var button in Buttons)
            {
                builder.AddButton(button);
            }
            return builder;
        }

        protected virtual ToastNotification GetToast(ToastContentBuilder builder)
        {
            var toast = new ToastNotification(builder.GetToastContent().GetXml())
            {
                Tag = Tag,
                Group = Group,
                Data = new NotificationData()
            };
            SetBingData(toast.Data);
            toast.Activated += Toast_Activated;
            toast.Dismissed += Toast_Dismissed;
            return toast;
        }

        protected virtual NotificationData SetBingData(NotificationData? data = null)
        {
            data ??= new();
            data.Values[TOAST_BINDING_TITLE] = Title;
            data.Values[TOAST_BINDING_TEXT1] = Text1;
            data.Values[TOAST_BINDING_TEXT2] = Text2;
            return data;
        }

        private void Toast_Activated(ToastNotification sender, object e)
        {
            if (e is not ToastActivatedEventArgs args)
                return;
            if (args.Arguments.Length != 0)
            {
                _actionManager.OnActivated(Tag, args.Arguments);
            }
            else
            {
                _actionManager.OnClosed(Tag);
            }
        }

        private void Toast_Dismissed(ToastNotification sender, ToastDismissedEventArgs args)
        {
            if (args.Reason is ToastDismissalReason.UserCanceled)
            {
                _actionManager.OnClosed(Tag);
            }
        }

        public void Show(NotificationConfig? config = null)
        {
            var builder = GetBuilder();
            if (config?.Silent is true)
            {
                builder.AddAudio(null, null, true);
            }
            _notifier.Show(GetToast(builder));
            _actionManager.AddSession(Tag, this);
            _expirationHelper ??= new ExpirationHelper(this);
            _expirationHelper.SetNoficifationDuration(config);
        }

        public bool Update()
        {
            return _notifier.Update(SetBingData(), Tag, Group) is NotificationUpdateResult.Succeeded;
        }

        public void Remove()
        {
            ToastNotificationManagerCompat.History.Remove(Tag, Group);
            _actionManager.OnClosed(Tag);
        }
    }
}

#endif