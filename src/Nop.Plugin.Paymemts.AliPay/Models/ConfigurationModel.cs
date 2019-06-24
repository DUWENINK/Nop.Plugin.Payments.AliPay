using System;
using System.Collections.Generic;
using System.Text;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.AliPay.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Payments.AliPay.SellerEmail")]
        public string SellerEmail { get; set; }
        public bool SellerEmail_OverrideForStore { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Payments.AliPay.Key")]
        public string Key { get; set; }
        public bool Key_OverrideForStore { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Payments.AliPay.Partner")]
        public string Partner { get; set; }
        public bool Partner_OverrideForStore { get; set; }
        [NopResourceDisplayName("Nop.Plugin.Payments.AliPay.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }
    }
}
