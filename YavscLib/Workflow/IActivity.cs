using System;

namespace YavscLib
{
    public interface IActivity
    {
        string Code { get; set; }
        string Name { get; set; }
        string ParentCode { get; set; }
        string Photo { get; set; }
        string Description { get; set; }
        string ModeratorGroupName { get; set; }
        int Rate { get; set; }
        string SettingsClassName { get; set; }
        DateTime DateCreated
        {
            get; set;
        }

        string UserCreated
        {
            get; set;
        }

        DateTime DateModified
        {
            get; set;
        }

        string UserModified
        {
            get; set;
        }
    }
}
