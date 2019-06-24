using System.Web;
using System.Text;
using System.IO;
using System.Net;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;

namespace Nop.Plugin.Payments.AliPay.AliPay
{
    public class AlipayConfig
    {

        //↓↓↓↓↓↓↓↓↓↓请在这里配置您的基本信息↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓

        //支付宝网关地址（新）
        public string GATEWAY_NEW = "https://mapi.alipay.com/gateway.do";

        //*  合作身份者ID，签约账号，以2088开头由16位纯数字组成的字符串，查看地址：https://b.alipay.com/order/pidAndKey.htm
        public string partner = "";

        //@ 收款支付宝账邮箱
        public string seller_email = "";

        //@ 收款支付宝账号，以2088开头由16位纯数字组成的字符串，一般情况下收款账号就是签约账号
        public string seller_id = "";

        // MD5密钥，安全检验码，由数字和字母组成的32位字符串，查看地址：https://b.alipay.com/order/pidAndKey.htm
        public string key = "";

        // * 签名方式
        public string sign_type = "MD5";

        // 调试用，创建TXT日志文件夹路径，见AlipayCore.cs类中的LogResult(string sWord)打印方法。
        public string log_path = "log\\";

        // * 字符编码格式 目前支持 gbk 或 utf-8
        public string input_charset = "utf-8";


        //↑↑↑↑↑↑↑↑↑↑请在这里配置您的基本信息↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑


    }
}
