using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V4.App;
using System;
using Com.Adyen.Core;
using System.Runtime.Remoting.Contexts;
using Com.Adyen.Core.Interfaces;
using Com.Adyen.Core.Models;
using Android.Views;
using Android.Util;
using Org.Json;
using System.Collections.Generic;
using Com.Adyen.Core.Utils;
using Java.Lang;
using Java.Util;
using Java.Nio.Charset;
using Android.Content;
using Android.Text;

namespace CheckoutDemoQs
{
    [Activity(Label = "CheckoutDemoQs", MainLauncher = true)]
    public class MainActivity : FragmentActivity
    {
        // maybe chane three properties later in  BuildConfig class
        private string SERVER_URL= "https://checkoutshopper-test.adyen.com/checkoutshopper/demoserver/";
        private string API_KEY="10001|BEF02479D565932A3A3AA08740D9A02B0746D091A9DDBF7C7B72206A195EA5AEB402A810F8FCCD4177408031499714503422D8B3726D7465F8136967776D690D871CBD6B9E7671433F2754F427744CA6DD0F2E82C892C09F7306AE6ACE4D9F728FE400FEB5D7EC0B7E26071EB7683983D3058BABB47BC83D7C9CDB681562BC5FA41CF4F52A322084DC0DE699E0FF53E724C752F5EFFB082367AD5810834B348061CC1F993B96720D7E8B9795A4B9EB80C0CC66E896FCB96D8D27CA055D95646102C9935475B896F05E1D4E1034F34FF044649743F41BF4E312339ED2D0DA9430B3E6090D61E7781938E3FBF865E5EEC2E0763C81B8F15120D4398D9282A6A975";
        private string API_HEADER_KEY = "0101398667F12C8EC76C1C47C349BF9F7439A9FDD57B92431986597A531BD27DB88A2B639BCB7A1FE16E2B079871B0EBD0DFC8DCCC4AF7A2228B441710C15D5B0DBEE47CDCB5588C48224C6007";
        //
        private static string TAG = "CheckoutDemoQs";
        private static string SETUP = "setup";
        private static string VERIFY = "verify";

        // Add the URL for your server here; or you can use the demo server of Adyen: https://checkoutshopper-test.adyen.com/checkoutshopper/demoserver/
        private static string merchantServerUrl = "";

        // Add the api secret key for your server here; you can retrieve this key from customer area.
        private static string merchantApiSecretKey = "";

        // Add the header key for merchant server api secret key here; e.g. "x-demo-server-api-key"
        private static string merchantApiHeaderKeyForApiSecretKey = "";
        PaymentRequestListener paymentRequestListener;

