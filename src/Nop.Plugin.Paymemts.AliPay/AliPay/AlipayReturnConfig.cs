using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Payments.AliPay.AliPay
{
    public class AlipayReturnConfig : AlipayConfig
    {

        //↓↓↓↓↓↓↓↓↓↓请在这里配置您的基本信息↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓

        // 退款日期 时间格式 yyyy-MM-dd HH:mm:ss
        public string refund_date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // 调用的接口名，无需修改
        public string service = "refund_fastpay_by_platform_pwd";

        //↑↑↑↑↑↑↑↑↑↑请在这里配置您的基本信息↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑

    }
}
