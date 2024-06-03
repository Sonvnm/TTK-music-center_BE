using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMZ.Database.Enums
{
    public enum EUserRoles
    {
        Member,
        Teacher,
        Admin,
    }
    public enum EPersonRoles
    {
        Teacher,
        Student,
        Admin
    }
    public enum ETypeHistory
    {
        Create,
        Update,
        Delete,
        Login,
        Logout,
        Error,
        Report,
        Help,
        Transaction,
        Other,
        Order,

    }
    public enum EClassStatus
    {
        New,
        InProgress,
        Canceled,
        Done
    }
    public enum EAccountType
    {
        Google,
        Facebook,
        Twitter,
        Github,
        Email
    }

    public enum EFeedBackStatus
    {
        New,
        InProgress,
        Canceled,
        Done
    }
    public enum ETypeFeedBack
    {
        Error,
        Report,
        Help,
        Other,
    }

    public enum EOrderStatus
    {
        New,
        Pending,
        Canceled,
        Done
    }

    public enum EScheduleStatus
    {
        Pending,
        Accepted,
        Rejected,
        Closed
    }

    public enum ELearningProcessStatus
    {
        New,
        Done,
    }
}
