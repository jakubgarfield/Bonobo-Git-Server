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
        public class ControllerDependendencyBuilders : ControllerTests
        {
            // TeamRepository Mock
            protected Mock<ITeamRepository> SetupControllerContextAndTeamRepository()
            {
                sut.ControllerContext = CreateControllerContext();
                return new Mock<ITeamRepository>();
            }

            protected Mock<ITeamRepository> SetupTeamRepositoryToSucceedWhenCreatingATeam(Mock<ITeamRepository> teamRepositoryMock)
            {
                teamRepositoryMock.Setup(r => r.Create(It.IsAny<TeamModel>()))
                                  .Returns(true);
                return teamRepositoryMock;
            }

            protected Mock<ITeamRepository> SetupTeamRepositoryMockToReturnASpecificTeamWhenCallingGetTeamMethod(Mock<ITeamRepository> teamRepositoryMock, Guid requestedGuid)
            {
                teamRepositoryMock.Setup(t => t.GetTeam(requestedGuid))
                                  .Returns(new TeamModel
                                  {
                                      Id = requestedGuid,
                                      Members = new UserModel[0]
                                  });
                return teamRepositoryMock;
            }

            // MembershipService Mock
            protected Mock<IMembershipService> SetupMembershipServiceMock()
            {
                return new Mock<IMembershipService>();
            }

            protected Mock<IMembershipService> SetupMembershipServiceMockToReturnAnEmptyListOfUsers(Mock<IMembershipService> membershipServiceMock)
            {
                membershipServiceMock.Setup(m => m.GetAllUsers())
                                     .Returns(new List<UserModel>());
                return membershipServiceMock;
            }

            protected Mock<IRepositoryRepository> SetupRepositoryRepositoryToReturnAnEmptyListForASpecificId(Guid requestedId)
            {
                var repositoryRepositoryMock = new Mock<IRepositoryRepository>();
                repositoryRepositoryMock.Setup(r => r.GetTeamRepositories(new[] { requestedId }))
                                        .Returns(new List<RepositoryModel>());
                return repositoryRepositoryMock;
            }
        }
    }
}
