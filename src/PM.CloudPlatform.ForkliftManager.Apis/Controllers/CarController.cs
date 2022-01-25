﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pang.AutoMapperMiddleware;
using PM.CloudPlatform.ForkliftManager.Apis.Controllers.Base;
using PM.CloudPlatform.ForkliftManager.Apis.DtoParameters.Base;
using PM.CloudPlatform.ForkliftManager.Apis.Entities;
using PM.CloudPlatform.ForkliftManager.Apis.Enums;
using PM.CloudPlatform.ForkliftManager.Apis.Extensions;
using PM.CloudPlatform.ForkliftManager.Apis.General;
using PM.CloudPlatform.ForkliftManager.Apis.Managers;
using PM.CloudPlatform.ForkliftManager.Apis.Models;
using PM.CloudPlatform.ForkliftManager.Apis.Repositories;

namespace PM.CloudPlatform.ForkliftManager.Apis.Controllers
{
    /// <summary>
    /// 车辆档案
    /// </summary>
    [ApiController]
    [EnableCors("Any")]
    [Route("api/[Controller]/[Action]")]
    [Authorize]
    public class CarController : MyControllerBase<CarRepository, Car, CarDto, CarAddOrUpdateDto>
    {
        private readonly CarRepository _repository;
        private readonly IMapper _mapper;
        private readonly IGeneralRepository _generalRepository;
        private readonly TerminalSessionManager gpsTrackerSessionManager;
        private readonly IQueryable<Car> Cars;

        /// <summary>
        /// </summary>
        /// <param name="repository">        </param>
        /// <param name="mapper">            </param>
        /// <param name="generalRepository"> </param>
        /// <param name="gpsTrackerSessionManager"></param>
        public CarController(CarRepository repository, IMapper mapper, IGeneralRepository generalRepository, TerminalSessionManager gpsTrackerSessionManager) : base(repository, mapper)
        {
            _repository = repository;
            _mapper = mapper;
            _generalRepository = generalRepository;
            this.gpsTrackerSessionManager = gpsTrackerSessionManager;
            Cars = _generalRepository.GetQueryable<Car>();
        }

        /// <summary>
        /// 获取车辆档案
        /// </summary>
        /// <param name="ids">        </param>
        /// <param name="parameters"> </param>
        /// <returns>
        /// <para> CarId: 车辆Id </para>
        /// <para> LicensePlateNumber: 车牌号 </para>
        /// <para> Brand: 品牌 </para>
        /// <para> SerialNumber: 编号 </para>
        /// <para> BuyTime: 购买时间 </para>
        /// <para> LengthOfUse: 使用时长 </para>
        /// <para> MaintenanceTimes: 保养次数 </para>
        /// <para> LastOfMaintenanceTime: 上次保养时间 </para>
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> GetCars([FromQuery] IEnumerable<Guid> ids, [FromQuery] DtoParametersBase parameters)
        {
            if (ids.Any())
            {
                var data = await _generalRepository.GetQueryable<Car>()
                    .FilterDeleted()
                    .Include(x => x.CarType)
                    .Include(x => x.UseRecords)
                    .Include(x => x.CarMaintenanceRecords)
                    .Where(x => ids!.Contains(x.Id))
                    .Select(x => new
                    {
                        //Source = x.MapTo<CarDto>(),
                        CarId = x.Id,
                        CarTypeId = x.CarType!.Id,
                        CarTypeName = x.CarType!.Name,
                        CarModel = x.CarModel,
                        LicensePlateNumber = x.LicensePlateNumber,
                        Brand = x.Brand,
                        SerialNumber = x.SerialNumber,
                        BuyTime = x.BuyTime,
                        LengthOfUse = x.UseRecords!.Sum(t => t.LengthOfTime) + x.LengthOfUse,
                        MaintenanceTimes = x.CarMaintenanceRecords!.Count,
                        LastOfMaintenanceTime = x.CarMaintenanceRecords.Max(t => t.CreateDate)
                    })
                    .AsSplitQuery()
                    .ToListAsync();
                return Success(data);
            }
            else
            {
                var data = await _generalRepository.GetQueryable<Car>()
                    .FilterDeleted()
                    .Include(x => x.CarType)
                    .Include(x => x.UseRecords)
                    .Include(x => x.CarMaintenanceRecords)
                    .ApplyPaged(parameters)
                    .Select(x => new
                    {
                        //Source = x.MapTo<CarDto>(),
                        CarId = x.Id,
                        CarTypeId = x.CarType!.Id,
                        CarTypeName = x.CarType!.Name,
                        CarModel = x.CarModel,
                        LicensePlateNumber = x.LicensePlateNumber,
                        Brand = x.Brand,
                        SerialNumber = x.SerialNumber,
                        BuyTime = x.BuyTime,
                        LengthOfUse = x.UseRecords!.Sum(t => t.LengthOfTime) + x.LengthOfUse,
                        MaintenanceTimes = x.CarMaintenanceRecords!.Count,
                        //.OrderByDescending(t => t.CreateDate).FirstOrDefault()!.CreateDate
                        LastOfMaintenanceTime = x.CarMaintenanceRecords.Max(t => t.CreateDate)
                    })
                    .AsSplitQuery()
                    .ToListAsync();
                return Success(data);
            }
        }

