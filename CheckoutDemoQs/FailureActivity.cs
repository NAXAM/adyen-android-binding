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
using Android.Support.V4.App;
using Android.Util;

namespace CheckoutDemoQs
{
    [Activity(Label = "FailureActivity")]
    public class FailureActivity : FragmentActivity
    {
        private Context context;
        private Button tryAgainAction;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.RequestWindowFeature(WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.activity_failure);
            context = this;

            SetStatusBarTranslucent(true);

            tryAgainAction = (Button)FindViewById(Resource.Id.try_again_action);
            tryAgainAction.Click += (s, e) => {
                // editting this event later
            };
            
        }

        protected void SetStatusBarTranslucent(bool makeTranslucent)
        {
            View v = FindViewById(Resource.Id.activity_failure);
            if (v != null)
            {
                int paddingTop = 0;
                TypedValue tv = new TypedValue();
                Theme.ResolveAttribute(0, tv, true);
                paddingTop += TypedValue.ComplexToDimensionPixelSize(tv.Data, Resources.DisplayMetrics);
                v.SetPadding(0, makeTranslucent ? paddingTop : 0, 0, 0);
            }

            if (makeTranslucent)
            {
                Window.AddFlags(WindowManagerFlags.TranslucentStatus);
            }
            else
            {
                Window.ClearFlags(WindowManagerFlags.TranslucentStatus);
            }
        }
    }
}