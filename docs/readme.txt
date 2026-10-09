


1. Choice of Azure Compute Service:


I recommend Azure Container Apps (ACA) mainly because the scenario describes a small service with weekly commitments of rocks - this indicates that we will likely have daytime spikes and an overnight lull in demand with idle periods. Also the word small seems to indicate a lower membership number initially. ACA  can scale to zero when idle and incure no compute costs and can scale out when we have weekly spikes.
Although development is constrained to using an in-memory collection, a production application would certain use database hence scaling down to zero is a real choice e.g. overnight and during the weekend. 

A decision will need to be made around cold-starts (if scaling to zero is an option) and acceptable user response times on teh first application starts.  Possible strategies to work around this tension are:
1. A scheduled replica  start as a warm up that can allow faster initial response from users say in the morning
2. In a multi environment scenario eg dev, staging and production,  dev and staging scale down to zero replicas when not in use. prod can maintain a sigle replica longer as needed. 
3. We can use Native AOT within ACA for faster Web API cold start response.  


Azure App Service would be a great contender except for the cost as it only only supports an always-on sceanario - hence you pay for compute even when teh site is idle.  

Azure functions are ideal for event handling scenarios and HTTP is one teh events supported but has a contrived support for APIs (e.g. limited ASP.NET Core pipeline) as it is not designed from teh ground up to handle HTTP. 


2. Exposing the API to external clients 

If the API is exposed directly and publiclly, this would cheap and simple and would scale all teh way down to zero but it would  would also expose the API to
- uncontroled rate usages and application level abuse that may overwhelm teh backedn 
- and some forms of firect DOS attacks by itself
 
By Adding Azure API management (APIM) - you get mechanisms that mitigate the above issues like Rate Limiting, Quotas, Concurrency Limits, IP filtering, Response caching that provide a layer of protection 
and would be sufficient for most applications although it does not porovide complete protection from DOS attacks. APIM adds a permanent always on cost to running and API.

Azure Front Door is a global edge networkk like Clouflare that provide services like Caching/CDN, DDoS absorption. It solves a different and complementary problem to APIM  APIM ussually sits in tamden with it.  Along with the added protection means additional costs. Azure Front Door adoption is ussually driven by business need - for example revenue critical availability, DDoS protection and service provided across multiple globally across multiple countries. For an initial release of a new service APIm would be sufficient.     



 
---------------------------------

Assumptions:
1. A mention of a note field appears for the first time in section 3. It is not immediately clear if this refers to a property of a  Rock as such property is not mentioned in section 1 when describing a Rock object.
I will assume "notes" is a property of the Rock classl and that for a Career Category, the active rule checks to see if the Note field   

2. The spec see,s to describe a single frontend client where it referes to teh shared API key. For production, For production, requirement 8's reference to "external clients" (Section 8) suggests multiple consumers/clients . I will assume one single client for this API.  



Design Tradeoff
1. I have appeoached the code organisation from a liqtweight Hexagonal Architecture approach where in teh solution project RockTrack.Core 

2. To implement teh in-memory collection as a sigleton and shared by requests, I need to consider thread safety. ConcurrencyDictionary seesm like a scallable fit but it turns out that ConcurrencyDictionary only locks part of the items in the dictionary which means that objects in the collection can still be changed and I would need to implement locking in a write scenario. The more aggressive mechanism in using a plain Dictionary + lock (neither read or write if there is a lock) is a strong guarantee and avoid designing locking mechanisms for write scenarios. So Dictionary + lock though less scallable is prefered.

3. In "Input validation requirements" (2), memberId was implemented as an int (as it is on the jsonplaceholder endpoint). Consequently, I will apply an equivalent requirement that member Id be a positive number instead.

4. In "Input validation requirements" (2), the requirement "Due date must be in the past" is not specifc becasue I use DateOnly type. I will assume that past mean the day before. 

5. Added a data seeder in RockTracker.Api project to pre-seed the im-memory collection to facilitate testing.








Security Concerns

1. If the App were multi-client, using one Key for all Clients (implied by the necessity of storage of a single) is a security concern as all members can use it to access members potentially not belonging to them.
2. 





If I had more time, I would: 

1. Refactor the tests - they can be collapsed into test cases

2. The Input Validation fixes were implemented but there are a few edge cases where teh validation messages returned malformed JSON indicating there is an issue withe teh validatioin engine. As I result I get validation messages like the below. This is likely resolvable but requires more testing.
"$.dueDate": ["…could not be converted to System.DateOnly…"]  - an error message when I validate DueDate. 




Other notes:
1. test url is https://localhost:7242/members/1/rocks

