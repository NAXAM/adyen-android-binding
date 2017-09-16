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
using Android.Support.CustomTabs;
using Com.Adyen.Core.Models.Paymentdetails;

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
                    CustomTabsIntent customTabsIntent = builder.Build();
                    customTabsIntent.LaunchUrl(context,Android.Net.Uri.Parse(redirectUrl));

                },
                PaymentDetailsRequired = (paymentRequest, inputDetails, callback) =>
                {
                    // Log.d(TAG, "paymentRequestDetailsListener.onPaymentDetailsRequired()");
                    String paymentMethodType = paymentRequest.PaymentMethod.GetType();

                    if (PaymentMethod.Type.Card.Equals(paymentMethodType))
                    {

                        CreditCardFragment creditCardFragment = new CreditCardFragment();
                        Bundle bundle = new Bundle();
                        //ONE CLICK CHECK
                        bool isOneClick = InputDetailsUtil.ContainsKey(inputDetails, "cardDetails.cvc");
                        if (isOneClick)
                        {
                            bundle.PutBoolean("oneClick", true);
                        }
                        creditCardFragment.setCreditCardInfoListener(new CreditCardInfoListener
                        {
                            CreditCardInfoProvided = (creditCardInfo) =>
                            {
                                if (isOneClick)
                                {
                                    CVCOnlyPaymentDetails cvcOnlyPaymentDetails = new CVCOnlyPaymentDetails(inputDetails);
                                    cvcOnlyPaymentDetails.FillCvc(creditCardInfo);
                                    callback.CompletionWithPaymentDetails(cvcOnlyPaymentDetails);

                                }
                                else
                                {
                                    CreditCardPaymentDetails creditCardPaymentDetails = new CreditCardPaymentDetails(inputDetails);
                                    creditCardPaymentDetails.FillCardToken(creditCardInfo);
                                    callback.CompletionWithPaymentDetails(creditCardPaymentDetails);
                                }

                            }


                        });
                        bundle.PutString("public_key", paymentRequest.PublicKey);
                        bundle.PutString("generation_time", paymentRequest.GenerationTime);
                        creditCardFragment.Arguments=bundle;

                        SupportFragmentManager.BeginTransaction().Replace(Android.Resource.Id.Content,
                                creditCardFragment).AddToBackStack(null).CommitAllowingStateLoss();

                    }
                }
                  

            };
        }
    }

    public class CreditCardInfoListener : CreditCardFragment.CreditCardInfoListener
    {
        public Action<string> CreditCardInfoProvided;


        public void onCreditCardInfoProvided(string paymentDetails)
        {
            CreditCardInfoProvided?.Invoke(paymentDetails);
        }
    }
}

