using FinancialPortalProject.Models.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FinancialPortalProject.Models.ViewModels
{
    public class NotificationsIndexVm
    {
        public List<Notification> NewNotifications { get; set; } = new List<Notification>();
        public List<Notification> OldNotifications { get; set; } = new List<Notification>();
        public HouseHold Household { get; set; }
    }
}
