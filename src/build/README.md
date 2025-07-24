# NativeNotification

A C# library for sending native system notifications on different desktop platforms. Supporting Windows(toast notification), Linux(freedesktop.org), macOS(NSUserNotification)

```csharp
var manager = ManagerFactory.GetNotificationManager(new NativeNotificationOption() { AppName = "AppName" });
var notification = manager.Create();
notification.Title = "Title";
notification.Message = "Hello Message.";
notification.Buttons.Add(new ActionButton("Button Text", () => Console.WriteLine("button clicked.")));
notification.Show(new NotificationDeliverOption() { Duration = TimeSpan.FromSeconds(10) });
```