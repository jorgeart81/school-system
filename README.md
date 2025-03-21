# School_System

## Development

1. Initialize the database.
2. Add the connection to the database - DefaultConnection
3. From the root path of the project execute:
	```dotnetcli
      ## windows
      dotnet ef database update --context TenantDbContext --startup-project ..\WebApi\
      ## macOs
      dotnet ef database update --context TenantDbContext --startup-project WebApi
    ```
4. From the ..\WebApi route execute:
	```dotnetcli
      dotnet run
    ```

