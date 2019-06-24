using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.AliPay.AliPay;
using Nop.Plugin.Payments.AliPay.Controllers;
using Nop.Plugin.Payments.AliPay.Data;
using Nop.Plugin.Payments.AliPay.Domain;
using Nop.Plugin.Payments.AliPay.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.Plugins;

namespace Nop.Plugin.Payments.AliPay
{
    public class AliPayPaymentProcessor : BasePlugin, IPaymentMethod
    {

        #region 属性

        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;
        private readonly IStoreContext _storeContext;
        private readonly AliPayPaymentSettings _aliPayPaymentSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly AliPayObjectContext _objectContext;
        private readonly IPaymentInfoService _paymentInfoService;
        private readonly IRefundInfoService _refundInfoService;
        #endregion

        #region 构造

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settingService"></param>
        /// <param name="webHelper"></param>
        /// <param name="storeContext"></param>
        /// <param name="aliPayPaymentSettings"></param>
        /// <param name="localizationService"></param>
        /// <param name="workContext"></param>
        /// <param name="objectContext"></param>
        /// <param name="paymentInfoService"></param>
        /// <param name="refundInfoService"></param>
        public AliPayPaymentProcessor(
            ISettingService settingService,
            IWebHelper webHelper,
            IStoreContext storeContext,
            AliPayPaymentSettings aliPayPaymentSettings,
            ILocalizationService localizationService,
            IWorkContext workContext,
            AliPayObjectContext objectContext,
            IPaymentInfoService paymentInfoService,
            IRefundInfoService refundInfoService)
        {
            this._settingService = settingService;
            this._webHelper = webHelper;
            this._storeContext = storeContext;
            this._aliPayPaymentSettings = aliPayPaymentSettings;
            this._localizationService = localizationService;
            this._workContext = workContext;
            this._objectContext = objectContext;
            this._paymentInfoService = paymentInfoService;
            this._refundInfoService = refundInfoService;
        }

        #endregion
        #region 辅助方法
        /// <summary> 
        /// 根据GUID获取19位的唯一数字序列 
        /// </summary> 
        /// <returns></returns> 
        public static long GuidToLongID()
        {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            return BitConverter.ToInt64(buffer, 0);
        }
        #endregion
        #region 方法

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult { NewPaymentStatus = PaymentStatus.Pending };

            return result;
        }

        #region 支付
        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var partner = _aliPayPaymentSettings.Partner;

            if (string.IsNullOrEmpty(partner))
                throw new Exception("合作身份者ID 不能为空");

            var key = _aliPayPaymentSettings.Key;

            if (string.IsNullOrEmpty(key))
                throw new Exception("MD5密钥不能为空");

            var sellerEmail = _aliPayPaymentSettings.SellerEmail;

            if (string.IsNullOrEmpty(sellerEmail))
                throw new Exception("卖家Email 不能为空");

            var customer = _workContext.CurrentCustomer;//当前用户
            string username = customer.Username;


            //商户订单号，商户网站订单系统中唯一订单号，必填
            string out_trade_no = postProcessPaymentRequest.Order.Id.ToString().Trim();//订单编号

            //订单名称，必填
            string subject = _storeContext.CurrentStore.Name + ":订单" + out_trade_no;

            //付款金额，必填
            string total_fee = postProcessPaymentRequest.Order.OrderTotal.ToString("0.00", CultureInfo.InvariantCulture);

            //商品描述，可空
            string body = _storeContext.CurrentStore.Name + ":用户_" + username;

