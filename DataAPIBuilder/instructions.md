[Github Link](https://github.com/Azure/data-api-builderck)
[Documentation](https://learn.microsoft.com/en-gb/azure/data-api-builder/)

To initially initialize the setup and create the configuration file run the following command:

```shell
    dab init --database-type "mssql" --connection-string "Data Source=Localhost;Initial Catalog=VoiceLauncher;Integrated Security=True;Connect Timeout=120;Encrypt=False;TrustServerCertificate=True;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"
```


Whenever dab is configured it is started by:

```shell
    dab start
```

To add individual tables to the configuration file use the following command:

```shell
    dab add SomeTable --source "dbo.sometable"  --permissions "anonymous:*"
``` 
For example:

```shell
    dab add Languages --source "dbo.Languages"  --permissions "anonymous:*"
```


dab add Languages --source "dbo.Languages"  --permissions "anonymous:*"
dab add Categories --source "dbo.Categories"  --permissions "anonymous:*"
dab add CustomIntelliSense --source "dbo.CustomIntelliSense"  --permissions "anonymous:*"

Using this method no longer gives you direct access to the database context you would have to access the data all through the Web API or web data API. Ideal if you're building a web assembly application or one that needs to switch between Blazor server and Blazor client.

Note that Docker has to be installed for this to work as it is running in a container.

Note could not get swagger or graph ql working