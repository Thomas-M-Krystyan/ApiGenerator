# ApiGenerator
Console application producing API interfaces from specified concrete C# classes


WHY TO USE:

- If you are working with **legacy code where "API first" rule wasn't a thing** (or it was introduced later, during the development)
  and you still need to create API interfaces from your classes (e.g. for proxies in WCF architecture, or while implementing DDD pattern,
  or for the purposes of services or microservices in general C# backend code (e.g. in Unity 3D, ASP.NET, console app, desktop app, etc.)
  ... then you have to do it manually!
  
  This will take a lot of time and effort to meticulously copy-paste the code from one place to another place, before of that you also need to
  create new interface files and folder structure for them, make a reference between interfaces and their implementation, configure binding for DI
  (Dependency Injection) provide meaningful documentation, and, of course - decide **which classes and which of their members should be even
  considered to be part of an API**!
  
  This application, with minimal effort invested, should help you to automatize generation of API from an already existing code,
  just like ReSharper is able to generate code from existing interface.
  
  The only thing you need to do is to
  1. configure the tool (basic steps)
  2. and decide what was emphasized in the previous paragraphs: "what should be included in my API?". 
  

HOW TO USE:

1. Currently, because of the way how the application is working (mapping assemblies + reflection), at least **ApiGenerator** + **ApiAnnotations** projects
   should be downloaded and included into your original solution. You can use "include existing project" option from Visual Studio.
   - **ApiGenerator** internal project is responsible for generating API from the **source** project (location) into a **target** project (location) e.g., MyCode.sln => MyApi.sln
   - **ApiAnnotations** public project is the only visible part of the ApiGenerator solution. This project delivers special annotations [ApiClass] and [ApiMember]. They will be
     used to determine which classes and members (properties, methods, events, etc.) should be included in the API interface. Later, annotations can be removed by selecting a de-
     dicated option to "clean up" the source project
   - **ApiExamples** internal project is a sample stub, acting like a sample **source** project. Based on this solution you can learn and try how the ApiGenerator app is working
   - **ApiGenerator.Tests** is obviously a unit test solution for the crucial parts of the application logic
2. Create reference between **ApiGenerator** project and your **source** (original code/implementations) projects.
   > It is possible that both source and target projects are inside of the same solution however, they can also persist separately. Only the reference to "source" project is
   > important, since only this project will be used to retrieve assemblies from it (and use retrieved classes as real objects, not just dummy-copy-pasted strings)
3. In **source** (original code/implementations) project create reference to **ApiAnnotations** project (to use API annotations decorators, indicating what should be mapped into API).
4. Configure paths (folders and subfolders) in **ApiGenerator / Configuration / Config.cs** (use included samples as an example)
5. Set **ApiGenerator** project to be your startup project in Visual Studio
6. Press play and check whether you did not make a mistake configuring paths or using API annotations in the source code
7. After setting up points 1-5, the step 6 can be re-triggered over and over again. API files will be overriden in case you modify something inside of your source classes.


NOTES and OBSERVATIONS:

- Once again, this application was designed only for legacy solutions to fill the technical debt gap in shorter time than manual work, since learning, configuring and using this
  application will be still shorter than generating your API manually.
  
  HOWEVER(!!!)
  
  Using this application will create API ==(from)==> CODE dependency, while the relationship should be defined otherwise: CODE (implementations) should be always dependent on the API (interfaces).
  Bear this in mind while using this application, fix your tech debt problems, use cleanup option (the last step of the generation process) to remove API annotations from the source project(s),
  and then eventually remove the **ApiGenerator** project from your solution (de-reference it, for the sake of sanity).
  
  In the future, you should always try to write your code starting from the top-to-bottom approach. In other words, please follow: "API first" rule. :)

- Since the application is already doing it's job, further development is not considered anymore.
  Application was tried in battle (production environment) and succeeded.
  
  For further reference, create your own enhancements if you like. Propositions for nice to have enhancements:
  a) extracting configurations into .json or .xml files, to not recompile the code every time when the current **Config.cs** file was changed. This will reduce the maintenance effort even more(!)
  b) using the new MSBuild library to create Project and Classes programatically and to provide an additional layer of syntax validation (my app will not check or prompt you about invalid syntax beforehand)
  > https://stackoverflow.com/questions/43078438/building-msbuild-15-project-programmatically
  c) creating from this application an extension for Visual Studio, so you do not have to include these projects into your own solution anymore
  
- For backward-compatibility purpose (working with WPF / UI layers) the code was rewritten and downgraded from .NET Core 5 back into .NET Framework 4.7.2 (feel free to use the latest benefit of modern .NET frameworks and latest C# syntax flavors <3 as soon as your legacy project allows for that)


Best wishess,
Thomas M. Krystyan
