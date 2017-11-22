using System;

namespace Bonobo.Git.Server.Data
{
    public interface INameProperty
    {
        Guid Id { get; }
        string Name { get; }
        string DisplayName { get; }
    }
}