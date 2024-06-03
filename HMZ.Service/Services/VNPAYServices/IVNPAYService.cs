using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HMZ.DTOs.Models;
using Microsoft.AspNetCore.Http;

namespace HMZ.Service.Services.VNPAYServices
{
    public interface IVNPAYService
    {
        string CreatePaymentUrl(Order_VNPAY model, HttpContext context);
        VNPayResponse PaymentExecute(IQueryCollection collections);
    }
}