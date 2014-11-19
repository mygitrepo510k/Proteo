using Cirrious.MvvmCross.Test.Core;
using Moq;
using MWF.Mobile.Core.Models.Instruction;
using MWF.Mobile.Core.Repositories;
using MWF.Mobile.Core.Repositories.Interfaces;
using MWF.Mobile.Core.Services;
using MWF.Mobile.Core.ViewModels;
using MWF.Mobile.Tests.Helpers;
using Ploeh.AutoFixture;
using Ploeh.AutoFixture.AutoMoq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MWF.Mobile.Tests.ViewModelTests
{
    public class ReviseQuantityViewModelTests
        : MvxIoCSupportingTest
    {

        #region Setup

        private IFixture _fixture;
        private MobileData _mobileData;
        private Mock<IMobileDataRepository> _mobileDataRepo;
        private Mock<INavigationService> _navigationService;

        protected override void AdditionalSetup()
        {
            _fixture = new Fixture().Customize(new AutoMoqCustomization());

            _mobileData = _fixture.Create<MobileData>();

            _mobileDataRepo = _fixture.InjectNewMock<IMobileDataRepository>();
            _mobileDataRepo.Setup(mdr => mdr.GetByID(It.Is<Guid>(i => i == _mobileData.ID))).Returns(_mobileData);

            _fixture.Inject<IRepositories>(_fixture.Create<Repositories>());

            _navigationService = _fixture.InjectNewMock<INavigationService>();

        }

        #endregion Setup

        #region Tests

        [Fact]
        public void ReviseQuantityVM_SuccessfulUpdate()
        {
            base.ClearAll();

            var reviseQuantityVM = _fixture.Create<ReviseQuantityViewModel>();

            int newQuantity = 123;
            reviseQuantityVM.Init(new NavItem<Item>() { ID = _mobileData.Order.Items.FirstOrDefault().ID, ParentID = _mobileData.ID });

            reviseQuantityVM.OrderQuantity = newQuantity.ToString();

            reviseQuantityVM.ReviseQuantityCommand.Execute(null);

            Assert.Equal(_mobileData.Order.Items.FirstOrDefault().Quantity, reviseQuantityVM.OrderQuantity);

        }

        #endregion Tests
    }
}
