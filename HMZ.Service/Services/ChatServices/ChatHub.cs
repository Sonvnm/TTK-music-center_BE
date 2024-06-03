using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HMZ.Database.Commons;
using HMZ.Database.Entities;
using HMZ.DTOs.Queries;
using HMZ.DTOs.Views;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace HMZ.Service.Services.ChatHub
{
    public class ChatHub : Hub
    {
        protected readonly IUnitOfWork _unitOfWork;
        public ChatHub(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task JoinClassroom(ChatQuery chatQuery)
        {
            var classs = await GetUserInClass(chatQuery);

            await Groups.AddToGroupAsync(Context.ConnectionId, classs.Id.ToString());
            var user = await _unitOfWork.GetRepository<User>().AsQueryable().FirstOrDefaultAsync(x => x.Id == chatQuery.UserId);
            if (user == null)
            {
                throw new HubException("User not found");
            }
            // send message to all clients in group
            await Clients.Group(classs.Id.ToString()).SendAsync("ReceiveMessage", new ChatView()
            {
                Content = user.UserName + " vừa vào nhóm chat",
                SendAt = DateTime.Now,
                UserId = chatQuery.UserId,
                ClassId = classs.Id,
                ClassName = classs.Name,
                TimeAgo = HMZHelper.TimeAgo(DateTime.Now),
                User = new UserView()
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Image = user.Image,
                }
            });
        }

        public async Task LeaveClassroom(ChatQuery chatQuery)
        {
            var classs = await GetUserInClass(chatQuery);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, classs.Id.ToString());
            var user = await _unitOfWork.GetRepository<User>().AsQueryable().FirstOrDefaultAsync(x => x.Id == chatQuery.UserId);
            if (user == null)
            {
                throw new HubException("User not found");
            }

            // send message to all clients in group
            await Clients.Group(classs.Id.ToString()).SendAsync("ReceiveMessage", new ChatView()
            {
                Content = user.UserName + " vừa rời lớp",
                SendAt = DateTime.Now,
                UserId = chatQuery.UserId,
                ClassId = classs.Id,
                ClassName = classs.Name,
                User = new UserView()
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Image = user.Image,
                }
            });
        }

        public async Task SendMessageToGroup(ChatQuery query)
        {


            var classs = await GetUserInClass(query);
            var user = await _unitOfWork.GetRepository<User>().AsQueryable().FirstOrDefaultAsync(x => x.Id == query.UserId);
            if (user == null)
            {
                throw new HubException("User not found");
            }
            var messageEntity = new Message
            {
                Code = "MSG" + DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + HMZHelper.GenerateCode(4),
                Content = query.Content,
                SendAt = DateTime.Now,
                UserId = query.UserId,
                ClassId = classs.Id
            };

            // Save message to database
            await _unitOfWork.GetRepository<Message>().Add(messageEntity);
            await _unitOfWork.SaveChangesAsync();
            // send message to all clients in group
            // find group by studentClassId
            await Clients.Group(classs.Id.ToString()).SendAsync("ReceiveMessage", new ChatView()
            {
                Content = query.Content,
                SendAt = DateTime.Now,
                UserId = query.UserId,
                ClassId = classs.Id,
                ClassName = classs.Name,
                TimeAgo = HMZHelper.TimeAgo(DateTime.Now),
                User = new UserView()
                {
                    Id = query.UserId,
                    Username = user.UserName,
                    Image = user.Image,
                }
            });


        }

        private async Task<Class> GetUserInClass(ChatQuery query)
        {
            var res = await _unitOfWork.GetRepository<Class>()
                .AsQueryable()
                .Include(x => x.StudentClasses)
                .FirstOrDefaultAsync(x => x.Id == query.ClassId && x.StudentClasses.Any(x => x.UserId == query.UserId));
            if (res == null)
            {
                throw new HubException("User not found in class");
            }
            return res;

        }
    }
}