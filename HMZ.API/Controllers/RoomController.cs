using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HMZ.API.Controllers.Base;
using HMZ.DTOs.Filters;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Views;
using HMZ.Service.Services.RoomServices;
using Microsoft.AspNetCore.Mvc;

namespace HMZ.API.Controllers
{
    public class RoomController : CRUDController<IRoomService, RoomQuery, RoomView, RoomFilter>
    {
        public RoomController(IRoomService service) : base(service)
        {
            
        }
    }
}