            //支付配置信息
            var aliPayDirectConfig = new AlipayDirectConfig()
            {
                key = _aliPayPaymentSettings.Key,
                partner = _aliPayPaymentSettings.Partner,
                seller_email = _aliPayPaymentSettings.SellerEmail,
                notify_url = _webHelper.GetStoreLocation(false) + "Plugins/AliPay/Notify",
                return_url = _webHelper.GetStoreLocation(false) + "Plugins/AliPay/Return",
                sign_type = "MD5",
                input_charset = "utf-8",
            };
            //把请求参数打包成数组
            SortedDictionary<string, string> sParaTemp = new SortedDictionary<string, string>();
            sParaTemp.Add("service", aliPayDirectConfig.service);
            sParaTemp.Add("partner", aliPayDirectConfig.partner);
            sParaTemp.Add("seller_email", aliPayDirectConfig.seller_email);
            sParaTemp.Add("payment_type", aliPayDirectConfig.payment_type);
            sParaTemp.Add("notify_url", aliPayDirectConfig.notify_url);
            sParaTemp.Add("return_url", aliPayDirectConfig.return_url);
            sParaTemp.Add("_input_charset", aliPayDirectConfig.input_charset);
            sParaTemp.Add("out_trade_no", out_trade_no);
            sParaTemp.Add("subject", subject);
            sParaTemp.Add("body", body);
            sParaTemp.Add("total_fee", total_fee);
            //创建支付宝请求
            var post = AlipaySubmit.BuildRequest(sParaTemp, aliPayDirectConfig, "POST");
            post.Post();

        }
        #endregion
        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return _aliPayPaymentSettings.AdditionalFee;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();

            result.AddError("Capture method not supported");

            return result;
        }

        #region 退款
        /// <summary>
        /// 退款
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            Order order = refundPaymentRequest.Order;
            if (order == null)
            {
                result.AddError("订单为空");
                return result;
            }
            PaymentInfo paymentInfo = _paymentInfoService.GetByOrderId(order.Id);
            if (!(paymentInfo != null && !string.IsNullOrEmpty(paymentInfo.Out_Trade_No)))
            {
                result.AddError("交易号为空");
                return result;
            }
            if (_aliPayPaymentSettings.Partner != paymentInfo.Seller_id)
            {
                result.AddError("退款合作身份者ID错误");
                return result;
            }
            if (refundPaymentRequest.AmountToRefund <= 0)
            {
                result.AddError("退款金额大于0");
                return result;
            }
            if (refundPaymentRequest.AmountToRefund + refundPaymentRequest.Order.RefundedAmount > paymentInfo.Total)
            {
                result.AddError("退款金额错误");
                return result;
            }

            //卖家账号,退款账号
            string seller_emailToRefund = paymentInfo.Seller_email;// 卖家退款账号邮箱
            string seller_user_id = paymentInfo.Seller_id;//卖家退款账号ID

            //批次号，必填，格式：当天日期[8位]+序列号[3至24位]，如：201603081000001

            string batch_no = DateTime.Now.ToString("yyyyMMdd") + GuidToLongID();//退款编号

            //退款笔数，必填，参数detail_data的值中，“#”字符出现的数量加1，最大支持1000笔（即“#”字符出现的数量999个）

            string batch_num = "1";

            //退款详细数据，必填，格式（支付宝交易号^退款金额^备注），多笔请用#隔开
            string out_trade_no = paymentInfo.Out_Trade_No;//支付宝交易号交易号
            string amountToRefund = refundPaymentRequest.AmountToRefund.ToString().TrimEnd('0');//退款金额
            string refundResult = "协商退款";//备注
            string detail_data = string.Format("{0}^{1}^{2}",
               out_trade_no,
               amountToRefund,
               refundResult
             );
            //退款通知
            string notify_url = _webHelper.GetStoreLocation(false) + "Plugins/AliPay/RefundNotify";

            //新增退款记录
            var refundInfo = new RefundInfo()
            {
                OrderId = refundPaymentRequest.Order.Id,
                Batch_no = batch_no,
                AmountToRefund = refundPaymentRequest.AmountToRefund,
                RefundStatusId = (int)RefundStatus.refunding,
                CreateOnUtc = DateTime.Now,
                Seller_Email = seller_emailToRefund,
                Seller_Id = seller_user_id,
                Out_Trade_No = out_trade_no,
            };
            _refundInfoService.Insert(refundInfo);

            ////////////////////////////////////////////////////////////////////////////////////////////////
            var alipayReturnConfig = new AlipayReturnConfig()
            {
                partner = _aliPayPaymentSettings.Partner,
                key = _aliPayPaymentSettings.Key,
                sign_type = "MD5",
                input_charset = "utf-8"
            };
            //把请求参数打包成数组
            SortedDictionary<string, string> sParaTemp = new SortedDictionary<string, string>();
            sParaTemp.Add("service", alipayReturnConfig.service);
            sParaTemp.Add("partner", alipayReturnConfig.partner);
            sParaTemp.Add("_input_charset", alipayReturnConfig.input_charset.ToLower());
            sParaTemp.Add("refund_date", alipayReturnConfig.refund_date);
            sParaTemp.Add("seller_user_id", seller_user_id);
            sParaTemp.Add("batch_no", batch_no);
            sParaTemp.Add("batch_num", batch_num);
            sParaTemp.Add("detail_data", detail_data);
            sParaTemp.Add("notify_url", notify_url);

            var post = AlipaySubmit.BuildRequest(sParaTemp, alipayReturnConfig, "POST");
            post.Post();

            result.AddError("退款请求已提交,请到支付宝网站中进行退款确认");//必须有,否则影响退款金额
            return result;
        }
        #endregion
        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();

            result.AddError("Void method not supported");

            return result;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();

            result.AddError("Recurring payment not supported");

            return result;
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();

            result.AddError("Recurring payment not supported");

            return result;
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //AliPay is the redirection payment method
            //It also validates whether order is also paid (after redirection) so customers will not be able to pay twice

            //payment status should be Pending
            if (order.PaymentStatus != PaymentStatus.Pending)
                return false;

            //let's ensure that at least 1 minute passed after order is placed
            return !((DateTime.UtcNow - order.CreatedOnUtc).TotalMinutes < 1);
        }

        /// <summary>
        /// 配置路由
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "AliPay";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.AliPay.Controllers" }, { "area", null } };
        }

        /// <summary>
        /// 支付信息路由
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "AliPay";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.AliPay.Controllers" }, { "area", null } };
        }

        public Type GetControllerType()
        {
            return typeof(PaymentAliPayController);
        }

        #region 插件安装/卸载

        public override void Install()
        {
            //配置
            var settings = new AliPayPaymentSettings
            {
                SellerEmail = "",
                Key = "",
                Partner = "",
                AdditionalFee = 0,
            };

            _settingService.SaveSetting(settings);

            //安装数据表
            _objectContext.Install();

            //本地化资源
            _localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugins.Payments.AliPay.RedirectionTip", "您将被重定向到支付宝网站完成订单.");
            _localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugins.Payments.AliPay.SellerEmail", "卖方邮箱");
            _localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugins.Payments.AliPay.SellerEmail.Hint", "支付宝卖方电子邮箱.");
            _localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugins.Payments.AliPay.Key", "Key");
            _localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugins.Payments.AliPay.Key.Hint", "输入 key.");
            _localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugins.Payments.AliPay.Partner", "Partner");
            _localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugins.Payments.AliPay.Partner.Hint", "输入 partner.");
            _localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugins.Payments.AliPay.AdditionalFee", "额外费用");
            _localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugins.Payments.AliPay.AdditionalFee.Hint", "客户选择此支付方式将付额外的费用.");
            _localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugins.Payments.AliPay.PaymentMethodDescription", "使用支付宝进行支付");

            base.Install();
        }

        public override void Uninstall()
        {
            //本地化资源
            _localizationService.DeletePluginLocaleResource("Nop.Plugins.Payments.AliPay.SellerEmail.RedirectionTip");
            _localizationService.DeletePluginLocaleResource("Nop.Plugins.Payments.AliPay.SellerEmail");
            _localizationService.DeletePluginLocaleResource("Nop.Plugins.Payments.AliPay.SellerEmail.Hint");
            _localizationService.DeletePluginLocaleResource("Nop.Plugins.Payments.AliPay.Key");
            _localizationService.DeletePluginLocaleResource("Nop.Plugins.Payments.AliPay.Key.Hint");
            _localizationService.DeletePluginLocaleResource("Nop.Plugins.Payments.AliPay.Partner");
            _localizationService.DeletePluginLocaleResource("Nop.Plugins.Payments.AliPay.Partner.Hint");
            _localizationService.DeletePluginLocaleResource("Nop.Plugins.Payments.AliPay.AdditionalFee");
            _localizationService.DeletePluginLocaleResource("Nop.Plugins.Payments.AliPay.AdditionalFee.Hint");
            _localizationService.DeletePluginLocaleResource("Nop.Plugins.Payments.AliPay.PaymentMethodDescription");

            //卸载数据表
            _objectContext.Uninstall();

            base.Uninstall();
        }

        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            throw new NotImplementedException();
        }

        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            throw new NotImplementedException();
        }

        public string GetPublicViewComponentName()
        {
            throw new NotImplementedException();
        }

        #endregion
        #endregion

        #region Properies

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// 支持部分退款
        /// true-支持,false-不支持
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// 支持退款
        /// true-支持,false-不支持
        /// </summary>
        public bool SupportRefund
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get
            {
                return RecurringPaymentType.NotSupported;
            }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get
            {
                return PaymentMethodType.Redirection;
            }
        }

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        public string PaymentMethodDescription
        {
            get { return _localizationService.GetResource("Nop.Plugins.Payments.AliPay.PaymentMethodDescription"); }
        }

        #endregion

    }
}
