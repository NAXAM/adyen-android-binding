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
using Org.Json;

namespace CustomWithCheckOutUiQs
{
    public class PaymentSetupRequest
    {
        private Amount amount;
        private String merchantReference;
        private String shopperIP;
        private String shopperLocale;
        private String merchantAccount;
        private String countryCode;
        private String paymentDeadline;
        private String returnURL;
        private String paymentToken;

        public Amount getAmount()
        {
            return amount;
        }

        public void setAmount( Amount amount)
        {
            this.amount = amount;
        }

        public String getMerchantReference()
        {
            return merchantReference;
        }

        public void setMerchantReference( String merchantReference)
        {
            this.merchantReference = merchantReference;
        }

        public String getShopperIP()
        {
            return shopperIP;
        }

        public void setShopperIP( String shopperIP)
        {
            this.shopperIP = shopperIP;
        }

        public String getShopperLocale()
        {
            return shopperLocale;
        }

        public void setShopperLocale( String shopperLocale)
        {
            this.shopperLocale = shopperLocale;
        }

        public String getMerchantAccount()
        {
            return merchantAccount;
        }

        public void setMerchantAccount( String merchantAccount)
        {
            this.merchantAccount = merchantAccount;
        }

        public String getCountryCode()
        {
            return countryCode;
        }

        public void setCountryCode( String countryCode)
        {
            this.countryCode = countryCode;
        }

        public String getPaymentDeadline()
        {
            return paymentDeadline;
        }

        public void setPaymentDeadline( String paymentDeadline)
        {
            this.paymentDeadline = paymentDeadline;
        }

        public String getReturnURL()
        {
            return returnURL;
        }

        public void setReturnURL( String returnURL)
        {
            this.returnURL = returnURL;
        }

        public String getPaymentToken()
        {
            return paymentToken;
        }

        public void setPaymentToken( String paymentToken)
        {
            this.paymentToken = paymentToken;
        }

    public String getSetupDataString()
        {
             JSONObject jsonObject = new JSONObject();
            try
            {
                jsonObject.Put("reference", merchantReference);
                jsonObject.Put("merchantAccount", merchantAccount);
                jsonObject.Put("shopperLocale", shopperLocale);

                jsonObject.Put("appUrl", returnURL);
                jsonObject.Put("countryCode", countryCode);
                jsonObject.Put("sessionValidity", paymentDeadline);

                 JSONObject paymentAmount = new JSONObject();
                paymentAmount.Put("currency", amount.Currency);
                paymentAmount.Put("value", amount.Value);
                jsonObject.Put("amount", paymentAmount);

                // HACK
                jsonObject.Put("shopperReference", "aap");

                //Device fingerprint
                jsonObject.Put("sdkToken", paymentToken);


            }
            catch ( JSONException jsonException) {
                //TODO: What to do?
            }

            return jsonObject.ToString();
            }
        }
}