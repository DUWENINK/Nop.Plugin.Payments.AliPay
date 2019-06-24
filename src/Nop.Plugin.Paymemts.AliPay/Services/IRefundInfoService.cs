using System;
using System.Collections.Generic;
using System.Text;
using Nop.Plugin.Payments.AliPay.Domain;

namespace Nop.Plugin.Payments.AliPay.Services
{
    public interface IRefundInfoService
    {
        void Delete(RefundInfo refundInfo);
        void Insert(RefundInfo refundInfo);
        void Update(RefundInfo refundInfo);

        IList<RefundInfo> GetAll();

        RefundInfo GetById(int refundInfoId);
        RefundInfo GetByOrderId(int orderId);
        /// <summary>
        /// 根据退款批次号获取退款信息
        /// </summary>
        /// <param name="Batch_no">批次号</param>
        /// <returns></returns>
        RefundInfo GetRefundInfoByBatch_no(string Batch_no);
    }
}
