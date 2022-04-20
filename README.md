# Inventory Module!

Inventory App is a web based solution where brands can add products and manage them.It has functionality to Create, edit, delete product. Product has details like ProductId, ItemId, Selling Price, Manufacturing Price and quantity
## Getting started


### Prerequisites  

 - Visual Studio 2019 or later  
 - .Net core (3.1)
 - Azure cosmos DB explorer


### Dependancies
#### Nuget Packages
    - AutoMapper.Extensions.Microsoft.DependencyInjection
    - Microsoft.Azure.Cosmos
    - Microsoft.Extensions.DependencyInjection.Abstractions
    - Microsoft.Extensions.Http.Polly
    - Microsoft.Extensions.Options
    - Microsoft.NET.Test.Sdk
    - Moq
    - RestSharp
    - Swashbuckle.AspNetCore
    - System.Net.Http.Json
    - xunit
    - xunit.runner.visualstudio


### Database 
Database Server: Cosmos DB explorer
Database Name: inventory
Database Container: corebidding

### Project Architecture
Specify your project architecture here. Suppose you are following Repository pattern then mention your all the projects along with short description here.

 - Product.Api (End points to manage product)
 - Product.Common (Contains references of other assemblies that are common among multiple projects) 
 - Product.DAL (Contains database operations)
 - Product.DataModel (Contains request, response and shared data model classes)
 - Product.Service (Contains business logic)
 - Product.Utility (Contains helper methods for business operations)
 - Product.ValidationEngine (Contains logic to validation product request model)
 - Playground (Web app to interact) 
 - Product.Api.UnitTests (Contains all the test methods)

## Running the tests
In this project we have used XUnit. You can run all the tests from the Test Explorer. If Test Explorer is not visible, choose  **Test**  on the Visual Studio menu, choose  **Windows**, and then choose  **Test Explorer**. All the unit tests will be listed so choose the test you want to run. You can also run alto tests by selecteing "Run All" option.

