
@rem You can pass /p:Proxy=http://127.0.0.1:8080 to use a proxy for download. This works also on get-git.msbuild
msbuild nuget-packages.msbuild
@rem You can pass /p:GitVersion=2.5.1 to get a certain git version.
msbuild get-git.msbuild
