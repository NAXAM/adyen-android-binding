using Android.App;
using Android.Widget;
using Android.OS;
using Android.Support.V4.App;
using System;
using Android.Content;
using Com.Adyen.Core.Interfaces;
using Com.Adyen.Core;
using Com.Adyen.Core.Models;
using Com.Adyen.Core.Models.Paymentdetails;
using System.Collections.Generic;
using Org.Json;
using Com.Adyen.Core.Utils;
using CheckoutDemoQs;
using Java.Nio.Charset;
using Android.Text;
using Com.Adyen.UI.Fragments;
using Android.Support.CustomTabs;

namespace CustomWithCheckOutUiQs
{
    [Activity(Label = "CustomWithCheckOutUiQs", MainLauncher = true)]
    public class MainActivity : FragmentActivity, IPaymentRequestListener, PaymentDataEntryFragment.PaymentRequestListener
    {
        // CreditCardFragment
        // maybe chane three properties later in  BuildConfig class
        private string SERVER_URL;
        private string API_KEY;
        private string API_HEADER_KEY;
        //
        private static String TAG = "MoreJump from Naxam";

        private PaymentSetupRequest paymentSetupRequest;

        private static String SETUP = "setup";
        private static String VERIFY = "verify";

        // Add the URL for your server here; or you can use the demo server of Adyen: https://checkoutshopper-test.adyen.com/checkoutshopper/demoserver/
        private static String merchantServerUrl = "";

        // Add the api secret key for your server here; you can retrieve this key from customer area.
        private static String merchantApiSecretKey = "";

        // Add the header key for merchant server api secret key here; e.g. "x-demo-server-api-key"
        private static String merchantApiHeaderKeyForApiSecretKey = "";

        private static Context context;
        private IUriCallback uriCallback;

        private PaymentRequest paymentRequest;
        //
        private PaymentRequestDetailsListener paymentRequestDetailsListener;




        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
           // SetContentView(Resource.Layout.Main);

