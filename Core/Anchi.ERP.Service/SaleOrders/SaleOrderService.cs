﻿using Anchi.ERP.Common.Filter;
using Anchi.ERP.Domain.Finances;
using Anchi.ERP.Domain.RepairOrders.Enum;
using Anchi.ERP.Domain.SaleOrders;
using Anchi.ERP.Domain.SaleOrders.Enum;
using Anchi.ERP.IRepository.Finances;
using Anchi.ERP.IRepository.SaleOrders;
using Anchi.ERP.Repository.Finances;
using Anchi.ERP.Repository.SaleOrders;
using Anchi.ERP.Service.Customers;
using Anchi.ERP.Service.Employees;
using Anchi.ERP.ServiceModel.Sales;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Anchi.ERP.Service.SaleOrders
{
    /// <summary>
    /// 销售管理服务类
    /// </summary>
    public class SaleOrderService : BaseService<SaleOrder>
    {
        #region 构造函数和属性
        public SaleOrderService(ISaleOrderRepository saleOrderRepository, CustomerService customerService, EmployeeService employeeService, IFinanceOrderRepository financeOrderRepository) : base(saleOrderRepository)
        {
            this.SaleOrderRepository = saleOrderRepository;
            this.CustomerService = customerService;
            this.EmployeeService = employeeService;
            this.FinanceOrderRepository = financeOrderRepository;
        }

        ISaleOrderRepository SaleOrderRepository
        {
            get;
        }

        CustomerService CustomerService
        {
            get;
        }

        EmployeeService EmployeeService
        {
            get;
        }

        IFinanceOrderRepository FinanceOrderRepository
        {
            get;
        }
        #endregion

        #region 保存销售单
        /// <summary>
        /// 保存销售单
        /// </summary>
        /// <param name="model"></param>
        public void SaveOrUpdate(SaleOrder model)
        {
            if (model == null)
                return;

            if (model.CustomerId == Guid.Empty)
                throw new Exception("请选择客户。");

            if (model.SaleById == Guid.Empty)
                throw new Exception("请选择销售人。");

            if (!model.ProductList.Any())
                throw new Exception("请选择销售配件。");

            if (model.ProductList.Any(item => item.Quantity == 0))
                throw new Exception("请填写销售配件的数量。");

            model.Amount = model.ProductList.Sum(item => item.UnitPrice * item.Quantity);

            var temp = this.SaleOrderRepository.GetModel(model.Id);
            if (temp == null)
            {
                model.Id = model.Id == Guid.Empty ? Guid.NewGuid() : model.Id;
                model.Code = this.SaleOrderRepository.GetSequenceNextCode();
                model.Status = EnumSaleOrderStatus.Initial;
                model.SettlementStatus = EnumSettlementStatus.Waiting;
                model.CreatedOn = DateTime.Now;

                this.SaleOrderRepository.Create(model);
            }
            else
            {
                if (temp.Status != EnumSaleOrderStatus.Initial)
                    throw new Exception("只能修改待出库的销售单。");

                temp.Amount = model.Amount;
                temp.CustomerId = model.CustomerId;
                temp.SaleById = model.SaleById;
                temp.SaleOn = model.SaleOn;
                temp.Remark = model.Remark;
                temp.ProductList = model.ProductList;

                this.SaleOrderRepository.UpdateModel(temp);
            }
        }
        #endregion

        #region 设置已出库
        /// <summary>
        /// 设置已出库
        /// </summary>
        /// <param name="IdList"></param>
        public void Outbound(IList<Guid> IdList)
        {
            if (IdList == null || !IdList.Any())
                return;

            foreach (var item in IdList)
            {
                var order = SaleOrderRepository.GetModel(item);
                if (order == null)
                    continue;

                if (order.Status != EnumSaleOrderStatus.Initial)
                    throw new Exception("只有对待出库的销售单做出库操作。");

                SaleOrderRepository.Outbound(order);
            }
        }
        #endregion

        #region 结算销售单
        /// <summary>
        /// 结算销售单
        /// </summary>
        /// <param name="model"></param>
        public void Settlement(SaleSettlementModel model)
        {
            if (model == null)
                return;

            var order = SaleOrderRepository.GetModel(model.SaleOrderId);
            if (order == null)
                throw new Exception("销售单不存在。");

            if (order.Status != EnumSaleOrderStatus.Outbound)
                throw new Exception("只能结算已出库的销售单。");

            if (order.SettlementStatus == EnumSettlementStatus.Completed)
                throw new Exception("只能结算未结算的销售单。");

            order.SettlementStatus = model.SettlementStatus;
            order.SettlementAmount = order.SettlementAmount + model.SettlementAmount;

            if (order.SettlementAmount >= order.Amount && order.SettlementStatus == EnumSettlementStatus.PartCompleted)
                throw new Exception("结算金额已超过销售单总金额，不允许部分结算。");

            var financeOrder = new FinanceOrder();
            financeOrder.Code = this.FinanceOrderRepository.GetSequenceNextCode();
            financeOrder.Amount = model.SettlementAmount;
            financeOrder.Remark = model.SettlementRemark;

            SaleOrderRepository.Settlement(order, financeOrder);
        }
        #endregion

        #region 分页查找销售单列表
        /// <summary>
        /// 分页查找销售单列表
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public PagedQueryResult<SaleOrderModel> FindList(PagedQueryFilter filter)
        {
            var result = SaleOrderRepository.FindPaged<SaleOrder>(filter);
            var modelList = new List<SaleOrderModel>();
            var response = new PagedQueryResult<SaleOrderModel>();
            foreach (var item in result.Data)
            {
                item.Customer = CustomerService.Get(item.CustomerId);
                item.SaleBy = EmployeeService.Get(item.SaleById);

                var model = new SaleOrderModel
                {
                    Id = item.Id,
                    Code = item.Code,
                    Amount = item.Amount,
                    CustomerName = item.Customer == null ? string.Empty : item.Customer.Name,
                    OutboundOn = item.OutboundOn,
                    Remark = item.Remark,
                    SaleByName = item.SaleBy == null ? string.Empty : item.SaleBy.Name,
                    SaleOn = item.SaleOn,
                    SettlementAmount = item.SettlementAmount,
                    SettlementOn = item.SettlementOn,
                    SettlementStatus = item.SettlementStatus,
                    Status = item.Status,
                };
                modelList.Add(model);
            }
            response.Data = modelList;
            response.PageIndex = result.PageIndex;
            response.PageSize = result.PageSize;
            response.TotalCount = result.TotalCount;
            response.TotalPage = result.TotalPage;

            return response;
        }
        #endregion

        #region 取消销售单
        /// <summary>
        /// 取消销售单
        /// </summary>
        /// <param name="idList"></param>
        public void CancelOrder(IList<Guid> idList)
        {
            if (idList == null || !idList.Any())
                return;

            foreach (var Id in idList)
            {
                var model = GetModel(Id);
                if (model == null)
                    continue;

                var order = new FinanceOrder();
                order.Code = this.FinanceOrderRepository.GetSequenceNextCode();

                SaleOrderRepository.Cancel(model, order);
            }
        }
        #endregion
    }
}
