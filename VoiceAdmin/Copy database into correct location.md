echo %HOME%
dir %HOME%\site\wwwroot\wwwroot\voicelauncher-azure.db
dir %HOME%\site\wwwroot\data\voicelauncher-azure.db

copy /y %HOME%\site\wwwroot\wwwroot\voicelauncher-azure.db %HOME%\site\data\voicelauncher-azure.db
copy /y %HOME%\site\wwwroot\data\voicelauncher-azure.db %HOME%\site\data\voicelauncher-azure.db

dir %HOME%\site\data\voicelauncher-azure.db