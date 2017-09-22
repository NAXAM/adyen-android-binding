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
using Com.Adyen.Core.Models;
using Com.Adyen.Core.Utils;
using Android.Util;

namespace CustomUiApplication
{
    public class PaymentDataEntryFragment : Android.Support.V4.App.Fragment
    {
        private static String TAG = "PaymentDataEntryFragment";
        private PaymentRequestListener paymentRequestListener;
        private PaymentSetupRequest paymentSetupRequest;
        private View fragmentView;

        /**
         * The listener interface for receiving payment request actions.
         * Container Activity must implement this interface.
         */
        public interface PaymentRequestListener
        {
            void onPaymentRequested(PaymentSetupRequest paymentSetupRequest);
        }

        public override void OnAttach(Context context)
        {
            base.OnAttach(context);
            this.paymentRequestListener = (PaymentRequestListener)context;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            fragmentView = inflater.Inflate(Resource.Layout.activity_main, container, false);
            Button proceedButton = (Button)fragmentView.FindViewById(Resource.Id.proceed_button);
            proceedButton.Click += (s, e) =>
            {
                paymentSetupRequest = buildPaymentRequest(fragmentView);
                paymentRequestListener.onPaymentRequested(paymentSetupRequest);
            };
            return fragmentView;
        }

        private PaymentSetupRequest buildPaymentRequest(View view)
        {
            Log.Verbose(TAG, "buildPaymentRequest()");
            PaymentSetupRequest paymentRequest = new PaymentSetupRequest();
            String amountValueString = ((EditText)view.FindViewById(Resource.Id.orderAmountEntry)).Text.ToString();
            String amountCurrencyString = ((EditText)view.FindViewById(Resource.Id.orderCurrencyEntry)).Text.ToString();
            paymentRequest.setAmount(new Amount(AmountUtil.ParseMajorAmount(amountCurrencyString, amountValueString),amountCurrencyString));
            paymentRequest.setCountryCode(((EditText)view.FindViewById(Resource.Id.countryEntry)).Text.ToString());
            paymentRequest.setShopperLocale(((EditText)view.FindViewById(Resource.Id.shopperLocaleEntry)).Text.ToString());
            paymentRequest.setShopperIP(((EditText)view.FindViewById(Resource.Id.shopperIpEntry)).Text.ToString());
            paymentRequest.setMerchantAccount(((EditText)view.FindViewById(Resource.Id.merchantAccountEntry)).Text.ToString());
            paymentRequest.setMerchantReference(((EditText)view.FindViewById(Resource.Id.merchantReferenceEntry)).Text.ToString());
            paymentRequest.setPaymentDeadline(((EditText)view.FindViewById(Resource.Id.paymentDeadlineEntry)).Text.ToString());
            paymentRequest.setReturnURL(((EditText)view.FindViewById(Resource.Id.returnUrlEntry)).Text.ToString());
            return paymentRequest;
        }

    }
}