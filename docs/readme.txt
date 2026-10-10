




Packages Used
--------------
Serilog.AspNetCore
Microsoft.Extensions.Logging.Abstractions  
Microsoft.Extensions.Http.Resilience
Microsoft.AspNetCore.OpenApi


Testing Frameworks
------------------
NUnit


Setup
-----
To run the service
------------------

After you have downloaded and built the solution in Visual Studio, set RockTrack.Api project as the start project and click F5. 
For tests,
1. test url is https://localhost:7242/members/1/rocks   (or https://localhost:7242/openapi/v1.json for openAPI endpoint readable only through Postman as it requires the API Key) 
2. the required API authetication API key are in : appsettings.Development.json
3. the same api key plus url above is included in Postman tests. 
2. 5 rocks objects are seeded each time the appliacation starts for testing purposes. The rock Id are Guids and are regenerated randomly each appliacation start.


To run Postman
--------------
1. In the docs folder of the repo you will find the file  VA-Assessment-Tests.postman_collection.json .Import this file into Postman.
2. The collection should already have the API Key (see X-Api-Key in the json file). 


Testing of endpoint:
--------------------
Below are all the tests availble in the Postman for testing various scenations. Many can be re-used to test edge cases. 
Note that for test that are PATCH requests will require selecting a valid rock Guid after each application start.

A) GET /members/{memberId}/rocks 
	GET  
		GetMemberRocks-member1
		GetMemberRocks-member2
		GetMemberRocks?Status=pending
		GetMemberRocks?Status=nonexistent

	GET (with Correlation Id)
		GetMemberRocks-member1-CorrId
		
		GetOpenAPI-Endpoint (needs API Key)  - for testing https://localhost:7242/openapi/v1.json 

B) POST /members/{memberId}/rocks
	POST 
		Sample-Rock-Creation(Post)

	POST (with Correlation Id)
		Sample-Rock-Creation-Post-CorrId
		Sample-Rock-Creation-Post-CorrId Invalid
		Sample-Rock-Creation-Post-CorrId Error Body
		Sample-Rock-Creation-Post-CorrId -CoorIdOff

C) PATCH /members/{memberId}/rocks/{rockId} 
	PATCH 
		RockUpdate

	PATCH (with Correlation Id)
		RockUpdate-Patch-CorrId  

D) GET /members/{memberId}/profile/enriched
	
	GET Profile Service
		GetMemberRocks-member1 -Profile
		GetMemberRocks-member3-Profile(no rocks)
		GetMemberRocks-member11-Profile(no member)
		

 
---------------------------------


Notes
-----
Due to time constraints I left out the completet solution to these sub-sections / spec points:
Section 4: If I understand  "Implemenete global exception handling middleware" as a custom implementation, I missed this point. I do implement app.UseExceptionHandler() in program.cs.
Section 7: I have no test for a scenario where 3rd part fails after all retries. I would look to write a test http level test. 

In this project I have used Gen AI to research, review and generate code to complete the tasks. All the code has been reviewed me on checkin.


Assumptions
-----------
1. A mention of a note field appears for the first time in section 3. It is not immediately clear if this refers to a property of a Rock as such property is not mentioned in section 1.
I will assume "notes" is a property of the Rock classl and that for a Career Category, the active rule checks to see if the Note field   

2. The spec seems to describe a single frontend client where it refers to the shared API key. For production, For production, requirement 8's reference to "external clients" (Section 8) suggests multiple consumers/clients . I will assume one single client for this API.  

3. In Section 3 - Revenue category rules - the assumption is that these are calendar quarter (not finacial), Also as we are dealing with dates and to soem extent time - we will assume UTC only as a simplification. As time zoen calculations can eb complicated.


Design Tradeoffs
----------------
1. I have approached the code organisation from a liqhtweight Hexagonal Architecture angle where in the solution all projects point inward into project RockTrack.Core. This way the core (domain and business rules) depend on nothing external, Storage and the profile API are adapters behind interfaces. So the in-memory storage could later be replaced, the resilience setup stays out of the business logic, and every rule is unit-tested without a web server or network.

2. To implement the in-memory collection as a sigleton and shared by requests, I need to consider thread safety. ConcurrencyDictionary seesm like a scallable fit but it turns out that ConcurrencyDictionary only locks part of the items in the dictionary which means that objects in the collection can still be changed and I would need to implement locking in a write scenario. The more aggressive mechanism in using a plain Dictionary + lock (neither read or write if there is a lock) is a strong guarantee and avoid designing locking mechanisms for write scenarios. So Dictionary + lock though less scallable is prefered.

3. In "Input validation requirements" (2), 
	a. MemberId was implemented as an int (as it is on the jsonplaceholder endpoint). Consequently, I will apply an equivalent requirement that member Id be a positive number instead.
	b. The requirement "Due date must be in the past" is not specifc becasue I use DateOnly type. I will assume that past mean the day before. 
	c. I considered both Data Annotations and FluentValidation to implement validation. I opted for Data Annotations as it is built into >NET so no 3rd party library and is simple for teh task at hand. I would consider FluentValidation for more complex validation logic.

