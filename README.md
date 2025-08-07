# NativeNotification
[![NuGet Version](https://img.shields.io/nuget/v/NativeNotification)](https://www.nuget.org/packages/NativeNotification)

A C# library for sending native system notifications across multiple desktop platforms. Supports Windows (toast notifications), Linux (freedesktop.org), and macOS (NSUserNotification).

```csharp
var manager = ManagerFactory.GetNotificationManager(new NativeNotificationOption() { AppName = "AppName" });
var notification = manager.Create();
notification.Title = "Title";
notification.Message = "Hello Message.";
notification.Buttons.Add(new ActionButton("Button Text", () => Console.WriteLine("button clicked.")));
notification.Show(new NotificationDeliverOption() { Duration = TimeSpan.FromSeconds(10) });
```

## Platform Support

|                               | Windows                    | Linux  | macOS        |
| ----------------------------- | -------------------------- | ------ | ------------ |
| TFM requirements              | net8.0-windows10.0.17763.0 | net8.0 | net8.0-macos |
| Title                         | ✅                          | ✅      | ✅            |
| Message                       | ✅                          | ✅      | ✅            |
| Duration                      | ✅                          | ✅      | ✅            |
| Images                        | ✅                          | ✅      | ✅            |
| Notification Actions          | ✅                          | ✅      | ✅            |
| Clear Delivered Notifications | ✅                          | ✅      | ✅            |
| Get Delivered Notifications   | ✅                          | ✅      | ✅            |
| Replace Notifications         | ✅                          | ✅      | ✅            |
| Progress Bar                  | ✅                          | ❌️      | ❌️            |

## Dependencies

[Microsoft.Toolkit.Uwp.Notifications]([Microsoft.Toolkit.Uwp.Notifications](https://github.com/CommunityToolkit/WindowsCommunityToolkit/tree/main/Microsoft.Toolkit.Uwp.Notifications))  
[Tmds.DBus](https://github.com/tmds/Tmds.DBus)  