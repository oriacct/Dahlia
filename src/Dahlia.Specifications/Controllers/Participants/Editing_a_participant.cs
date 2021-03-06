﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Dahlia.Controllers;
using Dahlia.Models;
using Dahlia.Repositories;
using Dahlia.Services.Builders;
using Dahlia.ViewModels;
using Machine.Specifications;
using MvcContrib.TestHelper;
using Rhino.Mocks;

namespace Dahlia.Specifications.Controllers.Participants
{
    [Subject("Editing a participant")]
    public class when_the_user_starts_to_edit_a_participant : ParticipantControllerContext
    {
        Establish context = () =>
        {
            _participant = new Participant
            {
                Id = 123,
                FirstName = "Fred",
                LastName = "Flintstone",
                DateReceived = DateTime.Parse("1/1/2010"),
                Notes = "note",
                PhysicalStatus = PhysicalStatus.Limited
            };
            _viewModel = new EditParticipantViewModel();
            _participantRepository.Stub(x => x.GetById(123)).Return(_participant);

            the_beds = new List<Bed>() { new Bed { Code = "abc", Id = 1 } };
            var _retreat = new Retreat { Id = 99, Description = "random desc" };
            var the_registrations = new List<Registration> { new Registration { Bed = the_beds.First(), Participant = _participant, Retreat = _retreat } };

            _retreat.AddRegistrations(the_registrations);
            the_retreats = new List<Retreat> { _retreat };

            _retreatRepository.Stub(x => x.GetForParticipant(Arg<int>.Is.Anything)).Return(the_retreats);
            _bedRepository.Stub(x => x.GetAll()).Return(the_beds);
            
       };

        Because of = () =>
        {
            _actionResult = _controller.Edit(123);
            _viewModel = _actionResult.AssertViewRendered().ForView("").WithViewData<EditParticipantViewModel>();
        };

        It should_display_the_edit_view_for_the_requested_participant = () =>
            _actionResult.AssertViewRendered().ForView("").WithViewData<EditParticipantViewModel>();

        It should_create_a_view_model_containing_the_participant_id = () =>
            _viewModel.Id.ShouldEqual(_participant.Id);

        It should_create_a_view_model_containing_the_participant_first_name = () =>
            _viewModel.FirstName.ShouldEqual(_participant.FirstName);

        It should_create_a_view_model_containing_the_participant_last_name = () =>
            _viewModel.LastName.ShouldEqual(_participant.LastName);

        It should_create_a_view_model_containing_the_participant_date_received = () =>
            _viewModel.DateReceived.ShouldEqual(_participant.DateReceived);

        It should_create_a_view_model_containing_the_participant_notes = () =>
            _viewModel.Notes.ShouldEqual(_participant.Notes);

        It should_create_a_view_model_containing_the_participant_physical_status = () =>
            _viewModel.PhysicalStatus.ShouldEqual(_participant.PhysicalStatus);

        It should_create_a_view_model_containing_the_registrations_for_the_participant = () =>
            _viewModel.CurrentRegistrations.ShouldNotBeEmpty();
        
    }

    [Subject("Editing a participant")]
    public class when_the_user_tries_to_edit_an_invalid_participant : ParticipantControllerContext
    {
        Establish context = () =>
        {
            _participantRepository.Stub(x => x.GetById(123)).Return(null);
        };

        Because of = () =>
        {
            _actionResult = _controller.Edit(123);
        };

        It should_redirect_to_the_retreat_index_view = () =>
            _actionResult.AssertActionRedirect().ToAction<RetreatController>(c => c.Index(null));
    }

    [Subject("Editing a participant")]
    public class when_the_user_saves_an_edited_participant : ParticipantControllerContext
    {
        Establish context = () =>
        {
            _viewModel = new EditParticipantViewModel();
            _invoker.ShouldSucceed = true;
        };

        Because of = () =>
        {
            _actionResult = _controller.Edit(_viewModel);
        };

        It should_cause_the_participant_to_be_edited = () =>
            _invoker.SuppliedInput.ShouldBeTheSameAs(_viewModel);

        It should_redirect_to_the_retreat_index_view = () =>
            _actionResult.AssertActionRedirect().ToAction<RetreatController>(c => c.Index(null));
    }

    [Subject("Editing a participant")]
    public class when_the_user_saves_an_invalid_participant : ParticipantControllerContext
    {
        Establish context = () =>
        {
            _viewModel = new EditParticipantViewModel();
            _invoker.ShouldSucceed = false;
        };

        Because of = () =>
        {
            _actionResult = _controller.Edit(_viewModel);
        };

        It should_display_the_edit_view_again = () =>
            _actionResult.AssertActionRedirect().ToAction("Edit");
    }

    public class ParticipantControllerContext
    {
        public static IParticipantRepository _participantRepository;
        public static FakeControllerCommandInvoker _invoker;
        public static ParticipantController _controller;
        public static ViewResult _viewResult;
        public static ActionResult _actionResult;
        public static EditParticipantViewModel _viewModel;
        public static Participant _participant;
        public static IRetreatRepository _retreatRepository;
        public static IBedRepository _bedRepository;

        public static IEnumerable<Retreat> the_retreats = new List<Retreat>();
        public static IEnumerable<Bed> the_beds = new List<Bed>();
        public static CurrentRegistrationBuilder _currentRegistrationBuilder;
        Establish context = () =>
        {
            _participantRepository = MockRepository.GenerateStub<IParticipantRepository>();
            _retreatRepository = MockRepository.GenerateStub<IRetreatRepository>();
            _bedRepository = MockRepository.GenerateStub<IBedRepository>();
            _invoker = new FakeControllerCommandInvoker();

            _currentRegistrationBuilder = new CurrentRegistrationBuilder(_retreatRepository, _bedRepository);

            _controller = new ParticipantController(_retreatRepository, _participantRepository, _invoker,_currentRegistrationBuilder);
        };
    }
}
