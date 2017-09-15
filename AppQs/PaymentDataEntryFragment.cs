using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V4.App;
using CheckoutDemoQs;
using Com.Adyen.Core.Models;
using Com.Adyen.Core.Utils;

namespace AppQs
{
    public class PaymentDataEntryFragment : Fragment
    {
        private static String TAG = "Morejump from Naxam";
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

        private PaymentSetupRequest buildPaymentRequest( View view) 
        {
           // Log.v(TAG, "buildPaymentRequest()");
        PaymentSetupRequest paymentRequest = new PaymentSetupRequest();
         String amountValueString = ((EditText) view.FindViewById(Resource.Id.orderAmountEntry)).Text.ToString();
         String amountCurrencyString = ((EditText) view.FindViewById(Resource.Id.orderCurrencyEntry))
                .Text.ToString();

        paymentRequest.setAmount(new Amount(AmountUtil.ParseMajorAmount(amountCurrencyString, amountValueString),
                amountCurrencyString));
        paymentRequest.setCountryCode(((EditText) view.FindViewById(Resource.Id.countryEntry)).Text.ToString());
        paymentRequest.setShopperLocale(((EditText) view.FindViewById(Resource.Id.shopperLocaleEntry)).Text.ToString());
        paymentRequest.setMerchantAccount(((EditText) view.FindViewById(Resource.Id.merchantAccountEntry)).Text
                .ToString());
        String maxNumberOfInstallments = ((String)((Spinner)view.FindViewById(Resource.Id.installmentsEntry)).SelectedItem);
        paymentRequest.setMaxNumberOfInstallments(maxNumberOfInstallments);

        return paymentRequest;
    }

}
}