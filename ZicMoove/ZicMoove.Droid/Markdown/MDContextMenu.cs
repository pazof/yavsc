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
using Android.Support.V7.App;
using XLabs.Ioc;
using XLabs.Platform.Mvvm;
using XLabs.Forms;
using static Android.Views.View;

namespace ZicMoove.Droid.Markdown
{
    class MDContextMenu : AppCompatDialog
    {
        private ActionMode mActionMode = null;
        public MDContextMenu(Context context) : base(context)
        {
            
        }
        public override void OnActionModeStarted(ActionMode mode)
        {
            if (mActionMode == null)
            {
                mActionMode = mode;
                var menu = mode.Menu;
                // Remove the default menu items (select all, copy, paste, search)
                menu.Clear();

                // If you want to keep any of the defaults,
                // remove the items you don't want individually:
                // menu.removeItem(android.R.id.[id_of_item_to_remove])

                // Inflate your own menu items
                mode.MenuInflater.Inflate(Resource.Menu.md_menu, menu);

            }
            mActionMode = mode;
            base.OnActionModeStarted(mode);
        }
        public override void OnActionModeFinished(ActionMode mode)
        {
            base.OnActionModeFinished(mode);
        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
        }
        public void OnCreateContextMenu(IContextMenu menu, View v, IContextMenuContextMenuInfo menuInfo)
        {
          /*  if (menuInfo!=null)
            {
               var info = menuInfo.ToString();
            }
            menu.Add(0, 0, 0, "test");
            var subMenu = menu.AddSubMenu(
                0, 1, 1, "...");
            subMenu.Add(0, 3, 0, "nkjnkjn");
            var app = Resolver.Resolve<IXFormsApp>() as IXFormsApp<XFormsCompatApplicationDroid>;

            var mgr = ClipboardManager.FromContext(app.AppContext);
            if (mgr.HasText)
                menu.Add(0, 0, 0, "Coller!");*/
            //base.OnCreateContextMenu(menu, v, menuInfo);
        }
    }
}