        /// <summary>
        /// 获取车辆状态
        /// </summary>
        /// <returns> </returns>
        [HttpGet]
        public async Task<IActionResult> GetCarArchives([FromQuery] DtoParametersBase parameters)
        {
            //var res = await Users.SelectMany(u => u.Roles!.Select(r => new { u, r })).ToListAsync();

            //var res = await Cars.SelectMany(x =>
            //    x.RentalRecords!.Select(r => new { x, r }).Where(t => t.r.IsReturn || t.x.RentalRecords!.Count <= 0)).ToListAsync();

            var data = await Cars.FilterDeleted()
                .Include(x => x.CarType)
                .Include(x => x.UseRecords)
                .Include(x => x.RentalRecords!.Where(t => !t.IsReturn))
                .ApplyPaged(parameters)
                .Select(x => new
                {
                    //Source = x.MapTo<CarDto>(),
                    CarId = x.Id,
                    CarTypeId = x.CarType!.Id,
                    CarTypeName = x.CarType!.Name,
                    CarModel = x.CarModel,
                    LicensePlateNumber = x.LicensePlateNumber,
                    Brand = x.Brand,
                    SerialNumber = x.SerialNumber,
                    BuyTime = x.BuyTime,
                    IsReturn = !x.RentalRecords!.Any(),
                    LengthOfUse = x.UseRecords!.Sum(t => t.LengthOfTime) + x.LengthOfUse,
                })
                .AsSplitQuery()
                .ToListAsync();

            //var returnDto = data.MapTo<CarDto>();

            return Success(data);
        }

        /// <summary>
        /// 获取车辆和终端的信息
        /// </summary>
        /// <returns> </returns>
        [HttpGet]
        public async Task<IActionResult> GetCarTerminals([FromQuery] DtoParametersBase parameters)
        {
            var data = await _generalRepository.GetQueryable<Car>()
                .FilterDeleted()
                .Include(x => x.CarType)
                .Include(x => x.TerminalBindRecords)
                .ThenInclude(t => t.Terminal)
                .ApplyPaged(parameters)
                .Select(x => new
                {
                    //Source = x.MapTo<CarDto>(),
                    // 可以往里面填写自己需要的数据
                    CarId = x.Id,
                    CarTypeId = x.CarType!.Id,
                    CarModel = x.CarModel,
                    LicensePlateNumber = x.LicensePlateNumber,
                    Brand = x.Brand,
                    SerialNumber = x.SerialNumber,
                    CarTypeName = x.CarType!.Name,
                    IMEI = x.TerminalBindRecords!.OrderByDescending(x => x.CreateDate).FirstOrDefault()!.Terminal!.IMEI
                })
                .ToListAsync();

            return Success(data);
        }

        /// <summary>
        /// 给车辆绑定终端信息
        /// </summary>
        /// <param name="carId">       车辆Id </param>
        /// <param name="terminalId">  终端Id </param>
        /// <param name="description"> 描述 </param>
        /// <returns> </returns>
        [HttpGet]
        public async Task<IActionResult> SetCarTerminalBind(Guid carId, Guid terminalId, string description)
        {
            var car = await _generalRepository.FindAsync<Car>(x => x.Id.Equals(carId));
            var terminal = await _generalRepository.FindAsync<Terminal>(x => x.Id.Equals(terminalId));

            car.TerminalId = terminalId;
            terminal.CarId = carId;

            var bindRecord = new TerminalBindRecord()
            {
                CarId = carId,
                TerminalId = terminalId,
                Description = description,
                IMEI = terminal.IMEI
            };

            bindRecord.Create();

            await _generalRepository.InsertAsync(bindRecord);
            await _generalRepository.UpdateAsync(car);
            await _generalRepository.UpdateAsync(terminal);
            await _generalRepository.SaveAsync();

            return Success("保存成功");
        }

