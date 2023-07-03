#EF Core

dotnet ef migrations add [name] -c SechatContext -s ..\Sechat.Service

Add-Migration InitialCreate -c SechatContext -Project Sechat.Data

Remove-Migration -c SechatContext -Project Sechat.Data -force
