using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V4.App;
using System;
using Com.Adyen.Core;
using CheckoutDemoQs;
using System.Collections.Generic;
using Com.Adyen.Core.Utils;
using Org.Json;
using Java.Lang;
using Android.Content;
using Com.Adyen.Core.Models;
using Android.Text;
using Android.Util;

namespace AppQs
{
    [Activity(Label = "AppQs", MainLauncher = true)]
    public class MainActivity : FragmentActivity, PaymentDataEntryFragment.PaymentRequestListener
    {
        private string SERVER_URL = "https://checkoutshopper-test.adyen.com/checkoutshopper/demoserver/";
        private string API_KEY = "0101328667EE5CD5932B441CFA24838B6A2CB2F7B97EC65908805E154D53CA74A4B96966AEEA90E1099717DF0722C58B1E9E40936910C15D5B0DBEE47CDCB5588C48224C6007";
        private string API_HEADER_KEY = "x-demo-server-api-key";
        //
        private static string TAG = "MainActivity";
        private PaymentSetupRequest paymentSetupRequest;
        private static string SETUP = "setup";
        private static string VERIFY = "verify";

        // Add the URL for your server here; or you can use the demo server of Adyen: https://checkoutshopper-test.adyen.com/checkoutshopper/demoserver/
        private string merchantServerUrl = "";

        // Add the api secret key for your server here; you can retrieve this key from customer area.
        private string merchantApiSecretKey = "";

        // Add the header key for merchant server api secret key here; e.g. "x-demo-server-api-key"
        private string merchantApiHeaderKeyForApiSecretKey = "";

        private PaymentRequest paymentRequest;

        private PaymentRequestListener paymentRequestListener;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            string resultString = "".ToString();
            paymentRequestListener = new PaymentRequestListener
            {
                PaymentDataRequested = (request, token, callback) =>
                {
                    if (paymentRequest != request)
                    {
                        Log.Debug(TAG, "onPaymentDataRequested(): This is not the payment request that we created.");
                        return;
                    }

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
                            paymentRequest.Cancel();
                        }

                    });
                },
                PaymentResult = (request, paymentResult) =>
                {
                    if (paymentRequest != request)
                    {
                        Log.Debug(TAG, "onPaymentResult(): This is not the payment request that we created.");
                        return;
                    }
                    string MresultString;
                    if (paymentResult.IsProcessed)
                    {
                        MresultString = paymentResult.Payment.GetPaymentStatus().ToString();
                        verifyPayment(paymentResult.Payment);
                    }
                    else
                    {
                        MresultString = paymentResult.Error.ToString();
                    }

                    Intent intent = new Intent(ApplicationContext, typeof(PaymentResultActivity));
                    intent.PutExtra("Result", MresultString);
                    intent.AddFlags(ActivityFlags.ClearTop);
                    intent.AddFlags(ActivityFlags.NewTask);
                    StartActivity(intent);
                    Finish(); // continuing these lines after add verifyPayment method
                }


            };
            PaymentDataEntryFragment paymentDataEntryFragment = new PaymentDataEntryFragment();
            SupportFragmentManager.BeginTransaction().Replace(Android.Resource.Id.Content,
                    paymentDataEntryFragment).CommitAllowingStateLoss();

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
            paymentRequest = new PaymentRequest(this, paymentRequestListener);
            paymentRequest.Start();
        }

        private string getSetupDataString(string token)
        {
            JSONObject jsonObject = new JSONObject();

            jsonObject.Put("merchantAccount", paymentSetupRequest.getMerchantAccount()); // Not required when communicating with merchant server
            jsonObject.Put("shopperLocale", paymentSetupRequest.getShopperLocale());
            jsonObject.Put("token", token);
            jsonObject.Put("returnUrl", "example-shopping-app://");
            jsonObject.Put("countryCode", paymentSetupRequest.getCountryCode());
            JSONObject amount = new JSONObject();
            amount.Put("value", paymentSetupRequest.getAmount().Value);
            amount.Put("currency", paymentSetupRequest.getAmount().Currency);
            jsonObject.Put("amount", amount);
            jsonObject.Put("channel", "android");
            jsonObject.Put("reference", "Android Checkout SDK Payment: " + DateTime.Now.ToString());
            jsonObject.Put("shopperReference", "example-customer@exampleprovider");

            short maxNumberOfInstallments = Short.ParseShort(paymentSetupRequest.getMaxNumberOfInstallments());
            if (maxNumberOfInstallments > 1)
            {
                JSONObject configuration = new JSONObject();
                JSONObject installments = new JSONObject();
                installments.Put("maxNumberOfInstallments", maxNumberOfInstallments);
                configuration.Put("installments", installments);
                jsonObject.Put("configuration", configuration);
            }

            return jsonObject.ToString();
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
                e.PrintStackTrace();
                Toast.MakeText(this, "Failed to verify payment.", ToastLength.Long).Show();
                return;
            }
            string verifyString = jsonObject.ToString();
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "application/json; charset=UTF-8");
            headers.Add(merchantApiHeaderKeyForApiSecretKey, merchantApiSecretKey);
            string resultString = "".ToString();
            AsyncHttpClient.Post(merchantServerUrl + VERIFY, headers, verifyString, new HttpResponseCallback
            {
                Success = (response) =>
                    {
                        try
                        {
                            JSONObject jsonVerifyResponse = new JSONObject(response.ToString());
                            string authResponse = jsonVerifyResponse.GetString("authResponse");
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
                            e.PrintStackTrace();
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


    }


}

