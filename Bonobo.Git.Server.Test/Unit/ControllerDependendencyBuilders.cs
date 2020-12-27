using Bonobo.Git.Server.Data;
using Bonobo.Git.Server.Models;
using Bonobo.Git.Server.Security;
using Moq;
using System;
using System.Collections.Generic;

namespace Bonobo.Git.Server.Test.Unit
{
    public static class ControllerDependendencyBuilders
    {
        // TeamRepository Mock
        public static Mock<ITeamRepository> SetupToSucceedWhenCreatingATeam(this Mock<ITeamRepository> teamRepositoryMock)
        {
            teamRepositoryMock.Setup(r => r.Create(It.IsAny<TeamModel>()))
                              .Returns(true);
            return teamRepositoryMock;
        }

        public static Mock<ITeamRepository> SetupToReturnASpecificTeamWhenCallingGetTeamMethod(this Mock<ITeamRepository> teamRepositoryMock, Guid requestedGuid)
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
        public static Mock<IMembershipService> SetupToReturnAnEmptyListOfUsers(this Mock<IMembershipService> membershipServiceMock)
        {
            membershipServiceMock.Setup(m => m.GetAllUsers())
                                 .Returns(new List<UserModel>());
            return membershipServiceMock;
        }

        public static Mock<IMembershipService> SetupToReturnAnEmptyUserModelListWhenCallingGetAllUsers(this Mock<IMembershipService> membershipServiceMock)
        {
            membershipServiceMock.Setup(s => s.GetAllUsers())
                                 .Returns(new List<UserModel> { });
            return membershipServiceMock;
        }

        // IRepositoryRepository Mock
        public static Mock<IRepositoryRepository> SetupToReturnAnEmptyListForASpecificIdWhenCallingGetTeamRepositories(this Mock<IRepositoryRepository> repositoryRepositoryMock, Guid requestedId)
        {
            repositoryRepositoryMock.Setup(r => r.GetTeamRepositories(new[] { requestedId }))
                                    .Returns(new List<RepositoryModel>());
            return repositoryRepositoryMock;
        }

        public static Mock<IRepositoryRepository> SetupToReturnAModelWithASpecificIdWhenCallingGetRepositoryMethod(this Mock<IRepositoryRepository> repositoryRepositoryMock, Guid guid)
        {
            repositoryRepositoryMock.Setup(s => s.GetRepository(guid))
                                    .Returns(new RepositoryModel
                                    {
                                        Id = guid,
                                        Administrators = new UserModel[0],
                                        Name = "name"
                                    });
            return repositoryRepositoryMock;
        }
    }
}
