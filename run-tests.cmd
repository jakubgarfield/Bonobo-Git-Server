
set config=Debug


rem END OF CONFIGURATIONS

set curpath=%CD%

call setup-env.cmd

msbuild Bonobo.Git.Server.sln /m /property:Configuration=%config%
if exist Bonobo.Git.Server.Test/bin/%config%/Bonobo.Git.Server.Test.dll (
	mstest /testcontainer:Bonobo.Git.Server.Test/bin/%config%/Bonobo.Git.Server.Test.dll
)

cd %curpath%
