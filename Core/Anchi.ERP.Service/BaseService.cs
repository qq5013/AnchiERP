﻿using Anchi.ERP.Common.Filter;
using Anchi.ERP.Domain;
using Anchi.ERP.IRespository;
using System;

namespace Anchi.ERP.Service
{
    /// <summary>
    /// 服务层基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseService<T> where T : BaseDomain, new()
    {
        #region 构造函数和属性
        public BaseService(IBaseRepository<T> repository)
        {
            this.Repository = repository;
        }
        IBaseRepository<T> Repository
        {
            get;
        }
        #endregion

        #region 根据ID获取对象
        /// <summary>
        /// 根据ID获取对象，不加载关联对象
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public T Get(Guid Id)
        {
            if (Id == Guid.Empty)
                return null;

            return Repository.Get(Id);
        }

        /// <summary>
        /// 根据ID获取对象，加载关联对象
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public T GetModel(Guid Id)
        {
            if (Id == Guid.Empty)
                return null;

            return Repository.GetModel(Id);
        }
        #endregion

        #region 新增数据
        /// <summary>
        /// 新增数据
        /// </summary>
        /// <param name="model"></param>
        public virtual void Create(T model)
        {
            if (model == null)
                return;

            Repository.Create(model);
        }
        #endregion

        #region 批量删除
        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="Ids"></param>
        public void DeleteList(params Guid[] Ids)
        {
            if (Ids.Length == 0)
                return;

            Repository.Delete(Ids);
        }
        #endregion

        #region 分页筛选查询列表
        /// <summary>
        /// 分页筛选查询列表
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public virtual PagedQueryResult<T> FindPaged(PagedQueryFilter filter)
        {
            if (filter == null)
                return new PagedQueryResult<T>();

            if (filter.PageSize == 0)
                return new PagedQueryResult<T>();

            return Repository.FindPaged<T>(filter);
        }
        #endregion
    }
}
