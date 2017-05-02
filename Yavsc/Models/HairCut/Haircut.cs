using System;
namespace YavscLib.Haircut
{
    public interface IProviderInfo
    {

        string UserId
        {
            get;
            set;
        }

        string UserName
        {
            get;
            set;
        }

        string Avatar
        {
            get;
            set;
        }
    }

    public interface IHaircutQuery
    {

        IProviderInfo ProviderInfo
        {
            get;
            set;
        }

        long Id
        {
            get;
            set;
        }

        IHairPrestation Prestation
        {
            get;
            set;
        }

        long Status
        {
            get;
            set;
        }

        ILocation Location
        {
            get;
            set;
        }

        DateTime EventDate
        {
            get;
            set;
        }
    }
}
