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
using Org.Json;
using Com.Adyen.Core.Utils;
using CheckoutDemoQs;
using Java.Util;
using Java.Nio.Charset;
using Android.Util;
using Android.Text;

namespace CustomUiApplication
{
    [Activity(Label = "CustomUiApplication", MainLauncher = true)]
    public class MainActivity : FragmentActivity, PaymentDataEntryFragment.PaymentRequestListener, PaymentMethodSelectionFragment.PaymentMethodSelectionListener
    {
        private String TAG = "Morejump from Naxam";
        // maybe chane three properties later in  BuildConfig class
        private string SERVER_URL = "https://checkoutshopper-test.adyen.com/checkoutshopper/demoserver/";
        private string API_KEY = "10001|BEF02479D565932A3A3AA08740D9A02B0746D091A9DDBF7C7B72206A195EA5AEB402A810F8FCCD4177408031499714503422D8B3726D7465F8136967776D690D871CBD6B9E7671433F2754F427744CA6DD0F2E82C892C09F7306AE6ACE4D9F728FE400FEB5D7EC0B7E26071EB7683983D3058BABB47BC83D7C9CDB681562BC5FA41CF4F52A322084DC0DE699E0FF53E724C752F5EFFB082367AD5810834B348061CC1F993B96720D7E8B9795A4B9EB80C0CC66E896FCB96D8D27CA055D95646102C9935475B896F05E1D4E1034F34FF044649743F41BF4E312339ED2D0DA9430B3E6090D61E7781938E3FBF865E5EEC2E0763C81B8F15120D4398D9282A6A975";
        private string API_HEADER_KEY = "0101398667F12C8EC76C1C47C349BF9F7439A9FDD57B92431986597A531BD27DB88A2B639BCB7A1FE16E2B079871B0EBD0DFC8DCCC4AF7A2228B441710C15D5B0DBEE47CDCB5588C48224C6007";

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
        private PaymentRequestListener paymentRequestListener;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
           // SetContentView(Resource.Layout.Main);
            paymentRequestDetailsListener = new PaymentRequestDetailsListener
            {
                PaymentMethodSelectionRequired = (paymentRequest, recurringMethods, otherMethods, callback) =>
                  {
                      paymentMethodCallback = callback;
                      preferredPaymentMethods.Clear();
                      preferredPaymentMethods.AddRange(recurringMethods);
                      availablePaymentMethods.Clear();
                      availablePaymentMethods.AddRange(otherMethods);
                      PaymentMethodSelectionFragment paymentMethodSelectionFragment = new PaymentMethodSelectionFragment();
                      SupportFragmentManager.BeginTransaction().Replace(Android.Resource.Id.Content, paymentMethodSelectionFragment).AddToBackStack(null).CommitAllowingStateLoss();

                  },
                RedirectRequired = (paymentRequest, redirectUrl, returnUriCallback) =>
                {
                    // Log.d(TAG, "paymentRequestDetailsListener.onRedirectRequired(): " + redirectUrl);
                    uriCallback = returnUriCallback;
                    CustomTabsIntent.Builder builder = new CustomTabsIntent.Builder();
                    CustomTabsIntent customTabsIntent = builder.Build();
                    customTabsIntent.LaunchUrl(context, Android.Net.Uri.Parse(redirectUrl));

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
                        creditCardFragment.Arguments = bundle;

                        SupportFragmentManager.BeginTransaction().Replace(Android.Resource.Id.Content,
                                creditCardFragment).AddToBackStack(null).CommitAllowingStateLoss();

                    }
                    else if (PaymentMethod.Type.Ideal.Equals(paymentMethodType))
                    {
                        AlertDialog.Builder alertDialog = new AlertDialog.Builder(this);
                        List<InputDetail.Item> issuers = InputDetailsUtil.GetInputDetail(inputDetails, "idealIssuer").Items as List<InputDetail.Item>;
                        IssuerListAdapter issuerListAdapter = new IssuerListAdapter(this, issuers);
                        alertDialog.SetSingleChoiceItems(issuerListAdapter, -1, new OnClickListener
                        {
                            Click = (dialogInterface, i) =>
                            {
                                IdealPaymentDetails idealPaymentDetails = new IdealPaymentDetails(inputDetails);
                                idealPaymentDetails.FillIssuer(issuers[i]);
                                dialogInterface.Dismiss();
                                callback.CompletionWithPaymentDetails(idealPaymentDetails);

                            }

                        });
                        alertDialog.Show();
                    }
                    else
                    {
                        String message = "UI for " + paymentMethodType + " has not been implemented.";
                        //Log.w(TAG, message);
                        Toast.MakeText(this, message, ToastLength.Long).Show();
                        paymentRequest.Cancel();
                    }
                }


            };
            paymentRequestListener = new PaymentRequestListener
            {
                PaymentDataRequested = (paymentRequest, token, callback) =>
                {
                    Log.Debug(TAG, "paymentRequestListener.onPaymentDataRequested()");
                    Dictionary<string, string> headers = new Dictionary<string, string>();
                    headers.Add("Content-Type", "application/json; charset=UTF-8");
                    headers.Add(merchantApiHeaderKeyForApiSecretKey, merchantApiSecretKey);
                    AsyncHttpClient.Post(merchantServerUrl + SETUP, headers, getSetupDataString(token), new HttpResponseCallback
                    {

                        Success = (response) =>
                        {
                            callback.CompletionWithPaymentData(response);
                        },
                        Failure = (e) =>
                        {
                            Log.Error(TAG, "HTTP Response problem: ", e);
                            paymentRequest.Cancel();
                        }

                    });
                },
                PaymentResult = (paymentRequest, paymentResult) =>
                {
                    Log.Debug(TAG, "paymentRequestListener.onPaymentResult()");
                    String resultString;
                    if (paymentResult.IsProcessed)
                    {
                        resultString = paymentResult.Payment.GetPaymentStatus().ToString();
                        verifyPayment(paymentResult.Payment);
                    }
                    else
                    {
                        resultString = paymentResult.Error.ToString();
                    }

                    Intent intent = new Intent(ApplicationContext, typeof(PaymentResultActivity));
                    intent.PutExtra("Result", resultString);
                    intent.AddFlags(ActivityFlags.ClearTop);
                    intent.AddFlags(ActivityFlags.NewTask);
                    StartActivity(intent);
                    Finish();
                }


            };
            Log.Debug(TAG, "onCreate()");
            context = this;
            Android.Net.Uri uri = Intent.Data;
            if (uri == null)
            {
                setupInitScreen();
            }
            else
            {
               // throw new IllegalStateException("Application was supposed to be declared singleTask");
            }
        }


        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            Log.Debug(TAG, "onNewIntent: " + intent);
            if (uriCallback != null)
            {
                Log.Debug(TAG, "Notifying paymentRequest about return URI");
                uriCallback.CompletionWithUri(intent.Data);
            }
        }
        private void setupInitScreen()
        {
            PaymentDataEntryFragment paymentDataEntryFragment = new PaymentDataEntryFragment();
            SupportFragmentManager.BeginTransaction().Replace(Android.Resource.Id.Content,
                    paymentDataEntryFragment).CommitAllowingStateLoss();
        }
        public List<PaymentMethod> getAvailablePaymentMethods()
        {
            return availablePaymentMethods;
        }
        public List<PaymentMethod> getPreferredPaymentMethods()
        {
            return preferredPaymentMethods;
        }
        public void onPaymentRequested(PaymentSetupRequest paymentSetupRequest)
        {
            Log.Debug(TAG, "onPaymentRequested");
            merchantServerUrl = TextUtils.IsEmpty(merchantServerUrl) ? SERVER_URL : merchantServerUrl;
            merchantApiSecretKey = TextUtils.IsEmpty(merchantApiSecretKey) ? API_KEY : merchantApiSecretKey;
            merchantApiHeaderKeyForApiSecretKey = TextUtils.IsEmpty(merchantApiHeaderKeyForApiSecretKey)
                    ? API_HEADER_KEY
                    : merchantApiHeaderKeyForApiSecretKey;

            if (TextUtils.IsEmpty(merchantApiSecretKey)
                    || TextUtils.IsEmpty(merchantApiHeaderKeyForApiSecretKey)
                    || TextUtils.IsEmpty(merchantServerUrl))
            {
                Toast.MakeText(ApplicationContext, "Server parameters have not been configured correctly", ToastLength.Long).Show();
                return;
            }
            this.paymentSetupRequest = paymentSetupRequest;
            if (paymentRequest != null)
            {
                paymentRequest.Cancel();
            }
            paymentRequest = new PaymentRequest(this, paymentRequestListener, paymentRequestDetailsListener);// here
            paymentRequest.Start();
        }
        private String getSetupDataString(String token)
        {
            JSONObject jsonObject = new JSONObject();
            try
            {
                jsonObject.Put("merchantAccount", "TestMerchant"); // Not required when communicating with merchant server
                jsonObject.Put("shopperLocale", paymentSetupRequest.getShopperLocale());
                jsonObject.Put("token", token);
                jsonObject.Put("returnUrl", "example-shopping-app://");
                jsonObject.Put("countryCode", paymentSetupRequest.getCountryCode());
                JSONObject amount = new JSONObject();
                amount.Put("value", paymentSetupRequest.getAmount().Value);
                amount.Put("currency", paymentSetupRequest.getAmount().Currency);
                jsonObject.Put("amount", amount);
                jsonObject.Put("channel", "android");
                jsonObject.Put("reference", "Android Checkout SDK Payment: " + DateTime.Now);
                jsonObject.Put("shopperReference", "example-customer@exampleprovider");
            }
            catch (JSONException jsonException)
            {
                //Log.e(TAG, "Setup failed", jsonException);
            }
            return jsonObject.ToString();
        }
        public void onPaymentMethodSelected(PaymentMethod paymentMethod)
        {
            Log.Debug(TAG, "onPaymentMethodSelected(): " + paymentMethod.GetType());
            if (paymentMethodCallback != null)
            {
                paymentMethodCallback.CompletionWithPaymentMethod(paymentMethod);
            }
        }
        private void verifyPayment(Payment payment)
        {
            JSONObject jsonObject = new JSONObject();
            try
            {
                jsonObject.Put("payload", payment.Payload);
            }
            catch (JSONException e)
            {
                //e.printStackTrace();
                Toast.MakeText(this, "Failed to verify payment.", ToastLength.Long).Show();
                return;
            }
            String verifyString = jsonObject.ToString();

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "application/json; charset=UTF-8");
            headers.Add(merchantApiHeaderKeyForApiSecretKey, merchantApiSecretKey);
            String resultString = "";


            AsyncHttpClient.Post(merchantServerUrl + VERIFY, headers, verifyString, new HttpResponseCallback
            {
                Success = (response) =>
                    {
                        try
                        {
                            JSONObject jsonVerifyResponse = new JSONObject((string)new Java.Lang.String(response, Charset.ForName("UTF-8")));
                            String authResponse = jsonVerifyResponse.GetString("authResponse");
                            if (authResponse.Equals(payment.GetPaymentStatus().ToString()))
                            {
                                resultString = "Payment is " + payment.GetPaymentStatus().ToString().ToLower() + " and verified.";
                            }
                            else
                            {
                                resultString = "Failed to verify payment.";
                            }
                        }
                        catch (JSONException e)
                        {
                            //e.printStackTrace();
                            resultString = "Failed to verify payment.";
                        }
                        Toast.MakeText(this, resultString, ToastLength.Long).Show();

                    },

                Failure = (e) =>
                {
                    Toast.MakeText(this, resultString, ToastLength.Long).Show();
                }

            });
        }
        public void onBackPressed()
        {
            if (SupportFragmentManager.BackStackEntryCount > 0)
            {
                SupportFragmentManager.PopBackStackImmediate();
            }
            else
            {
                base.OnBackPressed();
            }
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
    public class OnClickListener : Java.Lang.Object, IDialogInterfaceOnClickListener
    {
        public Action<IDialogInterface, int> Click;

        public void OnClick(IDialogInterface dialog, int which)
        {
            Click?.Invoke(dialog, which);
        }
    }
}