        /// <summary>
        /// 解除车辆与终端的绑定
        /// </summary>
        /// <param name="carId">      </param>
        /// <param name="terminalId"> </param>
        /// <returns> </returns>
        [HttpGet]
        public async Task<IActionResult> SetCarTerminalUnBind(Guid carId, Guid terminalId)
        {
            var car = await _generalRepository.FindAsync<Car>(x => x.Id.Equals(carId) && x.TerminalId.Equals(terminalId));

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (car is null)
            {
                return Fail("车辆信息获取失败");
            }

            car.TerminalId = null;
            await _generalRepository.UpdateAsync(car);
            await _generalRepository.SaveAsync();

            return Success("接触绑定成功");
        }

        /// <summary>
        /// 通过终端IMEI获取车辆信息
        /// </summary>
        /// <param name="imei"> </param>
        /// <returns> </returns>
        [HttpGet]
        public async Task<IActionResult> GetCarInfoByIMEI(string imei)
        {
            var terminal = await _generalRepository.GetQueryable<Terminal>()
                .FilterDeleted()
                .FilterDisabled()
                .Include(x => x.Car)
                .FirstOrDefaultAsync(x => x.IMEI.Equals(imei));

            if (terminal is null)
            {
                return Fail("终端未启用或不存在");
            }

            if (terminal.Car is null)
            {
                return Fail("终端未绑定车辆");
            }

            var returnDto = terminal.Car!.MapTo<CarDto>();

            return Success(returnDto);
        }

        /// <summary>
        /// 通过终端Id获取车辆信息
        /// </summary>
        /// <param name="id"> </param>
        /// <returns> </returns>
        [HttpGet]
        public async Task<IActionResult> GetCarInfoByTerminalId(Guid id)
        {
            var terminal = await _generalRepository.GetQueryable<Terminal>()
                .FilterDeleted()
                .FilterDisabled()
                .Include(x => x.Car)
                .FirstOrDefaultAsync(x => x.Id.Equals(id));

            if (terminal is null)
            {
                return Fail("终端未启用或不存在");
            }

            if (terminal.Car is null)
            {
                return Fail("终端未绑定车辆");
            }

            var returnDto = terminal.Car!.MapTo<CarDto>();

            return Success(returnDto);
        }

        /// <summary>
        /// 获取车辆工作状态
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetCarStatus()
        {

            var terminalOnlines = gpsTrackerSessionManager.GetAllSessions();

            var onlines = await _generalRepository.GetQueryable<Terminal>()
                .FilterDeleted()
                .FilterDisabled()
                .Include(x => x.Car)
                .Where(x => terminalOnlines.Keys.Contains(x.IMEI) && x.Car != null)
                .CountAsync();

            var offlines = await _generalRepository.GetQueryable<Car>().CountAsync();

            var errors = await _generalRepository.GetQueryable<AlarmRecord>().Where(x => !x.IsReturn).CountAsync();

            return Success(new
            {
                onlines,
                offlines,
                errors
            });
        }

        /// <summary>
        /// 获取月度数据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetMonthlyData()
        {
            var rentalRecords = await _generalRepository.GetQueryable<RentalRecord>()
                .GroupBy(x => x.CreateDate!.Value.ToShortDateString())
                .Select(t => new LineData { Date = t.Key, Count = t.Count() })
                .ToListAsync();

            var maintenanceRecords = await _generalRepository.GetQueryable<CarMaintenanceRecord>()
                .GroupBy(x => x.CreateDate!.Value.ToShortDateString())
                .Select(t => new LineData { Date = t.Key, Count = t.Count() })
                .ToListAsync();

            var carCount = await _generalRepository.GetQueryable<Car>().CountAsync();
            var carRecords = new List<LineData>();
            foreach (var item in rentalRecords)
            {
                carRecords.Add(new LineData
                {
                    Date = item.Date,
                    Count = carCount - item.Count
                });
            }
            return Success(new
            {
                rentalRecords,
                maintenanceRecords,
                carRecords
            });
        }

        class LineData
        {
            public string Date { get; set; }
            public int Count { get; set; }
        }
    }
}