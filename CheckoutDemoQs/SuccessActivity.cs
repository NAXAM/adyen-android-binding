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
    [Activity(Label = "SuccessActivity")]
    public class SuccessActivity : FragmentActivity
    {
        private Context context;
        private Button backToShopAction;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.RequestWindowFeature(WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.activity_success);
            context = this;
            SetStatusBarTranslucent(true);
            backToShopAction = (Button)FindViewById(Resource.Id.back_to_shop_action);
            backToShopAction.Click += (s, e) =>
            {
                // editting this event later

            };
            // Create your application here
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