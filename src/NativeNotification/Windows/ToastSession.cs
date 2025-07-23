#if WINDOWS

using Microsoft.Toolkit.Uwp.Notifications;
using NativeNotification.Common;
using NativeNotification.Interface;
using Windows.UI.Notifications;

namespace NativeNotification.Windows
{
    public class ToastSession : INotification, INotificationInternal
    {
        public string? Title { get; set; }
        public string? Message { get; set; }
        public Uri? Image { get; set; }
        public List<ActionButton> Buttons { get; set; } = [];
        public bool IsAlive { get; set; } = false;
        public void SetIsAlive(bool IsAlive) => this.IsAlive = IsAlive;
        public string NotificationId => _tag;
        private ExpirationHelper? _expirationHelper;
        private readonly string _group = "DEFAULT_GROUP";
        private readonly string _tag = Guid.NewGuid().ToString();
        internal bool CreatedByToast { get; } = false;
        private readonly WindowsNotificationManager _manager;

        private string? Text1 => Message;
        private string? Text2 { get; set; }
        public Action? ContentAction { get; set; }

        private const string TOAST_BINDING_TITLE = "TOAST_BINDING_TITLE";
        private const string TOAST_BINDING_TEXT1 = "TOAST_BINDING_TEXT1";
        private const string TOAST_BINDING_TEXT2 = "TOAST_BINDING_TEXT2";

        public ToastSession(WindowsNotificationManager manager)
        {
            _manager = manager;
        }

        public ToastSession(WindowsNotificationManager manager, ToastNotification toast)
        {
            _tag = toast.Tag;
            _group = toast.Group;
            _manager = manager;
            var dataMap = toast.Data.Values;
            Title = dataMap[TOAST_BINDING_TITLE];
            Message = dataMap[TOAST_BINDING_TEXT1];
            Text2 = dataMap[TOAST_BINDING_TEXT2];
            toast.Activated += Toast_Activated;
            toast.Dismissed += Toast_Dismissed;
            CreatedByToast = true;
        }

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
                Tag = _tag,
                Group = _group,
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
                _manager.ActivateNotification(NotificationId, args.Arguments, this);
            }
            else
            {
                _manager.ActivateNotification(NotificationId, null, this);
            }
        }

        private void Toast_Dismissed(ToastNotification sender, ToastDismissedEventArgs args)
        {
            if (args.Reason is ToastDismissalReason.UserCanceled)
            {
                _manager.RemoveHistory(_tag);
            }
        }

        public void Show(NotificationDeliverOption? config = null)
        {
            var builder = GetBuilder();
            if (config?.Silent is true)
            {
                builder.AddAudio(null, null, true);
            }
            _manager.AddHistory(_tag, this);
            _manager.Center.Show(GetToast(builder));
            IsAlive = true;
            _expirationHelper ??= new ExpirationHelper(this);
            _expirationHelper.SetNoficifationDuration(config);
        }

        public bool Update()
        {
            return _manager.Center.Update(SetBingData(), _tag, _group) is NotificationUpdateResult.Succeeded;
        }

        public void Remove()
        {
            ToastNotificationManagerCompat.History.Remove(_tag, _group);
            _manager.RemoveHistory(_tag);
        }
    }
}

#endif