using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HMZ.Database.Entities;
using HMZ.Database.Enums;
using HMZ.DTOs.Models;
using HMZ.Service.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace HMZ.Service.Services.VNPAYServices
{
    public class VNPAYService : IVNPAYService
    {
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        VNPAYConfig _config;
        public VNPAYService(IOptions<VNPAYConfig> config, IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _config = new VNPAYConfig()
            {
                BaseUrl = config.Value.BaseUrl,
                Command = config.Value.Command,
                CurrCode = config.Value.CurrCode,
                HashSecret = config.Value.HashSecret,
                Locale = config.Value.Locale,
                ReturnUrl = config.Value.ReturnUrl,
                TimeZoneId = config.Value.TimeZoneId,
                TmnCode = config.Value.TmnCode,
                Version = config.Value.Version
            };
            if (_config.HashSecret == null)
                throw new System.Exception("VNPAY Config is null");

        }
        public string CreatePaymentUrl(Order_VNPAY model, HttpContext context)
        {
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_config.TimeZoneId);
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            var tick = DateTime.Now.Ticks.ToString();
            var pay = new VnPayLibrary();
            var urlCallBack = _config.ReturnUrl;

            pay.AddRequestData("vnp_Version", _config.Version);
            pay.AddRequestData("vnp_Command", _config.Command);
            pay.AddRequestData("vnp_TmnCode", _config.TmnCode);
            pay.AddRequestData("vnp_Locale", _config.Locale);
            pay.AddRequestData("vnp_CurrCode", _config.CurrCode);

            pay.AddRequestData("vnp_Amount", ((double)model.Amount * 100).ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
            pay.AddRequestData("vnp_OrderInfo", $"Khoa hoc:{model.CourseId}Nguoi mua:{model.UserId}");
            pay.AddRequestData("vnp_OrderType", model.OrderType);
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            pay.AddRequestData("vnp_TxnRef", tick);

            var paymentUrl =
                pay.CreateRequestUrl(_config.BaseUrl, _config.HashSecret);

            return paymentUrl;
        }

        public VNPayResponse PaymentExecute(IQueryCollection collections)
        {
            var pay = new VnPayLibrary();
            var response = pay.GetFullResponseData(collections, _config.HashSecret);

            return response;
        }
    }
}