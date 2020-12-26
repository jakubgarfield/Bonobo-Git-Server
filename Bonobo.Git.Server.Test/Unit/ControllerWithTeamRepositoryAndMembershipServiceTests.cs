using Bonobo.Git.Server.Controllers;
using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;

namespace Bonobo.Git.Server.Test.Unit
{
    public partial class ControllerTests
    {
        [TestClass]
        public class ControllerWithTeamRepositoryAndMembershipServiceTests : ControllerTests
        {
            protected Mock<IMembershipService> membershipServiceMock;
            protected Mock<ITeamRepository> teamRepositoryMock;

            // TeamRepository Mock
            protected void SetupControllerContextAndTeamRepository()
            {
                sut.ControllerContext = CreateControllerContext();
                teamRepositoryMock = new Mock<ITeamRepository>();
            }

            // MembershipService Mock
            protected void SetupMembershipServiceMock()
            {
                membershipServiceMock = new Mock<IMembershipService>();
            }

            protected void SetupMembershipServiceMockToReturnAnEmptyListOfUsers()
            {
                membershipServiceMock.Setup(m => m.GetAllUsers())
                                     .Returns(new List<UserModel>());
            }

            protected void SetupTeamRepositoryToSucceedWhenCreatingATeam()
            {
                teamRepositoryMock.Setup(r => r.Create(It.IsAny<TeamModel>()))
                                  .Returns(true);
            }

            protected void SetupTeamRepositoryMockToReturnASpecificTeamWhenCallingGetTeamMethod(Guid requestedGuid)
            {
                teamRepositoryMock.Setup(t => t.GetTeam(requestedGuid))
                                  .Returns(new TeamModel
                                  {
                                      Id = requestedGuid,
                                      Members = new UserModel[0]
                                  });
            }
        }
    }
}
