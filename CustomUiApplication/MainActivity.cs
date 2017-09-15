using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V4.App;
using System;
using Com.Adyen.Core.Models;
using System.Collections.Generic;
using Com.Adyen.Core.Interfaces;
using Android.Content;
using Com.Adyen.Core;
using CustomWithCheckOutUiQs;

namespace CustomUiApplication
{
    [Activity(Label = "CustomUiApplication", MainLauncher = true)]
    public class MainActivity : FragmentActivity
    {
        private String TAG = "Morejump from Naxam";

        private PaymentSetupRequest paymentSetupRequest;

        private List<PaymentMethod> availablePaymentMethods = new List<PaymentMethod>();
        private List<PaymentMethod> preferredPaymentMethods = new List<PaymentMethod>();

        // Add the URL for your server here; or you can use the demo server of Adyen: https://checkoutshopper-test.adyen.com/checkoutshopper/demoserver/
        private String merchantServerUrl = "";

        // Add the api secret key for your server here; you can retrieve this key from customer area.
        private String merchantApiSecretKey = "";

        // Add the header key for merchant server api secret key here; e.g. "x-demo-server-api-key"
        private String merchantApiHeaderKeyForApiSecretKey = "";

        private static String SETUP = "setup";
        private static String VERIFY = "verify";

        private IPaymentMethodCallback paymentMethodCallback;
        private Context context;
        private IUriCallback uriCallback;

        private PaymentRequest paymentRequest;
        private PaymentRequestDetailsListener paymentRequestDetailsListener;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);
            paymentRequestDetailsListener = new PaymentRequestDetailsListener
            {
                PaymentMethodSelectionRequired = (paymentRequest, recurringMethods, otherMethods, callback) =>
                  {
                      paymentMethodCallback = callback;
                      preferredPaymentMethods.Clear();
                      preferredPaymentMethods.AddRange(recurringMethods);
                      availablePaymentMethods.Clear();
                      availablePaymentMethods.AddRange(otherMethods);
                      PaymentMethodSelectionFragment paymentMethodSelectionFragment= new PaymentMethodSelectionFragment();
                      SupportFragmentManager.BeginTransaction().Replace(Android.Resource.Id.Content,paymentMethodSelectionFragment).AddToBackStack(null).CommitAllowingStateLoss();

                  },
                RedirectRequired = (paymentRequest, redirectUrl, returnUriCallback) =>{
                   // Log.d(TAG, "paymentRequestDetailsListener.onRedirectRequired(): " + redirectUrl);
                    uriCallback = returnUriCallback;
                    CustomTabsIntent.Builder builder = new CustomTabsIntent.Builder();
                    CustomTabsIntent customTabsIntent = builder.build();
                    customTabsIntent.launchUrl(context, Uri.parse(redirectUrl));

                }
                  

            };
        }
    }
}