4.  I added a data seeder in RockTracker.Api project to pre-seed the in-memory collection to facilitate testing.

6. I have added detaisl of my approach to section 3  below "Strategy for Dealing with Section 3"

7. In section 5 (Structured Logging and Correlation), for easier implementation, I am going to use Serilog over the .NET built in logging (Microsoft.Extensions.Logging) as Serilog offers easier and finer external configuration of the multiple output types beyond just Levels (per-namespace overrides, outputs, format, enrichers).  For this I need the following packages:
	-Serilog.AspNetCore in the API project -  Hosting integration, request logging, console sink, CompactJsonFormatter, reading from configuration (all included)
	-Microsoft.Extensions.Logging.Abstractions in the Core project - ILogger<T> for RockService  - interfaces only, no Serilog in Core so this keeps the pure separation with other projects. With the interface we can pass details from RockService in the logging scheme.
	A series of Postman tests show the correlation Id omming back in the request for PATCH,GET and POST requests. 
	Also I can see output in Visual Studio output console window as hsown in this sample: 

		{"@t":"2026-10-09T20:37:16.0684350Z","@mt":"HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms","@r":["3.0071"],"@tr":"25bebf688b399f1fa84be012596f8e87","@sp":"9af3ae5b91e09e4e","RequestQuery":"","RequestMethod":"GET","RequestPath":"/members/1/rocks","StatusCode":200,"Elapsed":3.0071,"SourceContext":"Serilog.AspNetCore.RequestLoggingMiddleware","CorrelationId":"so-BEA-00001","RequestId":"0HNP68JGDRSHR:00000001","ConnectionId":"0HNP68JGDRSHR","Application":"RockTrack"}

8. In section 6, (API Key Authentication), I have decided to enforece Authentication using [Authentication] in each controller instead of using a fallback policy. Though the fallback policy is global handling ll endpoints in 	one go, [Authenticator] allows a more granular approach per controller. I have also taken a strict interpretation of "Protect all endpoints with API key authentication" to include  /openapi/v1.json for display openapi. hence this endpoint cannot be reached in the browser. I will include the endpoint as part of the Postman collection which provide the target API-key in requests.  
	

9. In section 7(3rd Party Integration with Resilience): 

	a. I chose a separate controller as this was the cleanest approach that preserves the existing endpoints in RockController and allows for the new dependency on a new resource (profile service) that will in turn call the 2 other dependent service (existing RockService and a new one getting the profile).
	b. due to the slow and unrealiable I/O involved in calling an external API, the new enpoint should use CancellationToken to improve 
	c. The calls to the 2 dependent services could be made in paralell but there is little benefit as the RockService call will be nearly instantaneous compared to the external Profile service call. Pralelilisation only add unneded complexity so I will the calls in sequence starting with the RockService.
	d. regarding supporting libraries, between AddStandardResilienceHandler() and Microsoft.Extensions.Http.Resilience with a custom pipeline(AddResilienceHandler), the later between Custom pipeline  was chosen so each requirement is explicit and testable. 
	e. I am limiting the amount of data I am getting from the jsonplaceholder API to a contract with name, username and email only. This keeps the payload simple and data minimised i.e. only taking the minimum information needed.	


Notes
-----

1. Section 6: API Key Authentication: regarding Postman and the API key
 For simplicity, the Postman collection sets X-Api-Key once at collection level with and every request inherits it. This is a development-only key used for local testing; it must match the key configured for the API (see Running locally).

  Regarding deployment to production: The API itself never hardcodes the key: it's read from configuration which in Azure would be Azure Key Vault in production.



Security Concerns
-----------------
1. If the App were multi-client, using one API-Key for all Clients (implied by the necessity of storage of a single) is a security concern.



Things I would add/improve given more time
------------------------------------------
1. Refactor the existing tests - they can be collapsed into test cases for example. I would also add more tests to increase coverage.

2. The Input Validation fixes were implemented and meets all the spec criteria but there are a few edge cases where the validation messages returned malformed JSON indicating there is an issue with the validation engine. As I result I get validation messages like the below. I would fix these. This is likely resolvable but requires more testing. - example of an error message when I validate DueDate. 
	"$.dueDate": ["…could not be converted to System.DateOnly…"]  
	
3. I would explore a lighter version of a spec-driven development workflow approach for the project.  	




Strategy for Dealing with Section 3
-----------------------------------

