﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Dahlia.Models;
using Dahlia.Repositories;
using Dahlia.Services;
using Dahlia.ViewModels;

namespace Dahlia.Controllers
{
    public class RetreatController : Controller
    {
        readonly IRetreatRepository _retreatRepository;
        readonly IUrlMapper _urlMapper;
        readonly JavaScriptSerializer _serializer;

        public RetreatController(IRetreatRepository retreatRepository, IUrlMapper urlMapper, JavaScriptSerializer serializer)
        {
            _retreatRepository = retreatRepository;
            _urlMapper = urlMapper;
            _serializer = serializer;
        }

        public ActionResult Index()
        {
            var model = GetModel();

            return View(model);
        }

        RetreatListViewModel GetModel()
        {
            var retreats = GetRetreats().ToArray();
            retreats.First().Active = true;
            return new RetreatListViewModel
                   {
                       CreateLink = new Uri("/Retreat/Create", UriKind.Relative),

                       Retreats = retreats
                   };
        }

        IEnumerable<RetreatListRetreatViewModel> GetRetreats()
        {
            return _retreatRepository.GetList().OrderBy(x => x.StartDate).Select(
                x => new RetreatListRetreatViewModel
                     {
                         Date = x.StartDate,
                         ViewLink = LinkForRetreat(x),
                         RegisteredParticipants = x.RegisteredParticipants.Select(
                             y => new RetreatListParticipantViewModel
                                  {
                                      FirstName = y.Participant.FirstName,
                                      LastName = y.Participant.LastName,
                                      BedCode = y.BedCode,
                                      DateReceived = y.Participant.DateReceived,
                                      Notes = y.Participant.Notes,
                                      DeleteLink = BuildDeleteLink(x, y)
                                  })
                     });
        }

        public ActionResult Get(DateTime retreatDate)
        {
            var retreat = _retreatRepository.Get(retreatDate);

            var viewModel = new RetreatListRetreatViewModel
                            {
                                Date = retreat.StartDate,
                                AddParticipantLink = AddParticipantLinkForRetreat(retreat),
                                RegisteredParticipants = retreat.RegisteredParticipants.Select(
                                    y => new RetreatListParticipantViewModel
                                         {
                                             FirstName = y.Participant.FirstName,
                                             LastName = y.Participant.LastName,
                                             BedCode = y.BedCode,
                                             DateReceived = y.Participant.DateReceived,
                                             Notes = y.Participant.Notes,
                                             DeleteLink = BuildDeleteLink(retreat, y)
                                         })
                            };

            return View(viewModel);
        }

        Uri BuildDeleteLink(Retreat retreat, RegisteredParticipant participant)
        {
            return _urlMapper.MapAction<ParticipantController>(
                x => x.DeleteFromRetreat(
                    retreat.StartDate, 
                    participant.Participant.FirstName,
                    participant.Participant.LastName));
        }

        Uri AddParticipantLinkForRetreat(Retreat retreat)
        {
            return _urlMapper.MapAction<ParticipantController>(
                x => x.AddToRetreat(retreat.StartDate));
        }

        Uri LinkForRetreat(Retreat retreat)
        {
            return _urlMapper.MapAction<RetreatController>(
                x => x.Get(retreat.StartDate));
        }

        public ActionResult Create()
        {
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Create(Retreat retreatModel)
        {
            if (!ModelState.IsValid)
                return View();
            _retreatRepository.Add(retreatModel);
            return RedirectToAction("Index");
        }

        public ViewResult App()
        {
            var retreatsVm = GetModel();
            return View(retreatsVm);
        }
    }
}