            paymentRequestDetailsListener = new PaymentRequestDetailsListener
            {
                PaymentMethodSelectionRequired = (paymentRequest, recurringMethods, otherMethods, callback) =>
                {
                    PaymentMethodSelectionFragment paymentMethodSelectionFragment
                        = new PaymentMethodSelectionFragmentBuilder()
                        .SetPaymentMethods(new List<PaymentMethod>(otherMethods))
                        .SetPreferredPaymentMethods(new List<PaymentMethod>(recurringMethods))
                        .SetPaymentMethodSelectionListener(new PaymentMethodSelectionListener
                        {
                            PaymentMethodSelected = (paymentMethod) =>
                            {
                                callback.CompletionWithPaymentMethod(paymentMethod);
                            }

                        })
                        .Build();
                    SupportFragmentManager.BeginTransaction().Replace(Android.Resource.Id.Content, paymentMethodSelectionFragment).AddToBackStack(null).CommitAllowingStateLoss();
                },
                RedirectRequired = (paymentRequest, redirectUrl, returnUriCallback) =>
                {
                    uriCallback = returnUriCallback;
                    CustomTabsIntent.Builder builder = new CustomTabsIntent.Builder();
                    CustomTabsIntent customTabsIntent = builder.Build();
                    customTabsIntent.LaunchUrl(context, Android.Net.Uri.Parse(redirectUrl));

                },

                //
                PaymentDetailsRequired = (paymentRequest, inputDetails, callback) =>
                {
                    String paymentMethodType = paymentRequest.PaymentMethod.GetType();

                    if (PaymentMethod.Type.Card.Equals(paymentMethodType))
                    {
                        CreditCardFragment creditCardFragment = new CreditCardFragmentBuilder()
                                .SetPaymentMethod(paymentRequest.PaymentMethod)
                                .SetPublicKey(paymentRequest.PublicKey)
                                .SetGenerationtime(paymentRequest.GenerationTime)
                                .SetAmount(paymentRequest.Amount)
                                .SetShopperReference(paymentRequest.ShopperReference)
                                .SetCreditCardInfoListener(new CreditCardInfoListener
                                {
                                    CreditCardInfoProvided = (p0) =>
                                    {
                                        callback.CompletionWithPaymentDetails(p0);

                                    }

                                })
                                .Build();

                        SupportFragmentManager.BeginTransaction()
                                .Replace(Android.Resource.Id.Content, creditCardFragment).AddToBackStack(null)
                                .CommitAllowingStateLoss();
                    }
                    else if (PaymentMethod.Type.Ideal.Equals(paymentMethodType))
                    {
                        IssuerSelectionFragment issuerSelectionFragment = new IssuerSelectionFragmentBuilder()
                                .SetPaymentMethod(paymentRequest.PaymentMethod)
                                .SetIssuerSelectionListener(new IssuerSelectionListener
                                {
                                    IssuerSelected = (p0) =>
                                    {
                                        IdealPaymentDetails idealPaymentDetails = new IdealPaymentDetails(inputDetails);
                                        idealPaymentDetails.FillIssuer(p0);
                                        callback.CompletionWithPaymentDetails(idealPaymentDetails);
                                    }

                                })

                        .Build();

                        SupportFragmentManager.BeginTransaction()
                                .Replace(Android.Resource.Id.Content, issuerSelectionFragment).AddToBackStack(null)
                                .CommitAllowingStateLoss();
                    }
                    else if (PaymentMethod.Type.SepaDirectDebit.Equals(paymentMethodType))
                    {
                        SepaDirectDebitFragment sepaDirectDebitFragment = new SepaDirectDebitFragmentBuilder()
                                .SetAmount(paymentRequest.Amount)
                                .SetSEPADirectDebitPaymentDetailsListener(new SEPADirectDebitPaymentDetailsListener
                                {
                                    PaymentDetails = (iban, accountHolder) =>
                                      {
                                          SepaDirectDebitPaymentDetails sepaDirectDebitPaymentDetails = new SepaDirectDebitPaymentDetails(inputDetails);
                                          sepaDirectDebitPaymentDetails.FillIban(iban);
                                          sepaDirectDebitPaymentDetails.FillOwner(accountHolder);
                                          callback.CompletionWithPaymentDetails(sepaDirectDebitPaymentDetails);
                                      }

                                })
                        .Build();

                        SupportFragmentManager.BeginTransaction()
                                 .Replace(Android.Resource.Id.Content, sepaDirectDebitFragment).AddToBackStack(null)
                            .CommitAllowingStateLoss();
                    }
                    else
                    {
                        //Log.w(TAG, "UI for " + paymentMethodType + " has not been implemented.");
                        Toast.MakeText(this, "UI for " + paymentMethodType + " has not been implemented.", ToastLength.Long).Show();
                        paymentRequest.Cancel();
                    }
                }

            };

            //            Log.d(TAG, "onCreate()");
            context = this;
            Android.Net.Uri uri = Intent.Data;
            if (uri == null)
            {
                setupInitScreen();
            }
            else
            {
                //throw new IllegalStateException("Application was supposed to be declared singleTask");
            }

        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            if (uriCallback != null)
            {
                //Log.d(TAG, "Notifying paymentRequest about return URI");
                uriCallback.CompletionWithUri(intent.Data);
            }
        }
        private void setupInitScreen()
        {
            PaymentDataEntryFragment paymentDataEntryFragment = new PaymentDataEntryFragment();
            SupportFragmentManager.BeginTransaction().Replace(Android.Resource.Id.Content,
                   paymentDataEntryFragment).CommitAllowingStateLoss();
        }

