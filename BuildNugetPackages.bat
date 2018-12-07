set BASE=%~dp0
set SEARCH=%BASE%
set OUT=%BASE%nuget
mkdir %OUT%
pushd %BASE%
dotnet build -c Release
pushd %SEARCH%
FOR /R %SEARCH% %%I in (Release\*.nupkg) DO move /Y %%I %OUT%
popd
popd