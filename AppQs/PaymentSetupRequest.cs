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

namespace AppQs
{
    public class PaymentSetupRequest
    {
        private Amount amount;
        private String shopperLocale;
        private String merchantAccount;
        private String countryCode;
        private String maxNumberOfInstallments;

        public Amount getAmount()
        {
            return amount;
        }

        public void setAmount( Amount amount)
        {
            this.amount = amount;
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

        public String getMaxNumberOfInstallments()
        {
            return maxNumberOfInstallments;
        }

        public void setMaxNumberOfInstallments( String maxNumberOfInstallments)
        {
            this.maxNumberOfInstallments = maxNumberOfInstallments;
        }
    }
}