On a high level I start with a crude implementation of the rules (Stage 1) implementing rules in RockCategoryValidator class with unit tests for one Category Health. Moving rules to RockCategoryValidator which implements IRockCategoryValidator allows me to use DI and inject into the existing code - this also faciliates testing. At this point I have taken a step toward strategy pattern while still in the old implementation. After all rules are captured in code (RockCategoryValidator) one category at a time (Health (simplest), Career, then Revenue) with passing tests, I then extend the implemenation (Stage 2)fully use the Startegy Pattern for the next category.
DI in program.cs is updated with  IRockCategoryValidator.
 
 Stage 1: 
 I have a basic structure with unit test that I can verify the rule logic
 RockTrack.Core/Validation/
	IRockCategoryValidator.cs        (interface)
	RockCategoryValidator.cs         (holds every category rule as an if/else branch)
 
 
 Stage 2: 
 I now almost lift and shift the rules in classes inheriting from ICategoryRule and transform RockCategoryValidator into the dispatcher.
 the unit tests files only have small additive changes. Existing tests stay the same - making for an easy error free transformation.
 Extra unit tests are added to ensure all categories adhere to a rule. Also DI in program.cs was updated with each new rule classes as ICategoryRule .
 
 RockTrack.Core/Validation/
		IRockCategoryValidator.cs        (unchanged)
		RockCategoryValidator.cs         (becomes the dispatcher)
		CategoryRules/
			ICategoryRule.cs
			CategoryRuleContext.cs
			RevenueRule.cs
			HealthRule.cs
			CareerRule.cs
			OtherRule.cs
 
 
 Steps for Adding a new Category become:
  1. add an enum value, 
  2. add a rule class implementing from ICategoryRule
  3. add one DI line
  
In summary my reasons for this approach is:
1. In stage 1, I can implement all the rules in one file and have the benefit of sufficiently decoupled code to  have unit tests.
2. Once stage 1 is complete, I can now with the same tests extend into a full strategy pattern
3. I find having less files initially easier to think about organisationally.
4. It is also flexible as I can implement just 2 rules in step 1, refactor into full stategy pattern and add my remaining rules as classes.  

 
 
Cloud Deployment and Arquitecture
----------------------------------

1. Choice of Azure Compute Service:
-----------------------------------

I'd recommend Azure Container Apps (ACA) mainly because the scenario describes a small service with weekly commitments of rocks - this indicates that we will likely have daytime spikes and an overnight lull in demand with idle periods. Also the word small seems to indicate a lower membership number initially. ACA  can scale to zero when idle and incure no compute costs and can scale out when we have weekly spikes.
Although development is constrained to using an in-memory collection, a production application would certain use database hence scaling down to zero is a real choice e.g. overnight and during the weekend. 

A decision will need to be made around cold-starts (if scaling to zero is an option) and acceptable user response times on the first application starts.  Possible strategies to work around this tension are:
1. A scheduled replica  start as a warm up that can allow faster initial response from users say in the morning
2. In a multi environment scenario eg dev, staging and production,  dev and staging scale down to zero replicas when not in use. prod can maintain a sigle replica longer as needed. 
3. We can use Native AOT within ACA for faster Web API cold start response.  


Azure App Service would be a great contender except for the cost as it only only supports an always-on sceanario - hence you pay for compute even when the site is idle.  

Azure functions are ideal for event handling scenarios and HTTP is one the events supported but has a contrived support for APIs (e.g. limited ASP.NET Core pipeline) as it is not designed from the ground up to handle HTTP. 


2. Exposing the API to external clients: 
---------------------------------------

If the API is exposed directly and publiclly, this would be cheap and simple and would scale all the way down to zero but it would  would also expose the API to
- uncontroled rate usages and application level abuse that may overwhelm the backedn 
- and some forms of firect DOS attacks by itself
 
By Adding Azure API management (APIM) - you get mechanisms that mitigate the above issues like Rate Limiting, Quotas, Concurrency Limits, IP filtering, Response caching that provide a layer of protection 
and would be sufficient for most applications although it does not porovide complete protection from DOS attacks. APIM adds a permanent always on cost to running and API.

Azure Front Door is a global edge networkk like Clouflare that provide services like Caching/CDN, DDoS absorption. It solves a different and complementary problem to APIM  APIM ussually sits in tamden with it.  Along with the added protection means additional costs. Azure Front Door adoption is ussually driven by business need - for example revenue critical availability, DDoS protection and service provided across multiple globally across multiple countries. For an initial release of a new service APIM would be sufficient.     


3. Secrets Management :
----------------------
At rest,  Azure Key Vault is used to store any secrets for the App is in production. When the App hosted in Azure requires a secret (often at startup or during periodic refreshing), the secret is looked up by the app using a managed identity and typically stored in the app process memory during runtime or lifetime of teh process or until teh next refresh.


4. CI/CD pipeline strategy:
--------------------------
I would define the infrasructure using Terraform. I would created 3 environments (dev, staging and production) each with its own settings.

Deployment would run through a multistage Azure devOps YAML pipeline which logs into Azure. All code would be stored in GitHub 

Deployment runs through a multi-stage Azure DevOps YAML pipeline, Every pull request passes (peer reviews) a validation run before merging which involves build and tests and a terraform plan that reviewers to read.

On merge, the app is built once, and that same artifact moves through each environment, Terraform applies the reviewed plan, the app is deployed to dev and smoke-tested, after an approval gate, it's deployed to a production staging slot and swapped live with no downtime.

Before running in production, RockTrack would need these changes:
- a real database in place of the in-memory store;
- logs sent to Application Insights.