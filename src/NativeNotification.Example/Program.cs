using NativeNotification.Interface;

namespace NativeNotification.Example;

internal class Program
{
    static INotificationManager Manager { get; set; } = ManagerFactory.GetNotificationManager();
    static void Main(string[] _)
    {
#if DEBUG
        // bool ok = false;
        // System.Diagnostics.Debugger.Launch();
        // while (!ok)
        // {
        //     System.Diagnostics.Debugger.Break();
        //     Thread.Sleep(1000);
        // }
#endif
#if MACOS
        NSApplication.Init();
        Task.Run(ShowSamples);
        NSApplication.SharedApplication.Run();
#else
        ShowSamples();
#endif
    }

    static void ShowSamples()
    {
        Manager.ActionActivated += (args) =>
        {
            Console.WriteLine($"Actived, id '{args.NotificationId}', type {args.Type}, actionId '{args.ActionId}', isLaunchedAction {args.IsLaunchedActivation}");
        };
        Console.WriteLine("c: clear all notifications");
        Console.WriteLine("q: quit");
        Console.WriteLine("1: Show a 10 seconds duration notification, with contents update.");
        Console.WriteLine("2: Show a notification, no sound, with image and buttons, no sound.");
        Console.WriteLine("3: Show a notification with progress bar and button.");
        Dictionary<string, Action> actions = new()
        {
            ["c"] = Manager.RomoveAllNotifications,
            ["1"] = DeliverText,
            ["2"] = DeliverImage,
            ["3"] = DeliverProgressBar,
        };
        while (true)
        {
            Console.WriteLine("Input: ");
            string input = Console.ReadLine() ?? string.Empty;
            input = input.Trim();
            if (input == "q")
            {
                if (Manager is IDisposable disposableManager)
                {
                    disposableManager.Dispose();
                }
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
    }

    static async void DeliverText()
    {
        var notification = Manager.Create();
        notification.Title = "Title";
        notification.Message = "This is a test notification, will be closed in 10 seconds.";
        notification.Buttons.Add(new ActionButton("buttun text", () =>
        {
            Console.WriteLine("buttun clicked.");
        }));
        notification.Show(new NotificationDeliverOption() { Duration = TimeSpan.FromSeconds(10) });
        for (int i = 8; i >= 0; i -= 2)
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
            notification.Message = $"This is a test notification, will be closed in {i} seconds.";
            if (!notification.Update())
            {
                Console.WriteLine("Update failed.");
                break;
            }
        }
    }

    static void DeliverImage()
    {
        var notification = Manager.Create();
        notification.Title = "Title";
        notification.Message = "This is a test notification with image and buttons.";
        var imagePath = Path.Combine("Assets", "house.jpg");
#if MACOS
        imagePath = Path.Combine("Contents/Resources", imagePath);
#endif
        notification.Image = new Uri(Path.GetFullPath(imagePath));
        for (int i = 0; i < 5; i++)
        {
            int idx = i;
            notification.Buttons.Add(new ActionButton($"Button {idx + 1}", $"button id {idx + 1}"));
        }
        notification.Show(new NotificationDeliverOption() { Silent = true });
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
}
