
using System;

namespace Yavsc.Services
{

    public interface IDiskUsageTracker
    {
        bool GetSpace(string userName, long space);
        void Release(string userName, long space);
    }
}
