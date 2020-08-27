USE [master]
RESTORE DATABASE [Traducir.Ru] 
FROM  DISK = N'C:\Distr\Traducir\20200827161428-Traducir.Ru.bak' 
WITH  FILE = 1,  MOVE N'Traducir.Ru' TO N'C:\DATA\MSSQL15.SQLEXPRESS\MSSQL\DATA\Traducir.Ru.mdf',  
      MOVE N'Traducir.Ru_log' TO N'C:\DATA\MSSQL15.SQLEXPRESS\MSSQL\DATA\Traducir.Ru_Log.ldf',  NOUNLOAD,  STATS = 5

GO


