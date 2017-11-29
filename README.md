# KnowledgeBank.

## Software dependencies  
1. [Node.js 6+ & npm 3+](https://nodejs.org/dist/v6.10.1/node-v6.10.1-x64.msi)
2. [Angular CLI](https://github.com/angular/angular-cli)
3. [Net Core 1.1](https://go.microsoft.com/fwlink/?linkid=843448)
4. SQL Server 2016

## Running the App
Due to shift to https protocol, application should be started from **visual studio 2017** under **IIS**, the localhost iis development certificate should be added to trusted certificate store.

New Url for identity is https://localhost:44321/identity

## Important

When creating an controller use [Branch] attribute to declare what branch controller refers to, otherwise it will be accessible from any branch, which may lead to issues. 

## Useful

### Adding Migrations from Web project director

#### Replace @Migration with appropriate name

ApplicationDbContext

	dotnet ef migrations add @Migration -p ..\KnowledgeBank.Persistence\KnowledgeBank.Persistence.csproj -c ApplicationDbContext -o Migrations/ApplicationDb

ApplicationIdentityDbContext

	dotnet ef migrations add @Migration -p ..\KnowledgeBank.Persistence\KnowledgeBank.Persistence.csproj -c ApplicationIdentityDbContext -o Migrations/IdentityDb

PersistentDbContext

	dotnet ef migrations add @Migration -p ..\KnowledgeBank.Persistence\KnowledgeBank.Persistence.csproj -c PersistedGrantDbContext -o Migrations/PersistedGrantDb
	
ConfigurationDbContext

	dotnet ef migrations add @Migration -p ..\KnowledgeBank.Persistence\KnowledgeBank.Persistence.csproj -c ConfigurationDbContext -o Migrations/ConfigurationDb

### Generate certificate
    makecert -r -pe -b 01/06/2016 -e 01/06/2029 -eku 1.3.6.1.5.5.7.3.1 -ss My -n CN=KnowledgeBank -sky exchange -sp "Microsoft RSA SChannel Cryptographic Provider" -sy 12 -len 2048

#### Adjust certificate name in appsettings.json if you decide to use other name

## Know-hows

### Refused to load/execute/apply ... because it violates the following Content Security Policy directive: "default-src: 'self'"
Solution: Set dev environment

	setx ASPNETCORE_ENVIRONMENT "Development"
	
### Cannot see the inserted intities in the DB
Solution: execute the folowing command in SSMS (be sure to adjust tenantId)

	exec sp_set_session_context @key=N'TenantId', @value=1

## Deployment build

### 1. build js bundles from Angular folder

	npm run build

### 2. then, publish web project from web folder

	dotnet build -c Release -o @Folder

use a folder outside of web project (example: ../pub ), because it may cause problems with razor compilation


### 3. find *auth-callback.html* file inside publish folder and change the login redirect location to '/knowledgebank/'. 

