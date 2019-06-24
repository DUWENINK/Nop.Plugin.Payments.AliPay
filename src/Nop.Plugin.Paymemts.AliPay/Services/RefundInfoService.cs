using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nop.Core.Data;
using Nop.Plugin.Payments.AliPay.Domain;

namespace Nop.Plugin.Payments.AliPay.Services
{
    public partial class RefundInfoService : IRefundInfoService
    {
        #region 属性
        private readonly IRepository<RefundInfo> _refundInfoRepository;
        #endregion
        #region 构造
        public RefundInfoService(IRepository<RefundInfo> refundInfoRepository)
        {
            this._refundInfoRepository = refundInfoRepository;
        }

        public void Delete(RefundInfo refundInfo)
        {
            if (refundInfo == null)
                throw new ArgumentNullException("refundInfo");

            _refundInfoRepository.Delete(refundInfo);
        }

        public IList<RefundInfo> GetAll()
        {
            var query = from p in _refundInfoRepository.Table
                        orderby p.Id
                        select p;
            var records = query.ToList();
            return records;
        }

        public RefundInfo GetById(int refundInfoId)
        {
            if (refundInfoId == 0)
                return null;

            return _refundInfoRepository.GetById(refundInfoId);
        }

        public RefundInfo GetByOrderId(int orderId)
        {
            if (orderId == 0)
                return null;
            var query = from p in _refundInfoRepository.Table
                        where p.OrderId == orderId
                        select p;
            var records = query.FirstOrDefault();
            return records;
        }


        public void Insert(RefundInfo refundInfo)
        {
            if (refundInfo == null)
                throw new ArgumentNullException("refundInfo");

            _refundInfoRepository.Insert(refundInfo);
        }

        public void Update(RefundInfo refundInfo)
        {
            if (refundInfo == null)
                throw new ArgumentNullException("refundInfo");

            _refundInfoRepository.Update(refundInfo);
        }

        public RefundInfo GetRefundInfoByBatch_no(string Batch_no)
        {
            if (String.IsNullOrEmpty(Batch_no))
            {
                throw new Exception("参数不能为空");
            }
            var query = _refundInfoRepository.Table;
            return query.Where(x => x.Batch_no == Batch_no).FirstOrDefault();
        }
        #endregion
    }
}
