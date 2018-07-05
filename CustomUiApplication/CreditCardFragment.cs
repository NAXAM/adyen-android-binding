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
using Adyen.Com.AdyenCSE.Pojo;
using Java.Util;
using Android.Util;

namespace CustomUiApplication
{
    public class CreditCardFragment : Fragment
    {
        private static String TAG = "CreditCardFragment";
        private CreditCardInfoListener creditCardInfoListener;
        private bool oneClick;
        private String publicKey;
        private String generationTime;

        /**
         * The listener interface for receiving payment method selection result.
         * Container Activity must implement this interface.
         */
        public interface CreditCardInfoListener
        {
            void onCreditCardInfoProvided(String paymentDetails);
        }
        public void setCreditCardInfoListener(CreditCardInfoListener creditCardInfoListener)
        {
            this.creditCardInfoListener = creditCardInfoListener;
        }
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            oneClick = Arguments.GetBoolean("oneClick");
            publicKey = Arguments.GetString("public_key");
            generationTime = Arguments.GetString("generation_time");
            View view;
            if (oneClick)
            {
                view = inflater.Inflate(Resource.Layout.credit_card_one_click_form, container, false);
                EditText cvcView = ((EditText)view.FindViewById(Resource.Id.credit_card_cvc));
                view.FindViewById(Resource.Id.collectCreditCardData).Click += (s, e) =>
                {
                    if (creditCardInfoListener != null)
                    {
                        creditCardInfoListener.onCreditCardInfoProvided(cvcView.Text.ToString());
                    }
                    else
                    {
                        Log.Warn(TAG, "No listener provided.");
                    }

                };
            }
            else
            {
                view = inflater.Inflate(Resource.Layout.credit_card_form, container, false);
                EditText creditCardNoView = ((EditText)view.FindViewById(Resource.Id.credit_card_no));
                EditText expiryDateView = ((EditText)view.FindViewById(Resource.Id.credit_card_exp_date));
                EditText cvcView = ((EditText)view.FindViewById(Resource.Id.credit_card_cvc));

                view.FindViewById(Resource.Id.collectCreditCardData).Click += (s, e) =>
                {
                    Card card = new Card();
                    card.Number = creditCardNoView.Text.ToString();
                    card.CardHolderName = "checkout shopper";
                    card.Cvc = cvcView.Text.ToString();
                    card.ExpiryMonth = expiryDateView.Text.Substring(0, 2).ToString();
                    card.ExpiryYear = "20" + expiryDateView.Text.Substring(2, 4).ToString();
                    card.GenerationTime = new Date();


                    String creditCardData = card.Serialize(publicKey);
                    if (creditCardInfoListener != null)
                    {
                        creditCardInfoListener.onCreditCardInfoProvided(creditCardData);
                    }
                    else
                    {
                        Log.Warn(TAG, "No listener provided.");
                    }
                };
            }
            return view;
        }
    }
}