using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Service.Chooser;

namespace BookAStar.Droid
{
    [Service(Name = "fr.pschneider.bas.YavscChooserTargetService", Label = "Yavsc share service", Permission = "android.permission.BIND_CHOOSER_TARGET_SERVICE", Icon = "@drawable/icon", Exported = true)]
    class YavscChooserTargetService : ChooserTargetService
    {
        public override IList<ChooserTarget> OnGetChooserTargets(ComponentName targetActivityName, IntentFilter matchedFilter)
        {
            Android.Graphics.Drawables.Icon i = 
                Android.Graphics.Drawables.Icon.CreateWithResource(this.BaseContext,
                Resource.Drawable.icon);
            ChooserTarget t = new ChooserTarget(
                new Java.Lang.String(
                "BookingStar"), i,
                .5f, new ComponentName(this.BaseContext, ".SendFilesActivity"),
                null);
            var res = new List<ChooserTarget>();
            res.Add(t);
            return res;
        }

        public override IBinder OnBind(Intent intent)
        {
            return base.OnBind(intent);
        }

        public override void OnCreate()
        {
            base.OnCreate();
        }
    }
}