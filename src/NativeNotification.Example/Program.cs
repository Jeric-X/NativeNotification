using NativeNotification.Interface;

namespace NativeNotification.Example;

internal class Program
{
    static INotificationManager Manager { get; } = ManagerFactory.GetNotificationManager();
    static void Main(string[] _)
    {
        Console.WriteLine("c: clear all notifications");
        Console.WriteLine("0: exit");
        Console.WriteLine("1: Show a 2 seconds duration notification");
        Console.WriteLine("2: Show a notification, no sound, with image and buttons.");
        Console.WriteLine("3: Show a notification with progress bar and button.");
        while (true)
        {
            Console.Write("Input: ");
            string? input = Console.ReadLine();
            if (input is null)
            {
                continue;
            }
            input = input.Trim();
            if (input.Equals("0"))
            {
                break;
            }
            else if (input.Equals("c"))
            {
                Manager.RomoveAllNotifications();
            }
            else if (input.Equals("1"))
            {
                DeliverText();
            }
            else if (input.Equals("2"))
            {
                DeliverImage();
            }
            else if (input.Equals("3"))
            {
                DeliverProgressBar();
            }
            else
            {
                Console.WriteLine("Invalid input, please try again.");
            }
        }
        Manager.Dispose();
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
        notification.Image = new Uri(Path.GetFullPath("Assets/house.jpg"));
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
}
