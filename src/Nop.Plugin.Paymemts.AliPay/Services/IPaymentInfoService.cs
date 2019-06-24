using System;
using System.Collections.Generic;
using System.Text;
using Nop.Plugin.Payments.AliPay.Domain;

namespace Nop.Plugin.Payments.AliPay.Services
{
    public interface IPaymentInfoService
    {
        void Delete(PaymentInfo paymentInfo);
        void Insert(PaymentInfo paymentInfo);
        void Update(PaymentInfo paymentInfo);

        IList<PaymentInfo> GetAll();

        PaymentInfo GetById(int paymentInfoId);
        PaymentInfo GetByOrderId(int orderId);
    }
}
