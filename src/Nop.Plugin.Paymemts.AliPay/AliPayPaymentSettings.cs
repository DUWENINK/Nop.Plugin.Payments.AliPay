using System;
using System.Collections.Generic;
using System.Text;
using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.AliPay
{
    public class AliPayPaymentSettings : ISettings
    {
        /// <summary>
        /// 卖家Email
        /// </summary>
        public string SellerEmail { get; set; }
        /// <summary>
        /// Key
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// PID
        /// </summary>
        public string Partner { get; set; }
        /// <summary>
        /// 额外费用
        /// </summary>
        public decimal AdditionalFee { get; set; }
    }
}
