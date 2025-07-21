using NativeNotification.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NativeNotification.Common;

public interface INotificationInternal : INotification
{
    void SetIsAlive(bool IsAlive);
}
