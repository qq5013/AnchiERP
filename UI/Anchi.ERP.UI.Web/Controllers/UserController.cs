﻿using Anchi.ERP.Domain.Users;
using Anchi.ERP.Domain.Users.Enum;
using Anchi.ERP.Domain.Users.Filter;
using Anchi.ERP.Service.Users;
using Anchi.ERP.UI.Web.Filter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace Anchi.ERP.UI.Web.Controllers
{
    /// <summary>
    /// 用户管理
    /// </summary>
    [UserAuthorize]
    public class UserController : BaseController
    {
        #region 构造函数和属性
        public UserController(UserService userService)
        {
            this.UserService = userService;
        }

        UserService UserService { get; }
        #endregion

        #region 用户管理
        /// <summary>
        /// 用户管理
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 查询用户列表
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public ActionResult List(FindUserFilter filter)
        {
            var result = UserService.FindPaged(filter);
            return new BetterJsonResult(result, true);
        }

        /// <summary>
        /// 查询用户列表
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public ActionResult ListUser(FindUserFilter filter)
        {
            var result = UserService.FindPaged(filter);
            return new BetterJsonResult(result, true);
        }
        #endregion

        #region 新增用户
        /// <summary>
        /// 新增用户
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Add()
        {
            var model = new User();
            return View("Edit", model);
        }
        #endregion

        #region 修改用户
        /// <summary>
        /// 修改用户
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Edit(Guid id)
        {
            var model = UserService.GetModel(id);
            return View(model);
        }
        #endregion

        #region 保存用户
        /// <summary>
        /// 保存用户
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Edit(User model)
        {
            try
            {
                UserService.SaveOrUpdate(model);
                return new BetterJsonResult(null, true);
            }
            catch (Exception ex)
            {
                return new BetterJsonResult(ex.Message);
            }
        }
        #endregion

        #region 删除用户
        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="Ids"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Delete(IList<Guid> Ids)
        {
            try
            {
                UserService.DeleteList(Ids.ToArray());

                if (Ids.Contains(this.CurrentUser.Id))
                    this.CurrentUser = null;

                return new BetterJsonResult(null, true);
            }
            catch (Exception ex)
            {
                return new BetterJsonResult(ex.Message);
            }
        }
        #endregion

        #region 修改用户状态
        /// <summary>
        /// 修改用户状态
        /// </summary>
        /// <param name="IdList"></param>
        /// <param name="Status"></param>
        /// <returns></returns>
        public ActionResult UpdateStatus(IList<Guid> IdList, EnumUserStatus Status)
        {
            foreach (var Id in IdList)
            {
                var model = UserService.Get(Id);
                if (model == null)
                    continue;

                model.Status = Status;
                UserService.SaveOrUpdate(model);

                if (model.Id == this.CurrentUser.Id)
                {
                    this.CurrentUser = model;
                }
            }
            return new BetterJsonResult(null, true);
        }
        #endregion
    }
}