        public void onPaymentRequested(PaymentSetupRequest paymentSetupRequest)
        {
            merchantServerUrl = TextUtils.IsEmpty(merchantServerUrl) ? SERVER_URL : merchantServerUrl;
            merchantApiSecretKey = TextUtils.IsEmpty(merchantApiSecretKey) ? API_KEY : merchantApiSecretKey;
            merchantApiHeaderKeyForApiSecretKey = TextUtils.IsEmpty(merchantApiHeaderKeyForApiSecretKey)
                    ? API_HEADER_KEY
                    : merchantApiHeaderKeyForApiSecretKey;

            if (TextUtils.IsEmpty(merchantApiSecretKey)
                    || TextUtils.IsEmpty(merchantApiHeaderKeyForApiSecretKey)
                    || TextUtils.IsEmpty(merchantServerUrl))
            {
                //Toast.makeText(getApplicationContext(), "Server parameters have not been configured correctly", Toast.LENGTH_SHORT).show();
                return;
            }

            this.paymentSetupRequest = paymentSetupRequest;
            if (paymentRequest != null)
            {
                paymentRequest.Cancel();
            }
            paymentRequest = new PaymentRequest(this, this, paymentRequestDetailsListener);
            paymentRequest.Start();
        }

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
        private static void verifyPayment(Payment payment)
        {
            string resultString = "".ToString();
            JSONObject jsonObject = new JSONObject();
            jsonObject.Put("payload", payment.Payload);

            string verifystring = (string)jsonObject;
            //Map<string, string> headers = new HashMap<>();
            //headers.Put("Content-Type", "application/json; charset=UTF-8");
            //headers.Put(merchantApiHeaderKeyForApiSecretKey, merchantApiSecretKey);
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

        #region implemeting IPaymentRequestListener

        public void OnPaymentDataRequested(PaymentRequest paymentRequest, string token, IPaymentDataCallback callback)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Content-Type", "application/json; charset=UTF-8");
            headers.Add(merchantApiHeaderKeyForApiSecretKey, merchantApiSecretKey);
            AsyncHttpClient.Post(merchantServerUrl + SETUP, headers, GetSetupDataString(token), new HttpResponseCallback
            {
                Success = (response) =>
                {
                    callback.CompletionWithPaymentData(response);

                },
                Failure = (p0) =>
                {
                    paymentRequest.Cancel();

                }

            });
        }

        public void OnPaymentResult(PaymentRequest paymentRequest, PaymentRequestResult paymentResult)
        {
            string resultString;
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

       
        #endregion
    }

    public class CreditCardInfoListener : Java.Lang.Object, CreditCardFragment.ICreditCardInfoListener
    {
        public Action<CreditCardPaymentDetails> CreditCardInfoProvided;
        public void OnCreditCardInfoProvided(CreditCardPaymentDetails p0)
        {
            CreditCardInfoProvided?.Invoke(p0);
        }

    }

    public class SEPADirectDebitPaymentDetailsListener : Java.Lang.Object, SepaDirectDebitFragment.ISEPADirectDebitPaymentDetailsListener
    {
        public Action<string, string> PaymentDetails;
        public void OnPaymentDetails(string p0, string p1)
        {
            PaymentDetails?.Invoke(p0, p1);
        }
    }
    public class IssuerSelectionListener : Java.Lang.Object, IssuerSelectionFragment.IIssuerSelectionListener
    {
        public Action<string> IssuerSelected;


        public void OnIssuerSelected(string p0)
        {
            IssuerSelected?.Invoke(p0);
        }
    }

    public class PaymentMethodSelectionListener : Java.Lang.Object, PaymentMethodSelectionFragment.IPaymentMethodSelectionListener
    {
        public Action<PaymentMethod> PaymentMethodSelected;
        public void OnPaymentMethodSelected(PaymentMethod p0)
        {
            PaymentMethodSelected?.Invoke(p0);
        }
    }

    public class PaymentRequestDetailsListener : Java.Lang.Object, IPaymentRequestDetailsListener
    {
        public Action<PaymentRequest, ICollection<InputDetail>, IPaymentDetailsCallback> PaymentDetailsRequired;
        public Action<PaymentRequest, IList<PaymentMethod>, IList<PaymentMethod>, IPaymentMethodCallback> PaymentMethodSelectionRequired;
        public Action<PaymentRequest, string, IUriCallback> RedirectRequired;

        public void OnPaymentDetailsRequired(PaymentRequest p0, ICollection<InputDetail> p1, IPaymentDetailsCallback p2)
        {
            PaymentDetailsRequired?.Invoke(p0, p1, p2);
        }

        public void OnPaymentMethodSelectionRequired(PaymentRequest p0, IList<PaymentMethod> p1, IList<PaymentMethod> p2, IPaymentMethodCallback p3)
        {
            PaymentMethodSelectionRequired?.Invoke(p0, p1, p2, p3);
        }

        public void OnRedirectRequired(PaymentRequest p0, string p1, IUriCallback p2)
        {
            RedirectRequired?.Invoke(p0, p1, p2);
        }
    }

}

