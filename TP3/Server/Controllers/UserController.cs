﻿using IFT585_TP3.Common.Reponses;
using IFT585_TP3.Common.UdpServer;
using IFT585_TP3.Server.Repositories.UserRepositories;
using IFT585_TP3.Server.RESTFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IFT585_TP3.Server.Controllers
{
    public class UserController
    {
        public UserInMemoryRepository UserRepo { get; set; }

        public void RegisterRoutes(RESTFramework.Server server)
        {
            server.Use(Method.GET, "/api/user", GetUsers);
            server.Use(Method.POST, "/api/user", VerifyIfSuperAdmin, CreateUser);
            server.Use(Method.DELETE, "/api/user/:username", VerifyIfSuperAdmin, DeleteUser);
        }

        #region Middlewares

        private async Task VerifyIfSuperAdmin(Request req, Response res)
        {
            if (req.Context.AuthenticatedUser.Username != "admin")
            {
                await res.Unauthorized("Action requires admin permission.");
            }
        }

        #endregion Middlewares

        #region Handlers

        /// <summary>
        /// Return the list of all users.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="res"></param>
        /// <returns></returns>
        private async Task GetUsers(Request req, Response res)
        {
            var users = UserRepo.RetrieveAll();

            await res.Json(new UserListResponse() { 
                Users = users.Select(_user => new Common.Reponses.User()
                {
                     Username = _user.Username,
                     LastActivity = _user.LastActivity
                })
            });
        }

        private async Task CreateUser(Request req, Response res)
        {
            var credentials = await req.GetBody<Credential>();

            if (UserRepo.Exists(credentials.userName))
            {
                await res.BadRequest($"A user already exist with the name {credentials.userName}.");
            }

            var salt = PasswordHelper.GenerateSalt();
            UserRepo.Create(new Model.User()
            {
                Username = credentials.userName,
                PasswordSalt = salt,
                PasswordHash = PasswordHelper.Hash(credentials.password, salt)
            });
            res.Close();
        }

        private async Task DeleteUser(Request req, Response res)
        {
            UserRepo.Delete(req.Params.Get("username"));
            res.Close();
        }

        #endregion Handlers
    }
}