        private PaymentRequest paymentRequest;
        private static Android.Content.Context context;
        static private string GetSetupDataString(string token)
        {
            JSONObject jsonObject = new JSONObject();

            jsonObject.Put("merchantAccount", "TestMerchant"); // Not required when communicating with merchant server
            jsonObject.Put("shopperLocale", "NL");
            jsonObject.Put("token", token);
            jsonObject.Put("returnUrl", "example-shopping-app://");
            jsonObject.Put("countryCode", "NL");
            JSONObject amount = new JSONObject();
            amount.Put("value", "17408");
            amount.Put("currency", "USD");
            jsonObject.Put("amount", amount);
            jsonObject.Put("channel", "android");
            jsonObject.Put("reference", "M+M Black dress & accessories");
            jsonObject.Put("shopperReference", "example-customer@exampleprovider");
            return jsonObject.ToString();
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

        private static void verifyPayment(Payment payment)
        {
            string resultString = "".ToString();
            JSONObject jsonObject = new JSONObject();
            jsonObject.Put("payload", payment.Payload);

            string verifystring = (string)jsonObject;
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "application/json; charset=UTF-8");
            headers.Add(merchantApiHeaderKeyForApiSecretKey, merchantApiSecretKey);
            //
            AsyncHttpClient.Post(merchantServerUrl + VERIFY, headers, verifystring, new HttpResponseCallback
            {
                Success = (response) =>
                {
                    Java.Lang.String mString = new Java.Lang.String(response, Charset.ForName("UTF-8"));
                    JSONObject jsonVerifyResponse = new JSONObject((string)mString);
                    Java.Lang.String authResponse = new Java.Lang.String(jsonVerifyResponse.GetString("authResponse"));
                    if (authResponse.EqualsIgnoreCase(payment.GetPaymentStatus().ToString()))
                    {
                        resultString = "Payment is " + payment.GetPaymentStatus().ToString().ToLower() + " and verified.";
                    }
                    else
                    {
                        resultString = "Failed to verify payment.";
                    }
                },
                Failure = (th) =>
                {
                    Toast.MakeText(context, resultString, ToastLength.Long).Show();
                }
            });

        }
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            this.RequestWindowFeature(WindowFeatures.NoTitle);
            SetContentView(Resource.Layout.activity_main);
            merchantServerUrl = TextUtils.IsEmpty(merchantServerUrl) ? SERVER_URL : merchantServerUrl;
            merchantApiSecretKey = TextUtils.IsEmpty(merchantApiSecretKey) ? API_KEY : merchantApiSecretKey;
            merchantApiHeaderKeyForApiSecretKey = TextUtils.IsEmpty(merchantApiHeaderKeyForApiSecretKey)? API_HEADER_KEY: merchantApiHeaderKeyForApiSecretKey;
            context = this;
            SetStatusBarTranslucent(true);
            Button checkoutButton = (Button)FindViewById(Resource.Id.checkout_button);
            //
            checkoutButton.Click += (s, e) =>
            {
                if (TextUtils.IsEmpty(merchantApiSecretKey)|| TextUtils.IsEmpty(merchantApiHeaderKeyForApiSecretKey)|| TextUtils.IsEmpty(merchantServerUrl))
                {
                    Toast.MakeText(ApplicationContext, "Server parameters have not been configured correctly", ToastLength.Long).Show();
                    return;
                }
                paymentRequest = new PaymentRequest(context, paymentRequestListener);
                paymentRequest.Start();

            };
            //
            paymentRequestListener = new PaymentRequestListener
            {
                PaymentDataRequested = (paymentRequest, token, paymentDataCallback) =>
                {
                    Dictionary<string, string> headers = new Dictionary<string, string>();
                    headers.Add("Content-Type", "application/json; charset=UTF-8");
                    headers.Add(merchantApiHeaderKeyForApiSecretKey, merchantApiSecretKey);
                    AsyncHttpClient.Post(merchantServerUrl + SETUP, headers, GetSetupDataString(token), new HttpResponseCallback
                    {
                        Success = (response) =>
                        {
                            paymentDataCallback.CompletionWithPaymentData(response);
                        },
                        Failure = (e) =>
                        {
                            Log.Error(TAG, "HTTP Response problem: ", e);
                            System.Diagnostics.Debug.Write("HTTP Response problem: " + e);
                        }
                    });
                },
                PaymentResult = (paymentRequest, paymentRequestResult) =>
                {
                    if (paymentRequestResult.IsProcessed && (paymentRequestResult.Payment.GetPaymentStatus() == Payment.PaymentStatus.Authorised || paymentRequestResult.Payment.GetPaymentStatus() == Payment.PaymentStatus.Received))
                    {
                        verifyPayment(paymentRequestResult.Payment);
                        Intent intent = new Intent(context, typeof(SuccessActivity));
                        StartActivity(intent);
                        Finish();
                    }
                    else
                    {
                        Intent intent = new Intent(context, typeof(FailureActivity));
                        StartActivity(intent);
                        Finish();
                    }

                }
            };
        }
    }
    public class HttpResponseCallback : Java.Lang.Object, IHttpResponseCallback
    {
        public Action<Throwable> Failure;
        public Action<byte[]> Success;

        public void OnFailure(Throwable p0)
        {
            Failure?.Invoke(p0);
        }

        public void OnSuccess(byte[] p0)
        {
            Success?.Invoke(p0);
        }
    }

    public class PaymentRequestListener : Java.Lang.Object, IPaymentRequestListener
    {
        public Action<PaymentRequest, string, IPaymentDataCallback> PaymentDataRequested;
        public Action<PaymentRequest, PaymentRequestResult> PaymentResult;
        public void OnPaymentDataRequested(PaymentRequest p0, string p1, IPaymentDataCallback p2)
        {
            PaymentDataRequested?.Invoke(p0, p1, p2);
        }

        public void OnPaymentResult(PaymentRequest p0, PaymentRequestResult p1)
        {
            PaymentResult?.Invoke(p0, p1);
        }
    }
   
}

