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
using Com.Adyen.UI.Adapters;

namespace CustomUiApplication
{
    public class PaymentMethodSelectionFragment : Android.Support.V4.App.Fragment
    {
        private static String TAG = "More from Naxam";
        private PaymentMethodSelectionListener paymentMethodSelectionListener;
        private List<PaymentMethod> paymentMethods = new List<PaymentMethod>();// maybe change this later

        /**
         * The listener interface for receiving payment method selection result.
         * Container Activity must implement this interface.
         */

        public interface PaymentMethodSelectionListener
        {
            void onPaymentMethodSelected(PaymentMethod paymentMethod);
        }

        public override void OnAttach(Context context)
        {
            base.OnAttach(context);
            this.paymentMethodSelectionListener = (PaymentMethodSelectionListener)context;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // return base.OnCreateView(inflater, container, savedInstanceState);
            View view = inflater.Inflate(Resource.Layout.payment_method_selection_fragment, container, false);
            PaymentListAdapter paymentListAdapter = new PaymentListAdapter(Activity, paymentMethods);
            ListView listView = (ListView)view.FindViewById(Android.Resource.Id.List);
            listView.SetAdapter(paymentListAdapter);
            listView.ItemClick += (s, e) =>
            {
                PaymentMethod selected = paymentMethods[e.Position];
                paymentMethodSelectionListener.onPaymentMethodSelected(selected);

            };
            paymentMethods.Clear();
          //  paymentMethods.AddRange(((MainActivity)Activity).getPreferredPaymentMethods);
           // paymentMethods.AddRange(((MainActivity)getActivity()).getAvailablePaymentMethods());
            paymentListAdapter.NotifyDataSetChanged();

            // Inflate the layout for this fragment
            return view;


        }


    }
}