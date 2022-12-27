#EF Core

dotnet ef migrations add -c SechatContext -s ..\Sechat.Data
Add-Migration InitialCreate -c SechatContext -s ..\Sechat.Data


