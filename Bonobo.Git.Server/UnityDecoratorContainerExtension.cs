using System;
using System.Collections.Generic;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;

namespace Bonobo.Git.Server
{
    // Credit to http://www.beefycode.com/post/Decorator-Unity-Container-Extension.aspx
    // for providing sample code for handling decorators in Unity
    public class UnityDecoratorContainerExtension : UnityContainerExtension
    {
        private Dictionary<Type, List<Type>> _typeStacks;
        protected override void Initialize()
        {
            _typeStacks = new Dictionary<Type, List<Type>>();
            Context.Registering += AddRegistration;

            Context.Strategies.Add(
                new UnityDecoratorBuildStrategy(_typeStacks),
                UnityBuildStage.PreCreation
            );
        }

        private void AddRegistration(object sender, RegisterEventArgs e)
        {
            if (!e.TypeFrom.IsInterface)
            {
                return;
            }

            List<Type> stack = null;
            if (!_typeStacks.ContainsKey(e.TypeFrom))
            {
                stack = new List<Type>();
                _typeStacks.Add(e.TypeFrom, stack);
            }
            else
            {
                stack = _typeStacks[e.TypeFrom];
            }

            stack.Add(e.TypeTo);
        }
    }
}
