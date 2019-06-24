using System;
using System.Collections.Generic;
using System.Text;
using Nop.Core.Data;
using System.Linq;
using Nop.Plugin.Payments.AliPay.Domain;

namespace Nop.Plugin.Payments.AliPay.Services
{
    public partial class PaymentInfoService : IPaymentInfoService
    {
        #region 属性
        private readonly IRepository<PaymentInfo> _paymentInfoRepository;
        #endregion
        #region 构造
        public PaymentInfoService(IRepository<PaymentInfo> paymentInfoRepository)
        {
            this._paymentInfoRepository = paymentInfoRepository;
        }

        public void Delete(PaymentInfo paymentInfo)
        {
            if (paymentInfo == null)
                throw new ArgumentNullException("paymentInfo");

            _paymentInfoRepository.Delete(paymentInfo);
        }

        public IList<PaymentInfo> GetAll()
        {
            var query = from p in _paymentInfoRepository.Table
                        //orderby p.Id
                        select p;
            var records = query.ToList();
            return records;
        }

        public PaymentInfo GetById(int paymentInfoId)
        {
            if (paymentInfoId == 0)
                return null;

            return _paymentInfoRepository.GetById(paymentInfoId);
        }

        public PaymentInfo GetByOrderId(int orderId)
        {
            if (orderId == 0)
                return null;
            var query = from p in _paymentInfoRepository.Table
                        where p.OrderId == orderId
                        select p;
            var records = query.FirstOrDefault();
            return records;
        }


        public void Insert(PaymentInfo paymentInfo)
        {
            if (paymentInfo == null)
                throw new ArgumentNullException("paymentInfo");

            _paymentInfoRepository.Insert(paymentInfo);
        }

        public void Update(PaymentInfo paymentInfo)
        {
            if (paymentInfo == null)
                throw new ArgumentNullException("paymentInfo");

            _paymentInfoRepository.Update(paymentInfo);
        }
        #endregion
    }
}
