using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Core.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.AliPay.AliPay;
using Nop.Plugin.Payments.AliPay.Domain;
using Nop.Plugin.Payments.AliPay.Models;
using Nop.Plugin.Payments.AliPay.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.AliPay.Controllers
{
    public class PaymentAliPayController : BasePaymentController
    {
        #region 属性
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IPermissionService _permissionService;
        private readonly INotificationService _notificationService;
        private readonly IWebHelper _webHelper;
        private readonly ShoppingCartSettings _shoppingCartSettings;
        private readonly ISettingService _settingService;
        private readonly IPaymentService _paymentService;
        private readonly IOrderService _orderService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly ILogger _logger;
        private readonly ILocalizationService _localizationService;
        private readonly PaymentSettings _paymentSettings;
        private readonly IPaymentInfoService _paymentInfoService;
        private readonly IRefundInfoService _refundInfoService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly IWorkContext _workContext;
        private readonly IStoreContext _storeContext;
        private readonly IStoreService _storeService;
        #endregion
        #region 构造

        public PaymentAliPayController(ISettingService settingService,
            IPaymentService paymentService,
            IOrderService orderService,
            IOrderProcessingService orderProcessingService,
            ILogger logger,
            ILocalizationService localizationService,
            PaymentSettings paymentSettings,
            IPaymentInfoService paymentInfoService,
            IRefundInfoService refundInfoService,
            IEventPublisher eventPublisher,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            IWorkContext workContext,
            IStoreContext storeContext,
            IStoreService storeService)
        {
            _settingService = settingService;
            _paymentService = paymentService;
            _orderService = orderService;
            _orderProcessingService = orderProcessingService;
            _logger = logger;
            _localizationService = localizationService;
            _paymentSettings = paymentSettings;
            _paymentInfoService = paymentInfoService;
            _refundInfoService = refundInfoService;
            _eventPublisher = eventPublisher;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _workContext = workContext;
            _storeContext = storeContext;
            _storeService = storeService;
        }

        #endregion

        #region 基础请求
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public ActionResult Configure()
        {
            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var aliPayPaymentSettings = _settingService.LoadSetting<AliPayPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                SellerEmail = aliPayPaymentSettings.SellerEmail,
                Key = aliPayPaymentSettings.Key,
                Partner = aliPayPaymentSettings.Partner,
                AdditionalFee = aliPayPaymentSettings.AdditionalFee,
                ActiveStoreScopeConfiguration = storeScope,
            };
            if (storeScope > 0)
            {
                model.SellerEmail_OverrideForStore = _settingService.SettingExists(aliPayPaymentSettings, x => x.SellerEmail, storeScope);
                model.Key_OverrideForStore = _settingService.SettingExists(aliPayPaymentSettings, x => x.Key, storeScope);
                model.Partner_OverrideForStore = _settingService.SettingExists(aliPayPaymentSettings, x => x.Partner, storeScope);
                model.AdditionalFee_OverrideForStore = _settingService.SettingExists(aliPayPaymentSettings, x => x.AdditionalFee, storeScope);
            }
            return View("~/Plugins/Nop.Plugin.Payments.AliPay/Views/Configure.cshtml", model);
        }


        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            var storeScope = _storeContext.ActiveStoreScopeConfiguration;
            var aliPayPaymentSettings = _settingService.LoadSetting<AliPayPaymentSettings>(storeScope);

            //save settings
            aliPayPaymentSettings.SellerEmail = model.SellerEmail;
            aliPayPaymentSettings.Key = model.Key;
            aliPayPaymentSettings.Partner = model.Partner;
            aliPayPaymentSettings.AdditionalFee = model.AdditionalFee;

            _settingService.SaveSettingOverridablePerStore(aliPayPaymentSettings, x => x.SellerEmail, model.SellerEmail_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(aliPayPaymentSettings, x => x.Key, model.Key_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(aliPayPaymentSettings, x => x.Partner, model.Partner_OverrideForStore, storeScope, false);
            _settingService.SaveSettingOverridablePerStore(aliPayPaymentSettings, x => x.AdditionalFee, model.AdditionalFee_OverrideForStore, storeScope, false);

            //now clear settings cache
            _settingService.ClearCache();
            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));
  
            return Configure();
        }

        [Area(AreaNames.Admin)]
        public ActionResult PaymentInfo()
        {
            return View("~/Plugins/Nop.Plugin.Payments.AliPay/Views/PaymentInfo.cshtml");
        }

        //[NonAction]
        //public override IList<string> ValidatePaymentForm(IFormCollection form)
        //{
        //    var warnings = new List<string>();

        //    return warnings;
        //}

        //[NonAction]
        //public override ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        //{
        //    var paymentInfo = new ProcessPaymentRequest();

        //    return paymentInfo;
        //}
        #endregion

        #region 支付请求
        /// <summary>
        /// 接收支付宝支付通知
        /// </summary>
        public ActionResult Notify(FormCollection form)
        {
            if (!(_paymentPluginManager.LoadPluginBySystemName("Nop.Plugin.Payments.AliPay") is AliPayPaymentProcessor processor) || !_paymentPluginManager.IsPluginActive(processor))
                throw new NopException("插件无法加载");
            var aliPayPaymentSettings = _settingService.LoadSetting<AliPayPaymentSettings>(_storeContext.CurrentStore.Id);
            var partner = aliPayPaymentSettings.Partner;
            if (string.IsNullOrEmpty(partner))
                throw new Exception("合作身份者ID 不能为空");
            var key = aliPayPaymentSettings.Key;
            if (string.IsNullOrEmpty(key))
                throw new Exception("MD5密钥不能为空");
            var sellerEmail = aliPayPaymentSettings.SellerEmail;
            if (string.IsNullOrEmpty(sellerEmail))
                throw new Exception("卖家Email 不能为空");
            ///↓↓↓↓↓↓↓ 获取支付宝POST过来通知消息，并以“参数名 = 参数值”的形式组成数组↓↓↓↓↓↓↓↓
            int i;
            var coll = Request.Form;
            var sortedStr = coll.Keys.ToList();

            SortedDictionary<string, string> sPara = new SortedDictionary<string, string>();
            for (i = 0; i < sortedStr.Count; i++)
            {
                sPara.Add(sortedStr[i], Request.Form[sortedStr[i]]);
            }
            ///↑↑↑↑↑↑↑ 获取支付宝POST过来通知消息，并以“参数名 = 参数值”的形式组成数组↑↑↑↑↑↑↑↑
            if (sPara.Count > 0)//判断是否有带返回参数
            {
                AlipayNotify aliNotify = new AlipayNotify(partner: partner, key: key, input_charset: "utf-8", sign_type: sPara["sign_type"]);
                var sign = Request.Form["sign"];
                var notify_id = Request.Form["notify_id"];
                bool verifyResult = aliNotify.Verify(sPara, notify_id, sign);
                if (verifyResult)//验证成功
                {
                    //商户订单号

                    string out_trade_no = Request.Form["out_trade_no"];

                    //支付宝交易号

                    string trade_no = Request.Form["trade_no"];

                    //交易状态
                    string trade_status = Request.Form["trade_status"];


                    if (coll["trade_status"] == "TRADE_FINISHED" || coll["trade_status"] == "TRADE_SUCCESS")
                    {
                        int orderId;

                        if (int.TryParse(out_trade_no, out orderId))
                        {
                            var order = _orderService.GetOrderById(orderId);

                            if (order != null && _orderProcessingService.CanMarkOrderAsPaid(order))
                            {
                                //修改订单状态
                                _orderProcessingService.MarkOrderAsPaid(order);
                                //添加付款信息
                                var paymentInfo = new PaymentInfo()
                                {
                                    OrderId = orderId,
                                    Name = processor.PluginDescriptor.SystemName,
                                    PaymentGuid = Guid.NewGuid(),
                                    Trade_no = trade_no,
                                    Total = decimal.Parse(Request.Form["price"]),
                                    Trade_status = Request.Form["trade_status"],
                                    Buyer_email = Request.Form["buyer_email"],
                                    Buyer_id = Request.Form["buyer_id"],
                                    Seller_email = Request.Form["seller_email"],
                                    Seller_id = Request.Form["seller_id"],
                                    Note = Request.Form["subject"],
                                    Out_Trade_No = Request.Form["trade_no"],
                                    CreateDateUtc = DateTime.Now,
                                };
                                _paymentInfoService.Insert(paymentInfo);
                            }
                        }
                    }
                    return Content("success"); //请不要修改或删除


                }
                else //验证失败
                {
                 
                    var logStr = string.Format("MD5:notify_id={0},sign={1}", notify_id, sign);
                    _logger.Error(logStr);
                    return Content("fail");
                }
            }
            return Content("无通知参数");
        }
        /// <summary>
        /// 支付页面跳转同步通知页面
        /// </summary>
        public ActionResult Return()
        {
            if (!(_paymentPluginManager.LoadPluginBySystemName("Nop.Plugin.Payments.AliPay") is AliPayPaymentProcessor processor) || !_paymentPluginManager.IsPluginActive(processor))
                throw new NopException("插件无法加载");
            return RedirectToAction("Index", "Home", new { area = "" });
        }
        #endregion

        #region 订单退款
        public ActionResult RefundNotify(FormCollection form)
        {
            if (!(_paymentPluginManager.LoadPluginBySystemName("Nop.Plugin.Payments.AliPay") is AliPayPaymentProcessor processor) || !_paymentPluginManager.IsPluginActive(processor))
                throw new NopException("插件无法加载");
            var aliPayPaymentSettings = _settingService.LoadSetting<AliPayPaymentSettings>(_storeContext.CurrentStore.Id);

            var partner = aliPayPaymentSettings.Partner;

            if (string.IsNullOrEmpty(partner))
                throw new Exception("合作身份者ID 不能为空");

            var key = aliPayPaymentSettings.Key;

            if (string.IsNullOrEmpty(key))
                throw new Exception("MD5密钥不能为空");

            var sellerEmail = aliPayPaymentSettings.SellerEmail;

            if (string.IsNullOrEmpty(sellerEmail))
                throw new Exception("卖家Email 不能为空");

            ///↓↓↓↓↓↓↓ 获取支付宝POST过来通知消息，并以“参数名 = 参数值”的形式组成数组↓↓↓↓↓↓↓↓
            int i;
            var coll = Request.Form;
            var sortedStr = coll.Keys.ToList();
            SortedDictionary<string, string> sPara = new SortedDictionary<string, string>();
            for (i = 0; i < sortedStr.Count; i++)
            {
                sPara.Add(sortedStr[i], Request.Form[sortedStr[i]]);
            }
            ///↑↑↑↑↑↑↑ 获取支付宝POST过来通知消息，并以“参数名 = 参数值”的形式组成数组↑↑↑↑↑↑↑↑
            if (sPara.Count > 0)//判断是否有带返回参数
            {
                AlipayNotify aliNotify = new AlipayNotify(partner: partner, key: key, input_charset: "utf-8", sign_type: sPara["sign_type"]);
                var notify_type = Request.Form["notify_type"];
                var notify_id = Request.Form["notify_id"];
                var sign = Request.Form["sign"];
                bool verifyResult = aliNotify.Verify(sPara, notify_id, sign);

                if (verifyResult)//验证成功
                {
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////                    

                    //批次号

                    string batch_no = Request.Form["batch_no"];

                    //批量退款数据中转账成功的笔数

                    string success_num = Request.Form["success_num"];

                    //批量退款数据中的详细信息
                    string result_details = Request.Form["result_details"];

                    //↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓业务处理↓↓↓↓↓↓↓↓↓↓↓↓↓↓
                    try
                    {
                        string create_time = batch_no.Substring(0, 8);
                        var refundInfo = _refundInfoService.GetRefundInfoByBatch_no(batch_no);
                        if (refundInfo != null && refundInfo.OrderId > 0)
                        {

                            if (refundInfo.RefundStatus == RefundStatus.refund || notify_id == refundInfo.Notify_Id)
                            {
                                return Content("success");
                            }

                            var result_list = result_details.Split('#');
                            var item = result_list[0];

                            refundInfo.Notify_Id = notify_id;
                            refundInfo.Notify_Type = notify_type;

                            var obj = item.Split('^');
                            var out_Trade_No = obj[0];//交易号
                            var AmountToRefund = decimal.Parse(obj[1]);//退款金额
                            var note = obj[2];//退款说明

                            var order = _orderService.GetOrderById(refundInfo.OrderId);
                            var paymentInfo = _paymentInfoService.GetByOrderId(refundInfo.OrderId);
                            if (order != null)
                            {
                                if (note.ToUpper() == "SUCCESS")
                                {

                                    if (AmountToRefund >= 0 && AmountToRefund == refundInfo.AmountToRefund)
                                    {
                                        #region 成功
                                        order.OrderNotes.Add(new OrderNote
                                        {
                                            Note = string.Format("支付宝退款成功,退款编号:{0},退款金额:{1},交易号:{2},说明:{3}", batch_no, AmountToRefund, out_Trade_No, note),
                                            DisplayToCustomer = true,
                                            CreatedOnUtc = DateTime.UtcNow
                                        });

                                        ////总退款
                                        decimal totalAmountRefunded = Math.Abs(order.RefundedAmount) + AmountToRefund;

                                        order.RefundedAmount = totalAmountRefunded;

                                        if (paymentInfo.Total > order.RefundedAmount)
                                        {
                                            order.PaymentStatus = PaymentStatus.PartiallyRefunded;
                                        }
                                        else
                                        {
                                            order.PaymentStatus = PaymentStatus.Refunded;
                                        }

                                        _orderService.UpdateOrder(order);

                                        ///改变订单状态
                                        _orderProcessingService.CheckOrderStatus(order);

                                        //修改退款记录为退款成功
                                        refundInfo.RefundStatusId = (int)RefundStatus.refund;
                                        refundInfo.RefundOnUtc = DateTime.Now;
                                        refundInfo.Result_Details = result_details;
                                        _refundInfoService.Update(refundInfo);

                                        ///通知
                                        var orderRefundedStoreOwnerNotificationQueuedEmailId = _workflowMessageService.SendOrderRefundedStoreOwnerNotification(order, AmountToRefund, _localizationSettings.DefaultAdminLanguageId);
                                        if (orderRefundedStoreOwnerNotificationQueuedEmailId.Count > 0&& orderRefundedStoreOwnerNotificationQueuedEmailId[0]>0)
                                        {
                                            order.OrderNotes.Add(new OrderNote
                                            {
                                                Note = string.Format("\"订单退款\" email (to store owner) has been queued. Queued email identifier: {0}.", orderRefundedStoreOwnerNotificationQueuedEmailId),
                                                DisplayToCustomer = false,
                                                CreatedOnUtc = DateTime.UtcNow
                                            });
                                            _orderService.UpdateOrder(order);
                                        }
                                        var orderRefundedCustomerNotificationQueuedEmailId = _workflowMessageService.SendOrderRefundedCustomerNotification(order, AmountToRefund, order.CustomerLanguageId);
                                        if (orderRefundedCustomerNotificationQueuedEmailId.Count > 0 && orderRefundedCustomerNotificationQueuedEmailId[0] > 0)
                                        {
                                            order.OrderNotes.Add(new OrderNote
                                            {
                                                Note = string.Format("\"订单退款\" email (to customer) has been queued. Queued email identifier: {0}.", orderRefundedCustomerNotificationQueuedEmailId),
                                                DisplayToCustomer = false,
                                                CreatedOnUtc = DateTime.UtcNow
                                            });
                                            _orderService.UpdateOrder(order);
                                        }

                                        //已退款事件   
                                        _eventPublisher.Publish(new OrderRefundedEvent(order, AmountToRefund));
                                        return Content("success");
                                        #endregion
                                    }
                                    else
                                    {
                                        #region 错误
                                        //退款异常
                                        refundInfo.RefundStatusId = (int)RefundStatus.error;
                                        _refundInfoService.Update(refundInfo);
                                        order.OrderNotes.Add(new OrderNote
                                        {
                                            Note = string.Format("支付宝退款异常,退款编号:{0},退款金额:{1},交易号:{2},说明:{3}", batch_no, AmountToRefund, out_Trade_No, "退款金额错误"),
                                            DisplayToCustomer = false,
                                            CreatedOnUtc = DateTime.UtcNow
                                        });
                                        _orderService.UpdateOrder(order);
                                        return Content("success");
                                        #endregion
                                    }

                                }
                            }

                        }
                        throw new Exception(string.Format("支付宝退款通知异常,退款编号:{0},退款金额:{1},交易号:{2},说明:{3}", batch_no, refundInfo.AmountToRefund, refundInfo.Out_Trade_No, "非正常处理"));
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex.Message);
                        return Content("fail");
                    }
                    //↑↑↑↑↑↑↑↑↑↑↑↑↑↑业务处理↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑
                    ///结束业务处理
                    return Content("success");//请不要修改或删除
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////
                }
                else//验证失败
                {
                    return Content("fail");
                }
            }
            else
            {
                return Content("无通知参数");
              
            }
            return Content("");
        }

        #endregion

    }
}
