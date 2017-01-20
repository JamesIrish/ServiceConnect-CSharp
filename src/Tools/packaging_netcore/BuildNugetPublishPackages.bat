SET OUTDIR=C:\Git\ServiceConnect\src\
SET OUTDIRFILTERS=C:\Git\ServiceConnect\filters\

@ECHO === === === === === === === ===

@ECHO ===NUGET Publishing ....

del *.nupkg

:: comment

NuGet pack "%OUTDIR%ServiceConnect\ServiceConnect.nuspec"
NuGet pack "%OUTDIR%ServiceConnect.Interfaces\ServiceConnect.Interfaces.nuspec"
NuGet pack "%OUTDIR%ServiceConnect.Container.StructureMap\ServiceConnect.Container.StructureMap.nuspec
NuGet pack "%OUTDIR%ServiceConnect.Persistance.MongoDb\ServiceConnect.Persistance.MongoDb.nuspec
NuGet pack "%OUTDIR%ServiceConnect.Persistance.MongoDbSsl\ServiceConnect.Persistance.MongoDbSsl.nuspec

nuget push ServiceConnect.4.0.0-pre.nupkg -Source https://www.nuget.org/api/v2/package
nuget push ServiceConnect.Client.RabbitMQ.4.0.0-pre.nupkg -Source https://www.nuget.org/api/v2/package
nuget push ServiceConnect.Interfaces.4.0.0-pre.nupkg -Source https://www.nuget.org/api/v2/package
nuget push ServiceConnect.Container.StructureMap.4.0.0-pre.nupkg -Source https://www.nuget.org/api/v2/package
nuget push ServiceConnect.Persistance.MongoDb.4.0.0-pre.nupkg -Source https://www.nuget.org/api/v2/package
nuget push ServiceConnect.Persistance.MongoDbSsl.4.0.0-pre.nupkg -Source https://www.nuget.org/api/v2/package


@ECHO === === === === === === === ===

PAUSE
