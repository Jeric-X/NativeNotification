using NativeNotification.Interface;
using System;

namespace NativeNotification.Example;

internal class Program
{
    static INotificationManager Manager { get; } = ManagerFactory.GetNotificationManager();
    static void Main(string[] _)
    {
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
        Console.WriteLine("c: clear all notifications");
        Console.WriteLine("q: quit");
        Console.WriteLine("1: Show a 10 seconds duration notification, with contents update.");
        Console.WriteLine("2: Show a notification, no sound, with image and buttons.");
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
            Console.Write("Input: ");
            string input = Console.ReadLine() ?? string.Empty;
            input = input.Trim();
            if (input == "q")
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
    }

    static async void DeliverText()
    {
        var notification = Manager.Create();
        notification.Title = "Title";
        notification.Message = "This is a test notification, will be closed in 10 seconds.";
        notification.Show(new NotificationDeliverOption() { Duration = TimeSpan.FromSeconds(10) });
        for (int i = 9; i >= 0; i--)
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
            notification.Message = $"This is a test notification, will be closed in {i} seconds.";
            if (!notification.Update())
            {
                Console.WriteLine("Update failed.");
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
        notification.Buttons = [
            new ActionButton("Button1", () => {
                Console.WriteLine("Button 1Clicked.");
            }),
            new ActionButton("Button2", () => {
                Console.WriteLine("Button2 Clicked.");
            }),
        ];
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
