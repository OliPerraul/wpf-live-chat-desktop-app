﻿using IFT585_TP3.Server.Controllers.Context;
using IFT585_TP3.Server.RESTFramework;
using System.Threading.Tasks;
using IFT585_TP3.Server.Model;
using IFT585_TP3.Server.Repositories;
using System.Collections.Generic;
using System.Linq;
using IFT585_TP3.Server.Repositories.UserRepositories;
using IFT585_TP3.Server.Repositories.MessageRepositories;
using System;
using System.Globalization;

namespace IFT585_TP3.Server.Controllers
{
    public class GroupController
    {
        public UserInMemoryRepository UserRepo { get; set; }
        public GroupInMemoryRepository GroupRepo { get; set; }

        public void RegisterRoutes(RESTFramework.Server server)
        {
            server.Use(Method.GET, "/api/group", GetAllForUser);
            server.Use(Method.POST, "/api/group", CreateGroup);

            server.Use(Method.GET, "/api/group/:group_name", SetGroupContext, VerifyIfMember, GetOne);
            server.Use(Method.DELETE, "/api/group/:group_name", SetGroupContext, VerifyIfAdmin, Delete);
           
            server.Use(Method.POST, "/api/group/:group_name/invitation/:username", SetGroupContext, VerifyIfAdmin, Invite);
            server.Use(Method.PUT, "/api/group/:group_name/invitation", SetGroupContext, AcceptInvitation);
            server.Use(Method.DELETE, "/api/group/:group_name/invitation", SetGroupContext, RefuseInvitation);

            server.Use(Method.DELETE, "/api/group/:group_name/:username", SetGroupContext, VerifyIfAdmin, RemoveUser);

            server.Use(Method.POST, "/api/group/:group_name/admin/:username", SetGroupContext, VerifyIfAdmin, SetGroupAdmin);
            server.Use(Method.DELETE, "/api/group/:group_name/admin/:username", SetGroupContext, VerifyIfAdmin, RevokeGroupAdmin);
        }

        #region Middleware

        private async Task SetGroupContext(Request req, Response res)
        {
            var groupName = req.Params.Get("group_name");
            Group group = GroupRepo.Retrieve(groupName);

            if (group == null)
            {
                await res.BadRequest($"No group with name {groupName}.");
            }
            else
            {
                req.Context = new GroupContext(req.Context) { Group = group };
            }
        }

        private async Task VerifyIfMember(Request req, Response res)
        {
            var group = ((GroupContext)req.Context).Group;
            if (!group.MemberUsernames.Contains(req.Context.AuthenticatedUser.Username))
            {
                await res.Forbidden($"User is a member of the requested group.");
            }
        }

        private async Task VerifyIfAdmin(Request req, Response res)
        {
            var group = ((GroupContext)req.Context).Group;
            if (!group.AdminUsernames.Contains(req.Context.AuthenticatedUser.Username))
            {
                await res.Unauthorized($"User is not an admin of the requested group.");
            }
        }

        #endregion Middleware

        #region Handler

        private async Task GetAllForUser(Request req, Response res)
        {
            var authUsername = req.Context.AuthenticatedUser.Username;

            var groups = GroupRepo.RetrieveAll();
            var onlineUsers = GetOnlineUsers();
            await res.Json(new Common.Reponses.GroupListResponse()
            {
                Groups = groups.Select(_group => new Common.Reponses.Group()
                {
                    AdminUsernames = _group.AdminUsernames,
                    ConnectedUsernames = _group.MemberUsernames.FindAll(_user => onlineUsers.Contains(_user)),
                    GroupName = _group.GroupName,
                    InvitedUsernames = _group.InvitedUsernames,
                    MemberUsernames = _group.MemberUsernames
                })
            });
        }

        private async Task CreateGroup(Request req, Response res)
        {
            var newGroup = await req.GetBody<Common.Reponses.Group>();
            if (GroupRepo.Exists(newGroup.GroupName))
            {
                await res.BadRequest($"A group already exist with the name {newGroup.GroupName}.");
                return;
            }

            var serverGroup = new Group()
            {
                GroupName = newGroup.GroupName
            };
            serverGroup.AdminUsernames.Add(req.Context.AuthenticatedUser.Username);
            serverGroup.MemberUsernames.Add(req.Context.AuthenticatedUser.Username);
            GroupRepo.Create(serverGroup);

            res.Close();
        }

