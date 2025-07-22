using NativeNotification.Interface;

namespace NativeNotification.Example;

internal class Program
{
    static INotificationManager Manager { get; } = ManagerFactory.GetNotificationManager();
    static void Main(string[] _)
    {
#if MACOS
        NSApplication.Init();
#endif
        Console.WriteLine("c: clear all notifications");
        Console.WriteLine("0: exit");
        Console.WriteLine("1: Show a 5 seconds duration notification, with contents changing.");
        Console.WriteLine("2: Show a notification, no sound, with image and buttons.");
        Console.WriteLine("3: Show a notification with progress bar and button.");
        Console.WriteLine("4: Show a notification update it self.");
        Dictionary<string, Action> actions = new()
        {
            ["c"] = Manager.RomoveAllNotifications,
            ["1"] = DeliverText,
            ["2"] = DeliverImage,
            ["3"] = DeliverProgressBar,
            ["4"] = DeliverUpdate,
        };
        Task.Run(() =>
        {
            while (true)
            {
                Console.Write("Input: ");
                string input = Console.ReadLine() ?? string.Empty;
                input = input.Trim();
                if (input == "0")
                {
                    Manager.Dispose();
                    Environment.Exit(0);
                }
                if (actions.TryGetValue(input, out var action))
                {
                    action();
                }
                else
                {
                    Console.WriteLine("Invalid input, please try again.");
                }
            }
        });
#if MACOS
        NSApplication.SharedApplication.Run();
#endif
    }

    static void DeliverText()
    {
        var notification = Manager.Create();
        notification.Title = "Title";
        notification.Message = "This is a test notification, will be closed in 2 seconds.";
        notification.Show(new NotificationConfig() { Duration = TimeSpan.FromSeconds(2) });
    }

    static void DeliverImage()
    {
        var notification = Manager.Create();
        notification.Title = "Title";
        notification.Message = "This is a test notification with image.";
        var imagePath = Path.Combine("Assets", "house.jpg");
#if MACOS
        imagePath = Path.Combine("Contents/Resources", imagePath);
#endif
        notification.Image = new Uri(Path.GetFullPath(imagePath));
        notification.Buttons = [
            new ActionButton("Button1", () => {
                Console.WriteLine("Button 1Clicked.");
            }),
            new ActionButton("Button2", () => {
                Console.WriteLine("Button2 Clicked.");
            }),
        ];
        notification.Show(new NotificationConfig() { Silent = true });
    }

    static void DeliverProgressBar()
    {
        static void SetProgress(IProgressNotification progress)
        {
            progress.Title = "Title";
            progress.Message = "This is a test progress with progress bar";
            progress.ProgressTitle = "Progress Title";
            progress.ProgressValue = 0;
            progress.ProgressValueTip = "Progress Value Tip";
            progress.IsIndeterminate = false;
        }

        var cts = new CancellationTokenSource();
        var notification = Manager.CreateProgress();
        SetProgress(notification);
        notification.Buttons = [
            new ActionButton("Cancel", () => {
                cts.Cancel();
                var newNotification = Manager.CreateProgress();
                SetProgress(newNotification);
                newNotification.ProgressValueTip = "Canceled.";
                newNotification.IsIndeterminate = true;
                newNotification.Show();
            })
        ];

        notification.Show();
        var task = Task.Run(async () =>
        {
            for (double i = 0; i <= 100; i += 5)
            {
                notification.ProgressValue = i / 100;
                notification.ProgressValueTip = $"Progress: {i}%";
                notification.Update();
                await Task.Delay(TimeSpan.FromSeconds(0.5), cts.Token);
            }
            notification.ProgressValueTip = "Complete.";
            notification.Update();
        });
    }

    static void DeliverUpdate()
    {
        var notification = Manager.Create();
        notification.Title = "Update Notification";
        notification.Message = "This notification will update itself.";
        notification.Show();

        Task.Run(async () =>
        {
            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(1000);
                notification.Message = $"Updated message {i + 1}";
                if (!notification.Update())
                {
                    Console.WriteLine("Notification is no longer alive, cannot update.");
                    return;
                }
            }
            notification.Update();
        });
    }
}
