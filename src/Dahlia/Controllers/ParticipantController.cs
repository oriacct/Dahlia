﻿using System;
using System.Web.Mvc;
using Dahlia.Models;
using Dahlia.Repositories;
using Dahlia.ViewModels;

namespace Dahlia.Controllers
{
    public class ParticipantController : Controller
    {
        readonly IRetreatRepository _retreatRepository;

        public ParticipantController(IRetreatRepository retreatRepository)
        {
            _retreatRepository = retreatRepository;
        }

        public ViewResult AddToRetreat(DateTime retreatDate)
        {
            var viewModel = new AddParticipantToRetreatViewModel
                                {
                                    RetreatDate = retreatDate,
                                    DateReceived = DateTime.Today,
                                };
            return View("AddToRetreat", viewModel);
        }

        public ActionResult DoAddToRetreat(AddParticipantToRetreatViewModel postBack)
        {
            var retreat = _retreatRepository.Get(postBack.RetreatDate);

            var newParticipant = new Participant
            {
                FirstName = postBack.FirstName,
                LastName = postBack.LastName,
                DateReceived = postBack.DateReceived,
                Notes = postBack.Notes
            };

            var newRegisteredParticipant = new RegisteredParticipant
            {
                Participant = newParticipant,
                Retreat = retreat,
                BedCode = postBack.BedCode
            };

            retreat.RegisteredParticipants.Add(newRegisteredParticipant);

            _retreatRepository.Save(retreat);

            return new EmptyResult();
        }
    }
}
