
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.OptionsModel;
using Yavsc;
using Yavsc.Models;
using Yavsc.Services;


public class DiskUsageTracker : IDiskUsageTracker
{
    public class DUTInfo
    {
        public DUTInfo()
        {
            Creation = DateTime.Now;
        }
        public long Usage { get; set; }
        public long Quota { get; set; }
        public readonly DateTime Creation;
    }

    readonly Dictionary<string, DUTInfo> DiskUsage;
    readonly ApplicationDbContext context;
    readonly int ulistLength;
    public DiskUsageTracker(IOptions<SiteSettings> options, ApplicationDbContext context)
    {
        ulistLength = options.Value.DUUserListLen;
        DiskUsage = new Dictionary<string, DUTInfo>();
        this.context = context;
    }

    readonly static object userInfoLock = new object();

    DUTInfo GetInfo(string username)
    {
        lock (userInfoLock)
        {
        if (!DiskUsage.ContainsKey(username))
        {
            var user = context.Users.SingleOrDefault(u => u.UserName == username);
            if (user == null) throw new Exception($"Not an user : {username}");
            DUTInfo usage = new DUTInfo
            {
                Usage = user.DiskUsage,
                Quota = user.DiskQuota
            };
            DiskUsage.Add(username, usage);
            if (DiskUsage.Count > ulistLength)
            {
                // remove the oldest
                var oldestts = DateTime.Now;
                DUTInfo oinfo = null;
                string ouname = null;
                foreach (var diskusage in DiskUsage)
                {
                    if (oldestts > usage.Creation)
                    {
                        oldestts = diskusage.Value.Creation;
                        ouname = diskusage.Key;
                        oinfo = diskusage.Value;
                    }
                }
                var ouser = context.Users.SingleOrDefault(u => u.UserName == ouname);
                ouser.DiskUsage = oinfo.Usage;
                context.SaveChanges();
                DiskUsage.Remove(ouname);
            }
            return usage;
        }
        return DiskUsage[username];
        }
    }
    public bool GetSpace(string userName, long space)
    {
        var info = GetInfo(userName);
        if (info.Quota < info.Usage + space) return false;
        info.Usage += space;
        #pragma warning disable CS4014
        SaveUserUsage(userName,info.Usage);
        #pragma warning restore CS4014
        return true;
    }

    public void Release(string userName, long space)
    {
        var info = GetInfo(userName);
        info.Usage -= space;
        #pragma warning disable CS4014
        SaveUserUsage(userName,info.Usage);
        #pragma warning restore CS4014
    }

    async Task SaveUserUsage(string username, long usage)
    {
        await Task.Run(() =>
        {
            var ouser = context.Users.SingleOrDefault(u => u.UserName == username);
            ouser.DiskUsage = usage;
            context.SaveChanges();
        });
    }

}
