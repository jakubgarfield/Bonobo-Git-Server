
set config=Debug


rem END OF CONFIGURATIONS

call setup-env.cmd

msbuild Bonobo.Git.Server.sln /m /property:Configuration=%config%
if exist Bonobo.Git.Server.Test/bin/%config%/Bonobo.Git.Server.Test.dll (
	vstest.console Bonobo.Git.Server.Test/bin/%config%/Bonobo.Git.Server.Test.dll
)
