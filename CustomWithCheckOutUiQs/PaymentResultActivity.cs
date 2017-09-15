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

namespace CustomWithCheckOutUiQs
{
    [Activity(Label = "PaymentResultActivity")]
    public class PaymentResultActivity : Activity
    {
        private String result;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.verification_activity);
            result = Intent.GetStringExtra("Result");
            ((TextView)FindViewById(Resource.Id.verificationTextView)).Text=result;// Create your application here
        }
    }
}