using System;
using System.Collections.Generic;
using System.Text;
using Nop.Core;

namespace Nop.Plugin.Payments.AliPay.Domain
{
    public partial class PaymentInfo : BaseEntity
    {
        #region Properties
        public Guid PaymentGuid { get; set; }
        /// <summary>
        /// 订单编号
        /// </summary>
        public int OrderId { get; set; }
        /// <summary>
        /// 插件SystemName
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 交易金额
        /// </summary>
        public decimal Total { get; set; }
        /// <summary>
        /// 订单编号外部交易号
        /// </summary>
        public string Out_Trade_No { get; set; }
        /// <summary>
        /// 说明
        /// </summary>
        public string Note { get; set; }
        /// <summary>
        /// 交易号，内部交易号，支付宝交易号或者微信交易号
        /// </summary>
        public string Trade_no { get; set; }
        /// <summary>
        /// 第三方交易状态
        /// </summary>
        public string Trade_status { get; set; }
        /// <summary>
        /// 收款单位email
        /// </summary>
        public string Seller_email { get; set; }
        /// <summary>
        /// 收款单位id
        /// </summary>
        public string Seller_id { get; set; }
        /// <summary>
        /// 付款账户id
        /// </summary>
        public string Buyer_id { get; set; }
        /// <summary>
        /// 付款账户email
        /// </summary>
        public string Buyer_email { get; set; }
        /// <summary>
        /// 内部订单创建时间
        /// </summary>
        public DateTime CreateDateUtc { get; set; }

        #endregion



    }
}
