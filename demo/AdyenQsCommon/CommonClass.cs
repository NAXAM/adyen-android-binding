using Com.Adyen.Core;
using Com.Adyen.Core.Interfaces;
using Com.Adyen.Core.Models;
using Com.Adyen.Core.Models.Paymentdetails;
using Java.Lang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace AdyenQsCommon
{
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
