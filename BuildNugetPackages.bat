set BASE=%~dp0
set SEARCH=%BASE%
set OUT=%BASE%nuget
mkdir %OUT%
pushd %SEARCH%
FOR /R %BASE% %%I in (*BuildNuget.bat) DO call %%I 
FOR /R %SEARCH% %%I in (Release\*.nupkg) DO move %%I %OUT%
popd