        private async Task GetOne(Request req, Response res)
        {
            var group = ((GroupContext)req.Context).Group;

            var onlineUsers = GetOnlineUsers();
            await res.Json(new Common.Reponses.Group()
            {
                AdminUsernames = group.AdminUsernames,
                ConnectedUsernames = group.MemberUsernames.FindAll(_user => onlineUsers.Contains(_user)),
                GroupName = group.GroupName,
                InvitedUsernames = group.InvitedUsernames,
                MemberUsernames = group.MemberUsernames
            });
        }

        private async Task Delete(Request req, Response res)
        {
            GroupRepo.Delete(req.Params.Get("group_name"));
            res.Close();
        }

        private async Task RemoveUser(Request req, Response res)
        {
            var group = ((GroupContext)req.Context).Group;
            var userToRemove = req.Params.Get("username");

            if (!group.MemberUsernames.Contains(userToRemove))
            {
                await res.BadRequest($"No user in this grou with the name {userToRemove}.");
                return;
            }
            if (userToRemove == req.Context.AuthenticatedUser.Username)
            {
                await res.BadRequest($"You cannot remove yourself from the group.");
                return;
            }
            group.MemberUsernames.Remove(userToRemove);
            group.AdminUsernames.Remove(userToRemove);
            GroupRepo.Update(group);

            res.Close();
        }

        private async Task Invite(Request req, Response res)
        {
            var group = ((GroupContext)req.Context).Group;

            group.InvitedUsernames.Add(req.Params.Get("username"));
            GroupRepo.Update(group);

            res.Close();
        }

        private async Task AcceptInvitation(Request req, Response res)
        {
            var group = ((GroupContext)req.Context).Group;
            if (!group.InvitedUsernames.Contains(req.Context.AuthenticatedUser.Username))
            {
                await res.BadRequest($"No invitation for this user.");
                return;
            }

            group.InvitedUsernames.Remove(req.Context.AuthenticatedUser.Username);
            group.MemberUsernames.Add(req.Context.AuthenticatedUser.Username);
            GroupRepo.Update(group);

            res.Close();
        }

        private async Task RefuseInvitation(Request req, Response res)
        {
            var group = ((GroupContext)req.Context).Group;
            if (!group.InvitedUsernames.Contains(req.Context.AuthenticatedUser.Username))
            {
                await res.BadRequest($"No invitation for this user.");
                return;
            }

            group.InvitedUsernames.Remove(req.Context.AuthenticatedUser.Username);
            GroupRepo.Update(group);

            res.Close();
        }

        private async Task SetGroupAdmin(Request req, Response res)
        {
            var group = ((GroupContext)req.Context).Group;
            var newAdmin = req.Params.Get("username");

            if (!group.AdminUsernames.Contains(newAdmin))
            {
                group.AdminUsernames.Add(newAdmin);
                GroupRepo.Update(group);
            }

            res.Close();
        }

        private async Task RevokeGroupAdmin(Request req, Response res)
        {
            var group = ((GroupContext)req.Context).Group;
            var newAdmin = req.Params.Get("username");

            if (newAdmin == req.Context.AuthenticatedUser.Username)
            {
                await res.BadRequest("Cannot revoke own admin rights.");
                return;
            }

            if (group.AdminUsernames.Contains(newAdmin))
            {
                group.AdminUsernames.Remove(newAdmin);
                GroupRepo.Update(group);
            }

            res.Close();
        }

        #endregion Handler

        private IEnumerable<string> GetOnlineUsers()
        {
            var fewSecondsAgo = DateTime.Now.Subtract(new TimeSpan(0, 0, 11));
            return UserRepo.RetrieveAll()
                .ToList()
                .FindAll(_user => _user.LastActivity > fewSecondsAgo)
                .Select(_user => _user.Username);
        }
    }
}
